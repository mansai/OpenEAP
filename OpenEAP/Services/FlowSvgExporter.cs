using Avalonia;
using Avalonia.Media;
using OpenEAP.Models.FlowChartDesigner;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OpenEAP.Services
{
    /// <summary>将流程图导出为独立可缩放的 SVG 矢量图。</summary>
    public static class FlowSvgExporter
    {
        private static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";
        private const string FontFamily = "Microsoft YaHei UI, Segoe UI, Arial, sans-serif";

        public static string Export(FlowDiagram document, double padding = 64)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var routes = new Dictionary<FlowLink, Route>(ReferenceEqualityComparer.Instance);
            var content = new Rect(0, 0, 0, 0);
            var hasContent = false;

            foreach (var node in document.Nodes)
            {
                var rect = GetNodeVisualBounds(node);
                content = hasContent ? content.Union(rect) : rect;
                hasContent = true;
            }

            foreach (var link in document.Links)
            {
                var source = document.Nodes.FirstOrDefault(n => n.Id == link.SourceId);
                var target = document.Nodes.FirstOrDefault(n => n.Id == link.TargetId);
                if (source == null || target == null) continue;
                var route = BuildRoute(link, source, target);
                routes[link] = route;
                foreach (var point in route.Points)
                {
                    var pointRect = new Rect(point.X, point.Y, 0.1, 0.1);
                    content = hasContent ? content.Union(pointRect) : pointRect;
                    hasContent = true;
                }
            }

            if (!hasContent || content.Width < 1 || content.Height < 1)
            {
                content = new Rect(0, 0, 640, 360);
            }

            content = content.Inflate(padding);
            var width = Math.Max(1, content.Width);
            var height = Math.Max(1, content.Height);

            var root = new XElement(SvgNs + "svg",
                new XAttribute("xmlns", SvgNs.NamespaceName),
                new XAttribute("version", "1.1"),
                new XAttribute("width", F(width)),
                new XAttribute("height", F(height)),
                new XAttribute("viewBox", $"{F(content.X)} {F(content.Y)} {F(width)} {F(height)}"),
                new XAttribute("preserveAspectRatio", "xMidYMid meet"));

            root.Add(new XElement(SvgNs + "defs",
                new XElement(SvgNs + "filter",
                    new XAttribute("id", "flow-node-shadow"),
                    new XAttribute("x", "-20%"), new XAttribute("y", "-20%"),
                    new XAttribute("width", "140%"), new XAttribute("height", "150%"),
                    new XElement(SvgNs + "feDropShadow",
                        new XAttribute("dx", "0"), new XAttribute("dy", "2.5"),
                        new XAttribute("stdDeviation", "2.2"),
                        new XAttribute("flood-color", "#0F172A"),
                        new XAttribute("flood-opacity", "0.22")))));

            root.Add(new XElement(SvgNs + "rect",
                new XAttribute("x", F(content.X)), new XAttribute("y", F(content.Y)),
                new XAttribute("width", F(width)), new XAttribute("height", F(height)),
                new XAttribute("fill", "#FFFFFF")));

            var linkLayer = new XElement(SvgNs + "g", new XAttribute("id", "links"));
            foreach (var link in document.Links)
            {
                if (!routes.TryGetValue(link, out var route)) continue;
                var color = string.IsNullOrWhiteSpace(link.Color) ? "#71809A" : link.Color;
                var points = route.Points;
                if (points.Count >= 2)
                {
                    if (route.Curved)
                    {
                        linkLayer.Add(new XElement(SvgNs + "path",
                            new XAttribute("d", BuildCurvePath(points[0], route.Control1, route.Control2, points[^1])),
                            new XAttribute("fill", "none"),
                            new XAttribute("stroke", color),
                            new XAttribute("stroke-width", F(link.StrokeWidth)),
                            new XAttribute("stroke-linecap", "round"),
                            new XAttribute("stroke-linejoin", "round"),
                            link.IsDashed ? new XAttribute("stroke-dasharray", "6 4") : null));
                    }
                    else
                    {
                        linkLayer.Add(new XElement(SvgNs + "polyline",
                            new XAttribute("points", string.Join(" ", points.Select(p => $"{F(p.X)},{F(p.Y)}"))),
                            new XAttribute("fill", "none"),
                            new XAttribute("stroke", color),
                            new XAttribute("stroke-width", F(link.StrokeWidth)),
                            new XAttribute("stroke-linecap", "round"),
                            new XAttribute("stroke-linejoin", "round"),
                            link.IsDashed ? new XAttribute("stroke-dasharray", "6 4") : null));
                    }

                    var arrowSize = Math.Clamp(link.StrokeWidth * 4.4, 7, 13);
                    if (link.Arrow is FlowArrowStyle.End or FlowArrowStyle.Both)
                    {
                        var tip = points[^1];
                        var direction = route.Curved
                            ? tip - route.Control2
                            : tip - points[^2];
                        AddArrow(linkLayer, tip, direction, color, arrowSize);
                    }
                    if (link.Arrow == FlowArrowStyle.Both)
                    {
                        var tip = points[0];
                        var direction = route.Curved
                            ? route.Control1 - tip
                            : points[0] - points[1];
                        AddArrow(linkLayer, tip, direction, color, arrowSize);
                    }
                }

                if (!string.IsNullOrWhiteSpace(link.Label))
                {
                    linkLayer.Add(BuildLinkLabel(link, route.LabelPoint, color));
                }
            }
            root.Add(linkLayer);

            var nodeLayer = new XElement(SvgNs + "g", new XAttribute("id", "nodes"));
            foreach (var node in document.Nodes)
            {
                nodeLayer.Add(BuildNode(node));
            }
            root.Add(nodeLayer);

            return new XDocument(new XDeclaration("1.0", "utf-8", "no"), root)
                .ToString(SaveOptions.DisableFormatting);
        }

        private static XElement BuildNode(FlowNode node)
        {
            var stroke = string.IsNullOrWhiteSpace(node.Stroke)
                ? node.Kind == FlowNodeKind.Image ? "#CBD5E1" : Lighten(node.Fill, 0.22)
                : node.Stroke;
            var group = new XElement(SvgNs + "g",
                new XAttribute("id", "node-" + XmlId(node.Id)),
                new XAttribute("filter", "url(#flow-node-shadow)"));
            if (Math.Abs(node.Rotation) >= 0.01)
            {
                group.Add(new XAttribute("transform",
                    $"rotate({F(node.Rotation)} {F(node.CenterX)} {F(node.CenterY)})"));
            }

            if (node.Kind == FlowNodeKind.Image)
            {
                BuildImageNode(group, node, stroke);
                return group;
            }

            group.Add(BuildNodeShape(node, stroke));

            if (node.Kind == FlowNodeKind.Subprocess)
            {
                group.Add(new XElement(SvgNs + "line",
                    new XAttribute("x1", F(node.X + 13)), new XAttribute("y1", F(node.Y + 9)),
                    new XAttribute("x2", F(node.X + 13)), new XAttribute("y2", F(node.Bottom - 9)),
                    new XAttribute("stroke", "#FFFFFF"), new XAttribute("stroke-opacity", "0.32"),
                    new XAttribute("stroke-width", "1")));
                group.Add(new XElement(SvgNs + "line",
                    new XAttribute("x1", F(node.Right - 13)), new XAttribute("y1", F(node.Y + 9)),
                    new XAttribute("x2", F(node.Right - 13)), new XAttribute("y2", F(node.Bottom - 9)),
                    new XAttribute("stroke", "#FFFFFF"), new XAttribute("stroke-opacity", "0.32"),
                    new XAttribute("stroke-width", "1")));
            }
            else if (node.Kind == FlowNodeKind.Database)
            {
                var ry = Math.Min(16, node.Height * 0.16);
                group.Add(new XElement(SvgNs + "ellipse",
                    new XAttribute("cx", F(node.CenterX)), new XAttribute("cy", F(node.Y + ry + 1)),
                    new XAttribute("rx", F(Math.Max(1, node.Width / 2 - 1))), new XAttribute("ry", F(ry)),
                    new XAttribute("fill", "none"), new XAttribute("stroke", "#FFFFFF"),
                    new XAttribute("stroke-opacity", "0.38"), new XAttribute("stroke-width", "1.1")));
            }

            foreach (var text in BuildNodeText(node))
            {
                group.Add(text);
            }
            return group;
        }

        private static void BuildImageNode(XElement group, FlowNode node, string stroke)
        {
            var radius = Math.Max(0, Math.Min(node.CornerRadius, Math.Min(node.Width, node.Height) / 2));
            var clipId = "clip-node-" + XmlId(node.Id);
            group.Add(new XElement(SvgNs + "defs",
                new XElement(SvgNs + "clipPath", new XAttribute("id", clipId),
                    new XElement(SvgNs + "rect",
                        new XAttribute("x", F(node.X)), new XAttribute("y", F(node.Y)),
                        new XAttribute("width", F(node.Width)), new XAttribute("height", F(node.Height)),
                        new XAttribute("rx", F(radius)), new XAttribute("ry", F(radius))))));

            group.Add(new XElement(SvgNs + "rect",
                new XAttribute("x", F(node.X)), new XAttribute("y", F(node.Y)),
                new XAttribute("width", F(node.Width)), new XAttribute("height", F(node.Height)),
                new XAttribute("rx", F(radius)), new XAttribute("ry", F(radius)),
                new XAttribute("fill", string.IsNullOrWhiteSpace(node.Fill) ? "#F1F5F9" : node.Fill),
                new XAttribute("stroke", stroke), new XAttribute("stroke-width", F(node.StrokeWidth))));

            if (!string.IsNullOrWhiteSpace(node.ImageData))
            {
                var mime = string.IsNullOrWhiteSpace(node.ImageMime) ? "image/png" : node.ImageMime;
                var image = new XElement(SvgNs + "image",
                    new XAttribute("x", F(node.X + 2)), new XAttribute("y", F(node.Y + 2)),
                    new XAttribute("width", F(Math.Max(1, node.Width - 4))),
                    new XAttribute("height", F(Math.Max(1, node.Height - 4))),
                    new XAttribute("preserveAspectRatio", "xMidYMid meet"),
                    new XAttribute("clip-path", $"url(#{clipId})"),
                    new XAttribute("href", $"data:{mime};base64,{node.ImageData}"),
                    new XAttribute(XNamespace.Xml + "space", "preserve"));
                group.Add(image);
            }
            else
            {
                var placeholder = new XElement(SvgNs + "text",
                    new XAttribute("x", F(node.CenterX)), new XAttribute("y", F(node.CenterY)),
                    new XAttribute("fill", "#64748B"), new XAttribute("font-family", FontFamily),
                    new XAttribute("font-size", "11"), new XAttribute("text-anchor", "middle"),
                    OpenEAP.Localizer.Localizer.Instance["FlowShapeImage"]);
                group.Add(placeholder);
            }

            if (!string.IsNullOrWhiteSpace(node.Title))
            {
                var captionHeight = Math.Min(node.Height * 0.32, 34);
                group.Add(new XElement(SvgNs + "rect",
                    new XAttribute("x", F(node.X)), new XAttribute("y", F(node.Bottom - captionHeight)),
                    new XAttribute("width", F(node.Width)), new XAttribute("height", F(captionHeight)),
                    new XAttribute("rx", F(radius)), new XAttribute("ry", F(radius)),
                    new XAttribute("fill", "#0F172A"), new XAttribute("fill-opacity", "0.72"),
                    new XAttribute("clip-path", $"url(#{clipId})")));
                group.Add(new XElement(SvgNs + "text",
                    new XAttribute("x", F(node.CenterX)),
                    new XAttribute("y", F(node.Bottom - captionHeight / 2 + 4)),
                    new XAttribute("fill", "#FFFFFF"), new XAttribute("font-family", FontFamily),
                    new XAttribute("font-size", "12"), new XAttribute("font-weight", "600"),
                    new XAttribute("text-anchor", "middle"),
                    Truncate(node.Title.Trim(), 28)));
            }

            group.Add(new XElement(SvgNs + "rect",
                new XAttribute("x", F(node.X)), new XAttribute("y", F(node.Y)),
                new XAttribute("width", F(node.Width)), new XAttribute("height", F(node.Height)),
                new XAttribute("rx", F(radius)), new XAttribute("ry", F(radius)),
                new XAttribute("fill", "none"), new XAttribute("stroke", stroke),
                new XAttribute("stroke-width", F(node.StrokeWidth))));
        }

        private static XElement BuildNodeShape(FlowNode node, string stroke)
        {
            var common = new object[]
            {
                new XAttribute("fill", string.IsNullOrWhiteSpace(node.Fill) ? "#2563EB" : node.Fill),
                new XAttribute("stroke", stroke),
                new XAttribute("stroke-width", F(node.StrokeWidth))
            };
            var left = node.X;
            var top = node.Y;
            var right = node.Right;
            var bottom = node.Bottom;
            var centerX = node.CenterX;
            var centerY = node.CenterY;

            switch (node.Kind)
            {
                case FlowNodeKind.Connector:
                    return new XElement(SvgNs + "ellipse",
                        new object[]
                        {
                            new XAttribute("cx", F(centerX)), new XAttribute("cy", F(centerY)),
                            new XAttribute("rx", F(node.Width / 2)), new XAttribute("ry", F(node.Height / 2))
                        }.Concat(common));
                case FlowNodeKind.Decision:
                    return new XElement(SvgNs + "path",
                        new object[]
                        {
                            new XAttribute("d", $"M {F(centerX)} {F(top)} L {F(right)} {F(centerY)} L {F(centerX)} {F(bottom)} L {F(left)} {F(centerY)} Z")
                        }.Concat(common));
                case FlowNodeKind.Data:
                {
                    var skew = Math.Min(24, node.Width * 0.12);
                    return new XElement(SvgNs + "path",
                        new object[]
                        {
                            new XAttribute("d", $"M {F(left + skew)} {F(top)} L {F(right)} {F(top)} L {F(right - skew)} {F(bottom)} L {F(left)} {F(bottom)} Z")
                        }.Concat(common));
                }
                case FlowNodeKind.Document:
                {
                    var waveTop = bottom - node.Height * 0.12;
                    var d = $"M {F(left)} {F(top)} L {F(right)} {F(top)} L {F(right)} {F(waveTop)} " +
                            $"C {F(right * 0.84 + left * 0.16)} {F(bottom + 1)} {F(right * 0.66 + left * 0.34)} {F(bottom - node.Height * 0.24)} {F(centerX)} {F(bottom - node.Height * 0.12)} " +
                            $"C {F(right * 0.28 + left * 0.72)} {F(bottom - 1)} {F(right * 0.14 + left * 0.86)} {F(bottom - node.Height * 0.23)} {F(left)} {F(waveTop)} Z";
                    return new XElement(SvgNs + "path", new object[] { new XAttribute("d", d) }.Concat(common));
                }
                case FlowNodeKind.Database:
                {
                    var ry = Math.Min(16, node.Height * 0.16);
                    var rx = Math.Max(1, node.Width / 2);
                    var d = $"M {F(left)} {F(top + ry)} A {F(rx)} {F(ry)} 0 0 1 {F(right)} {F(top + ry)} " +
                            $"L {F(right)} {F(bottom - ry)} A {F(rx)} {F(ry)} 0 0 1 {F(left)} {F(bottom - ry)} Z";
                    return new XElement(SvgNs + "path", new object[] { new XAttribute("d", d) }.Concat(common));
                }
                default:
                {
                    var radius = node.Kind == FlowNodeKind.StartEnd
                        ? Math.Min(node.Height / 2, node.Width / 2)
                        : Math.Max(0, Math.Min(node.CornerRadius, Math.Min(node.Width, node.Height) / 2));
                    return new XElement(SvgNs + "rect",
                        new object[]
                        {
                            new XAttribute("x", F(left)), new XAttribute("y", F(top)),
                            new XAttribute("width", F(node.Width)), new XAttribute("height", F(node.Height)),
                            new XAttribute("rx", F(radius)), new XAttribute("ry", F(radius))
                        }.Concat(common));
                }
            }
        }

        private static IEnumerable<XElement> BuildNodeText(FlowNode node)
        {
            var title = string.IsNullOrWhiteSpace(node.Title) ? string.Empty : node.Title.Trim();
            var subtitle = !string.IsNullOrWhiteSpace(node.Subtitle) && node.Kind != FlowNodeKind.Connector
                ? node.Subtitle.Trim()
                : string.Empty;
            if (title.Length == 0 && subtitle.Length == 0) yield break;

            var availableWidth = node.Kind == FlowNodeKind.Decision
                ? node.Width * 0.58
                : node.Kind == FlowNodeKind.Connector
                    ? node.Width * 0.68
                    : node.Width - 34;
            availableWidth = Math.Max(30, availableWidth);

            var titleFontSize = node.Kind == FlowNodeKind.Connector
                ? Math.Min(node.FontSize, 22)
                : node.FontSize;
            var titleLines = WrapText(title, titleFontSize, availableWidth, 4);
            var subtitleFontSize = Math.Max(9, titleFontSize * 0.72);
            var subtitleLines = subtitle.Length == 0
                ? new List<string>()
                : WrapText(subtitle, subtitleFontSize, availableWidth, 3);

            var titleLineHeight = titleFontSize * 1.24;
            var subtitleLineHeight = subtitleFontSize * 1.22;
            var gap = subtitleLines.Count > 0 ? 4 : 0;
            var totalHeight = titleLines.Count * titleLineHeight + gap + subtitleLines.Count * subtitleLineHeight;
            var startY = node.CenterY - totalHeight / 2;
            if (node.Kind == FlowNodeKind.Decision) startY += 1;
            if (node.Kind == FlowNodeKind.Database) startY += 8;

            var fill = string.IsNullOrWhiteSpace(node.TextColor) ? "#FFFFFF" : node.TextColor;
            var fontWeight = node.IsBold ? "600" : "400";
            var y = startY;
            foreach (var line in titleLines)
            {
                y += titleFontSize;
                yield return new XElement(SvgNs + "text",
                    new XAttribute("x", F(node.CenterX)), new XAttribute("y", F(y)),
                    new XAttribute("fill", fill), new XAttribute("font-family", FontFamily),
                    new XAttribute("font-size", F(titleFontSize)), new XAttribute("font-weight", fontWeight),
                    new XAttribute("text-anchor", "middle"), line);
                y += titleLineHeight - titleFontSize;
            }

            if (subtitleLines.Count > 0)
            {
                y += gap;
                var subtitleFill = string.IsNullOrWhiteSpace(node.TextColor) ? "#E2E8F0" : node.TextColor;
                foreach (var line in subtitleLines)
                {
                    y += subtitleFontSize;
                    yield return new XElement(SvgNs + "text",
                        new XAttribute("x", F(node.CenterX)), new XAttribute("y", F(y)),
                        new XAttribute("fill", subtitleFill), new XAttribute("fill-opacity", "0.78"),
                        new XAttribute("font-family", FontFamily),
                        new XAttribute("font-size", F(subtitleFontSize)), new XAttribute("font-weight", "400"),
                        new XAttribute("text-anchor", "middle"), line);
                    y += subtitleLineHeight - subtitleFontSize;
                }
            }
        }

        private static XElement BuildLinkLabel(FlowLink link, Point center, string color)
        {
            var fontSize = 11.5;
            var label = FitLabel(link.Label.Trim(), fontSize, 150);
            var textWidth = Math.Min(150, Math.Max(1, MeasureText(label, fontSize)));
            var group = new XElement(SvgNs + "g", new XAttribute("id", "link-label-" + XmlId(link.Id)));

            if (link.ShowLabel)
            {
                var width = textWidth + 18;
                var height = fontSize + 8;
                var x = center.X - width / 2;
                var y = center.Y - height / 2;
                group.Add(new XElement(SvgNs + "rect",
                    new XAttribute("x", F(x)), new XAttribute("y", F(y)),
                    new XAttribute("width", F(width)), new XAttribute("height", F(height)),
                    new XAttribute("rx", F(height / 2)), new XAttribute("ry", F(height / 2)),
                    new XAttribute("fill", "#FFFFFF"), new XAttribute("fill-opacity", "0.96"),
                    new XAttribute("stroke", color), new XAttribute("stroke-opacity", "0.68"),
                    new XAttribute("stroke-width", "0.9")));
            }

            group.Add(new XElement(SvgNs + "text",
                new XAttribute("x", F(center.X)), new XAttribute("y", F(center.Y + fontSize * 0.35)),
                new XAttribute("fill", "#263243"), new XAttribute("font-family", FontFamily),
                new XAttribute("font-size", F(fontSize)), new XAttribute("text-anchor", "middle"),
                new XAttribute("paint-order", "stroke"),
                new XAttribute("stroke", "#FFFFFF"), new XAttribute("stroke-width", "2.4"),
                new XAttribute("stroke-linejoin", "round"), label));
            return group;
        }

        private static void AddArrow(XElement parent, Point tip, Vector direction, string color, double size)
        {
            if (direction.Length < 0.001) return;
            direction = direction / direction.Length;
            var normal = new Vector(-direction.Y, direction.X);
            var baseCenter = tip - direction * size;
            var left = baseCenter + normal * (size * 0.52);
            var right = baseCenter - normal * (size * 0.52);
            parent.Add(new XElement(SvgNs + "path",
                new XAttribute("d", $"M {F(tip.X)} {F(tip.Y)} L {F(left.X)} {F(left.Y)} L {F(right.X)} {F(right.Y)} Z"),
                new XAttribute("fill", color)));
        }

        private static Route BuildRoute(FlowLink link, FlowNode source, FlowNode target)
        {
            var sourceSide = link.SourcePort == FlowPortSide.Auto
                ? ChooseBestSide(source, target)
                : link.SourcePort;
            var targetSide = link.TargetPort == FlowPortSide.Auto
                ? ChooseBestSide(target, source)
                : link.TargetPort;
            var p0 = GetAnchor(source, sourceSide);
            var p3 = GetAnchor(target, targetSide);

            if (link.Waypoints is { Count: >= 2 })
            {
                var imported = link.Waypoints.Select(p => new Point(p.X, p.Y)).ToList();
                if (Distance(imported[0], p0) <= 36 && Distance(imported[^1], p3) <= 36)
                {
                    imported[0] = p0;
                    imported[^1] = p3;
                    imported = Simplify(imported);
                    return new Route(imported, false, default, default, MidPoint(imported));
                }
            }

            if (link.Style == FlowLinkStyle.Straight)
            {
                var points = new List<Point> { p0, p3 };
                return new Route(points, false, default, default,
                    new Point((p0.X + p3.X) / 2, (p0.Y + p3.Y) / 2));
            }

            if (link.Style == FlowLinkStyle.Curved)
            {
                var distance = Math.Max(80, Distance(p0, p3) * 0.42);
                var c1 = p0 + RotateVector(NormalOf(sourceSide), source.Rotation) * distance;
                var c2 = p3 + RotateVector(NormalOf(targetSide), target.Rotation) * distance;
                var points = new List<Point> { p0, p3 };
                return new Route(points, true, c1, c2, BezierPoint(p0, c1, c2, p3, 0.5));
            }

            const double stub = 28.0;
            var p1 = p0 + RotateVector(NormalOf(sourceSide), source.Rotation) * stub;
            var p2 = p3 + RotateVector(NormalOf(targetSide), target.Rotation) * stub;
            var routePoints = new List<Point> { p0, p1 };
            var sourceHorizontal = sourceSide is FlowPortSide.Left or FlowPortSide.Right;
            var targetHorizontal = targetSide is FlowPortSide.Left or FlowPortSide.Right;

            if (sourceHorizontal && targetHorizontal)
            {
                if (Math.Sign(p1.X - p0.X) != Math.Sign(p2.X - p3.X))
                {
                    var midX = (p1.X + p2.X) / 2;
                    routePoints.Add(new Point(midX, p1.Y));
                    routePoints.Add(new Point(midX, p2.Y));
                }
                else
                {
                    var detourX = p1.X >= p2.X
                        ? Math.Max(p1.X, p2.X) + 44
                        : Math.Min(p1.X, p2.X) - 44;
                    routePoints.Add(new Point(detourX, p1.Y));
                    routePoints.Add(new Point(detourX, p2.Y));
                }
            }
            else if (!sourceHorizontal && !targetHorizontal)
            {
                if (Math.Sign(p1.Y - p0.Y) != Math.Sign(p2.Y - p3.Y))
                {
                    var midY = (p1.Y + p2.Y) / 2;
                    routePoints.Add(new Point(p1.X, midY));
                    routePoints.Add(new Point(p2.X, midY));
                }
                else
                {
                    var detourY = p1.Y >= p2.Y
                        ? Math.Max(p1.Y, p2.Y) + 44
                        : Math.Min(p1.Y, p2.Y) - 44;
                    routePoints.Add(new Point(p1.X, detourY));
                    routePoints.Add(new Point(p2.X, detourY));
                }
            }
            else if (sourceHorizontal)
            {
                routePoints.Add(new Point(p2.X, p1.Y));
            }
            else
            {
                routePoints.Add(new Point(p1.X, p2.Y));
            }

            routePoints.Add(p2);
            routePoints.Add(p3);
            routePoints = Simplify(routePoints);
            return new Route(routePoints, false, default, default, MidPoint(routePoints));
        }

        private static Point MidPoint(IReadOnlyList<Point> points)
        {
            var total = 0.0;
            for (var i = 1; i < points.Count; i++) total += Distance(points[i - 1], points[i]);
            if (total <= 0.001) return points.Count > 0 ? points[0] : default;
            var half = total / 2;
            var walked = 0.0;
            for (var i = 1; i < points.Count; i++)
            {
                var segment = Distance(points[i - 1], points[i]);
                if (walked + segment >= half)
                {
                    var t = segment <= 0.001 ? 0 : (half - walked) / segment;
                    return new Point(
                        points[i - 1].X + (points[i].X - points[i - 1].X) * t,
                        points[i - 1].Y + (points[i].Y - points[i - 1].Y) * t);
                }
                walked += segment;
            }
            return points[^1];
        }

        private static List<Point> Simplify(IReadOnlyList<Point> points)
        {
            var result = new List<Point>();
            foreach (var point in points)
            {
                if (result.Count > 0 && Distance(result[^1], point) < 0.5) continue;
                result.Add(point);
            }
            for (var i = result.Count - 2; i > 0; i--)
            {
                var a = result[i - 1];
                var b = result[i];
                var c = result[i + 1];
                var cross = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
                if (Math.Abs(cross) < 0.01) result.RemoveAt(i);
            }
            return result;
        }

        private static List<string> WrapText(string text, double fontSize, double maxWidth, int maxLines)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text)) return result;
            var builder = new StringBuilder();
            var width = 0.0;
            foreach (var ch in text)
            {
                if (ch == '\r') continue;
                if (ch == '\n')
                {
                    result.Add(builder.ToString().TrimEnd());
                    builder.Clear();
                    width = 0;
                    continue;
                }

                var charWidth = CharWidth(ch, fontSize);
                if (builder.Length > 0 && width + charWidth > maxWidth)
                {
                    result.Add(builder.ToString().TrimEnd());
                    builder.Clear();
                    width = 0;
                    if (result.Count >= maxLines) break;
                }
                builder.Append(ch);
                width += charWidth;
            }

            if (result.Count < maxLines && builder.Length > 0)
            {
                result.Add(builder.ToString().TrimEnd());
            }
            else if (result.Count >= maxLines && builder.Length > 0)
            {
                var last = result[^1];
                if (last.Length > 1) result[^1] = last[..^1] + "…";
            }
            if (result.Count == 0) result.Add(string.Empty);
            return result;
        }

        private static double CharWidth(char ch, double fontSize)
        {
            if (char.IsWhiteSpace(ch)) return fontSize * 0.34;
            if (ch < 128) return fontSize * 0.57;
            return fontSize;
        }

        private static double MeasureText(string text, double fontSize)
        {
            return text.Sum(ch => CharWidth(ch, fontSize));
        }

        private static string FitLabel(string text, double fontSize, double maxWidth)
        {
            if (text.Length == 0 || MeasureText(text, fontSize) <= maxWidth) return text;
            var length = text.Length;
            while (length > 1 && MeasureText(text[..length] + "…", fontSize) > maxWidth)
            {
                length--;
            }
            return text[..length] + "…";
        }

        private static FlowPortSide ChooseBestSide(FlowNode node, FlowNode other)
        {
            var dx = other.CenterX - node.CenterX;
            var dy = other.CenterY - node.CenterY;
            if (Math.Abs(dx) >= Math.Abs(dy) * 0.95)
            {
                return dx >= 0 ? FlowPortSide.Right : FlowPortSide.Left;
            }
            return dy >= 0 ? FlowPortSide.Bottom : FlowPortSide.Top;
        }

        private static Point GetAnchor(FlowNode node, FlowPortSide side)
        {
            var anchor = side switch
            {
                FlowPortSide.Left => new Point(node.X, node.CenterY),
                FlowPortSide.Top => new Point(node.CenterX, node.Y),
                FlowPortSide.Right => new Point(node.Right, node.CenterY),
                FlowPortSide.Bottom => new Point(node.CenterX, node.Bottom),
                _ => new Point(node.CenterX, node.CenterY)
            };
            return RotatePoint(anchor, new Point(node.CenterX, node.CenterY), node.Rotation);
        }

        private static Point RotatePoint(Point point, Point center, double degrees)
        {
            if (Math.Abs(degrees) < 0.01) return point;
            var radians = degrees * Math.PI / 180;
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);
            var dx = point.X - center.X;
            var dy = point.Y - center.Y;
            return new Point(center.X + dx * cos - dy * sin, center.Y + dx * sin + dy * cos);
        }

        private static Vector RotateVector(Vector vector, double degrees)
        {
            if (Math.Abs(degrees) < 0.01) return vector;
            var radians = degrees * Math.PI / 180;
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);
            return new Vector(vector.X * cos - vector.Y * sin, vector.X * sin + vector.Y * cos);
        }

        private static Rect GetNodeVisualBounds(FlowNode node)
        {
            if (Math.Abs(node.Rotation) < 0.01) return node.Bounds;
            var center = new Point(node.CenterX, node.CenterY);
            var corners = new[]
            {
                RotatePoint(node.Bounds.TopLeft, center, node.Rotation),
                RotatePoint(node.Bounds.TopRight, center, node.Rotation),
                RotatePoint(node.Bounds.BottomRight, center, node.Rotation),
                RotatePoint(node.Bounds.BottomLeft, center, node.Rotation)
            };
            var left = corners.Min(p => p.X);
            var top = corners.Min(p => p.Y);
            var right = corners.Max(p => p.X);
            var bottom = corners.Max(p => p.Y);
            return new Rect(left, top, Math.Max(1, right - left), Math.Max(1, bottom - top));
        }

        private static string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value[..Math.Max(1, maxLength - 1)] + "…";
        }

        private static Vector NormalOf(FlowPortSide side)
        {
            return side switch
            {
                FlowPortSide.Left => new Vector(-1, 0),
                FlowPortSide.Top => new Vector(0, -1),
                FlowPortSide.Right => new Vector(1, 0),
                FlowPortSide.Bottom => new Vector(0, 1),
                _ => new Vector(0, 1)
            };
        }

        private static Point BezierPoint(Point p0, Point p1, Point p2, Point p3, double t)
        {
            var mt = 1 - t;
            var x = mt * mt * mt * p0.X + 3 * mt * mt * t * p1.X + 3 * mt * t * t * p2.X + t * t * t * p3.X;
            var y = mt * mt * mt * p0.Y + 3 * mt * mt * t * p1.Y + 3 * mt * t * t * p2.Y + t * t * t * p3.Y;
            return new Point(x, y);
        }

        private static string BuildCurvePath(Point p0, Point c1, Point c2, Point p3)
        {
            return $"M {F(p0.X)} {F(p0.Y)} C {F(c1.X)} {F(c1.Y)} {F(c2.X)} {F(c2.Y)} {F(p3.X)} {F(p3.Y)}";
        }

        private static double Distance(Point a, Point b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static string Lighten(string color, double amount)
        {
            try
            {
                var parsed = Color.Parse(color);
                var r = (byte)Math.Clamp(parsed.R + (255 - parsed.R) * amount, 0, 255);
                var g = (byte)Math.Clamp(parsed.G + (255 - parsed.G) * amount, 0, 255);
                var b = (byte)Math.Clamp(parsed.B + (255 - parsed.B) * amount, 0, 255);
                return $"#{r:X2}{g:X2}{b:X2}";
            }
            catch
            {
                return "#64748B";
            }
        }

        private static string XmlId(string value)
        {
            if (string.IsNullOrEmpty(value)) return "item";
            var builder = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                builder.Append(char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '_');
            }
            return builder.ToString();
        }

        private static string F(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private sealed record Route(
            List<Point> Points,
            bool Curved,
            Point Control1,
            Point Control2,
            Point LabelPoint);

        private sealed class ReferenceEqualityComparer : IEqualityComparer<FlowLink>
        {
            public static readonly ReferenceEqualityComparer Instance = new();

            public bool Equals(FlowLink? x, FlowLink? y) => ReferenceEquals(x, y);
            public int GetHashCode(FlowLink obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
