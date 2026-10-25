using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace OpenEAP.Models.FlowChartDesigner
{
    /// <summary>流程图节点类型。</summary>
    public enum FlowNodeKind
    {
        StartEnd = 0,
        Process = 1,
        Decision = 2,
        Data = 3,
        Document = 4,
        Database = 5,
        Subprocess = 6,
        Connector = 7,
        Image = 8
    }

    /// <summary>节点执行动画效果。</summary>
    public enum FlowExecutionEffect
    {
        None = 0,
        Breathing = 1,
        Pulse = 2,
        Glow = 3
    }

    /// <summary>连线走线方式。</summary>
    public enum FlowLinkStyle
    {
        Orthogonal = 0,
        Straight = 1,
        Curved = 2
    }

    /// <summary>箭头样式。</summary>
    public enum FlowArrowStyle
    {
        End = 0,
        Both = 1,
        None = 2
    }

    /// <summary>连线在节点上的锚点位置。</summary>
    public enum FlowPortSide
    {
        Auto = 0,
        Left = 1,
        Top = 2,
        Right = 3,
        Bottom = 4
    }

    /// <summary>流程图节点。所有属性变化都会通知画布与属性面板。</summary>
    public sealed class FlowNode : ReactiveObject
    {
        private string _id = Guid.NewGuid().ToString("N");
        private FlowNodeKind _kind;
        private string _title = string.Empty;
        private string _subtitle = string.Empty;
        private double _x;
        private double _y;
        private double _width = 200;
        private double _height = 78;
        private string _fill = "#2563EB";
        private string _stroke = string.Empty;
        private string _textColor = "#FFFFFF";
        private double _fontSize = 14;
        private double _strokeWidth = 1.5;
        private double _cornerRadius = 12;
        private bool _isBold = true;
        private double _rotation;
        private bool _isRunning;
        private FlowExecutionEffect _executionEffect = FlowExecutionEffect.Breathing;
        private string _imageData = string.Empty;
        private string _imageMime = string.Empty;
        private string _imageName = string.Empty;

        public string Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        public FlowNodeKind Kind
        {
            get => _kind;
            set => this.RaiseAndSetIfChanged(ref _kind, value);
        }

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Subtitle
        {
            get => _subtitle;
            set => this.RaiseAndSetIfChanged(ref _subtitle, value);
        }

        public double X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        public double Width
        {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, Math.Max(48, value));
        }

        public double Height
        {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, Math.Max(40, value));
        }

        public string Fill
        {
            get => _fill;
            set => this.RaiseAndSetIfChanged(ref _fill, value);
        }

        public string Stroke
        {
            get => _stroke;
            set => this.RaiseAndSetIfChanged(ref _stroke, value);
        }

        public string TextColor
        {
            get => _textColor;
            set => this.RaiseAndSetIfChanged(ref _textColor, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set => this.RaiseAndSetIfChanged(ref _fontSize, Math.Clamp(value, 9, 36));
        }

        public double StrokeWidth
        {
            get => _strokeWidth;
            set => this.RaiseAndSetIfChanged(ref _strokeWidth, Math.Clamp(value, 0, 8));
        }

        public double CornerRadius
        {
            get => _cornerRadius;
            set => this.RaiseAndSetIfChanged(ref _cornerRadius, Math.Clamp(value, 0, 32));
        }

        public bool IsBold
        {
            get => _isBold;
            set => this.RaiseAndSetIfChanged(ref _isBold, value);
        }

        /// <summary>节点旋转角度，0-360 度，围绕节点中心旋转。</summary>
        public double Rotation
        {
            get => _rotation;
            set => this.RaiseAndSetIfChanged(ref _rotation, Math.Clamp(value, 0, 360));
        }

        /// <summary>是否处于执行状态；启用后由画布绘制动态效果。</summary>
        public bool IsRunning
        {
            get => _isRunning;
            set => this.RaiseAndSetIfChanged(ref _isRunning, value);
        }

        /// <summary>执行中的动态效果。</summary>
        public FlowExecutionEffect ExecutionEffect
        {
            get => _executionEffect;
            set => this.RaiseAndSetIfChanged(ref _executionEffect, value);
        }

        /// <summary>图片节点的 Base64 图片数据。</summary>
        public string ImageData
        {
            get => _imageData;
            set => this.RaiseAndSetIfChanged(ref _imageData, value ?? string.Empty);
        }

        /// <summary>图片 MIME 类型，例如 image/png。</summary>
        public string ImageMime
        {
            get => _imageMime;
            set => this.RaiseAndSetIfChanged(ref _imageMime, value ?? string.Empty);
        }

        /// <summary>图片原始文件名，用于显示与辅助信息。</summary>
        public string ImageName
        {
            get => _imageName;
            set => this.RaiseAndSetIfChanged(ref _imageName, value ?? string.Empty);
        }

        [JsonIgnore]
        public bool HasImage => !string.IsNullOrWhiteSpace(ImageData);

        [JsonIgnore]
        public double CenterX => X + Width / 2;

        [JsonIgnore]
        public double CenterY => Y + Height / 2;

        [JsonIgnore]
        public double Right => X + Width;

        [JsonIgnore]
        public double Bottom => Y + Height;

        [JsonIgnore]
        public Rect Bounds => new(X, Y, Width, Height);

        public FlowNode Clone(string? newId = null)
        {
            return new FlowNode
            {
                Id = newId ?? Guid.NewGuid().ToString("N"),
                Kind = Kind,
                Title = Title,
                Subtitle = Subtitle,
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                Fill = Fill,
                Stroke = Stroke,
                TextColor = TextColor,
                FontSize = FontSize,
                StrokeWidth = StrokeWidth,
                CornerRadius = CornerRadius,
                IsBold = IsBold,
                Rotation = Rotation,
                IsRunning = IsRunning,
                ExecutionEffect = ExecutionEffect,
                ImageData = ImageData,
                ImageMime = ImageMime,
                ImageName = ImageName
            };
        }
    }

    /// <summary>导入流程时保留的连线路径点。</summary>
    public sealed class FlowWaypoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public FlowWaypoint()
        {
        }

        public FlowWaypoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>流程图连线。</summary>
    public sealed class FlowLink : ReactiveObject
    {
        private string _id = Guid.NewGuid().ToString("N");
        private string _sourceId = string.Empty;
        private string _targetId = string.Empty;
        private FlowPortSide _sourcePort = FlowPortSide.Auto;
        private FlowPortSide _targetPort = FlowPortSide.Auto;
        private string _label = string.Empty;
        private bool _showLabel = true;
        private FlowLinkStyle _style = FlowLinkStyle.Orthogonal;
        private FlowArrowStyle _arrow = FlowArrowStyle.End;
        private string _color = "#7C8AA0";
        private double _strokeWidth = 2;
        private bool _isDashed;

        public string Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        public string SourceId
        {
            get => _sourceId;
            set => this.RaiseAndSetIfChanged(ref _sourceId, value);
        }

        public string TargetId
        {
            get => _targetId;
            set => this.RaiseAndSetIfChanged(ref _targetId, value);
        }

        public FlowPortSide SourcePort
        {
            get => _sourcePort;
            set => this.RaiseAndSetIfChanged(ref _sourcePort, value);
        }

        public FlowPortSide TargetPort
        {
            get => _targetPort;
            set => this.RaiseAndSetIfChanged(ref _targetPort, value);
        }

        public string Label
        {
            get => _label;
            set => this.RaiseAndSetIfChanged(ref _label, value);
        }

        /// <summary>是否在连线上显示标签框。</summary>
        public bool ShowLabel
        {
            get => _showLabel;
            set => this.RaiseAndSetIfChanged(ref _showLabel, value);
        }

        /// <summary>导入文件自带的路径点；为空时由设计器自动布线。</summary>
        public List<FlowWaypoint> Waypoints { get; set; } = new();

        public FlowLinkStyle Style
        {
            get => _style;
            set => this.RaiseAndSetIfChanged(ref _style, value);
        }

        public FlowArrowStyle Arrow
        {
            get => _arrow;
            set => this.RaiseAndSetIfChanged(ref _arrow, value);
        }

        public string Color
        {
            get => _color;
            set => this.RaiseAndSetIfChanged(ref _color, value);
        }

        public double StrokeWidth
        {
            get => _strokeWidth;
            set => this.RaiseAndSetIfChanged(ref _strokeWidth, Math.Clamp(value, 1, 6));
        }

        public bool IsDashed
        {
            get => _isDashed;
            set => this.RaiseAndSetIfChanged(ref _isDashed, value);
        }

        public FlowLink Clone(string? newId = null)
        {
            return new FlowLink
            {
                Id = newId ?? Guid.NewGuid().ToString("N"),
                SourceId = SourceId,
                TargetId = TargetId,
                SourcePort = SourcePort,
                TargetPort = TargetPort,
                Label = Label,
                ShowLabel = ShowLabel,
                Waypoints = Waypoints.Select(p => new FlowWaypoint(p.X, p.Y)).ToList(),
                Style = Style,
                Arrow = Arrow,
                Color = Color,
                StrokeWidth = StrokeWidth,
                IsDashed = IsDashed
            };
        }
    }

    /// <summary>可序列化的流程文档。</summary>
    public sealed class FlowDiagram
    {
        public string Title { get; set; } = OpenEAP.Localizer.Localizer.Instance["FlowUntitled"];
        public string Description { get; set; } = string.Empty;
        public int Version { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
        public ObservableCollection<FlowNode> Nodes { get; set; } = new();
        public ObservableCollection<FlowLink> Links { get; set; } = new();

        /// <summary>按 ID 获取指定节点；不存在时返回 null。</summary>
        public FlowNode? GetNodeById(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return Nodes.FirstOrDefault(n => string.Equals(n.Id, id, StringComparison.Ordinal));
        }

        /// <summary>获取指定节点的全部后继节点（按连线顺序）。</summary>
        public IReadOnlyList<FlowNode> GetNextNodes(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return Array.Empty<FlowNode>();
            return Links
                .Where(l => string.Equals(l.SourceId, id, StringComparison.Ordinal))
                .Select(l => GetNodeById(l.TargetId))
                .Where(n => n != null)
                .Cast<FlowNode>()
                .ToList();
        }

        /// <summary>获取指定节点的第一个后继节点；没有后继时返回 null。</summary>
        public FlowNode? GetNextNode(string? id) => GetNextNodes(id).FirstOrDefault();
    }

    /// <summary>形状库定义与节点默认值。显示文本从 TSysResources 读取。</summary>
    public sealed record FlowShapeDefinition(
        FlowNodeKind Kind,
        string DisplayNameKey,
        string ShortNameKey,
        string DefaultTitleKey,
        string DefaultFill,
        double DefaultWidth,
        double DefaultHeight,
        double DefaultCornerRadius)
    {
        public string DisplayName => OpenEAP.Localizer.Localizer.Instance[DisplayNameKey];
        public string ShortName => OpenEAP.Localizer.Localizer.Instance[ShortNameKey];
        public string DefaultTitle => OpenEAP.Localizer.Localizer.Instance[DefaultTitleKey];
    }

    public static class FlowShapeCatalog
    {
        public static readonly FlowShapeDefinition[] Shapes =
        {
            new(FlowNodeKind.StartEnd, "FlowShapeStartEnd", "FlowShapeStartEnd", "FlowShapeStartEndTitle", "#16A34A", 170, 62, 31),
            new(FlowNodeKind.Process, "FlowShapeProcess", "FlowShapeProcess", "FlowShapeProcessTitle", "#2563EB", 204, 78, 14),
            new(FlowNodeKind.Decision, "FlowShapeDecision", "FlowShapeDecision", "FlowShapeDecisionTitle", "#D97706", 194, 112, 10),
            new(FlowNodeKind.Data, "FlowShapeData", "FlowShapeData", "FlowShapeDataTitle", "#0891B2", 204, 76, 8),
            new(FlowNodeKind.Document, "FlowShapeDocument", "FlowShapeDocument", "FlowShapeDocumentTitle", "#7C3AED", 204, 86, 8),
            new(FlowNodeKind.Database, "FlowShapeDatabase", "FlowShapeDatabase", "FlowShapeDatabaseTitle", "#DB2777", 184, 96, 10),
            new(FlowNodeKind.Subprocess, "FlowShapeSubprocess", "FlowShapeSubprocess", "FlowShapeSubprocessTitle", "#475569", 214, 78, 10),
            new(FlowNodeKind.Connector, "FlowShapeConnector", "FlowShapeConnector", "FlowShapeConnectorTitle", "#64748B", 72, 72, 36),
            new(FlowNodeKind.Image, "FlowShapeImage", "FlowShapeImage", "FlowShapeImageTitle", "#F1F5F9", 220, 150, 12)
        };

        public static FlowShapeDefinition Get(FlowNodeKind kind)
        {
            var index = (int)kind;
            return index >= 0 && index < Shapes.Length ? Shapes[index] : Shapes[0];
        }

        public static FlowNode Create(FlowNodeKind kind, string? title = null)
        {
            var def = Get(kind);
            return new FlowNode
            {
                Kind = kind,
                Title = title ?? def.DefaultTitle,
                Subtitle = string.Empty,
                Width = def.DefaultWidth,
                Height = def.DefaultHeight,
                Fill = def.DefaultFill,
                CornerRadius = def.DefaultCornerRadius,
                FontSize = kind == FlowNodeKind.Connector ? 18 : 14,
                TextColor = kind == FlowNodeKind.Image ? "#0F172A" : "#FFFFFF",
                Stroke = string.Empty,
                IsBold = kind != FlowNodeKind.Image
            };
        }

        public static void ApplyKind(FlowNode node, FlowNodeKind kind)
        {
            var def = Get(kind);
            node.Kind = kind;
            node.Fill = def.DefaultFill;
            node.CornerRadius = def.DefaultCornerRadius;
            node.Width = def.DefaultWidth;
            node.Height = def.DefaultHeight;
            node.FontSize = kind == FlowNodeKind.Connector ? 18 : 14;
            node.Title = def.DefaultTitle;
            node.TextColor = kind == FlowNodeKind.Image ? "#0F172A" : "#FFFFFF";
            if (kind != FlowNodeKind.Image)
            {
                node.ImageData = string.Empty;
                node.ImageMime = string.Empty;
                node.ImageName = string.Empty;
            }
        }

        public static string KindName(FlowNodeKind kind) => Get(kind).DisplayName;
    }
}
