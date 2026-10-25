using OpenEAP.Models.FlowChartDesigner;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenEAP.ViewModels
{
    public enum FlowAlignMode
    {
        Left,
        CenterX,
        Right,
        Top,
        CenterY,
        Bottom,
        DistributeHorizontal,
        DistributeVertical
    }

    /// <summary>流程图设计器状态、文档操作与撤销栈。</summary>
    public sealed class HomeViewModel : ViewModelBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly Stack<string> _undo = new();
        private readonly Stack<string> _redo = new();
        private FlowDiagram _document;
        private FlowNode? _selectedNode;
        private FlowLink? _selectedLink;
        private int _nodeSelectionCount;
        private int _selectionCount;
        private bool _isDirty;
        private bool _snapToGrid = true;
        private bool _showGrid = true;
        private bool _showMiniMap = true;
        private bool _isShapePanelVisible = true;
        private bool _isInspectorVisible = true;
        private bool _isPreviewMode;
        private bool _isRestoring;
        private double _zoom = 1;
        private string _statusText = L("FlowReady");
        private string _pointerText = string.Empty;
        private string? _currentFilePath;

        public event EventHandler? DiagramReplaced;
        public event EventHandler? ItemChanged;
        public event EventHandler? FitRequested;
        public event EventHandler? SelectionDoubleClicked;

        public HomeViewModel()
        {
            _document = CreateSampleDocument();
            AttachDocument(_document);
            _currentFilePath = null;
            IsDirty = false;
            UpdateStatistics();
        }

        public FlowDiagram Document
        {
            get => _document;
            private set => this.RaiseAndSetIfChanged(ref _document, value);
        }

        public FlowNode? SelectedNode
        {
            get => _selectedNode;
            private set
            {
                if (ReferenceEquals(_selectedNode, value)) return;
                this.RaiseAndSetIfChanged(ref _selectedNode, value);
                NotifySelectionState();
            }
        }

        public FlowLink? SelectedLink
        {
            get => _selectedLink;
            private set
            {
                if (ReferenceEquals(_selectedLink, value)) return;
                this.RaiseAndSetIfChanged(ref _selectedLink, value);
                NotifySelectionState();
            }
        }

        public int SelectionCount
        {
            get => _selectionCount;
            private set
            {
                if (_selectionCount == value) return;
                this.RaiseAndSetIfChanged(ref _selectionCount, value);
                NotifySelectionState();
            }
        }

        public bool HasNodeSelection => _nodeSelectionCount == 1 && SelectedNode != null && SelectedLink == null;
        public bool HasLinkSelection => SelectedLink != null;
        public bool HasMultipleSelection => _selectionCount > 1 && SelectedLink == null;
        public bool HasNoSelection => _selectionCount == 0 && SelectedLink == null;
        public bool HasSelection => _selectionCount > 0 || SelectedLink != null;
        public bool CanAlign => _nodeSelectionCount > 1;

        public string SelectionTitle
        {
            get
            {
                if (SelectedNode != null) return string.IsNullOrWhiteSpace(SelectedNode.Title) ? L("FlowUnnamedNode") : SelectedNode.Title;
                if (SelectedLink != null) return string.IsNullOrWhiteSpace(SelectedLink.Label) ? L("FlowUnnamedLink") : SelectedLink.Label;
                if (HasMultipleSelection) return string.Format(L("FlowSelectedNodesFormat"), SelectionCount);
                return L("FlowNoObject");
            }
        }

        public string SelectionTypeText
        {
            get
            {
                if (SelectedNode != null) return FlowShapeCatalog.KindName(SelectedNode.Kind);
                if (SelectedLink != null) return L("FlowLinkType");
                if (HasMultipleSelection) return L("FlowMultiSelect");
                return L("FlowCanvasType");
            }
        }

        public string SelectionPositionText
        {
            get
            {
                if (SelectedNode == null) return string.Empty;
                return $"X {SelectedNode.X:0}   Y {SelectedNode.Y:0}   W {SelectedNode.Width:0}   H {SelectedNode.Height:0}";
            }
        }

        public int NodeKindIndex
        {
            get => SelectedNode == null ? -1 : (int)SelectedNode.Kind;
            set
            {
                if (SelectedNode == null || value < 0 || value >= FlowShapeCatalog.Shapes.Length) return;
                ChangeNodeKind(SelectedNode, (FlowNodeKind)value);
            }
        }

        public string NodeTitle
        {
            get => SelectedNode?.Title ?? string.Empty;
            set
            {
                if (SelectedNode == null || SelectedNode.Title == value) return;
                PushUndo();
                SelectedNode.Title = value;
            }
        }

        public string NodeSubtitle
        {
            get => SelectedNode?.Subtitle ?? string.Empty;
            set
            {
                if (SelectedNode == null || SelectedNode.Subtitle == value) return;
                PushUndo();
                SelectedNode.Subtitle = value;
            }
        }

        public decimal NodeX
        {
            get => (decimal)(SelectedNode?.X ?? 0);
            set
            {
                if (SelectedNode == null || SelectedNode.X == (double)value) return;
                PushUndo();
                SelectedNode.X = (double)value;
            }
        }

        public decimal NodeY
        {
            get => (decimal)(SelectedNode?.Y ?? 0);
            set
            {
                if (SelectedNode == null || SelectedNode.Y == (double)value) return;
                PushUndo();
                SelectedNode.Y = (double)value;
            }
        }

        public decimal NodeWidth
        {
            get => (decimal)(SelectedNode?.Width ?? 0);
            set
            {
                if (SelectedNode == null || Math.Abs(SelectedNode.Width - (double)value) < 0.01) return;
                PushUndo();
                SelectedNode.Width = (double)value;
            }
        }

        public decimal NodeHeight
        {
            get => (decimal)(SelectedNode?.Height ?? 0);
            set
            {
                if (SelectedNode == null || Math.Abs(SelectedNode.Height - (double)value) < 0.01) return;
                PushUndo();
                SelectedNode.Height = (double)value;
            }
        }

        public decimal NodeRotation
        {
            get => (decimal)(SelectedNode?.Rotation ?? 0);
            set
            {
                if (SelectedNode == null || Math.Abs(SelectedNode.Rotation - (double)value) < 0.01) return;
                PushUndo();
                SelectedNode.Rotation = (double)value;
            }
        }

        public bool NodeIsRunning
        {
            get => SelectedNode?.IsRunning ?? false;
            set
            {
                if (SelectedNode == null || SelectedNode.IsRunning == value) return;
                PushUndo();
                SelectedNode.IsRunning = value;
            }
        }

        public int NodeExecutionEffectIndex
        {
            get => SelectedNode == null ? -1 : (int)SelectedNode.ExecutionEffect;
            set
            {
                if (SelectedNode == null || value < 0 || value > (int)FlowExecutionEffect.Glow ||
                    (int)SelectedNode.ExecutionEffect == value) return;
                PushUndo();
                SelectedNode.ExecutionEffect = (FlowExecutionEffect)value;
            }
        }

        public bool IsImageNodeSelected => SelectedNode?.Kind == FlowNodeKind.Image;

        public bool NodeHasImage => !string.IsNullOrWhiteSpace(SelectedNode?.ImageData);

        public double NodeFontSize
        {
            get => SelectedNode?.FontSize ?? 14;
            set
            {
                if (SelectedNode == null || Math.Abs(SelectedNode.FontSize - value) < 0.01) return;
                PushUndo();
                SelectedNode.FontSize = value;
            }
        }

        public double NodeCornerRadius
        {
            get => SelectedNode?.CornerRadius ?? 0;
            set
            {
                if (SelectedNode == null || Math.Abs(SelectedNode.CornerRadius - value) < 0.01) return;
                PushUndo();
                SelectedNode.CornerRadius = value;
            }
        }

        public double NodeStrokeWidth
        {
            get => SelectedNode?.StrokeWidth ?? 1.5;
            set
            {
                if (SelectedNode == null || Math.Abs(SelectedNode.StrokeWidth - value) < 0.01) return;
                PushUndo();
                SelectedNode.StrokeWidth = value;
            }
        }

        public bool NodeIsBold
        {
            get => SelectedNode?.IsBold ?? true;
            set
            {
                if (SelectedNode == null || SelectedNode.IsBold == value) return;
                PushUndo();
                SelectedNode.IsBold = value;
            }
        }

        public string NodeFill
        {
            get => SelectedNode?.Fill ?? string.Empty;
            set
            {
                if (SelectedNode == null || string.Equals(SelectedNode.Fill, value, StringComparison.OrdinalIgnoreCase)) return;
                PushUndo();
                SelectedNode.Fill = value;
            }
        }

        public string NodeTextColor
        {
            get => SelectedNode?.TextColor ?? string.Empty;
            set
            {
                if (SelectedNode == null || string.Equals(SelectedNode.TextColor, value, StringComparison.OrdinalIgnoreCase)) return;
                PushUndo();
                SelectedNode.TextColor = value;
            }
        }

        public int LinkStyleIndex
        {
            get => SelectedLink == null ? -1 : (int)SelectedLink.Style;
            set
            {
                if (SelectedLink == null || value < 0 || value > 2 || (int)SelectedLink.Style == value) return;
                PushUndo();
                SelectedLink.Style = (FlowLinkStyle)value;
            }
        }

        public int ArrowStyleIndex
        {
            get => SelectedLink == null ? -1 : (int)SelectedLink.Arrow;
            set
            {
                if (SelectedLink == null || value < 0 || value > 2 || (int)SelectedLink.Arrow == value) return;
                PushUndo();
                SelectedLink.Arrow = (FlowArrowStyle)value;
            }
        }

        public string LinkLabel
        {
            get => SelectedLink?.Label ?? string.Empty;
            set
            {
                if (SelectedLink == null || SelectedLink.Label == value) return;
                PushUndo();
                SelectedLink.Label = value;
            }
        }

        public string LinkColor
        {
            get => SelectedLink?.Color ?? string.Empty;
            set
            {
                if (SelectedLink == null || string.Equals(SelectedLink.Color, value, StringComparison.OrdinalIgnoreCase)) return;
                PushUndo();
                SelectedLink.Color = value;
            }
        }

        public double LinkStrokeWidth
        {
            get => SelectedLink?.StrokeWidth ?? 2;
            set
            {
                if (SelectedLink == null || Math.Abs(SelectedLink.StrokeWidth - value) < 0.01) return;
                PushUndo();
                SelectedLink.StrokeWidth = value;
            }
        }

        public bool LinkIsDashed
        {
            get => SelectedLink?.IsDashed ?? false;
            set
            {
                if (SelectedLink == null || SelectedLink.IsDashed == value) return;
                PushUndo();
                SelectedLink.IsDashed = value;
            }
        }

        public bool LinkShowLabel
        {
            get => SelectedLink?.ShowLabel ?? true;
            set
            {
                if (SelectedLink == null || SelectedLink.ShowLabel == value) return;
                PushUndo();
                SelectedLink.ShowLabel = value;
            }
        }

        public string DocumentTitle
        {
            get => Document.Title;
            set
            {
                if (Document.Title == value) return;
                PushUndo();
                Document.Title = value ?? string.Empty;
                MarkDirtyInternal(true);
                this.RaisePropertyChanged(nameof(DocumentTitleWithState));
                this.RaisePropertyChanged(nameof(WindowTitle));
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (_isDirty == value) return;
                this.RaiseAndSetIfChanged(ref _isDirty, value);
                this.RaisePropertyChanged(nameof(DocumentTitleWithState));
                this.RaisePropertyChanged(nameof(SaveStateText));
                this.RaisePropertyChanged(nameof(WindowTitle));
            }
        }

        public string DocumentTitleWithState => string.IsNullOrWhiteSpace(Document.Title) ? L("FlowUntitled") : Document.Title;
        public string SaveStateText => IsDirty ? L("FlowUnsavedChanges") : L("FlowAllSaved");
        public string WindowTitle => string.Format(L("FlowWindowTitleFormat"), DocumentTitleWithState, IsDirty ? " *" : string.Empty);

        public string FileDisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_currentFilePath)) return L("FlowLocalDraft");
                try { return System.IO.Path.GetFileName(_currentFilePath); }
                catch { return _currentFilePath; }
            }
        }

        public bool SnapToGrid
        {
            get => _snapToGrid;
            set => this.RaiseAndSetIfChanged(ref _snapToGrid, value);
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                if (_showGrid == value) return;
                this.RaiseAndSetIfChanged(ref _showGrid, value);
                ItemChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool ShowMiniMap
        {
            get => _showMiniMap;
            set
            {
                if (_showMiniMap == value) return;
                this.RaiseAndSetIfChanged(ref _showMiniMap, value);
                this.RaisePropertyChanged(nameof(MiniMapButtonText));
            }
        }

        public string MiniMapButtonText => ShowMiniMap ? L("FlowHideMiniMap") : L("FlowShowMiniMap");

        public bool IsShapePanelVisible
        {
            get => _isShapePanelVisible;
            set
            {
                if (_isShapePanelVisible == value) return;
                this.RaiseAndSetIfChanged(ref _isShapePanelVisible, value);
                this.RaisePropertyChanged(nameof(IsShapePanelShown));
                this.RaisePropertyChanged(nameof(IsShapePanelCollapsed));
            }
        }

        public bool IsInspectorVisible
        {
            get => _isInspectorVisible;
            set
            {
                if (_isInspectorVisible == value) return;
                this.RaiseAndSetIfChanged(ref _isInspectorVisible, value);
                this.RaisePropertyChanged(nameof(IsInspectorShown));
                this.RaisePropertyChanged(nameof(IsInspectorCollapsed));
            }
        }

        public bool IsShapePanelShown => IsShapePanelVisible && !IsPreviewMode;
        public bool IsInspectorShown => IsInspectorVisible && !IsPreviewMode;
        public bool IsShapePanelCollapsed => !IsShapePanelVisible && !IsPreviewMode;
        public bool IsInspectorCollapsed => !IsInspectorVisible && !IsPreviewMode;

        public bool IsPreviewMode
        {
            get => _isPreviewMode;
            set
            {
                if (_isPreviewMode == value) return;
                this.RaiseAndSetIfChanged(ref _isPreviewMode, value);
                this.RaisePropertyChanged(nameof(PreviewButtonText));
                this.RaisePropertyChanged(nameof(IsShapePanelShown));
                this.RaisePropertyChanged(nameof(IsInspectorShown));
                this.RaisePropertyChanged(nameof(IsShapePanelCollapsed));
                this.RaisePropertyChanged(nameof(IsInspectorCollapsed));
                StatusText = value ? L("FlowPreviewModeStatus") : L("FlowPreviewExited");
            }
        }

        public string PreviewButtonText => IsPreviewMode ? L("FlowExitPreviewButton") : L("FlowPreviewButton");

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public double Zoom
        {
            get => _zoom;
            private set
            {
                if (Math.Abs(_zoom - value) < 0.0001) return;
                this.RaiseAndSetIfChanged(ref _zoom, value);
                this.RaisePropertyChanged(nameof(ZoomText));
            }
        }

        public string ZoomText => $"{Zoom * 100:0}%";

        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

        public string PointerText
        {
            get => _pointerText;
            set => this.RaiseAndSetIfChanged(ref _pointerText, value);
        }

        public int NodeCount => Document.Nodes.Count;
        public int LinkCount => Document.Links.Count;
        public string StatisticsText => string.Format(L("FlowStatisticsFormat"), NodeCount, LinkCount);
        public bool IsEmptyDocument => NodeCount == 0;
        public string CurrentFilePath => _currentFilePath ?? string.Empty;

        public IReadOnlyList<FlowShapeDefinition> ShapePalette => FlowShapeCatalog.Shapes;
        public string ShapeCountText => string.Format(L("FlowShapeCountFormat"), ShapePalette.Count);

        /// <summary>按 ID 获取节点。</summary>
        public FlowNode? GetNodeById(string? id) => Document.GetNodeById(id);

        /// <summary>获取指定节点的全部后继节点。</summary>
        public IReadOnlyList<FlowNode> GetNextNodes(string? id) => Document.GetNextNodes(id);

        /// <summary>获取指定节点的第一个后继节点。</summary>
        public FlowNode? GetNextNode(string? id) => Document.GetNextNode(id);

        public void ResetSelectedNodeRotation()
        {
            if (SelectedNode == null || Math.Abs(SelectedNode.Rotation) < 0.01) return;
            PushUndo();
            SelectedNode.Rotation = 0;
            StatusText = L("FlowRotationReset");
        }

        public void SetSelectedNodeImage(string imageData, string mime, string fileName, double width, double height)
        {
            if (SelectedNode == null) return;
            PushUndo();
            SelectedNode.Kind = FlowNodeKind.Image;
            SelectedNode.ImageData = imageData;
            SelectedNode.ImageMime = mime;
            SelectedNode.ImageName = fileName;
            SelectedNode.Title = System.IO.Path.GetFileNameWithoutExtension(fileName);
            SelectedNode.TextColor = "#0F172A";
            SelectedNode.Fill = "#F1F5F9";
            SelectedNode.Width = Math.Clamp(width, 80, 900);
            SelectedNode.Height = Math.Clamp(height, 60, 700);
            this.RaisePropertyChanged(nameof(IsImageNodeSelected));
            this.RaisePropertyChanged(nameof(NodeHasImage));
        }

        public IReadOnlyList<string> FillPalette { get; } = new[]
        {
            "#2563EB", "#0891B2", "#0EA5E9", "#16A34A", "#65A30D",
            "#D97706", "#EA580C", "#DC2626", "#DB2777", "#7C3AED",
            "#6366F1", "#475569", "#1E293B", "#0F766E"
        };

        public IReadOnlyList<string> TextPalette { get; } = new[]
        {
            "#FFFFFF", "#E2E8F0", "#CBD5E1", "#0F172A",
            "#1E293B", "#FDE68A", "#BFDBFE", "#FECACA"
        };

        public IReadOnlyList<string> LinkPalette { get; } = new[]
        {
            "#71809A", "#4C8DFF", "#22C55E", "#F59E0B",
            "#EF4444", "#A855F7", "#06B6D4", "#E2E8F0"
        };

        public string SerializeDocument() => JsonSerializer.Serialize(Document, JsonOptions);

        public FlowDiagram DeserializeDocument(string json)
        {
            var document = JsonSerializer.Deserialize<FlowDiagram>(json, JsonOptions)
                           ?? throw new InvalidOperationException(L("FlowInvalidDocument"));
            document.Nodes ??= new ObservableCollection<FlowNode>();
            document.Links ??= new ObservableCollection<FlowLink>();
            foreach (var link in document.Links)
            {
                link.Waypoints ??= new List<FlowWaypoint>();
            }
            return document;
        }

        public FlowDiagram CreateBlankDocument()
        {
            return new FlowDiagram
            {
                Title = L("FlowUntitled"),
                Description = L("FlowBlankDescription"),
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };
        }

        public FlowDiagram CreateSampleDocument()
        {
            var document = new FlowDiagram
            {
                Title = L("FlowSampleTitle"),
                Description = L("FlowSampleDescription"),
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };

            FlowNode Add(FlowNodeKind kind, string title, string subtitle, double x, double y, double w, double h)
            {
                var node = FlowShapeCatalog.Create(kind, title);
                node.Subtitle = subtitle;
                node.X = x;
                node.Y = y;
                node.Width = w;
                node.Height = h;
                document.Nodes.Add(node);
                return node;
            }

            var start = Add(FlowNodeKind.StartEnd, L("FlowSampleStartTitle"), L("FlowSampleStartSubtitle"), 465, 50, 170, 62);
            var scan = Add(FlowNodeKind.Process, L("FlowSampleScanTitle"), L("FlowSampleScanSubtitle"), 450, 160, 200, 78);
            var inspect = Add(FlowNodeKind.Process, L("FlowSampleInspectTitle"), L("FlowSampleInspectSubtitle"), 450, 286, 200, 78);
            var decision = Add(FlowNodeKind.Decision, L("FlowSampleDecisionTitle"), L("FlowSampleDecisionSubtitle"), 453, 412, 194, 112);
            var pack = Add(FlowNodeKind.Process, L("FlowSamplePackTitle"), L("FlowSamplePackSubtitle"), 190, 586, 200, 78);
            var review = Add(FlowNodeKind.Process, L("FlowSampleReviewTitle"), L("FlowSampleReviewSubtitle"), 710, 586, 200, 78);
            var end = Add(FlowNodeKind.StartEnd, L("FlowSampleEndTitle"), L("FlowSampleEndSubtitle"), 465, 718, 170, 62);
            var database = Add(FlowNodeKind.Database, L("FlowSampleDatabaseTitle"), L("FlowSampleDatabaseSubtitle"), 940, 286, 184, 96);

            void Link(FlowNode from, FlowNode to, string label, FlowPortSide sourcePort = FlowPortSide.Auto, FlowPortSide targetPort = FlowPortSide.Auto)
            {
                document.Links.Add(new FlowLink
                {
                    SourceId = from.Id,
                    TargetId = to.Id,
                    Label = label,
                    SourcePort = sourcePort,
                    TargetPort = targetPort,
                    Color = "#71809A",
                    StrokeWidth = 2
                });
            }

            Link(start, scan, string.Empty, FlowPortSide.Bottom, FlowPortSide.Top);
            Link(scan, inspect, L("FlowSampleLinkDone"), FlowPortSide.Bottom, FlowPortSide.Top);
            Link(inspect, decision, string.Empty, FlowPortSide.Bottom, FlowPortSide.Top);
            Link(decision, pack, "OK", FlowPortSide.Left, FlowPortSide.Top);
            Link(decision, review, "NG", FlowPortSide.Right, FlowPortSide.Top);
            Link(pack, end, string.Empty, FlowPortSide.Bottom, FlowPortSide.Left);
            Link(review, end, L("FlowSampleLinkReviewDone"), FlowPortSide.Bottom, FlowPortSide.Right);
            Link(inspect, database, L("FlowSampleLinkArchive"), FlowPortSide.Right, FlowPortSide.Left);

            return document;
        }

        public void LoadDocument(FlowDiagram document, string? filePath = null, bool markClean = true)
        {
            DetachDocument();
            _isRestoring = true;
            Document = document;
            AttachDocument(Document);
            _isRestoring = false;
            _undo.Clear();
            _redo.Clear();
            _currentFilePath = filePath;
            ClearSelectionInternal();
            MarkDirtyInternal(!markClean);
            UpdateStatistics();
            this.RaisePropertyChanged(nameof(DocumentTitleWithState));
            this.RaisePropertyChanged(nameof(SaveStateText));
            this.RaisePropertyChanged(nameof(WindowTitle));
            this.RaisePropertyChanged(nameof(FileDisplayName));
            this.RaisePropertyChanged(nameof(CurrentFilePath));
            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
            DiagramReplaced?.Invoke(this, EventArgs.Empty);
        }

        public void NewDocument()
        {
            LoadDocument(CreateBlankDocument());
            StatusText = L("FlowBlankCreated");
            FitRequested?.Invoke(this, EventArgs.Empty);
        }

        public void LoadSample()
        {
            LoadDocument(CreateSampleDocument());
            StatusText = L("FlowSampleLoaded");
            FitRequested?.Invoke(this, EventArgs.Empty);
        }

        public void MarkSaved(string? filePath)
        {
            _currentFilePath = filePath;
            MarkDirtyInternal(false);
            this.RaisePropertyChanged(nameof(FileDisplayName));
            this.RaisePropertyChanged(nameof(CurrentFilePath));
            StatusText = L("FlowSaved");
        }

        public void PushUndo()
        {
            if (_isRestoring) return;
            var snapshot = SerializeDocument();
            if (_undo.Count > 0 && _undo.Peek() == snapshot) return;
            _undo.Push(snapshot);
            while (_undo.Count > 80)
            {
                var items = _undo.ToArray();
                _undo.Clear();
                foreach (var item in items.Take(80).Reverse()) _undo.Push(item);
            }
            _redo.Clear();
            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
        }

        public bool Undo()
        {
            if (_undo.Count == 0) return false;
            var current = SerializeDocument();
            var snapshot = _undo.Pop();
            _redo.Push(current);
            RestoreSnapshot(snapshot);
            StatusText = L("FlowUndone");
            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
            return true;
        }

        public bool Redo()
        {
            if (_redo.Count == 0) return false;
            var current = SerializeDocument();
            var snapshot = _redo.Pop();
            _undo.Push(current);
            RestoreSnapshot(snapshot);
            StatusText = L("FlowRedone");
            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
            return true;
        }

        private void RestoreSnapshot(string snapshot)
        {
            var document = DeserializeDocument(snapshot);
            DetachDocument();
            _isRestoring = true;
            Document = document;
            AttachDocument(Document);
            _isRestoring = false;
            ClearSelectionInternal();
            MarkDirtyInternal(true);
            UpdateStatistics();
            this.RaisePropertyChanged(nameof(DocumentTitleWithState));
            this.RaisePropertyChanged(nameof(WindowTitle));
            DiagramReplaced?.Invoke(this, EventArgs.Empty);
        }

        public FlowNode AddNodeAt(FlowNodeKind kind, double centerX, double centerY)
        {
            var node = FlowShapeCatalog.Create(kind);
            node.X = centerX - node.Width / 2;
            node.Y = centerY - node.Height / 2;
            if (SnapToGrid)
            {
                node.X = Snap(node.X);
                node.Y = Snap(node.Y);
            }
            PushUndo();
            Document.Nodes.Add(node);
            StatusText = string.Format(L("FlowNodeAddedFormat"), FlowShapeCatalog.KindName(kind));
            return node;
        }

        public FlowNode AddImageNodeAt(string imageData, string mime, string fileName,
            double centerX, double centerY, double width, double height)
        {
            var node = FlowShapeCatalog.Create(FlowNodeKind.Image, System.IO.Path.GetFileNameWithoutExtension(fileName));
            node.ImageData = imageData;
            node.ImageMime = mime;
            node.ImageName = fileName;
            node.Title = System.IO.Path.GetFileNameWithoutExtension(fileName);
            node.Width = Math.Clamp(width, 80, 900);
            node.Height = Math.Clamp(height, 60, 700);
            node.X = centerX - node.Width / 2;
            node.Y = centerY - node.Height / 2;
            if (SnapToGrid)
            {
                node.X = Snap(node.X);
                node.Y = Snap(node.Y);
            }
            PushUndo();
            Document.Nodes.Add(node);
            StatusText = L("FlowImageNodeAdded");
            return node;
        }

        public FlowLink? AddLink(FlowNode source, FlowNode target, FlowPortSide sourcePort, FlowPortSide targetPort)
        {
            if (ReferenceEquals(source, target))
            {
                StatusText = L("FlowCannotConnectSelf");
                return null;
            }

            var exists = Document.Links.FirstOrDefault(l => l.SourceId == source.Id && l.TargetId == target.Id);
            if (exists != null)
            {
                StatusText = L("FlowLinkExists");
                return exists;
            }

            PushUndo();
            var link = new FlowLink
            {
                SourceId = source.Id,
                TargetId = target.Id,
                SourcePort = sourcePort,
                TargetPort = targetPort,
                Color = "#71809A",
                StrokeWidth = 2,
                Style = FlowLinkStyle.Orthogonal,
                Arrow = FlowArrowStyle.End
            };
            Document.Links.Add(link);
            StatusText = L("FlowLinkCreated");
            return link;
        }

        public void RemoveNodes(IEnumerable<FlowNode> nodes)
        {
            var list = nodes.Distinct().ToList();
            if (list.Count == 0) return;
            PushUndo();
            var ids = list.Select(n => n.Id).ToHashSet();
            foreach (var link in Document.Links.Where(l => ids.Contains(l.SourceId) || ids.Contains(l.TargetId)).ToList())
            {
                Document.Links.Remove(link);
            }
            foreach (var node in list) Document.Nodes.Remove(node);
            ClearSelectionInternal();
            StatusText = string.Format(L("FlowNodesDeletedFormat"), list.Count);
        }

        public void RemoveLink(FlowLink link)
        {
            if (!Document.Links.Contains(link)) return;
            PushUndo();
            Document.Links.Remove(link);
            ClearSelectionInternal();
            StatusText = L("FlowLinkDeleted");
        }

        public IReadOnlyList<FlowNode> DuplicateNodes(IEnumerable<FlowNode> nodes)
        {
            var list = nodes.Distinct().ToList();
            if (list.Count == 0) return Array.Empty<FlowNode>();
            PushUndo();
            var map = new Dictionary<string, FlowNode>();
            var copies = new List<FlowNode>();
            foreach (var source in list)
            {
                var copy = source.Clone();
                copy.X += 34;
                copy.Y += 34;
                map[source.Id] = copy;
                copies.Add(copy);
                Document.Nodes.Add(copy);
            }

            var links = Document.Links.Where(l => map.ContainsKey(l.SourceId) && map.ContainsKey(l.TargetId)).ToList();
            foreach (var link in links)
            {
                var copy = link.Clone();
                copy.SourceId = map[link.SourceId].Id;
                copy.TargetId = map[link.TargetId].Id;
                Document.Links.Add(copy);
            }
            StatusText = string.Format(L("FlowNodesCopiedFormat"), copies.Count);
            return copies;
        }

        public void AlignNodes(IReadOnlyList<FlowNode> nodes, FlowAlignMode mode)
        {
            if (nodes.Count < 2) return;
            PushUndo();
            var left = nodes.Min(n => n.X);
            var right = nodes.Max(n => n.Right);
            var top = nodes.Min(n => n.Y);
            var bottom = nodes.Max(n => n.Bottom);
            var centerX = (left + right) / 2;
            var centerY = (top + bottom) / 2;

            switch (mode)
            {
                case FlowAlignMode.Left:
                    foreach (var n in nodes) n.X = left;
                    break;
                case FlowAlignMode.CenterX:
                    foreach (var n in nodes) n.X = centerX - n.Width / 2;
                    break;
                case FlowAlignMode.Right:
                    foreach (var n in nodes) n.X = right - n.Width;
                    break;
                case FlowAlignMode.Top:
                    foreach (var n in nodes) n.Y = top;
                    break;
                case FlowAlignMode.CenterY:
                    foreach (var n in nodes) n.Y = centerY - n.Height / 2;
                    break;
                case FlowAlignMode.Bottom:
                    foreach (var n in nodes) n.Y = bottom - n.Height;
                    break;
                case FlowAlignMode.DistributeHorizontal:
                {
                    var ordered = nodes.OrderBy(n => n.CenterX).ToList();
                    var totalWidth = ordered.Sum(n => n.Width);
                    var gap = (right - left - totalWidth) / Math.Max(1, ordered.Count - 1);
                    var x = left;
                    foreach (var n in ordered)
                    {
                        n.X = x;
                        x += n.Width + gap;
                    }
                    break;
                }
                case FlowAlignMode.DistributeVertical:
                {
                    var ordered = nodes.OrderBy(n => n.CenterY).ToList();
                    var totalHeight = ordered.Sum(n => n.Height);
                    var gap = (bottom - top - totalHeight) / Math.Max(1, ordered.Count - 1);
                    var y = top;
                    foreach (var n in ordered)
                    {
                        n.Y = y;
                        y += n.Height + gap;
                    }
                    break;
                }
            }
            StatusText = L("FlowAligned");
        }

        public void AutoLayout()
        {
            var nodes = Document.Nodes.ToList();
            if (nodes.Count == 0) return;
            PushUndo();

            var byId = nodes.ToDictionary(n => n.Id);
            var validLinks = Document.Links
                .Where(l => byId.ContainsKey(l.SourceId) && byId.ContainsKey(l.TargetId) && l.SourceId != l.TargetId)
                .ToList();

            if (validLinks.Count == 0)
            {
                const int columns = 4;
                const double gapX = 70;
                const double gapY = 60;
                var x = 120.0;
                var y = 120.0;
                var rowHeight = 0.0;
                for (var i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    if (i > 0 && i % columns == 0)
                    {
                        x = 120;
                        y += rowHeight + gapY;
                        rowHeight = 0;
                    }
                    node.X = x;
                    node.Y = y;
                    x += node.Width + gapX;
                    rowHeight = Math.Max(rowHeight, node.Height);
                }
            }
            else
            {
                var outgoing = nodes.ToDictionary(n => n.Id, _ => new List<string>());
                var incoming = nodes.ToDictionary(n => n.Id, _ => new List<string>());
                var indegree = nodes.ToDictionary(n => n.Id, _ => 0);
                foreach (var link in validLinks)
                {
                    outgoing[link.SourceId].Add(link.TargetId);
                    incoming[link.TargetId].Add(link.SourceId);
                    indegree[link.TargetId]++;
                }

                var levels = nodes.ToDictionary(n => n.Id, _ => 0);
                var queue = new Queue<string>(nodes.Where(n => indegree[n.Id] == 0).Select(n => n.Id));
                var visited = new HashSet<string>();

                while (queue.Count > 0)
                {
                    var id = queue.Dequeue();
                    if (!visited.Add(id)) continue;
                    foreach (var target in outgoing[id])
                    {
                        levels[target] = Math.Max(levels[target], levels[id] + 1);
                        indegree[target]--;
                        if (indegree[target] <= 0) queue.Enqueue(target);
                    }
                }

                var unresolved = nodes.Where(n => !visited.Contains(n.Id)).ToList();
                if (unresolved.Count > 0)
                {
                    var maxLevel = levels.Values.DefaultIfEmpty(0).Max();
                    var index = 0;
                    foreach (var node in unresolved.OrderBy(n => n.Y).ThenBy(n => n.X))
                    {
                        levels[node.Id] = maxLevel + 1 + index / 3;
                        index++;
                    }
                }

                var layers = nodes
                    .GroupBy(n => levels[n.Id])
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(n => Document.Nodes.IndexOf(n)).ToList())
                    .ToList();

                const double gapX = 58;
                const double gapY = 78;
                var centerX = 560.0;
                var y = 70.0;
                foreach (var layer in layers)
                {
                    var layerWidth = layer.Sum(n => n.Width) + gapX * Math.Max(0, layer.Count - 1);
                    var x = centerX - layerWidth / 2;
                    var maxHeight = layer.Max(n => n.Height);
                    foreach (var node in layer)
                    {
                        node.X = x;
                        node.Y = y + (maxHeight - node.Height) / 2;
                        x += node.Width + gapX;
                    }
                    y += maxHeight + gapY;
                }
            }

            SnapAllNodes();
            StatusText = L("FlowAutoLaidOut");
            FitRequested?.Invoke(this, EventArgs.Empty);
        }

        public void NudgeSelection(IEnumerable<FlowNode> nodes, double dx, double dy)
        {
            foreach (var node in nodes)
            {
                node.X += dx;
                node.Y += dy;
            }
        }

        public void ChangeNodeKind(FlowNode node, FlowNodeKind kind)
        {
            if (node.Kind == kind) return;
            PushUndo();
            var oldDefault = FlowShapeCatalog.Get(node.Kind).DefaultTitle;
            var preserveTitle = !string.IsNullOrWhiteSpace(node.Title) && node.Title != oldDefault;
            var title = node.Title;
            FlowShapeCatalog.ApplyKind(node, kind);
            if (preserveTitle) node.Title = title;
            StatusText = string.Format(L("FlowKindChangedFormat"), FlowShapeCatalog.KindName(kind));
        }

        public void SetSelectedNodeFill(string color)
        {
            if (SelectedNode == null || string.Equals(SelectedNode.Fill, color, StringComparison.OrdinalIgnoreCase)) return;
            PushUndo();
            SelectedNode.Fill = color;
        }

        public void SetSelectedNodeStroke(string color)
        {
            if (SelectedNode == null || string.Equals(SelectedNode.Stroke, color, StringComparison.OrdinalIgnoreCase)) return;
            PushUndo();
            SelectedNode.Stroke = color;
        }

        public void SetSelectedLinkColor(string color)
        {
            if (SelectedLink == null || string.Equals(SelectedLink.Color, color, StringComparison.OrdinalIgnoreCase)) return;
            PushUndo();
            SelectedLink.Color = color;
        }

        public void SetNodeFillFromPalette(string color)
        {
            if (SelectedNode == null) return;
            PushUndo();
            SelectedNode.Fill = color;
            SelectedNode.TextColor = "#FFFFFF";
        }

        public void NotifySelectionDoubleClicked()
        {
            SelectionDoubleClicked?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateSelection(IReadOnlyList<FlowNode> nodes, FlowLink? link)
        {
            _nodeSelectionCount = nodes.Count;
            _selectionCount = nodes.Count + (link != null ? 1 : 0);
            SelectedNode = nodes.Count == 1 && link == null ? nodes[0] : null;
            SelectedLink = link;
            this.RaisePropertyChanged(nameof(SelectionCount));
            NotifySelectionState();
        }

        public void ClearSelection()
        {
            ClearSelectionInternal();
            NotifySelectionState();
        }

        public void UpdateZoom(double zoom)
        {
            Zoom = Math.Clamp(zoom, 0.2, 3.0);
        }

        private void ClearSelectionInternal()
        {
            _nodeSelectionCount = 0;
            _selectionCount = 0;
            SelectedNode = null;
            SelectedLink = null;
        }

        private void NotifySelectionState()
        {
            this.RaisePropertyChanged(nameof(HasNodeSelection));
            this.RaisePropertyChanged(nameof(HasLinkSelection));
            this.RaisePropertyChanged(nameof(HasMultipleSelection));
            this.RaisePropertyChanged(nameof(HasNoSelection));
            this.RaisePropertyChanged(nameof(HasSelection));
            this.RaisePropertyChanged(nameof(CanAlign));
            this.RaisePropertyChanged(nameof(SelectionTitle));
            this.RaisePropertyChanged(nameof(SelectionTypeText));
            this.RaisePropertyChanged(nameof(SelectionPositionText));
            this.RaisePropertyChanged(nameof(NodeKindIndex));
            this.RaisePropertyChanged(nameof(LinkStyleIndex));
            this.RaisePropertyChanged(nameof(ArrowStyleIndex));
            this.RaisePropertyChanged(nameof(NodeTitle));
            this.RaisePropertyChanged(nameof(NodeSubtitle));
            this.RaisePropertyChanged(nameof(NodeX));
            this.RaisePropertyChanged(nameof(NodeY));
            this.RaisePropertyChanged(nameof(NodeWidth));
            this.RaisePropertyChanged(nameof(NodeHeight));
            this.RaisePropertyChanged(nameof(NodeRotation));
            this.RaisePropertyChanged(nameof(NodeIsRunning));
            this.RaisePropertyChanged(nameof(NodeExecutionEffectIndex));
            this.RaisePropertyChanged(nameof(IsImageNodeSelected));
            this.RaisePropertyChanged(nameof(NodeHasImage));
            this.RaisePropertyChanged(nameof(NodeFontSize));
            this.RaisePropertyChanged(nameof(NodeCornerRadius));
            this.RaisePropertyChanged(nameof(NodeStrokeWidth));
            this.RaisePropertyChanged(nameof(NodeIsBold));
            this.RaisePropertyChanged(nameof(NodeFill));
            this.RaisePropertyChanged(nameof(NodeTextColor));
            this.RaisePropertyChanged(nameof(LinkLabel));
            this.RaisePropertyChanged(nameof(LinkColor));
            this.RaisePropertyChanged(nameof(LinkStrokeWidth));
            this.RaisePropertyChanged(nameof(LinkIsDashed));
            this.RaisePropertyChanged(nameof(LinkShowLabel));
            this.RaisePropertyChanged(nameof(DocumentTitle));
        }

        private void AttachDocument(FlowDiagram document)
        {
            document.Nodes.CollectionChanged += OnNodesChanged;
            document.Links.CollectionChanged += OnLinksChanged;
            foreach (var node in document.Nodes)
            {
                node.PropertyChanged += OnItemPropertyChanged;
            }
            foreach (var link in document.Links)
            {
                link.PropertyChanged += OnItemPropertyChanged;
            }
        }

        private void DetachDocument()
        {
            if (_document == null) return;
            _document.Nodes.CollectionChanged -= OnNodesChanged;
            _document.Links.CollectionChanged -= OnLinksChanged;
            foreach (var node in _document.Nodes) node.PropertyChanged -= OnItemPropertyChanged;
            foreach (var link in _document.Links) link.PropertyChanged -= OnItemPropertyChanged;
        }

        private void OnNodesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (FlowNode node in e.NewItems) node.PropertyChanged += OnItemPropertyChanged;
            }
            if (e.OldItems != null)
            {
                foreach (FlowNode node in e.OldItems) node.PropertyChanged -= OnItemPropertyChanged;
            }
            OnDocumentStructureChanged();
        }

        private void OnLinksChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (FlowLink link in e.NewItems) link.PropertyChanged += OnItemPropertyChanged;
            }
            if (e.OldItems != null)
            {
                foreach (FlowLink link in e.OldItems) link.PropertyChanged -= OnItemPropertyChanged;
            }
            OnDocumentStructureChanged();
        }

        private void OnDocumentStructureChanged()
        {
            if (!_isRestoring)
            {
                MarkDirtyInternal(true);
            }
            UpdateStatistics();
            ItemChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!_isRestoring)
            {
                MarkDirtyInternal(true);
                Document.ModifiedAt = DateTime.Now;
            }
            if (e.PropertyName == nameof(FlowNode.X) || e.PropertyName == nameof(FlowNode.Y) ||
                e.PropertyName == nameof(FlowNode.Width) || e.PropertyName == nameof(FlowNode.Height))
            {
                this.RaisePropertyChanged(nameof(SelectionPositionText));
            }

            if (sender is FlowNode)
            {
                this.RaisePropertyChanged(nameof(NodeTitle));
                this.RaisePropertyChanged(nameof(NodeSubtitle));
                this.RaisePropertyChanged(nameof(NodeX));
                this.RaisePropertyChanged(nameof(NodeY));
                this.RaisePropertyChanged(nameof(NodeWidth));
                this.RaisePropertyChanged(nameof(NodeHeight));
                this.RaisePropertyChanged(nameof(NodeRotation));
                this.RaisePropertyChanged(nameof(NodeIsRunning));
                this.RaisePropertyChanged(nameof(NodeExecutionEffectIndex));
                this.RaisePropertyChanged(nameof(IsImageNodeSelected));
                this.RaisePropertyChanged(nameof(NodeHasImage));
                this.RaisePropertyChanged(nameof(NodeFontSize));
                this.RaisePropertyChanged(nameof(NodeCornerRadius));
                this.RaisePropertyChanged(nameof(NodeStrokeWidth));
                this.RaisePropertyChanged(nameof(NodeIsBold));
                this.RaisePropertyChanged(nameof(NodeFill));
                this.RaisePropertyChanged(nameof(NodeTextColor));
                this.RaisePropertyChanged(nameof(SelectionTitle));
            }
            else if (sender is FlowLink)
            {
                this.RaisePropertyChanged(nameof(LinkLabel));
                this.RaisePropertyChanged(nameof(LinkColor));
                this.RaisePropertyChanged(nameof(LinkStrokeWidth));
                this.RaisePropertyChanged(nameof(LinkIsDashed));
                this.RaisePropertyChanged(nameof(LinkShowLabel));
                this.RaisePropertyChanged(nameof(SelectionTitle));
            }

            ItemChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MarkDirtyInternal(bool dirty)
        {
            if (IsDirty == dirty) return;
            IsDirty = dirty;
        }

        private void UpdateStatistics()
        {
            this.RaisePropertyChanged(nameof(NodeCount));
            this.RaisePropertyChanged(nameof(LinkCount));
            this.RaisePropertyChanged(nameof(StatisticsText));
            this.RaisePropertyChanged(nameof(IsEmptyDocument));
        }

        private void SnapAllNodes()
        {
            if (!SnapToGrid) return;
            foreach (var node in Document.Nodes)
            {
                node.X = Snap(node.X);
                node.Y = Snap(node.Y);
            }
        }

        private static double Snap(double value) => Math.Round(value / 20) * 20;
        private static string L(string key) => OpenEAP.Localizer.Localizer.Instance[key];
    }
}
