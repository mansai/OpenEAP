using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using OpenEAP.Models.FlowChartDesigner;
using System;

namespace OpenEAP.Controls
{
    /// <summary>形状库中的迷你节点预览。</summary>
    public sealed class FlowShapePreview : Control
    {
        public static readonly StyledProperty<FlowNodeKind> KindProperty =
            AvaloniaProperty.Register<FlowShapePreview, FlowNodeKind>(nameof(Kind));

        public static readonly StyledProperty<string> FillProperty =
            AvaloniaProperty.Register<FlowShapePreview, string>(nameof(Fill), "#4C8DFF");

        static FlowShapePreview()
        {
            AffectsRender<FlowShapePreview>(KindProperty, FillProperty);
        }

        public FlowNodeKind Kind
        {
            get => GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }

        public string Fill
        {
            get => GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var width = Bounds.Width - 2;
            var height = Bounds.Height - 2;
            if (width < 4 || height < 4) return;

            var fill = ParseColor(Fill);
            var node = new FlowNode
            {
                Kind = Kind,
                X = 1,
                Y = 1,
                Width = width,
                Height = height,
                Fill = Fill,
                CornerRadius = Math.Min(13, Math.Min(width, height) * 0.3),
                StrokeWidth = 1
            };

            var geometry = FlowDiagramCanvas.BuildNodeGeometry(node);
            context.DrawGeometry(new SolidColorBrush(fill), null, geometry);
            context.DrawGeometry(null, new Pen(new SolidColorBrush(Lighten(fill, 0.28)), 1), geometry);

            if (Kind is FlowNodeKind.Process or FlowNodeKind.Subprocess or FlowNodeKind.Data or FlowNodeKind.Document)
            {
                context.DrawGeometry(new SolidColorBrush(Color.FromArgb(26, 255, 255, 255)), null, geometry);
            }

            if (Kind == FlowNodeKind.Image)
            {
                var iconColor = Color.Parse("#64748B");
                var pen = new Pen(new SolidColorBrush(iconColor), 1.2);
                var icon = new Rect(width / 2 - 9, height / 2 - 7, 18, 14);
                context.DrawRectangle(null, pen, icon, 2, 2);
                context.DrawEllipse(new SolidColorBrush(iconColor), null, new Point(icon.Right - 4.5, icon.Y + 4), 1.3, 1.3);
                var mountain = new StreamGeometry();
                using (var ctx = mountain.Open())
                {
                    ctx.BeginFigure(new Point(icon.X + 2, icon.Bottom - 2), false);
                    ctx.LineTo(new Point(icon.X + 7, icon.Center.Y + 1));
                    ctx.LineTo(new Point(icon.X + 11, icon.Bottom - 4));
                    ctx.LineTo(new Point(icon.X + 14, icon.Center.Y + 2));
                    ctx.LineTo(new Point(icon.Right - 2, icon.Bottom - 2));
                    ctx.EndFigure(false);
                }
                context.DrawGeometry(null, pen, mountain);
            }
        }

        private static Color ParseColor(string color)
        {
            try { return Color.Parse(color); }
            catch { return Colors.SlateGray; }
        }

        private static Color Lighten(Color color, double amount)
        {
            var r = (byte)Math.Clamp(color.R + (255 - color.R) * amount, 0, 255);
            var g = (byte)Math.Clamp(color.G + (255 - color.G) * amount, 0, 255);
            var b = (byte)Math.Clamp(color.B + (255 - color.B) * amount, 0, 255);
            return Color.FromArgb(color.A, r, g, b);
        }
    }
}
