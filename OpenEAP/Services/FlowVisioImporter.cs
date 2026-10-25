using OpenEAP.Models.FlowChartDesigner;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OpenEAP.Services
{
    /// <summary>
    /// 基础 Visio (.vsdx) 导入：读取第一页的形状、文本、连接关系与位置。
    /// 旧版二进制 .vsd 不受支持。
    /// </summary>
    public static class FlowVisioImporter
    {
        private static readonly XNamespace Ns = "http://schemas.microsoft.com/office/visio/2012/main";
        private const double PixelsPerInch = 96.0;

        public static FlowDiagram Import(Stream stream, string? fileName = null)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
            var masters = ReadMasters(archive);
            var pageEntry = archive.Entries
                .Where(e => e.FullName.StartsWith("visio/pages/page", StringComparison.OrdinalIgnoreCase) &&
                            e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) &&
                            !e.FullName.EndsWith("pages.xml", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (pageEntry == null)
            {
                throw new InvalidDataException(L("FlowVisioNoPage"));
            }

            XDocument page;
            using (var pageStream = pageEntry.Open())
            {
                page = XDocument.Load(pageStream);
            }

            var pageHeight = ReadCell(page.Root, "PageHeight") ?? 11.0;
            var pageWidth = ReadCell(page.Root, "PageWidth") ?? 8.5;
            var diagram = new FlowDiagram
            {
                Title = string.IsNullOrWhiteSpace(fileName)
                    ? L("FlowVisioImportedTitle")
                    : Path.GetFileNameWithoutExtension(fileName),
                Description = L("FlowVisioDescription")
            };

            var connectors = new HashSet<string>(StringComparer.Ordinal);
            var connects = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var connect in page.Descendants(Ns + "Connect"))
            {
                var from = (string?)connect.Attribute("FromSheet");
                var to = (string?)connect.Attribute("ToSheet");
                if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to)) continue;
                if (!connects.TryGetValue(from, out var targets))
                {
                    targets = new List<string>();
                    connects[from] = targets;
                }
                if (!targets.Contains(to)) targets.Add(to);
            }

            var shapeElements = page.Descendants(Ns + "Shape")
                .Where(s => s.Attribute("ID") != null && s.Ancestors(Ns + "Shape").FirstOrDefault() == null)
                .ToList();
            var shapesById = new Dictionary<string, XElement>(StringComparer.Ordinal);
            foreach (var shape in shapeElements)
            {
                var id = (string?)shape.Attribute("ID");
                if (!string.IsNullOrWhiteSpace(id)) shapesById[id] = shape;
            }
            foreach (var id in connects.Keys) connectors.Add(id);

            var connectorCells = new Dictionary<string, (double BeginX, double BeginY, double EndX, double EndY)>(StringComparer.Ordinal);
            foreach (var shape in shapeElements)
            {
                var id = (string?)shape.Attribute("ID");
                if (string.IsNullOrWhiteSpace(id)) continue;
                var beginX = ReadCell(shape, "BeginX");
                var beginY = ReadCell(shape, "BeginY");
                var endX = ReadCell(shape, "EndX");
                var endY = ReadCell(shape, "EndY");
                if (beginX.HasValue && beginY.HasValue && endX.HasValue && endY.HasValue)
                {
                    connectorCells[id] = (beginX.Value, beginY.Value, endX.Value, endY.Value);
                    if (ReadCell(shape, "PinX").HasValue && !connects.ContainsKey(id))
                    {
                        // 具有起终点坐标但没有连接关系的图形仍按普通节点处理。
                        connectorCells.Remove(id);
                    }
                }
            }

            var nodesById = new Dictionary<string, FlowNode>(StringComparer.Ordinal);
            foreach (var shape in shapeElements)
            {
                var id = (string?)shape.Attribute("ID");
                if (string.IsNullOrWhiteSpace(id) || connectors.Contains(id) || connectorCells.ContainsKey(id)) continue;

                var masterName = ResolveMasterName(shape, masters);
                var text = ExtractText(shape);
                var kind = KindFor(masterName, text);
                var width = Math.Max(48, (ReadCell(shape, "Width") ?? 1.5) * PixelsPerInch);
                var height = Math.Max(40, (ReadCell(shape, "Height") ?? 0.8) * PixelsPerInch);
                var pinX = ReadCell(shape, "PinX");
                var pinY = ReadCell(shape, "PinY");
                if (!pinX.HasValue || !pinY.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    pinX = pageWidth / 2;
                    pinY = pageHeight / 2;
                }

                if (width < 20 || height < 20) continue;
                var node = FlowShapeCatalog.Create(kind,
                    string.IsNullOrWhiteSpace(text) ? FlowShapeCatalog.KindName(kind) : text);
                node.Id = "visio-" + id;
                node.Width = width;
                node.Height = height;
                node.X = (pinX.Value - width / PixelsPerInch / 2) * PixelsPerInch;
                node.Y = (pageHeight - pinY.Value - height / PixelsPerInch / 2) * PixelsPerInch;
                diagram.Nodes.Add(node);
                nodesById[id] = node;
            }

            foreach (var pair in connectorCells)
            {
                if (!connects.TryGetValue(pair.Key, out var targets) || targets.Count == 0) continue;
                var targetNodes = targets
                    .Where(nodesById.ContainsKey)
                    .Select(id => nodesById[id])
                    .ToList();
                if (targetNodes.Count == 0) continue;

                FlowNode source;
                FlowNode target;
                if (targetNodes.Count >= 2)
                {
                    var begin = ToWorld(pair.Value.BeginX, pair.Value.BeginY, pageHeight);
                    var end = ToWorld(pair.Value.EndX, pair.Value.EndY, pageHeight);
                    source = targetNodes.OrderBy(n => DistanceToNode(begin, n)).First();
                    target = targetNodes.OrderBy(n => DistanceToNode(end, n)).First();
                }
                else
                {
                    source = targetNodes[0];
                    target = targetNodes[0];
                }
                if (ReferenceEquals(source, target)) continue;
                if (diagram.Links.Any(l => l.SourceId == source.Id && l.TargetId == target.Id)) continue;

                diagram.Links.Add(new FlowLink
                {
                    SourceId = source.Id,
                    TargetId = target.Id,
                    Style = FlowLinkStyle.Straight,
                    Arrow = FlowArrowStyle.End,
                    Color = "#71809A",
                    StrokeWidth = 2,
                    ShowLabel = false
                });
            }

            if (diagram.Nodes.Count == 0)
            {
                throw new InvalidDataException(L("FlowVisioNoShapes"));
            }
            return diagram;
        }

        private static Dictionary<string, string> ReadMasters(ZipArchive archive)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            var entry = archive.GetEntry("visio/masters/masters.xml");
            if (entry == null) return result;
            try
            {
                using var stream = entry.Open();
                var document = XDocument.Load(stream);
                foreach (var master in document.Descendants(Ns + "Master"))
                {
                    var id = (string?)master.Attribute("ID");
                    var name = (string?)master.Attribute("NameU") ?? (string?)master.Attribute("Name");
                    if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name))
                    {
                        result[id] = name;
                    }
                }
            }
            catch
            {
                // 主图形名称只影响形状映射，失败时使用默认处理。
            }
            return result;
        }

        private static string ResolveMasterName(XElement shape, IReadOnlyDictionary<string, string> masters)
        {
            var masterId = (string?)shape.Attribute("Master");
            return masterId != null && masters.TryGetValue(masterId, out var name) ? name : string.Empty;
        }

        private static FlowNodeKind KindFor(string masterName, string text)
        {
            var value = (masterName + " " + text).ToLowerInvariant();
            if (Matches(value, "start", "end", "terminator", "开始", "结束", "termin")) return FlowNodeKind.StartEnd;
            if (Matches(value, "decision", "判断", "decide", "condition")) return FlowNodeKind.Decision;
            if (Matches(value, "database", "data store", "数据库", "存储")) return FlowNodeKind.Database;
            if (Matches(value, "document", "文档", "report")) return FlowNodeKind.Document;
            if (Matches(value, "data", "input", "output", "数据", "输入", "输出")) return FlowNodeKind.Data;
            if (Matches(value, "subprocess", "sub process", "子流程", "subroutine")) return FlowNodeKind.Subprocess;
            return FlowNodeKind.Process;
        }

        private static bool Matches(string value, params string[] keywords)
        {
            return keywords.Any(k => value.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        private static string ExtractText(XElement shape)
        {
            var textElement = shape.Elements(Ns + "Text").FirstOrDefault();
            if (textElement == null) return string.Empty;
            var builder = new StringBuilder();
            foreach (var node in textElement.DescendantNodes())
            {
                if (node is XText text) builder.Append(text.Value);
                else if (node is XCData cdata) builder.Append(cdata.Value);
            }
            var value = builder.ToString();
            if (string.IsNullOrWhiteSpace(value)) value = textElement.Value;
            return string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }

        private static double? ReadCell(XElement? root, string name)
        {
            if (root == null) return null;
            var cell = root.Descendants(Ns + "Cell")
                .FirstOrDefault(c => string.Equals((string?)c.Attribute("N"), name, StringComparison.OrdinalIgnoreCase));
            var value = (string?)cell?.Attribute("V");
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private static Avalonia.Point ToWorld(double x, double y, double pageHeight)
        {
            return new Avalonia.Point(x * PixelsPerInch, (pageHeight - y) * PixelsPerInch);
        }

        private static double DistanceToNode(Avalonia.Point point, FlowNode node)
        {
            var dx = Math.Max(Math.Max(node.X - point.X, 0), point.X - node.Right);
            var dy = Math.Max(Math.Max(node.Y - point.Y, 0), point.Y - node.Bottom);
            return Math.Sqrt(dx * dx + dy * dy);
        }
        private static string L(string key) => OpenEAP.Localizer.Localizer.Instance[key];

    }
}
