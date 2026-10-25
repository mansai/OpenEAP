using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using OpenEAP.Models.FlowChartDesigner;
using OpenEAP.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OpenEAP.Controls
{
    public enum FlowToolMode
    {
        Select,
        Connect,
        Pan
    }

    /// <summary>
    /// 流程图设计画布：负责绘制、缩放平移、选择、拖拽、缩放节点与连线创建。
    /// 所有样式与布局参数都按矢量坐标绘制，因此在高分屏与任意缩放下保持清晰。
    /// </summary>
    public sealed class FlowDiagramCanvas : Control
    {
        private enum InteractionMode
        {
            None,
            MoveNodes,
            ResizeNode,
            Marquee,
            Pan,
            Connect,
            MiniMap
        }

        private enum ResizeHandle
        {
            None,
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left
        }

        private sealed class LinkRoute
        {
            public List<Point> Points { get; } = new();
            public Geometry Geometry { get; set; } = new LineGeometry();
            public Point LabelPoint { get; set; }
            public double Length { get; set; }
        }

        private sealed class ImageCacheEntry
        {
            public string Data { get; init; } = string.Empty;
            public Bitmap? Bitmap { get; init; }
        }

        private static readonly Cursor ArrowCursor = new(StandardCursorType.Arrow);
        private static readonly Cursor HandCursor = new(StandardCursorType.Hand);
        private static readonly Cursor CrossCursor = new(StandardCursorType.Cross);
        private static readonly Cursor MoveCursor = new(StandardCursorType.SizeAll);
        private static readonly Cursor SizeNsCursor = new(StandardCursorType.SizeNorthSouth);
        private static readonly Cursor SizeWeCursor = new(StandardCursorType.SizeWestEast);
        private static readonly Cursor SizeNwseCursor = new(StandardCursorType.TopLeftCorner);
        private static readonly Cursor SizeNeswCursor = new(StandardCursorType.TopRightCorner);

        private readonly List<FlowNode> _selectedNodes = new();
        private readonly Dictionary<FlowNode, Point> _dragOrigins = new();
        private readonly Dictionary<Color, IBrush> _brushCache = new();
        private readonly Dictionary<string, ImageCacheEntry> _imageCache = new(StringComparer.Ordinal);
        private readonly DispatcherTimer _animationTimer = new() { Interval = TimeSpan.FromMilliseconds(33) };
        private readonly Typeface _regularTypeface = new(new FontFamily("Microsoft YaHei UI, Segoe UI, sans-serif"), FontStyle.Normal, FontWeight.Normal);
        private readonly Typeface _boldTypeface = new(new FontFamily("Microsoft YaHei UI, Segoe UI, sans-serif"), FontStyle.Normal, FontWeight.SemiBold);
        private double _animationSeconds;

        private HomeViewModel? _viewModel;
        private FlowLink? _selectedLink;
        private FlowToolMode _tool = FlowToolMode.Select;
        private InteractionMode _interaction;
        private ResizeHandle _resizeHandle;
        private FlowNode? _hoverNode;
        private FlowPortSide? _hoverPort;
        private FlowLink? _hoverLink;
        private FlowNode? _connectSource;
        private FlowNode? _connectTarget;
        private FlowPortSide _connectSourcePort = FlowPortSide.Right;
        private Point _connectPoint;
        private Point _pressScreen;
        private Point _pressWorld;
        private Point _lastScreen;
        private double _panX;
        private double _panY;
        private double _zoom = 1;
        private double _panStartX;
        private double _panStartY;
        private bool _spacePressed;
        private bool _dragMoved;
        private bool _isExporting;
        private bool _marqueeAdditive;
        private List<FlowNode> _marqueeBase = new();
        private Rect _marqueeWorld;
        private Point _resizeStartWorld;
        private Point _resizeStartCenter;
        private Point _resizeAnchorWorld;
        private Rect _resizeStartBounds;
        private FlowNode? _resizeNode;
        private bool _needsInitialFit = true;
        private Rect _miniMapRect = default;
        private Rect _miniMapCloseRect = default;
        private Rect _miniMapContentRect = default;
        private Rect _miniMapWorldBounds = default;
        private double _miniMapScale;
        private Point _miniMapOffset;
        private bool _miniMapVisible;

        public FlowDiagramCanvas()
        {
            Focusable = true;
            ClipToBounds = true;
            Cursor = ArrowCursor;
            SizeChanged += (_, _) =>
            {
                if (_needsInitialFit && Bounds.Width > 80 && Bounds.Height > 80)
                {
                    FitToContent();
                }
                InvalidateVisual();
            };
            DoubleTapped += OnDoubleTapped;
            _animationTimer.Tick += (_, _) =>
            {
                _animationSeconds += _animationTimer.Interval.TotalSeconds;
                InvalidateVisual();
            };
        }

        public event EventHandler? SelectionChanged;
        public event EventHandler? ToolChanged;
        public event EventHandler? NodeActivated;

        public HomeViewModel? ViewModel => _viewModel;
        public IReadOnlyList<FlowNode> SelectedNodes => _selectedNodes;
        public FlowLink? SelectedLink => _selectedLink;
        public FlowToolMode Tool => _tool;
        public double Zoom => _zoom;

        public void Attach(HomeViewModel viewModel)
        {
            if (ReferenceEquals(_viewModel, viewModel)) return;
            if (_viewModel != null)
            {
                _viewModel.DiagramReplaced -= OnDiagramReplaced;
                _viewModel.ItemChanged -= OnItemChanged;
                _viewModel.FitRequested -= OnFitRequested;
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            _viewModel = viewModel;
            _viewModel.DiagramReplaced += OnDiagramReplaced;
            _viewModel.ItemChanged += OnItemChanged;
            _viewModel.FitRequested += OnFitRequested;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            ClearSelectionInternal();
            _imageCache.Clear();
            _needsInitialFit = true;
            if (Bounds.Width > 80 && Bounds.Height > 80)
            {
                FitToContent();
            }
            EnsureAnimationTimer();
            InvalidateVisual();
        }

        public void SetTool(FlowToolMode tool)
        {
            if (_tool == tool) return;
            _tool = tool;
            _connectSource = null;
            _connectTarget = null;
            UpdateHover(_lastScreen);
            Cursor = tool switch
            {
                FlowToolMode.Pan => HandCursor,
                FlowToolMode.Connect => CrossCursor,
                _ => ArrowCursor
            };
            ToolChanged?.Invoke(this, EventArgs.Empty);
            InvalidateVisual();
        }

        public void AddShape(FlowNodeKind kind)
        {
            if (_viewModel == null) return;
            var center = ScreenToWorld(new Point(Bounds.Width / 2, Bounds.Height / 2));
            var cascade = (_viewModel.Document.Nodes.Count % 5) * 22;
            var node = _viewModel.AddNodeAt(kind, center.X + cascade, center.Y + cascade);
            SetSelection(new[] { node }, null);
            Focus();
            InvalidateVisual();
        }

        public void AddShapeAt(FlowNodeKind kind, Point screenPoint)
        {
            if (_viewModel == null) return;
            var world = ScreenToWorld(screenPoint);
            var node = _viewModel.AddNodeAt(kind, world.X, world.Y);
            SetSelection(new[] { node }, null);
            Focus();
            InvalidateVisual();
        }

        public FlowNode? AddImageShape(string imageData, string mime, string fileName,
            double width, double height, Point? screenPoint = null)
        {
            if (_viewModel == null) return null;
            var center = screenPoint.HasValue
                ? ScreenToWorld(screenPoint.Value)
                : ScreenToWorld(new Point(Bounds.Width / 2, Bounds.Height / 2));
            var cascade = (_viewModel.Document.Nodes.Count % 5) * 22;
            var node = _viewModel.AddImageNodeAt(imageData, mime, fileName,
                center.X + cascade, center.Y + cascade, width, height);
            SetSelection(new[] { node }, null);
            Focus();
            InvalidateVisual();
            return node;
        }

        public void DeleteSelection()
        {
            if (_viewModel == null) return;
            if (_selectedLink != null)
            {
                _viewModel.RemoveLink(_selectedLink);
                ClearSelectionInternal();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                InvalidateVisual();
                return;
            }
            if (_selectedNodes.Count > 0)
            {
                _viewModel.RemoveNodes(_selectedNodes);
                ClearSelectionInternal();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                InvalidateVisual();
            }
        }

        public void DuplicateSelection()
        {
            if (_viewModel == null || _selectedNodes.Count == 0) return;
            var copies = _viewModel.DuplicateNodes(_selectedNodes);
            SetSelection(copies, null);
            InvalidateVisual();
        }

        public void SelectAll()
        {
            if (_viewModel == null) return;
            SetSelection(_viewModel.Document.Nodes.ToList(), null);
            InvalidateVisual();
        }

        public void AlignSelection(FlowAlignMode mode)
        {
            if (_viewModel == null || _selectedNodes.Count < 2) return;
            _viewModel.AlignNodes(_selectedNodes, mode);
            InvalidateVisual();
        }

        public void ZoomIn()
        {
            ZoomAt(_zoom * 1.15, new Point(Bounds.Width / 2, Bounds.Height / 2));
        }

        public void ZoomOut()
        {
            ZoomAt(_zoom / 1.15, new Point(Bounds.Width / 2, Bounds.Height / 2));
        }

        public void ResetZoom()
        {
            ZoomAt(1, new Point(Bounds.Width / 2, Bounds.Height / 2));
        }

        public void FitToContent()
        {
            if (_viewModel == null) return;
            var width = Math.Max(1, Bounds.Width);
            var height = Math.Max(1, Bounds.Height);
            if (_viewModel.Document.Nodes.Count == 0)
            {
                _zoom = 1;
                _panX = width / 2;
                _panY = height / 2;
            }
            else
            {
                var bounds = GetContentBounds(80);
                var zoom = Math.Min((width - 150) / Math.Max(1, bounds.Width), (height - 120) / Math.Max(1, bounds.Height));
                _zoom = Math.Clamp(zoom, 0.22, 1.45);
                _panX = width / 2 - (bounds.X + bounds.Width / 2) * _zoom;
                _panY = height / 2 - (bounds.Y + bounds.Height / 2) * _zoom;
            }
            _needsInitialFit = false;
            _viewModel.UpdateZoom(_zoom);
            InvalidateVisual();
        }

        public void CenterOnSelection()
        {
            if (_viewModel == null) return;
            var bounds = _selectedNodes.Count > 0
                ? GetBounds(_selectedNodes)
                : GetContentBounds(40);
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            _panX = Bounds.Width / 2 - (bounds.X + bounds.Width / 2) * _zoom;
            _panY = Bounds.Height / 2 - (bounds.Y + bounds.Height / 2) * _zoom;
            InvalidateVisual();
        }

        /// <summary>将完整流程图渲染为白底位图，用于 PNG 导出和打印。</summary>
        public RenderTargetBitmap RenderDocumentToBitmap(double scale = 2)
        {
            if (_viewModel == null) throw new InvalidOperationException(L("FlowCanvasNotLoaded"));
            var content = GetContentBounds(64, includeLinks: true);
            if (content.Width <= 0 || content.Height <= 0)
            {
                content = new Rect(0, 0, 640, 360);
            }

            scale = Math.Clamp(scale, 1, 4);
            var pixelWidth = Math.Clamp((int)Math.Ceiling(content.Width * scale), 1, 12000);
            var pixelHeight = Math.Clamp((int)Math.Ceiling(content.Height * scale), 1, 12000);
            var bitmap = new RenderTargetBitmap(new PixelSize(pixelWidth, pixelHeight), new Vector(96, 96));
            var exportPalette = GetExportPalette();
            var oldZoom = _zoom;

            try
            {
                using var context = bitmap.CreateDrawingContext();
                context.DrawRectangle(Brush(exportPalette.Background), null,
                    new Rect(0, 0, pixelWidth, pixelHeight));
                _zoom = 1;
                _isExporting = true;
                using (context.PushTransform(Matrix.CreateScale(scale, scale) *
                                             Matrix.CreateTranslation(-content.X * scale, -content.Y * scale)))
                {
                    DrawLinks(context, exportPalette);
                    DrawNodes(context, exportPalette);
                }
            }
            finally
            {
                _zoom = oldZoom;
                _isExporting = false;
            }

            return bitmap;
        }

        public byte[] ExportPng(double scale = 2)
        {
            using var bitmap = RenderDocumentToBitmap(scale);
            using var stream = new MemoryStream();
            bitmap.Save(stream, PngBitmapEncoderOptions.Default);
            return stream.ToArray();
        }

        private static CanvasPalette GetExportPalette()
        {
            return new CanvasPalette(
                Color.Parse("#FFFFFF"), Color.Parse("#E2E8F0"), Color.Parse("#CBD5E1"),
                Color.Parse("#FFFFFF"), Color.Parse("#D8DFE9"), Color.Parse("#2563EB"),
                Color.Parse("#1F2937"), Color.Parse("#FFFFFF"), Color.Parse("#F8FAFD"),
                Color.Parse("#FFFFFF"), Color.Parse("#FFFFFF"));
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var palette = GetPalette();
            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            context.DrawRectangle(Brush(palette.Background), null, bounds);

            if (_viewModel == null) return;
            if (_viewModel.ShowGrid && !_viewModel.IsPreviewMode)
            {
                DrawGrid(context, palette);
            }

            using (context.PushClip(bounds))
            using (context.PushTransform(Matrix.CreateScale(_zoom, _zoom) * Matrix.CreateTranslation(_panX, _panY)))
            {
                DrawLinks(context, palette);
                DrawNodes(context, palette);
                DrawSelectionOverlays(context, palette);
                DrawConnectionPreview(context, palette);
                DrawMarquee(context, palette);
            }

            DrawMiniMap(context, palette);
        }

        private void DrawGrid(DrawingContext context, CanvasPalette palette)
        {
            var spacing = 20 * _zoom;
            while (spacing < 11) spacing *= 2;
            var majorEvery = 5;
            var startX = _panX % spacing;
            var startY = _panY % spacing;
            if (startX > 0) startX -= spacing;
            if (startY > 0) startY -= spacing;
            var columns = (int)Math.Ceiling(Bounds.Width / spacing) + 2;
            var rows = (int)Math.Ceiling(Bounds.Height / spacing) + 2;
            if (columns * rows > 14000) return;

            var minorBrush = Brush(palette.GridMinor);
            var majorBrush = Brush(palette.GridMajor);
            var minorSize = _zoom > 1.2 ? 1.4 : 1.1;
            var majorSize = _zoom > 1.2 ? 2.4 : 1.9;

            for (var col = 0; col < columns; col++)
            {
                var x = startX + col * spacing;
                var majorX = (col % majorEvery) == 0;
                for (var row = 0; row < rows; row++)
                {
                    var y = startY + row * spacing;
                    var major = majorX && (row % majorEvery) == 0;
                    var size = major ? majorSize : minorSize;
                    context.DrawRectangle(major ? majorBrush : minorBrush, null,
                        new Rect(x - size / 2, y - size / 2, size, size));
                }
            }
        }

        private void DrawLinks(DrawingContext context, CanvasPalette palette)
        {
            if (_viewModel == null) return;
            foreach (var link in _viewModel.Document.Links)
            {
                var source = FindNode(link.SourceId);
                var target = FindNode(link.TargetId);
                if (source == null || target == null) continue;

                var route = BuildRoute(link, source, target);
                var selected = !_isExporting && ReferenceEquals(link, _selectedLink);
                var hovered = !_isExporting && ReferenceEquals(link, _hoverLink);

                if (selected || hovered)
                {
                    var glow = selected ? WithAlpha(palette.Accent, 80) : WithAlpha(palette.Accent, 38);
                    var glowPen = new Pen(Brush(glow), link.StrokeWidth + (selected ? 7 : 4), null, PenLineCap.Round, PenLineJoin.Round);
                    context.DrawGeometry(null, glowPen, route.Geometry);
                }

                var color = ParseColor(link.Color);
                var pen = CreateLinkPen(color, link.StrokeWidth, link.IsDashed);
                context.DrawGeometry(null, pen, route.Geometry);

                if (link.Arrow is FlowArrowStyle.End or FlowArrowStyle.Both && route.Points.Count > 1)
                {
                    var tip = route.Points[^1];
                    var from = route.Points[^2];
                    DrawArrow(context, tip, from, Brush(color), Math.Clamp(link.StrokeWidth * 4.4, 7, 13));

                    if (link.Arrow == FlowArrowStyle.Both)
                    {
                        DrawArrow(context, route.Points[0], route.Points[1], Brush(color), Math.Clamp(link.StrokeWidth * 4.4, 7, 13));
                    }
                }

                if (!string.IsNullOrWhiteSpace(link.Label))
                {
                    DrawLinkLabel(context, palette, link, route);
                }
            }
        }

        private void DrawNodes(DrawingContext context, CanvasPalette palette)
        {
            if (_viewModel == null) return;
            foreach (var node in _viewModel.Document.Nodes)
            {
                DrawNode(context, palette, node);
            }
        }

        private void DrawNode(DrawingContext context, CanvasPalette palette, FlowNode node)
        {
            if (Math.Abs(node.Rotation) < 0.01)
            {
                DrawNodeCore(context, palette, node, BuildNodeGeometry(node));
                return;
            }

            using (context.PushTransform(GetNodeRotationMatrix(node)))
            {
                DrawNodeCore(context, palette, node, BuildNodeGeometry(node));
            }
        }

        private void DrawNodeCore(DrawingContext context, CanvasPalette palette, FlowNode node, Geometry geometry)
        {
            var selected = !_isExporting && _selectedNodes.Contains(node);
            var hovered = !_isExporting && ReferenceEquals(node, _hoverNode);
            var baseColor = ParseColor(node.Fill);
            var strokeColor = string.IsNullOrWhiteSpace(node.Stroke)
                ? node.Kind == FlowNodeKind.Image ? Color.Parse("#CBD5E1") : Lighten(baseColor, 0.22)
                : ParseColor(node.Stroke);

            if (_zoom > 0.32)
            {
                using (context.PushTransform(Matrix.CreateTranslation(0, 6)))
                {
                    context.DrawGeometry(Brush(Color.FromArgb(38, 0, 0, 0)), null, geometry);
                }
                using (context.PushTransform(Matrix.CreateTranslation(0, 2)))
                {
                    context.DrawGeometry(Brush(Color.FromArgb(54, 0, 0, 0)), null, geometry);
                }
            }

            if (!_isExporting && node.IsRunning && node.ExecutionEffect != FlowExecutionEffect.None)
            {
                DrawExecutionEffect(context, node, geometry);
            }

            if (node.Kind == FlowNodeKind.Image)
            {
                DrawImageNode(context, node, geometry, baseColor, strokeColor);
            }
            else
            {
                context.DrawGeometry(Brush(baseColor), null, geometry);

                if (node.Kind is FlowNodeKind.Process or FlowNodeKind.Subprocess or FlowNodeKind.Data or FlowNodeKind.Document)
                {
                    var gloss = Color.FromArgb(22, 255, 255, 255);
                    context.DrawGeometry(Brush(gloss), null, geometry);
                }

                var borderPen = new Pen(Brush(strokeColor), node.StrokeWidth);
                context.DrawGeometry(null, borderPen, geometry);

                DrawNodeDetails(context, node);
            }

            if (selected || hovered)
            {
                var glowColor = WithAlpha(palette.Accent, (byte)(selected ? 92 : 45));
                var glowPen = new Pen(Brush(glowColor), selected ? 7 / _zoom : 5 / _zoom, null, PenLineCap.Round, PenLineJoin.Round);
                context.DrawGeometry(null, glowPen, geometry);
                var outlinePen = new Pen(Brush(palette.Accent), (selected ? 2.2 : 1.4) / _zoom, null, PenLineCap.Round, PenLineJoin.Round);
                context.DrawGeometry(null, outlinePen, geometry);
            }

            if (node.Kind != FlowNodeKind.Image)
            {
                DrawNodeText(context, node);
            }
        }

        private void DrawExecutionEffect(DrawingContext context, FlowNode node, Geometry geometry)
        {
            var phase = _animationSeconds * 2.4 + (node.Id.Length % 11) * 0.27;
            var wave = (Math.Sin(phase * Math.PI) + 1) / 2;
            switch (node.ExecutionEffect)
            {
                case FlowExecutionEffect.Breathing:
                {
                    var alpha = (byte)(35 + wave * 105);
                    var width = (5 + wave * 7) / Math.Max(0.35, _zoom);
                    context.DrawGeometry(null,
                        new Pen(Brush(Color.FromArgb(alpha, 44, 141, 255)), width, null, PenLineCap.Round, PenLineJoin.Round),
                        geometry);
                    context.DrawGeometry(Brush(Color.FromArgb((byte)(wave * 24), 120, 190, 255)), null, geometry);
                    break;
                }
                case FlowExecutionEffect.Pulse:
                {
                    var progress = (phase * 0.72) % 1.0;
                    var alpha = (byte)((1 - progress) * 130);
                    var width = (2 + progress * 16) / Math.Max(0.35, _zoom);
                    context.DrawGeometry(null,
                        new Pen(Brush(Color.FromArgb(alpha, 76, 141, 255)), width, null, PenLineCap.Round, PenLineJoin.Round),
                        geometry);
                    var inner = 30 + (byte)(wave * 50);
                    context.DrawGeometry(null,
                        new Pen(Brush(Color.FromArgb((byte)inner, 76, 141, 255)), 4 / Math.Max(0.35, _zoom),
                            null, PenLineCap.Round, PenLineJoin.Round), geometry);
                    break;
                }
                default:
                {
                    var alpha = (byte)(55 + wave * 35);
                    var width = (8 + wave * 3) / Math.Max(0.35, _zoom);
                    context.DrawGeometry(null,
                        new Pen(Brush(Color.FromArgb(alpha, 94, 234, 212)), width, null, PenLineCap.Round, PenLineJoin.Round),
                        geometry);
                    context.DrawGeometry(Brush(Color.FromArgb((byte)(20 + wave * 18), 94, 234, 212)), null, geometry);
                    break;
                }
            }
        }

        private void DrawImageNode(DrawingContext context, FlowNode node, Geometry geometry, Color baseColor, Color strokeColor)
        {
            var image = GetNodeImage(node);
            using (context.PushClip(new RoundedRect(node.Bounds, node.CornerRadius)))
            {
                context.DrawRectangle(Brush(baseColor), null, node.Bounds);
                if (image != null)
                {
                    var source = new Rect(0, 0, image.PixelSize.Width, image.PixelSize.Height);
                    var target = FitImage(new Rect(node.X + 2, node.Y + 2, Math.Max(1, node.Width - 4), Math.Max(1, node.Height - 4)),
                        source.Width, source.Height);
                    context.DrawImage(image, source, target);
                }
                else
                {
                    DrawImagePlaceholder(context, node);
                }

                if (!string.IsNullOrWhiteSpace(node.Title))
                {
                    DrawImageCaption(context, node);
                }
            }

            var borderPen = new Pen(Brush(strokeColor), node.StrokeWidth);
            context.DrawGeometry(null, borderPen, geometry);
        }

        private void DrawImagePlaceholder(DrawingContext context, FlowNode node)
        {
            var color = WithAlpha(ParseColor(node.TextColor), 135);
            var center = new Point(node.CenterX, node.CenterY - 10);
            var icon = new Rect(center.X - 19, center.Y - 14, 38, 28);
            var pen = new Pen(Brush(color), 1.4);
            context.DrawRectangle(null, pen, icon, 4, 4);
            context.DrawEllipse(Brush(color), null, new Point(icon.Right - 10, icon.Y + 8), 2.2, 2.2);
            var mountain = new StreamGeometry();
            using (var ctx = mountain.Open())
            {
                ctx.BeginFigure(new Point(icon.X + 4, icon.Bottom - 5), false);
                ctx.LineTo(new Point(icon.X + 14, icon.Center.Y + 2));
                ctx.LineTo(new Point(icon.X + 22, icon.Bottom - 8));
                ctx.LineTo(new Point(icon.X + 28, icon.Center.Y + 5));
                ctx.LineTo(new Point(icon.Right - 4, icon.Bottom - 5));
                ctx.EndFigure(false);
            }
            context.DrawGeometry(null, pen, mountain);

            var text = new FormattedText(L("FlowShapeImage"),
                CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                _regularTypeface, 11, Brush(color));
            context.DrawText(text, new Point(node.CenterX - text.Width / 2, center.Y + 19));
        }

        private void DrawImageCaption(DrawingContext context, FlowNode node)
        {
            var captionHeight = Math.Min(node.Height * 0.32, 34);
            var caption = new Rect(node.X, node.Bottom - captionHeight, node.Width, captionHeight);
            context.DrawRectangle(Brush(Color.FromArgb(186, 15, 23, 42)), null, caption);
            var fontSize = Math.Min(node.FontSize, 13);
            var availableWidth = Math.Max(20, node.Width - 20);
            var text = new FormattedText(node.Title.Trim(), CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, _boldTypeface, fontSize, Brush(Colors.White))
            {
                TextAlignment = TextAlignment.Center,
                Trimming = TextTrimming.CharacterEllipsis,
                MaxTextWidth = availableWidth,
                MaxTextHeight = Math.Max(12, captionHeight - 6)
            };
            context.DrawText(text, new Point(node.CenterX - availableWidth / 2,
                caption.Y + (captionHeight - text.Height) / 2));
        }

        private Bitmap? GetNodeImage(FlowNode node)
        {
            if (string.IsNullOrWhiteSpace(node.ImageData)) return null;
            if (_imageCache.TryGetValue(node.Id, out var cached) &&
                ReferenceEquals(cached.Data, node.ImageData))
            {
                return cached.Bitmap;
            }

            Bitmap? bitmap = null;
            try
            {
                using var stream = new MemoryStream(Convert.FromBase64String(node.ImageData));
                bitmap = new Bitmap(stream);
            }
            catch
            {
                bitmap = null;
            }

            _imageCache[node.Id] = new ImageCacheEntry { Data = node.ImageData, Bitmap = bitmap };
            return bitmap;
        }

        private static Rect FitImage(Rect target, double width, double height)
        {
            if (width <= 0 || height <= 0) return target;
            var scale = Math.Min(target.Width / width, target.Height / height);
            var w = width * scale;
            var h = height * scale;
            return new Rect(target.Center.X - w / 2, target.Center.Y - h / 2, w, h);
        }

        private void DrawNodeDetails(DrawingContext context, FlowNode node)
        {
            if (node.Kind == FlowNodeKind.Subprocess)
            {
                var pen = new Pen(Brush(Color.FromArgb(80, 255, 255, 255)), 1 / Math.Max(0.4, _zoom));
                context.DrawLine(pen, new Point(node.X + 13, node.Y + 9), new Point(node.X + 13, node.Bottom - 9));
                context.DrawLine(pen, new Point(node.Right - 13, node.Y + 9), new Point(node.Right - 13, node.Bottom - 9));
            }
            else if (node.Kind == FlowNodeKind.Database)
            {
                var ry = Math.Min(16, node.Height * 0.16);
                var pen = new Pen(Brush(Color.FromArgb(95, 255, 255, 255)), 1.1);
                context.DrawEllipse(null, pen, new Rect(node.X + 1, node.Y + 1, node.Width - 2, ry * 2));
            }
        }

        private void DrawNodeText(DrawingContext context, FlowNode node)
        {
            var availableWidth = node.Kind == FlowNodeKind.Decision
                ? node.Width * 0.58
                : node.Kind == FlowNodeKind.Connector
                    ? node.Width * 0.68
                    : node.Width - 34;
            availableWidth = Math.Max(30, availableWidth);

            var titleText = string.IsNullOrWhiteSpace(node.Title) ? " " : node.Title.Trim();
            var titleFontSize = node.FontSize;
            if (node.Kind == FlowNodeKind.Connector)
            {
                titleFontSize = Math.Min(node.FontSize, 22);
            }
            var typeface = node.IsBold ? _boldTypeface : _regularTypeface;
            var title = new FormattedText(
                titleText,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                titleFontSize,
                Brush(node.TextColor))
            {
                TextAlignment = TextAlignment.Center,
                Trimming = TextTrimming.CharacterEllipsis,
                MaxTextWidth = availableWidth
            };

            var hasSubtitle = !string.IsNullOrWhiteSpace(node.Subtitle) && node.Kind != FlowNodeKind.Connector;
            FormattedText? subtitle = null;
            if (hasSubtitle)
            {
                subtitle = new FormattedText(
                    node.Subtitle.Trim(),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    _regularTypeface,
                    Math.Max(10, titleFontSize * 0.72),
                    Brush(WithAlpha(ParseColor(node.TextColor), 190)))
                {
                    TextAlignment = TextAlignment.Center,
                    Trimming = TextTrimming.CharacterEllipsis,
                    MaxTextWidth = availableWidth,
                    MaxTextHeight = Math.Max(20, node.Height * 0.42)
                };
            }

            var gap = subtitle != null ? 5 : 0;
            var totalHeight = title.Height + gap + (subtitle?.Height ?? 0);
            var startY = node.CenterY - totalHeight / 2;

            if (node.Kind == FlowNodeKind.Decision)
            {
                startY = node.CenterY - totalHeight / 2 + 1;
            }
            if (node.Kind == FlowNodeKind.Database)
            {
                startY += 8;
            }

            context.DrawText(title, new Point(node.CenterX - availableWidth / 2, startY));
            if (subtitle != null)
            {
                context.DrawText(subtitle, new Point(node.CenterX - availableWidth / 2, startY + title.Height + gap));
            }
        }

        private void DrawSelectionOverlays(DrawingContext context, CanvasPalette palette)
        {
            if (_viewModel?.IsPreviewMode == true) return;
            var accentBrush = Brush(palette.Accent);
            var whiteBrush = Brush(palette.HandleFill);
            var handlePen = new Pen(accentBrush, 1.4 / _zoom);
            var handleSize = 8 / _zoom;

            foreach (var node in _viewModel?.Document.Nodes ?? Enumerable.Empty<FlowNode>())
            {
                if (!_selectedNodes.Contains(node) && !ReferenceEquals(node, _hoverNode)) continue;
                if (!_selectedNodes.Contains(node) && _tool != FlowToolMode.Connect) continue;

                foreach (var (side, _) in GetPorts(node))
                {
                    var isHover = ReferenceEquals(_hoverNode, node) && _hoverPort == side;
                    var radius = (isHover ? 5.5 : 4.2) / _zoom;
                    var fill = isHover ? accentBrush : whiteBrush;
                    var pen = new Pen(accentBrush, (isHover ? 2 : 1.4) / _zoom);
                    var point = GetPortVisualPoint(node, side);
                    context.DrawEllipse(fill, pen, point, radius, radius);
                }
            }

            if (_selectedNodes.Count == 1)
            {
                var node = _selectedNodes[0];
                foreach (var (_, point) in GetHandles(node))
                {
                    context.DrawRectangle(whiteBrush, handlePen,
                        new Rect(point.X - handleSize / 2, point.Y - handleSize / 2, handleSize, handleSize), 1 / _zoom, 1 / _zoom);
                }
            }
            else if (_selectedNodes.Count > 1)
            {
                var bounds = GetBounds(_selectedNodes);
                var pen = new Pen(accentBrush, 1.5 / _zoom,
                    new DashStyle(new[] { 5d, 4d }, 0), PenLineCap.Round, PenLineJoin.Round);
                context.DrawRectangle(null, pen,
                    new Rect(bounds.X - 8 / _zoom, bounds.Y - 8 / _zoom, bounds.Width + 16 / _zoom, bounds.Height + 16 / _zoom),
                    5 / _zoom, 5 / _zoom);
            }

            if (_selectedLink != null)
            {
                var source = FindNode(_selectedLink.SourceId);
                var target = FindNode(_selectedLink.TargetId);
                if (source != null && target != null)
                {
                    var route = BuildRoute(_selectedLink, source, target);
                    var radius = 5 / _zoom;
                    foreach (var point in new[] { route.Points[0], route.Points[^1] })
                    {
                        context.DrawEllipse(whiteBrush, new Pen(accentBrush, 1.6 / _zoom), point, radius, radius);
                    }
                }
            }
        }

        private void DrawConnectionPreview(DrawingContext context, CanvasPalette palette)
        {
            if (_interaction != InteractionMode.Connect || _connectSource == null) return;
            var target = _connectTarget;
            Point end;
            if (target != null)
            {
                var targetSide = ChooseBestSide(target, _connectSource, _connectSourcePort);
                end = GetAnchor(target, targetSide);
            }
            else
            {
                end = _connectPoint;
            }

            var start = GetAnchor(_connectSource, _connectSourcePort);
            var pen = new Pen(Brush(WithAlpha(palette.Accent, 220)), 2.2 / _zoom,
                new DashStyle(new[] { 7d, 5d }, 0), PenLineCap.Round, PenLineJoin.Round);
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                var offset = Math.Max(26, Math.Abs(end.X - start.X) * 0.22 + Math.Abs(end.Y - start.Y) * 0.22);
                var normal = RotateVector(NormalOf(_connectSourcePort), _connectSource.Rotation);
                var c1 = start + normal * offset;
                var c2 = end + new Vector(0, 0);
                if (target != null)
                {
                    var targetSide = ChooseBestSide(target, _connectSource, _connectSourcePort);
                    c2 = end + RotateVector(NormalOf(targetSide), target.Rotation) * offset;
                }
                ctx.BeginFigure(start, false);
                ctx.CubicBezierTo(c1, c2, end);
                ctx.EndFigure(false);
            }
            context.DrawGeometry(null, pen, geometry);
            DrawArrow(context, end, end - new Vector(0, 1), Brush(palette.Accent), 10 / _zoom);

            if (target != null)
            {
                using (context.PushTransform(GetNodeRotationMatrix(target)))
                {
                    var targetGeometry = BuildNodeGeometry(target);
                    context.DrawGeometry(null, new Pen(Brush(palette.Accent), 2 / _zoom), targetGeometry);
                }
            }

            var tipRadius = 5 / _zoom;
            context.DrawEllipse(Brush(palette.Accent), null, start, tipRadius, tipRadius);
        }

        private void DrawMarquee(DrawingContext context, CanvasPalette palette)
        {
            if (_interaction != InteractionMode.Marquee) return;
            var fill = Brush(WithAlpha(palette.Accent, 28));
            var pen = new Pen(Brush(WithAlpha(palette.Accent, 200)), 1.2 / _zoom,
                new DashStyle(new[] { 4d, 4d }, 0), PenLineCap.Flat, PenLineJoin.Miter);
            context.DrawRectangle(fill, pen, _marqueeWorld, 3 / _zoom, 3 / _zoom);
        }

        private void DrawMiniMap(DrawingContext context, CanvasPalette palette)
        {
            _miniMapVisible = false;
            if (_viewModel == null || _viewModel.IsPreviewMode || !_viewModel.ShowMiniMap ||
                _viewModel.Document.Nodes.Count == 0) return;
            if (Bounds.Width < 420 || Bounds.Height < 260) return;

            var width = 186.0;
            var height = 136.0;
            var rect = new Rect(Bounds.Width - width - 20, Bounds.Height - height - 20, width, height);
            _miniMapRect = rect;
            _miniMapVisible = true;

            var bg = WithAlpha(palette.Panel, 238);
            var border = WithAlpha(palette.Border, 220);
            context.DrawRectangle(Brush(bg), new Pen(Brush(border), 1), rect, 10, 10);

            // 标题栏与关闭按钮
            var title = new FormattedText(L("FlowNavigation"), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                _regularTypeface, 10, Brush(WithAlpha(palette.LabelText, 190)));
            context.DrawText(title, new Point(rect.X + 12, rect.Y + 7));
            _miniMapCloseRect = new Rect(rect.Right - 26, rect.Y + 5, 20, 20);
            var closeCenter = new Point(_miniMapCloseRect.Center.X, _miniMapCloseRect.Center.Y);
            context.DrawEllipse(Brush(WithAlpha(palette.Border, 80)), null, closeCenter, 8, 8);
            var closePen = new Pen(Brush(WithAlpha(palette.LabelText, 185)), 1.2, null, PenLineCap.Round, PenLineJoin.Round);
            context.DrawLine(closePen, closeCenter + new Vector(-3, -3), closeCenter + new Vector(3, 3));
            context.DrawLine(closePen, closeCenter + new Vector(3, -3), closeCenter + new Vector(-3, 3));

            var content = new Rect(rect.X + 10, rect.Y + 31, rect.Width - 20, rect.Height - 41);
            _miniMapContentRect = content;
            var worldBounds = GetContentBounds(30);
            _miniMapWorldBounds = worldBounds;
            if (worldBounds.Width <= 0 || worldBounds.Height <= 0) return;
            var scale = Math.Min(content.Width / worldBounds.Width, content.Height / worldBounds.Height);
            _miniMapScale = scale;
            _miniMapOffset = new Point(
                content.X + (content.Width - worldBounds.Width * scale) / 2,
                content.Y + (content.Height - worldBounds.Height * scale) / 2);

            foreach (var link in _viewModel.Document.Links)
            {
                var source = FindNode(link.SourceId);
                var target = FindNode(link.TargetId);
                if (source == null || target == null) continue;
                var p1 = WorldToMiniMap(new Point(source.CenterX, source.CenterY));
                var p2 = WorldToMiniMap(new Point(target.CenterX, target.CenterY));
                context.DrawLine(new Pen(Brush(WithAlpha(ParseColor(link.Color), 120)), 0.8), p1, p2);
            }

            foreach (var node in _viewModel.Document.Nodes)
            {
                var p1 = WorldToMiniMap(new Point(node.X, node.Y));
                var p2 = WorldToMiniMap(new Point(node.Right, node.Bottom));
                var r = new Rect(p1, p2);
                if (r.Width < 2 || r.Height < 1.5) r = new Rect(p1.X, p1.Y, Math.Max(2, r.Width), Math.Max(1.5, r.Height));
                context.DrawRectangle(Brush(WithAlpha(ParseColor(node.Fill), 220)), null, r, 1.2, 1.2);
            }

            var topLeft = ScreenToWorld(new Point(0, 0));
            var bottomRight = ScreenToWorld(new Point(Bounds.Width, Bounds.Height));
            var v1 = WorldToMiniMap(topLeft);
            var v2 = WorldToMiniMap(bottomRight);
            var viewport = new Rect(v1, v2);
            viewport = viewport.Intersect(content);
            if (viewport.Width > 1 && viewport.Height > 1)
            {
                context.DrawRectangle(Brush(WithAlpha(palette.Accent, 18)), new Pen(Brush(WithAlpha(palette.Accent, 170)), 1), viewport, 2, 2);
            }
        }

        private void DrawLinkLabel(DrawingContext context, CanvasPalette palette, FlowLink link, LinkRoute route)
        {
            const double horizontalPadding = 9;
            const double verticalPadding = 4;
            var fontSize = Math.Clamp(11.5, 9.5, 13);
            var text = new FormattedText(
                link.Label.Trim(),
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                _regularTypeface,
                fontSize,
                Brush(palette.LabelText))
            {
                MaxTextWidth = 150,
                TextAlignment = TextAlignment.Left,
                Trimming = TextTrimming.CharacterEllipsis
            };
            var textWidth = Math.Min(150, Math.Max(1, text.Width));
            var point = route.LabelPoint;
            var textOrigin = new Point(point.X - textWidth / 2, point.Y - text.Height / 2);

            if (link.ShowLabel)
            {
                var w = textWidth + horizontalPadding * 2;
                var h = text.Height + verticalPadding * 2;
                var rect = new Rect(point.X - w / 2, point.Y - h / 2, w, h);
                context.DrawRectangle(Brush(WithAlpha(palette.LabelBackground, 245)),
                    new Pen(Brush(WithAlpha(ParseColor(link.Color), 170)), 0.9 / Math.Max(0.6, _zoom)), rect, h / 2, h / 2);
                context.DrawText(text, new Point(rect.Center.X - textWidth / 2, rect.Center.Y - text.Height / 2));
            }
            else
            {
                var halo = new FormattedText(
                    link.Label.Trim(),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    _regularTypeface,
                    fontSize,
                    Brush(WithAlpha(palette.LabelBackground, 235)))
                {
                    MaxTextWidth = 150,
                    TextAlignment = TextAlignment.Left,
                    Trimming = TextTrimming.CharacterEllipsis
                };
                var haloWidth = Math.Min(150, Math.Max(1, halo.Width));
                var haloOrigin = new Point(point.X - haloWidth / 2, point.Y - halo.Height / 2);
                context.DrawText(halo, haloOrigin + new Vector(-1, 0));
                context.DrawText(halo, haloOrigin + new Vector(1, 0));
                context.DrawText(halo, haloOrigin + new Vector(0, -1));
                context.DrawText(halo, haloOrigin + new Vector(0, 1));
                context.DrawText(text, textOrigin);
            }
        }

        private void DrawArrow(DrawingContext context, Point tip, Point from, IBrush brush, double size)
        {
            Vector vector = tip - from;
            if (vector.Length < 0.001) return;
            var direction = vector.Normalize();
            var normal = new Vector(-direction.Y, direction.X);
            var basePoint = tip - direction * size;
            var p1 = basePoint + normal * (size * 0.46);
            var p2 = basePoint - normal * (size * 0.46);
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(tip, true);
                ctx.LineTo(p1);
                ctx.LineTo(p2);
                ctx.EndFigure(true);
            }
            context.DrawGeometry(brush, null, geometry);
        }

        private LinkRoute BuildRoute(FlowLink link, FlowNode source, FlowNode target)
        {
            var sourceSide = link.SourcePort == FlowPortSide.Auto
                ? ChooseBestSide(source, target, FlowPortSide.Auto)
                : link.SourcePort;
            var targetSide = link.TargetPort == FlowPortSide.Auto
                ? ChooseBestSide(target, source, sourceSide)
                : link.TargetPort;

            var p0 = GetAnchor(source, sourceSide);
            var p3 = GetAnchor(target, targetSide);
            var route = new LinkRoute();

            // BPMN / Visio 导入时保留原始走线，节点移动后自动回退到设计器布线。
            if (link.Waypoints is { Count: >= 2 })
            {
                var imported = link.Waypoints
                    .Select(p => new Point(p.X, p.Y))
                    .ToList();
                if (Distance(imported[0], p0) <= 36 && Distance(imported[^1], p3) <= 36)
                {
                    imported[0] = p0;
                    imported[^1] = p3;
                    imported = Simplify(imported);
                    route.Points.AddRange(imported);
                    route.Geometry = BuildRoundedPolyline(imported, 9);
                    route.LabelPoint = PointAtLength(imported, PathLength(imported) / 2);
                    route.Length = PathLength(imported);
                    return route;
                }
            }

            if (link.Style == FlowLinkStyle.Straight)
            {
                route.Points.Add(p0);
                route.Points.Add(p3);
                route.Geometry = BuildLineGeometry(p0, p3);
                route.LabelPoint = new Point((p0.X + p3.X) / 2, (p0.Y + p3.Y) / 2);
                route.Length = Distance(p0, p3);
                return route;
            }

            if (link.Style == FlowLinkStyle.Curved)
            {
                var distance = Math.Max(80, Distance(p0, p3) * 0.42);
                var c1 = p0 + RotateVector(NormalOf(sourceSide), source.Rotation) * distance;
                var c2 = p3 + RotateVector(NormalOf(targetSide), target.Rotation) * distance;
                route.Geometry = BuildCurveGeometry(p0, c1, c2, p3);
                for (var i = 0; i <= 28; i++)
                {
                    route.Points.Add(BezierPoint(p0, c1, c2, p3, i / 28.0));
                }
                route.LabelPoint = BezierPoint(p0, c1, c2, p3, 0.5);
                for (var i = 1; i < route.Points.Count; i++) route.Length += Distance(route.Points[i - 1], route.Points[i]);
                return route;
            }

            var stub = 28.0;
            var p1 = p0 + RotateVector(NormalOf(sourceSide), source.Rotation) * stub;
            var p2 = p3 + RotateVector(NormalOf(targetSide), target.Rotation) * stub;
            var points = new List<Point> { p0, p1 };
            var sourceHorizontal = sourceSide is FlowPortSide.Left or FlowPortSide.Right;
            var targetHorizontal = targetSide is FlowPortSide.Left or FlowPortSide.Right;

            if (sourceHorizontal && targetHorizontal)
            {
                if (Math.Sign(p1.X - p0.X) != Math.Sign(p2.X - p3.X))
                {
                    var midX = (p1.X + p2.X) / 2;
                    points.Add(new Point(midX, p1.Y));
                    points.Add(new Point(midX, p2.Y));
                }
                else
                {
                    var detourX = p1.X >= p2.X
                        ? Math.Max(p1.X, p2.X) + 44
                        : Math.Min(p1.X, p2.X) - 44;
                    points.Add(new Point(detourX, p1.Y));
                    points.Add(new Point(detourX, p2.Y));
                }
            }
            else if (!sourceHorizontal && !targetHorizontal)
            {
                if (Math.Sign(p1.Y - p0.Y) != Math.Sign(p2.Y - p3.Y))
                {
                    var midY = (p1.Y + p2.Y) / 2;
                    points.Add(new Point(p1.X, midY));
                    points.Add(new Point(p2.X, midY));
                }
                else
                {
                    var detourY = p1.Y >= p2.Y
                        ? Math.Max(p1.Y, p2.Y) + 44
                        : Math.Min(p1.Y, p2.Y) - 44;
                    points.Add(new Point(p1.X, detourY));
                    points.Add(new Point(p2.X, detourY));
                }
            }
            else if (sourceHorizontal)
            {
                points.Add(new Point(p2.X, p1.Y));
            }
            else
            {
                points.Add(new Point(p1.X, p2.Y));
            }

            points.Add(p2);
            points.Add(p3);
            points = Simplify(points);
            route.Points.AddRange(points);
            route.Geometry = BuildRoundedPolyline(points, Math.Min(13, 10 + _zoom * 2));
            route.LabelPoint = PointAtLength(points, PathLength(points) / 2);
            route.Length = PathLength(points);
            return route;
        }

        private static Geometry BuildLineGeometry(Point p1, Point p2)
        {
            var geometry = new StreamGeometry();
            using var ctx = geometry.Open();
            ctx.BeginFigure(p1, false);
            ctx.LineTo(p2);
            ctx.EndFigure(false);
            return geometry;
        }

        private static Geometry BuildCurveGeometry(Point p0, Point c1, Point c2, Point p3)
        {
            var geometry = new StreamGeometry();
            using var ctx = geometry.Open();
            ctx.BeginFigure(p0, false);
            ctx.CubicBezierTo(c1, c2, p3);
            ctx.EndFigure(false);
            return geometry;
        }

        private static Geometry BuildRoundedPolyline(IReadOnlyList<Point> points, double radius)
        {
            var geometry = new StreamGeometry();
            using var ctx = geometry.Open();
            if (points.Count == 0) return geometry;
            ctx.BeginFigure(points[0], false);
            for (var i = 1; i < points.Count - 1; i++)
            {
                var previous = points[i - 1];
                var current = points[i];
                var next = points[i + 1];
                Vector v1 = previous - current;
                Vector v2 = next - current;
                var l1 = v1.Length;
                var l2 = v2.Length;
                if (l1 < 0.001 || l2 < 0.001)
                {
                    ctx.LineTo(current);
                    continue;
                }
                var r = Math.Min(radius, Math.Min(l1, l2) / 2);
                var a = current + v1 / l1 * r;
                var b = current + v2 / l2 * r;
                ctx.LineTo(a);
                ctx.QuadraticBezierTo(current, b);
            }
            ctx.LineTo(points[^1]);
            ctx.EndFigure(false);
            return geometry;
        }

        private static List<Point> Simplify(List<Point> points)
        {
            if (points.Count <= 2) return points;
            var result = new List<Point> { points[0] };
            for (var i = 1; i < points.Count; i++)
            {
                if (Distance(result[^1], points[i]) < 0.2) continue;
                result.Add(points[i]);
            }
            var simplified = new List<Point> { result[0] };
            for (var i = 1; i < result.Count - 1; i++)
            {
                var a = result[i - 1];
                var b = result[i];
                var c = result[i + 1];
                var cross = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
                if (Math.Abs(cross) < 0.001) continue;
                simplified.Add(b);
            }
            simplified.Add(result[^1]);
            return simplified;
        }

        private static double PathLength(IReadOnlyList<Point> points)
        {
            var length = 0.0;
            for (var i = 1; i < points.Count; i++) length += Distance(points[i - 1], points[i]);
            return length;
        }

        private static Point PointAtLength(IReadOnlyList<Point> points, double targetLength)
        {
            if (points.Count == 0) return default;
            var length = 0.0;
            for (var i = 1; i < points.Count; i++)
            {
                var segment = Distance(points[i - 1], points[i]);
                if (length + segment >= targetLength)
                {
                    var t = segment < 0.001 ? 0 : (targetLength - length) / segment;
                    return points[i - 1] + (points[i] - points[i - 1]) * t;
                }
                length += segment;
            }
            return points[^1];
        }

        private static Point BezierPoint(Point p0, Point p1, Point p2, Point p3, double t)
        {
            var mt = 1 - t;
            var x = mt * mt * mt * p0.X + 3 * mt * mt * t * p1.X + 3 * mt * t * t * p2.X + t * t * t * p3.X;
            var y = mt * mt * mt * p0.Y + 3 * mt * mt * t * p1.Y + 3 * mt * t * t * p2.Y + t * t * t * p3.Y;
            return new Point(x, y);
        }

        internal static Geometry BuildNodeGeometry(FlowNode node)
        {
            var rect = new Rect(node.X, node.Y, node.Width, node.Height);
            var geometry = new StreamGeometry();
            if (node.Kind == FlowNodeKind.Connector)
            {
                using var circle = geometry.Open();
                var rx = node.Width / 2;
                var ry = node.Height / 2;
                var center = new Point(rect.Center.X, rect.Center.Y);
                circle.BeginFigure(new Point(center.X, rect.Top), true);
                circle.ArcTo(new Point(rect.Right, center.Y), new Size(rx, ry), 0, false, SweepDirection.Clockwise);
                circle.ArcTo(new Point(center.X, rect.Bottom), new Size(rx, ry), 0, false, SweepDirection.Clockwise);
                circle.ArcTo(new Point(rect.Left, center.Y), new Size(rx, ry), 0, false, SweepDirection.Clockwise);
                circle.ArcTo(new Point(center.X, rect.Top), new Size(rx, ry), 0, false, SweepDirection.Clockwise);
                circle.EndFigure(true);
                return geometry;
            }

            using var ctx = geometry.Open();
            switch (node.Kind)
            {
                case FlowNodeKind.StartEnd:
                    AddRoundedRectangle(ctx, rect, Math.Min(node.Height / 2, node.Width / 2));
                    break;
                case FlowNodeKind.Decision:
                    ctx.BeginFigure(new Point(rect.Center.X, rect.Top), true);
                    ctx.LineTo(new Point(rect.Right, rect.Center.Y));
                    ctx.LineTo(new Point(rect.Center.X, rect.Bottom));
                    ctx.LineTo(new Point(rect.Left, rect.Center.Y));
                    ctx.EndFigure(true);
                    break;
                case FlowNodeKind.Data:
                {
                    var skew = Math.Min(24, node.Width * 0.12);
                    ctx.BeginFigure(new Point(rect.Left + skew, rect.Top), true);
                    ctx.LineTo(new Point(rect.Right, rect.Top));
                    ctx.LineTo(new Point(rect.Right - skew, rect.Bottom));
                    ctx.LineTo(new Point(rect.Left, rect.Bottom));
                    ctx.EndFigure(true);
                    break;
                }
                case FlowNodeKind.Document:
                {
                    ctx.BeginFigure(new Point(rect.Left, rect.Top), true);
                    ctx.LineTo(new Point(rect.Right, rect.Top));
                    var waveTop = rect.Bottom - node.Height * 0.12;
                    ctx.LineTo(new Point(rect.Right, waveTop));
                    ctx.CubicBezierTo(
                        new Point(rect.Right * 0.84 + rect.Left * 0.16, rect.Bottom + 1),
                        new Point(rect.Right * 0.66 + rect.Left * 0.34, rect.Bottom - node.Height * 0.24),
                        new Point(rect.Center.X, rect.Bottom - node.Height * 0.12));
                    ctx.CubicBezierTo(
                        new Point(rect.Right * 0.28 + rect.Left * 0.72, rect.Bottom - 1),
                        new Point(rect.Right * 0.14 + rect.Left * 0.86, rect.Bottom - node.Height * 0.23),
                        new Point(rect.Left, waveTop));
                    ctx.EndFigure(true);
                    break;
                }
                case FlowNodeKind.Database:
                {
                    var ry = Math.Min(16, node.Height * 0.16);
                    ctx.BeginFigure(new Point(rect.Left, rect.Top + ry), true);
                    ctx.ArcTo(new Point(rect.Right, rect.Top + ry), new Size(rect.Width / 2, ry), 0, false, SweepDirection.Clockwise);
                    ctx.LineTo(new Point(rect.Right, rect.Bottom - ry));
                    ctx.ArcTo(new Point(rect.Left, rect.Bottom - ry), new Size(rect.Width / 2, ry), 0, false, SweepDirection.Clockwise);
                    ctx.LineTo(new Point(rect.Left, rect.Top + ry));
                    ctx.EndFigure(true);
                    break;
                }
                default:
                    AddRoundedRectangle(ctx, rect, Math.Min(node.CornerRadius, Math.Min(node.Width, node.Height) / 2));
                    break;
            }
            return geometry;
        }

        private static void AddRoundedRectangle(StreamGeometryContext ctx, Rect rect, double radius)
        {
            radius = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
            if (radius <= 0.01)
            {
                ctx.BeginFigure(rect.TopLeft, true);
                ctx.LineTo(rect.TopRight);
                ctx.LineTo(rect.BottomRight);
                ctx.LineTo(rect.BottomLeft);
                ctx.EndFigure(true);
                return;
            }

            ctx.BeginFigure(new Point(rect.Left + radius, rect.Top), true);
            ctx.LineTo(new Point(rect.Right - radius, rect.Top));
            ctx.ArcTo(new Point(rect.Right, rect.Top + radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
            ctx.LineTo(new Point(rect.Right, rect.Bottom - radius));
            ctx.ArcTo(new Point(rect.Right - radius, rect.Bottom), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
            ctx.LineTo(new Point(rect.Left + radius, rect.Bottom));
            ctx.ArcTo(new Point(rect.Left, rect.Bottom - radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
            ctx.LineTo(new Point(rect.Left, rect.Top + radius));
            ctx.ArcTo(new Point(rect.Left + radius, rect.Top), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
            ctx.EndFigure(true);
        }

        private void OnDiagramReplaced(object? sender, EventArgs e)
        {
            ClearSelectionInternal();
            _imageCache.Clear();
            _needsInitialFit = true;
            EnsureAnimationTimer();
            Dispatcher.UIThread.Post(() =>
            {
                if (_needsInitialFit) FitToContent();
                InvalidateVisual();
            }, DispatcherPriority.Background);
        }

        private void OnItemChanged(object? sender, EventArgs e)
        {
            EnsureAnimationTimer();
            InvalidateVisual();
        }

        private void EnsureAnimationTimer()
        {
            var shouldAnimate = !_isExporting && _viewModel != null &&
                                _viewModel.Document.Nodes.Any(n => n.IsRunning && n.ExecutionEffect != FlowExecutionEffect.None);
            if (shouldAnimate && !_animationTimer.IsEnabled)
            {
                _animationTimer.Start();
            }
            else if (!shouldAnimate && _animationTimer.IsEnabled)
            {
                _animationTimer.Stop();
            }
        }

        private void OnFitRequested(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                FitToContent();
                InvalidateVisual();
            }, DispatcherPriority.Background);
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(HomeViewModel.ShowGrid) or nameof(HomeViewModel.SnapToGrid) or nameof(HomeViewModel.ShowMiniMap))
            {
                InvalidateVisual();
            }
            else if (e.PropertyName == nameof(HomeViewModel.IsPreviewMode))
            {
                ClearHover();
                EnsureAnimationTimer();
                InvalidateVisual();
                Dispatcher.UIThread.Post(() =>
                {
                    FitToContent();
                    InvalidateVisual();
                }, DispatcherPriority.Background);
            }
            else if (e.PropertyName == nameof(HomeViewModel.SelectedNode) ||
                     e.PropertyName == nameof(HomeViewModel.SelectedLink))
            {
                // 属性面板引起的选择状态变化由画布主动同步，无需在此重绘。
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (_viewModel == null) return;
            Focus();

            var screen = e.GetPosition(this);
            _pressScreen = screen;
            _lastScreen = screen;
            _pressWorld = ScreenToWorld(screen);
            var point = e.GetCurrentPoint(this);
            var properties = point.Properties;
            var left = properties.IsLeftButtonPressed;
            var middle = properties.IsMiddleButtonPressed;

            if (_viewModel.IsPreviewMode)
            {
                if (left && e.ClickCount == 2)
                {
                    _viewModel.IsPreviewMode = false;
                }
                return;
            }

            if (_miniMapVisible && _miniMapCloseRect.Contains(screen) && left)
            {
                _viewModel.ShowMiniMap = false;
                ClearHover();
                InvalidateVisual();
                e.Handled = true;
                return;
            }

            if (_miniMapVisible && _miniMapContentRect.Contains(screen) && left)
            {
                _interaction = InteractionMode.MiniMap;
                e.Pointer.Capture(this);
                MoveViewportToMiniMap(screen);
                e.Handled = true;
                return;
            }

            if (middle || _spacePressed || _tool == FlowToolMode.Pan)
            {
                _interaction = InteractionMode.Pan;
                _panStartX = _panX;
                _panStartY = _panY;
                Cursor = HandCursor;
                e.Pointer.Capture(this);
                e.Handled = true;
                return;
            }

            var modifiers = e.KeyModifiers;
            var additive = modifiers.HasFlag(KeyModifiers.Control) || modifiers.HasFlag(KeyModifiers.Shift);

            if (_tool == FlowToolMode.Connect)
            {
                var connectNode = HitTestNode(_pressWorld);
                if (connectNode != null)
                {
                    StartConnection(connectNode, _pressWorld);
                    e.Pointer.Capture(this);
                    e.Handled = true;
                }
                return;
            }

            var handle = HitTestHandle(_pressWorld);
            if (handle != ResizeHandle.None && _selectedNodes.Count == 1)
            {
                _interaction = InteractionMode.ResizeNode;
                _resizeHandle = handle;
                _resizeNode = _selectedNodes[0];
                _resizeStartCenter = new Point(_resizeNode.CenterX, _resizeNode.CenterY);
                _resizeStartWorld = RotatePoint(_pressWorld, _resizeStartCenter, -_resizeNode.Rotation);
                _resizeStartBounds = new Rect(_resizeNode.X, _resizeNode.Y, _resizeNode.Width, _resizeNode.Height);
                var (anchorX, anchorY) = GetResizeDirection(handle);
                var anchorLocal = new Point(
                    _resizeStartCenter.X - anchorX * _resizeStartBounds.Width / 2,
                    _resizeStartCenter.Y - anchorY * _resizeStartBounds.Height / 2);
                _resizeAnchorWorld = RotatePoint(anchorLocal, _resizeStartCenter, _resizeNode.Rotation);
                _dragMoved = false;
                e.Pointer.Capture(this);
                e.Handled = true;
                return;
            }

            var portNode = _hoverNode;
            if (portNode != null && _hoverPort != null && (IsNodeSelected(portNode) || _tool == FlowToolMode.Connect))
            {
                StartConnection(portNode, _pressWorld, _hoverPort.Value);
                e.Pointer.Capture(this);
                e.Handled = true;
                return;
            }

            var node = HitTestNode(_pressWorld);
            if (node != null)
            {
                if (additive)
                {
                    var list = _selectedNodes.ToList();
                    if (list.Contains(node)) list.Remove(node);
                    else list.Add(node);
                    SetSelection(list, null);
                }
                else if (!IsNodeSelected(node))
                {
                    SetSelection(new[] { node }, null);
                }

                _interaction = InteractionMode.MoveNodes;
                _dragOrigins.Clear();
                foreach (var selected in _selectedNodes) _dragOrigins[selected] = new Point(selected.X, selected.Y);
                _dragMoved = false;
                Cursor = MoveCursor;
                e.Pointer.Capture(this);
                e.Handled = true;
                return;
            }

            var link = HitTestLink(_pressWorld);
            if (link != null)
            {
                SetSelection(Array.Empty<FlowNode>(), link);
                e.Handled = true;
                return;
            }

            if (!additive) SetSelection(Array.Empty<FlowNode>(), null);
            _marqueeAdditive = additive;
            _marqueeBase = additive ? _selectedNodes.ToList() : new List<FlowNode>();
            _marqueeWorld = new Rect(_pressWorld, _pressWorld);
            _interaction = InteractionMode.Marquee;
            e.Pointer.Capture(this);
            e.Handled = true;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_viewModel == null) return;
            var screen = e.GetPosition(this);
            var world = ScreenToWorld(screen);
            _lastScreen = screen;
            UpdatePointerText(world);

            if (_interaction == InteractionMode.None)
            {
                UpdateHover(screen);
                return;
            }

            switch (_interaction)
            {
                case InteractionMode.Pan:
                {
                    _panX = _panStartX + (screen.X - _pressScreen.X);
                    _panY = _panStartY + (screen.Y - _pressScreen.Y);
                    InvalidateVisual();
                    break;
                }
                case InteractionMode.MiniMap:
                    MoveViewportToMiniMap(screen);
                    break;
                case InteractionMode.MoveNodes:
                {
                    if (!_dragMoved && Distance(_pressScreen, screen) > 2)
                    {
                        _dragMoved = true;
                        _viewModel.PushUndo();
                    }
                    if (!_dragMoved) break;
                    var dx = world.X - _pressWorld.X;
                    var dy = world.Y - _pressWorld.Y;
                    if (_viewModel.SnapToGrid && _dragOrigins.Count > 0)
                    {
                        var primary = _dragOrigins.Keys.First();
                        var origin = _dragOrigins[primary];
                        dx = Snap(origin.X + dx) - origin.X;
                        dy = Snap(origin.Y + dy) - origin.Y;
                    }
                    foreach (var kv in _dragOrigins)
                    {
                        kv.Key.X = kv.Value.X + dx;
                        kv.Key.Y = kv.Value.Y + dy;
                    }
                    _viewModel.StatusText = L("FlowMovingNodes");
                    InvalidateVisual();
                    break;
                }
                case InteractionMode.ResizeNode:
                {
                    if (!_dragMoved && Distance(_pressScreen, screen) > 2)
                    {
                        _dragMoved = true;
                        _viewModel.PushUndo();
                    }
                    if (!_dragMoved || _resizeNode == null) break;
                    ResizeNode(world);
                    InvalidateVisual();
                    break;
                }
                case InteractionMode.Marquee:
                {
                    _marqueeWorld = NormalizeRect(new Rect(_pressWorld, world));
                    var hits = _viewModel.Document.Nodes.Where(n => n.Bounds.Intersects(_marqueeWorld)).ToList();
                    if (_marqueeAdditive)
                    {
                        foreach (var node in _marqueeBase)
                        {
                            if (!hits.Contains(node)) hits.Add(node);
                        }
                    }
                    SetSelection(hits, null);
                    InvalidateVisual();
                    break;
                }
                case InteractionMode.Connect:
                {
                    _connectPoint = world;
                    _connectTarget = HitTestNode(world);
                    UpdateHover(screen);
                    InvalidateVisual();
                    break;
                }
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (_viewModel == null) return;
            var screen = e.GetPosition(this);
            var world = ScreenToWorld(screen);

            switch (_interaction)
            {
                case InteractionMode.Connect:
                {
                    var target = _connectTarget;
                    if (target != null && _connectSource != null && !ReferenceEquals(target, _connectSource))
                    {
                        var targetSide = ChooseBestSide(target, _connectSource, _connectSourcePort);
                        var link = _viewModel.AddLink(_connectSource, target, _connectSourcePort, targetSide);
                        if (link != null) SetSelection(Array.Empty<FlowNode>(), link);
                    }
                    _connectSource = null;
                    _connectTarget = null;
                    break;
                }
                case InteractionMode.MoveNodes:
                    if (_dragMoved)
                    {
                        DropImportedRoutes();
                        _viewModel.StatusText = L("FlowNodesMoved");
                    }
                    break;
                case InteractionMode.ResizeNode:
                    if (_dragMoved)
                    {
                        DropImportedRoutes();
                        _viewModel.StatusText = L("FlowNodeResized");
                    }
                    break;
                case InteractionMode.Marquee:
                    if (_selectedNodes.Count > 0) _viewModel.StatusText = string.Format(L("FlowSelectedNodesFormat"), _selectedNodes.Count);
                    break;
            }

            if (e.Pointer.Captured == this) e.Pointer.Capture(null);
            _interaction = InteractionMode.None;
            _resizeHandle = ResizeHandle.None;
            _resizeNode = null;
            _dragMoved = false;
            _dragOrigins.Clear();
            UpdateHover(screen);
            InvalidateVisual();
            e.Handled = true;
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            if (_viewModel == null) return;
            if (_viewModel.IsPreviewMode) return;
            var point = e.GetPosition(this);
            var factor = e.Delta.Y > 0 ? 1.12 : 1 / 1.12;
            ZoomAt(_zoom * factor, point);
            e.Handled = true;
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            if (_interaction == InteractionMode.None)
            {
                ClearHover();
                Cursor = _tool switch
                {
                    FlowToolMode.Pan => HandCursor,
                    FlowToolMode.Connect => CrossCursor,
                    _ => ArrowCursor
                };
                InvalidateVisual();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_viewModel == null) return;

            if (e.Key == Key.Space && !_spacePressed)
            {
                _spacePressed = true;
                Cursor = HandCursor;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                if (_interaction == InteractionMode.Connect)
                {
                    _interaction = InteractionMode.None;
                    _connectSource = null;
                    _connectTarget = null;
                    InvalidateVisual();
                }
                else if (_viewModel.IsPreviewMode)
                {
                    _viewModel.IsPreviewMode = false;
                }
                else
                {
                    ClearSelectionInternal();
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    InvalidateVisual();
                }
                e.Handled = true;
                return;
            }

            if (_viewModel.IsPreviewMode) return;

            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                switch (e.Key)
                {
                    case Key.A:
                        SelectAll();
                        e.Handled = true;
                        return;
                    case Key.D:
                        DuplicateSelection();
                        e.Handled = true;
                        return;
                    case Key.Z:
                        _viewModel.Undo();
                        ClearSelectionInternal();
                        SelectionChanged?.Invoke(this, EventArgs.Empty);
                        InvalidateVisual();
                        e.Handled = true;
                        return;
                    case Key.Y:
                        _viewModel.Redo();
                        ClearSelectionInternal();
                        SelectionChanged?.Invoke(this, EventArgs.Empty);
                        InvalidateVisual();
                        e.Handled = true;
                        return;
                    case Key.D0:
                        FitToContent();
                        e.Handled = true;
                        return;
                    case Key.OemPlus:
                    case Key.Add:
                        ZoomIn();
                        e.Handled = true;
                        return;
                    case Key.OemMinus:
                    case Key.Subtract:
                        ZoomOut();
                        e.Handled = true;
                        return;
                }
            }

            if (_selectedNodes.Count > 0)
            {
                var step = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? 10 : 1;
                var moved = true;
                switch (e.Key)
                {
                    case Key.Left:
                        _viewModel.PushUndo();
                        _viewModel.NudgeSelection(_selectedNodes, -step, 0);
                        break;
                    case Key.Right:
                        _viewModel.PushUndo();
                        _viewModel.NudgeSelection(_selectedNodes, step, 0);
                        break;
                    case Key.Up:
                        _viewModel.PushUndo();
                        _viewModel.NudgeSelection(_selectedNodes, 0, -step);
                        break;
                    case Key.Down:
                        _viewModel.PushUndo();
                        _viewModel.NudgeSelection(_selectedNodes, 0, step);
                        break;
                    case Key.Delete:
                    case Key.Back:
                        DeleteSelection();
                        break;
                    default:
                        moved = false;
                        break;
                }
                if (moved)
                {
                    e.Handled = true;
                    InvalidateVisual();
                    return;
                }
            }
            else if ((e.Key is Key.Delete or Key.Back) && _selectedLink != null)
            {
                DeleteSelection();
                e.Handled = true;
                return;
            }

            if (e.KeyModifiers == KeyModifiers.None)
            {
                if (e.Key == Key.V)
                {
                    SetTool(FlowToolMode.Select);
                    e.Handled = true;
                }
                else if (e.Key == Key.C)
                {
                    SetTool(FlowToolMode.Connect);
                    e.Handled = true;
                }
                else if (e.Key == Key.H)
                {
                    SetTool(FlowToolMode.Pan);
                    e.Handled = true;
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Key.Space)
            {
                _spacePressed = false;
                if (_interaction != InteractionMode.Pan)
                {
                    Cursor = _tool switch
                    {
                        FlowToolMode.Pan => HandCursor,
                        FlowToolMode.Connect => CrossCursor,
                        _ => ArrowCursor
                    };
                }
                e.Handled = true;
            }
        }

        private void OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (_viewModel == null || _viewModel.IsPreviewMode) return;
            var screen = e.GetPosition(this);
            var world = ScreenToWorld(screen);
            var node = HitTestNode(world);
            if (node != null)
            {
                SetSelection(new[] { node }, null);
                _viewModel.NotifySelectionDoubleClicked();
                NodeActivated?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                return;
            }
            if (HitTestLink(world) != null) return;
            var created = _viewModel.AddNodeAt(FlowNodeKind.Process, world.X, world.Y);
            SetSelection(new[] { created }, null);
            _viewModel.NotifySelectionDoubleClicked();
            NodeActivated?.Invoke(this, EventArgs.Empty);
            InvalidateVisual();
            e.Handled = true;
        }

        private void StartConnection(FlowNode node, Point world, FlowPortSide? side = null)
        {
            _connectSource = node;
            _connectSourcePort = side ?? GetNearestSide(node, world);
            _connectPoint = world;
            _connectTarget = null;
            _interaction = InteractionMode.Connect;
            Cursor = CrossCursor;
            SetSelection(new[] { node }, null);
            InvalidateVisual();
        }

        private void UpdateHover(Point screen)
        {
            if (_viewModel == null) return;
            var world = ScreenToWorld(screen);
            var oldNode = _hoverNode;
            var oldPort = _hoverPort;
            var oldLink = _hoverLink;
            _hoverNode = null;
            _hoverPort = null;
            _hoverLink = null;

            if (!_miniMapVisible || !_miniMapRect.Contains(screen))
            {
                var node = HitTestNode(world);
                if (node != null) _hoverNode = node;

                // 锚点位于节点边界上，允许在节点外侧附近抓取，提升连线手感。
                var portNode = _hoverNode ?? (_selectedNodes.Count == 1 ? _selectedNodes[0] : null);
                if (portNode != null && (IsNodeSelected(portNode) || _tool == FlowToolMode.Connect))
                {
                    foreach (var (side, _) in GetPorts(portNode))
                    {
                        if (Distance(world, GetPortVisualPoint(portNode, side)) <= 8 / Math.Max(0.35, _zoom))
                        {
                            _hoverNode = portNode;
                            _hoverPort = side;
                            break;
                        }
                    }
                }

                if (_hoverNode == null)
                {
                    _hoverLink = HitTestLink(world);
                }
            }

            Cursor = GetCursor(world, screen);
            if (!ReferenceEquals(oldNode, _hoverNode) || oldPort != _hoverPort || !ReferenceEquals(oldLink, _hoverLink))
            {
                InvalidateVisual();
            }
            UpdatePointerText(world);
        }

        private Cursor GetCursor(Point world, Point screen)
        {
            if (_spacePressed || _tool == FlowToolMode.Pan) return HandCursor;
            if (_viewModel?.IsPreviewMode == true) return ArrowCursor;
            if (_miniMapVisible && _miniMapCloseRect.Contains(screen)) return HandCursor;
            if (_miniMapVisible && _miniMapContentRect.Contains(screen)) return HandCursor;
            if (_interaction == InteractionMode.Pan) return HandCursor;
            if (_interaction == InteractionMode.Marquee) return CrossCursor;
            if (_interaction == InteractionMode.MoveNodes) return MoveCursor;
            if (_interaction == InteractionMode.ResizeNode) return CursorForHandle(_resizeHandle);
            if (_tool == FlowToolMode.Connect) return CrossCursor;

            // 缩放手柄优先于锚点，避免二者视觉靠近时出现光标与实际操作不一致。
            var handle = HitTestHandle(world);
            if (handle != ResizeHandle.None && _selectedNodes.Count == 1) return CursorForHandle(handle);
            if (_hoverPort != null) return CrossCursor;
            if (_hoverNode != null) return MoveCursor;
            if (_hoverLink != null) return HandCursor;
            return ArrowCursor;
        }

        private static Cursor CursorForHandle(ResizeHandle handle)
        {
            return handle switch
            {
                ResizeHandle.Top or ResizeHandle.Bottom => SizeNsCursor,
                ResizeHandle.Left or ResizeHandle.Right => SizeWeCursor,
                ResizeHandle.TopLeft or ResizeHandle.BottomRight => SizeNwseCursor,
                ResizeHandle.TopRight or ResizeHandle.BottomLeft => SizeNeswCursor,
                _ => ArrowCursor
            };
        }

        private void ClearHover()
        {
            _hoverNode = null;
            _hoverPort = null;
            _hoverLink = null;
        }

        private void DropImportedRoutes()
        {
            if (_viewModel == null || _selectedNodes.Count == 0) return;
            var ids = _selectedNodes.Select(n => n.Id).ToHashSet();
            foreach (var link in _viewModel.Document.Links)
            {
                if (link.Waypoints.Count > 0 &&
                    (ids.Contains(link.SourceId) || ids.Contains(link.TargetId)))
                {
                    link.Waypoints.Clear();
                }
            }
        }

        private void SetSelection(IEnumerable<FlowNode> nodes, FlowLink? link)
        {
            _selectedNodes.Clear();
            if (link == null) _selectedNodes.AddRange(nodes.Distinct());
            _selectedLink = link;
            _viewModel?.UpdateSelection(_selectedNodes, _selectedLink);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ClearSelectionInternal()
        {
            _selectedNodes.Clear();
            _selectedLink = null;
            _viewModel?.UpdateSelection(Array.Empty<FlowNode>(), null);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool IsNodeSelected(FlowNode node) => _selectedNodes.Contains(node);

        private FlowNode? HitTestNode(Point world)
        {
            if (_viewModel == null) return null;
            for (var i = _viewModel.Document.Nodes.Count - 1; i >= 0; i--)
            {
                var node = _viewModel.Document.Nodes[i];
                var local = RotatePoint(world, new Point(node.CenterX, node.CenterY), -node.Rotation);
                if (node.Bounds.Contains(local)) return node;
            }
            return null;
        }

        private FlowLink? HitTestLink(Point world)
        {
            if (_viewModel == null) return null;
            var tolerance = 7 / Math.Max(0.35, _zoom);
            for (var i = _viewModel.Document.Links.Count - 1; i >= 0; i--)
            {
                var link = _viewModel.Document.Links[i];
                var source = FindNode(link.SourceId);
                var target = FindNode(link.TargetId);
                if (source == null || target == null) continue;
                var route = BuildRoute(link, source, target);
                for (var p = 1; p < route.Points.Count; p++)
                {
                    if (DistanceToSegment(world, route.Points[p - 1], route.Points[p]) <= tolerance) return link;
                }
            }
            return null;
        }

        private FlowNode? FindNode(string id)
        {
            if (_viewModel == null) return null;
            return _viewModel.Document.Nodes.FirstOrDefault(n => n.Id == id);
        }

        private ResizeHandle HitTestHandle(Point world)
        {
            if (_selectedNodes.Count != 1) return ResizeHandle.None;
            var tolerance = 8 / Math.Max(0.35, _zoom);
            foreach (var (handle, point) in GetHandles(_selectedNodes[0]))
            {
                if (Math.Abs(world.X - point.X) <= tolerance && Math.Abs(world.Y - point.Y) <= tolerance) return handle;
            }
            return ResizeHandle.None;
        }

        private void ResizeRotatedNode(Point world)
        {
            if (_resizeNode == null || _viewModel == null) return;
            var (directionX, directionY) = GetResizeDirection(_resizeHandle);
            var minWidth = _resizeNode.Kind == FlowNodeKind.Decision ? 120 : 56;
            var minHeight = _resizeNode.Kind == FlowNodeKind.Decision ? 80 : 42;
            var delta = RotateVector(new Vector(
                world.X - _resizeAnchorWorld.X,
                world.Y - _resizeAnchorWorld.Y), -_resizeNode.Rotation);

            var width = directionX == 0
                ? _resizeStartBounds.Width
                : Math.Max(minWidth, directionX * delta.X);
            var height = directionY == 0
                ? _resizeStartBounds.Height
                : Math.Max(minHeight, directionY * delta.Y);

            if (_viewModel.SnapToGrid)
            {
                if (directionX != 0) width = Math.Max(minWidth, Snap(width));
                if (directionY != 0) height = Math.Max(minHeight, Snap(height));
            }

            var center = _resizeAnchorWorld + RotateVector(
                new Vector(directionX * width / 2, directionY * height / 2),
                _resizeNode.Rotation);
            _resizeNode.X = center.X - width / 2;
            _resizeNode.Y = center.Y - height / 2;
            _resizeNode.Width = width;
            _resizeNode.Height = height;
            _viewModel.StatusText = L("FlowResizingNode");
        }

        private static (int X, int Y) GetResizeDirection(ResizeHandle handle) => handle switch
        {
            ResizeHandle.TopLeft => (-1, -1),
            ResizeHandle.Top => (0, -1),
            ResizeHandle.TopRight => (1, -1),
            ResizeHandle.Right => (1, 0),
            ResizeHandle.BottomRight => (1, 1),
            ResizeHandle.Bottom => (0, 1),
            ResizeHandle.BottomLeft => (-1, 1),
            ResizeHandle.Left => (-1, 0),
            _ => (0, 0)
        };

        private void ResizeNode(Point world)
        {
            if (_resizeNode == null || _viewModel == null) return;
            if (Math.Abs(_resizeNode.Rotation) >= 0.01)
            {
                ResizeRotatedNode(world);
                return;
            }

            var local = world;
            var dx = local.X - _resizeStartWorld.X;
            var dy = local.Y - _resizeStartWorld.Y;
            var left = _resizeStartBounds.Left;
            var top = _resizeStartBounds.Top;
            var right = _resizeStartBounds.Right;
            var bottom = _resizeStartBounds.Bottom;
            var minWidth = _resizeNode.Kind == FlowNodeKind.Decision ? 120 : 56;
            var minHeight = _resizeNode.Kind == FlowNodeKind.Decision ? 80 : 42;

            switch (_resizeHandle)
            {
                case ResizeHandle.Left:
                case ResizeHandle.TopLeft:
                case ResizeHandle.BottomLeft:
                    left = Math.Min(right - minWidth, _resizeStartBounds.Left + dx);
                    if (_viewModel.SnapToGrid) left = Snap(left);
                    break;
            }
            switch (_resizeHandle)
            {
                case ResizeHandle.Right:
                case ResizeHandle.TopRight:
                case ResizeHandle.BottomRight:
                    right = Math.Max(left + minWidth, _resizeStartBounds.Right + dx);
                    if (_viewModel.SnapToGrid) right = Snap(right);
                    break;
            }
            switch (_resizeHandle)
            {
                case ResizeHandle.Top:
                case ResizeHandle.TopLeft:
                case ResizeHandle.TopRight:
                    top = Math.Min(bottom - minHeight, _resizeStartBounds.Top + dy);
                    if (_viewModel.SnapToGrid) top = Snap(top);
                    break;
            }
            switch (_resizeHandle)
            {
                case ResizeHandle.Bottom:
                case ResizeHandle.BottomLeft:
                case ResizeHandle.BottomRight:
                    bottom = Math.Max(top + minHeight, _resizeStartBounds.Bottom + dy);
                    if (_viewModel.SnapToGrid) bottom = Snap(bottom);
                    break;
            }

            _resizeNode.X = left;
            _resizeNode.Y = top;
            _resizeNode.Width = Math.Max(minWidth, right - left);
            _resizeNode.Height = Math.Max(minHeight, bottom - top);
            _viewModel.StatusText = L("FlowResizingNode");
        }

        private void ZoomAt(double zoom, Point anchor)
        {
            if (_viewModel == null) return;
            var newZoom = Math.Clamp(zoom, 0.22, 2.6);
            if (Math.Abs(newZoom - _zoom) < 0.0001) return;
            var world = ScreenToWorld(anchor);
            _zoom = newZoom;
            _panX = anchor.X - world.X * _zoom;
            _panY = anchor.Y - world.Y * _zoom;
            _viewModel.UpdateZoom(_zoom);
            InvalidateVisual();
        }

        private void MoveViewportToMiniMap(Point screen)
        {
            if (!_miniMapVisible || _miniMapScale <= 0) return;
            var x = (screen.X - _miniMapOffset.X) / _miniMapScale + _miniMapWorldBounds.X;
            var y = (screen.Y - _miniMapOffset.Y) / _miniMapScale + _miniMapWorldBounds.Y;
            _panX = Bounds.Width / 2 - x * _zoom;
            _panY = Bounds.Height / 2 - y * _zoom;
            InvalidateVisual();
        }

        private Point WorldToMiniMap(Point world)
        {
            return new Point(
                _miniMapOffset.X + (world.X - _miniMapWorldBounds.X) * _miniMapScale,
                _miniMapOffset.Y + (world.Y - _miniMapWorldBounds.Y) * _miniMapScale);
        }

        private Point ScreenToWorld(Point screen) => new((screen.X - _panX) / _zoom, (screen.Y - _panY) / _zoom);

        private void UpdatePointerText(Point world)
        {
            if (_viewModel == null) return;
            _viewModel.PointerText = $"X {world.X:0}   Y {world.Y:0}";
        }

        private Rect GetContentBounds(double margin, bool includeLinks = false)
        {
            if (_viewModel == null || _viewModel.Document.Nodes.Count == 0) return default;
            var bounds = GetBounds(_viewModel.Document.Nodes);
            if (includeLinks)
            {
                foreach (var link in _viewModel.Document.Links)
                {
                    var source = FindNode(link.SourceId);
                    var target = FindNode(link.TargetId);
                    if (source == null || target == null) continue;
                    var route = BuildRoute(link, source, target);
                    foreach (var point in route.Points)
                    {
                        bounds = bounds.Union(new Rect(point.X, point.Y, 0.1, 0.1));
                    }
                }
            }
            return bounds.Inflate(margin);
        }

        private static Rect GetBounds(IEnumerable<FlowNode> nodes)
        {
            var list = nodes.ToList();
            if (list.Count == 0) return default;
            var left = list.Min(n => GetNodeVisualBounds(n).Left);
            var top = list.Min(n => GetNodeVisualBounds(n).Top);
            var right = list.Max(n => GetNodeVisualBounds(n).Right);
            var bottom = list.Max(n => GetNodeVisualBounds(n).Bottom);
            return new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
        }

        private static Rect NormalizeRect(Rect rect)
        {
            var left = Math.Min(rect.Left, rect.Right);
            var top = Math.Min(rect.Top, rect.Bottom);
            return new Rect(left, top, Math.Abs(rect.Width), Math.Abs(rect.Height));
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

        /// <summary>
        /// 连接锚点的可视化位置：沿外法线偏移，避免与节点边中点的缩放手柄重叠。
        /// </summary>
        private Point GetPortVisualPoint(FlowNode node, FlowPortSide side)
        {
            var offset = 10 / Math.Max(0.35, _zoom);
            return GetAnchor(node, side) + RotateVector(NormalOf(side), node.Rotation) * offset;
        }

        private static Matrix GetNodeRotationMatrix(FlowNode node)
        {
            var radians = node.Rotation * Math.PI / 180;
            return Matrix.CreateTranslation(-node.CenterX, -node.CenterY) *
                   Matrix.CreateRotation(radians) *
                   Matrix.CreateTranslation(node.CenterX, node.CenterY);
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
            return new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
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

        private static FlowPortSide ChooseBestSide(FlowNode node, FlowNode other, FlowPortSide sourceSide)
        {
            var dx = other.CenterX - node.CenterX;
            var dy = other.CenterY - node.CenterY;
            _ = sourceSide;
            if (Math.Abs(dx) >= Math.Abs(dy) * 0.95)
            {
                return dx >= 0 ? FlowPortSide.Right : FlowPortSide.Left;
            }
            return dy >= 0 ? FlowPortSide.Bottom : FlowPortSide.Top;
        }

        private static FlowPortSide GetNearestSide(FlowNode node, Point point)
        {
            var anchors = new[]
            {
                (FlowPortSide.Left, GetAnchor(node, FlowPortSide.Left)),
                (FlowPortSide.Top, GetAnchor(node, FlowPortSide.Top)),
                (FlowPortSide.Right, GetAnchor(node, FlowPortSide.Right)),
                (FlowPortSide.Bottom, GetAnchor(node, FlowPortSide.Bottom))
            };
            return anchors.OrderBy(a => Distance(point, a.Item2)).First().Item1;
        }

        private static IEnumerable<(FlowPortSide Side, Point Point)> GetPorts(FlowNode node)
        {
            yield return (FlowPortSide.Left, GetAnchor(node, FlowPortSide.Left));
            yield return (FlowPortSide.Top, GetAnchor(node, FlowPortSide.Top));
            yield return (FlowPortSide.Right, GetAnchor(node, FlowPortSide.Right));
            yield return (FlowPortSide.Bottom, GetAnchor(node, FlowPortSide.Bottom));
        }

        private static IEnumerable<(ResizeHandle Handle, Point Point)> GetHandles(FlowNode node)
        {
            if (Math.Abs(node.Rotation) < 0.01) return GetHandles(node.Bounds);
            var center = new Point(node.CenterX, node.CenterY);
            return GetHandles(node.Bounds)
                .Select(item => (item.Handle, RotatePoint(item.Point, center, node.Rotation)));
        }

        private static IEnumerable<(ResizeHandle Handle, Point Point)> GetHandles(Rect bounds)
        {
            yield return (ResizeHandle.TopLeft, bounds.TopLeft);
            yield return (ResizeHandle.Top, new Point(bounds.Center.X, bounds.Top));
            yield return (ResizeHandle.TopRight, bounds.TopRight);
            yield return (ResizeHandle.Right, new Point(bounds.Right, bounds.Center.Y));
            yield return (ResizeHandle.BottomRight, bounds.BottomRight);
            yield return (ResizeHandle.Bottom, new Point(bounds.Center.X, bounds.Bottom));
            yield return (ResizeHandle.BottomLeft, bounds.BottomLeft);
            yield return (ResizeHandle.Left, new Point(bounds.Left, bounds.Center.Y));
        }

        private static double Distance(Point a, Point b) => ((Vector)(b - a)).Length;

        private static double DistanceToSegment(Point p, Point a, Point b)
        {
            var ab = b - a;
            var lengthSquared = ab.X * ab.X + ab.Y * ab.Y;
            if (lengthSquared < 0.0001) return Distance(a, p);
            var t = Math.Clamp(((p - a).X * ab.X + (p - a).Y * ab.Y) / lengthSquared, 0, 1);
            var projection = a + ab * t;
            return Distance(projection, p);
        }

        private static double Snap(double value) => Math.Round(value / 20) * 20;

        private static string L(string key) => OpenEAP.Localizer.Localizer.Instance[key];

        private static Color ParseColor(string color)
        {
            try { return Color.Parse(color); }
            catch { return Colors.Gray; }
        }

        private static Color Lighten(Color color, double amount)
        {
            var r = (byte)Math.Clamp(color.R + (255 - color.R) * amount, 0, 255);
            var g = (byte)Math.Clamp(color.G + (255 - color.G) * amount, 0, 255);
            var b = (byte)Math.Clamp(color.B + (255 - color.B) * amount, 0, 255);
            return Color.FromArgb(color.A, r, g, b);
        }

        private static Color WithAlpha(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

        private IBrush Brush(Color color)
        {
            if (!_brushCache.TryGetValue(color, out var brush))
            {
                brush = new SolidColorBrush(color);
                _brushCache[color] = brush;
            }
            return brush;
        }

        private IBrush Brush(string color) => Brush(ParseColor(color));

        private static IPen CreateLinkPen(Color color, double width, bool dashed)
        {
            var dash = dashed ? new DashStyle(new[] { 6d, 4d }, 0) : null;
            return new Pen(new SolidColorBrush(color), width, dash, PenLineCap.Round, PenLineJoin.Round);
        }

        private CanvasPalette GetPalette()
        {
            var variant = TopLevel.GetTopLevel(this)?.ActualThemeVariant;
            var light = variant == Avalonia.Styling.ThemeVariant.Light;
            return light
                ? new CanvasPalette(
                    Color.Parse("#F7F9FC"), Color.Parse("#DCE3ED"), Color.Parse("#C7D1DF"),
                    Color.Parse("#FFFFFF"), Color.Parse("#D8DFE9"), Color.Parse("#2563EB"),
                    Color.Parse("#1F2937"), Color.Parse("#EFF3F8"), Color.Parse("#F5F8FD"),
                    Color.Parse("#FFFFFF"), Color.Parse("#FFFFFF"))
                : new CanvasPalette(
                    Color.Parse("#0F131A"), Color.Parse("#202633"), Color.Parse("#2B3444"),
                    Color.Parse("#171C25"), Color.Parse("#2A3241"), Color.Parse("#4C8DFF"),
                    Color.Parse("#E8EEF8"), Color.Parse("#171C25"), Color.Parse("#10151D"),
                    Color.Parse("#F8FAFD"), Color.Parse("#FFFFFF"));
        }

        private readonly record struct CanvasPalette(
            Color Background,
            Color GridMinor,
            Color GridMajor,
            Color Panel,
            Color Border,
            Color Accent,
            Color LabelText,
            Color LabelBackground,
            Color HandleFill,
            Color NodeHighlight,
            Color White);
    }
}
