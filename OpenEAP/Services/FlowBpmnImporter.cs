using OpenEAP.Models.FlowChartDesigner;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OpenEAP.Services
{
    /// <summary>读取 BPMN 2.0 XML 的流程元素与 BPMN DI 坐标。</summary>
    public static class FlowBpmnImporter
    {
        private static readonly HashSet<string> StartEndElements = new(StringComparer.Ordinal)
        {
            "startEvent", "endEvent", "intermediateCatchEvent", "intermediateThrowEvent", "boundaryEvent"
        };

        private static readonly HashSet<string> ProcessElements = new(StringComparer.Ordinal)
        {
            "task", "userTask", "serviceTask", "scriptTask", "manualTask", "businessRuleTask",
            "sendTask", "receiveTask", "ruleTask"
        };

        private static readonly HashSet<string> SubprocessElements = new(StringComparer.Ordinal)
        {
            "subProcess", "transaction", "adHocSubProcess", "callActivity"
        };

        private static readonly HashSet<string> GatewayElements = new(StringComparer.Ordinal)
        {
            "exclusiveGateway", "inclusiveGateway", "parallelGateway", "complexGateway", "eventBasedGateway"
        };

        private static readonly HashSet<string> DataElements = new(StringComparer.Ordinal)
        {
            "dataObjectReference", "dataInput", "dataOutput", "dataObject"
        };

        public static FlowDiagram Import(string xml, string? fileName = null)
        {
            XDocument document;
            try
            {
                document = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(L("FlowBpmnInvalidXml"), ex);
            }

            var allElements = document.Descendants().ToList();
            var semanticById = allElements
                .Where(e => e.Attribute("id") != null)
                .GroupBy(e => (string)e.Attribute("id")!)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            var diagram = new FlowDiagram
            {
                Title = ResolveTitle(semanticById, fileName),
                Description = L("FlowBpmnDescription")
            };

            var nodesById = new Dictionary<string, FlowNode>(StringComparer.Ordinal);
            var shapes = allElements.Where(e => e.Name.LocalName is "BPMNShape" or "Shape").ToList();
            foreach (var shape in shapes)
            {
                var elementId = (string?)shape.Attribute("bpmnElement") ?? (string?)shape.Attribute("element");
                if (string.IsNullOrWhiteSpace(elementId) || nodesById.ContainsKey(elementId)) continue;
                if (!semanticById.TryGetValue(elementId, out var semantic)) continue;
                if (!IsFlowNode(semantic.Name.LocalName)) continue;

                var bounds = shape.Descendants().FirstOrDefault(e => e.Name.LocalName == "Bounds");
                var x = ReadDouble(bounds?.Attribute("x"), 0);
                var y = ReadDouble(bounds?.Attribute("y"), 0);
                var width = Math.Max(48, ReadDouble(bounds?.Attribute("width"), 160));
                var height = Math.Max(40, ReadDouble(bounds?.Attribute("height"), 70));

                var kind = KindFor(semantic.Name.LocalName);
                var node = FlowShapeCatalog.Create(kind, ResolveNodeTitle(semantic, kind));
                node.Id = elementId;
                node.X = x;
                node.Y = y;
                node.Width = width;
                node.Height = height;
                ApplyBoundsStyle(shape, node);
                diagram.Nodes.Add(node);
                nodesById[elementId] = node;
            }

            // 没有 BPMN DI 坐标时，仍然把语义层节点导入并自动排布。
            if (nodesById.Count == 0)
            {
                ImportSemanticNodes(allElements, nodesById, diagram);
            }

            var edges = allElements.Where(e => e.Name.LocalName is "BPMNEdge" or "Edge").ToList();
            foreach (var edge in edges)
            {
                var elementId = (string?)edge.Attribute("bpmnElement") ?? (string?)edge.Attribute("element");
                if (string.IsNullOrWhiteSpace(elementId) || !semanticById.TryGetValue(elementId, out var semantic)) continue;
                AddLink(semantic, edge, nodesById, diagram);
            }

            if (edges.Count == 0)
            {
                foreach (var sequence in allElements.Where(e =>
                             e.Name.LocalName is "sequenceFlow" or "messageFlow" or "association"))
                {
                    AddLink(sequence, null, nodesById, diagram);
                }
            }

            if (diagram.Nodes.Count == 0)
            {
                throw new InvalidDataException(L("FlowBpmnNoNodes"));
            }

            if (string.IsNullOrWhiteSpace(diagram.Title))
            {
                diagram.Title = L("FlowBpmnImportedTitle");
            }
            return diagram;
        }

        private static void ImportSemanticNodes(IEnumerable<XElement> allElements,
            IDictionary<string, FlowNode> nodesById, FlowDiagram diagram)
        {
            const int columns = 4;
            var index = 0;
            foreach (var element in allElements.Where(e => IsFlowNode(e.Name.LocalName) && e.Attribute("id") != null))
            {
                var id = (string)element.Attribute("id")!;
                if (nodesById.ContainsKey(id)) continue;

                var kind = KindFor(element.Name.LocalName);
                var node = FlowShapeCatalog.Create(kind, ResolveNodeTitle(element, kind));
                node.Id = id;
                node.X = 120 + index % columns * 260;
                node.Y = 100 + index / columns * 150;
                nodesById[id] = node;
                diagram.Nodes.Add(node);
                index++;
            }
        }

        private static void AddLink(XElement semantic, XElement? edge,
            IReadOnlyDictionary<string, FlowNode> nodesById, FlowDiagram diagram)
        {
            var sourceId = (string?)semantic.Attribute("sourceRef");
            var targetId = (string?)semantic.Attribute("targetRef");
            if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(targetId)) return;
            if (!nodesById.ContainsKey(sourceId) || !nodesById.ContainsKey(targetId) || sourceId == targetId) return;
            if (diagram.Links.Any(l => l.SourceId == sourceId && l.TargetId == targetId)) return;

            var label = ((string?)semantic.Attribute("name"))?.Trim() ?? string.Empty;
            var link = new FlowLink
            {
                Id = (string?)semantic.Attribute("id") ?? Guid.NewGuid().ToString("N"),
                SourceId = sourceId,
                TargetId = targetId,
                Label = label,
                ShowLabel = !string.IsNullOrWhiteSpace(label),
                Style = FlowLinkStyle.Straight,
                Arrow = FlowArrowStyle.End,
                Color = "#71809A",
                StrokeWidth = 2
            };

            if (edge != null)
            {
                foreach (var waypoint in edge.Descendants().Where(e => e.Name.LocalName == "waypoint"))
                {
                    link.Waypoints.Add(new FlowWaypoint(
                        ReadDouble(waypoint.Attribute("x"), 0),
                        ReadDouble(waypoint.Attribute("y"), 0)));
                }
            }

            diagram.Links.Add(link);
        }

        private static void ApplyBoundsStyle(XElement shape, FlowNode node)
        {
            // 兼容部分工具写入的 BPMN DI 颜色扩展，未指定时使用形状库默认色。
            var fill = shape.Attributes().FirstOrDefault(a => a.Name.LocalName == "fill")?.Value
                       ?? shape.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fill")
                           ?.Attribute("color")?.Value;
            if (fill is { Length: 7 or 9 } && fill.StartsWith('#'))
            {
                node.Fill = fill;
            }
        }

        private static bool IsFlowNode(string localName)
        {
            return StartEndElements.Contains(localName) ||
                   ProcessElements.Contains(localName) ||
                   SubprocessElements.Contains(localName) ||
                   GatewayElements.Contains(localName) ||
                   DataElements.Contains(localName) ||
                   localName is "textAnnotation" or "dataStoreReference";
        }

        private static FlowNodeKind KindFor(string localName)
        {
            if (StartEndElements.Contains(localName)) return FlowNodeKind.StartEnd;
            if (SubprocessElements.Contains(localName)) return FlowNodeKind.Subprocess;
            if (GatewayElements.Contains(localName)) return FlowNodeKind.Decision;
            if (localName == "dataStoreReference") return FlowNodeKind.Database;
            if (DataElements.Contains(localName)) return FlowNodeKind.Data;
            if (localName == "textAnnotation") return FlowNodeKind.Document;
            return FlowNodeKind.Process;
        }

        private static string ResolveNodeTitle(XElement element, FlowNodeKind kind)
        {
            var name = ((string?)element.Attribute("name"))?.Trim();
            if (!string.IsNullOrWhiteSpace(name)) return name;

            return element.Name.LocalName switch
            {
                "startEvent" => L("FlowBpmnStart"),
                "endEvent" => L("FlowBpmnEnd"),
                "exclusiveGateway" => L("FlowBpmnGateway"),
                "parallelGateway" => L("FlowBpmnParallelGateway"),
                "inclusiveGateway" => L("FlowBpmnInclusiveGateway"),
                "eventBasedGateway" => L("FlowBpmnEventGateway"),
                "dataObjectReference" => L("FlowBpmnDataObject"),
                "dataStoreReference" => L("FlowBpmnDataStore"),
                "textAnnotation" => L("FlowBpmnAnnotation"),
                "subProcess" or "callActivity" => L("FlowBpmnSubprocess"),
                _ => FlowShapeCatalog.KindName(kind)
            };
        }

        private static string ResolveTitle(IReadOnlyDictionary<string, XElement> semanticById, string? fileName)
        {
            var process = semanticById.Values.FirstOrDefault(e => e.Name.LocalName is "process" or "collaboration");
            var name = ((string?)process?.Attribute("name"))?.Trim();
            if (!string.IsNullOrWhiteSpace(name)) return name;

            var candidate = string.IsNullOrWhiteSpace(fileName) ? L("FlowBpmnImportedTitle") : Path.GetFileNameWithoutExtension(fileName);
            return string.IsNullOrWhiteSpace(candidate) ? L("FlowBpmnImportedTitle") : candidate;
        }

        private static double ReadDouble(XAttribute? attribute, double fallback)
        {
            if (attribute == null) return fallback;
            return double.TryParse(attribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;
        }
        private static string L(string key) => OpenEAP.Localizer.Localizer.Instance[key];

    }
}
