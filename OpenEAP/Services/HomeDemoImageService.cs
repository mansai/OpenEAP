using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;

namespace OpenEAP.Services
{
    /// <summary>
    /// 演示图片生成：无真实图片时，按串号绘制可复现的电池串外观图 / EL 图
    /// </summary>
    public static class HomeDemoImageService
    {
        private const int W = 1020;
        private const int H = 130;

        /// <summary>
        /// 从运行目录 images/ 加载真实图片，文件名为 {串号}_{外观|EL}.png；不存在返回 null
        /// </summary>
        public static Bitmap? TryLoadImage(string serial, string kind)
        {
            try
            {
                var file = Path.Combine(AppContext.BaseDirectory, "images", $"{serial}_{kind}.png");
                if (File.Exists(file))
                {
                    return new Bitmap(file);
                }
            }
            catch
            {
                // 图片读取失败按无图处理，走绘制兜底
            }
            return null;
        }

        public static Bitmap DrawAppearance(string serial)
        {
            var rnd = mulberry32(hashStr(serial));
            var bmp = new RenderTargetBitmap(new PixelSize(W, H), new Vector(96, 96));
            using (var ctx = bmp.CreateDrawingContext())
            {
                var bg = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(Color.FromRgb(0xE4, 0xE8, 0xEE), 0),
                        new GradientStop(Color.FromRgb(0xB6, 0xBE, 0xC9), 1),
                    }
                };
                ctx.DrawRectangle(bg, null, new Rect(0, 0, W, H));

                const int n = 12, gap = 4, margin = 8, y0 = 12;
                double cell = (W - margin * 2 - gap * (n - 1)) / (double)n;
                for (int i = 0; i < n; i++)
                {
                    double cx = margin + i * (cell + gap);
                    double shade = 0.82 + rnd() * 0.3;
                    var cellBrush = new LinearGradientBrush
                    {
                        StartPoint = new RelativePoint(cx, y0, RelativeUnit.Absolute),
                        EndPoint = new RelativePoint(cx + cell, y0 + cell, RelativeUnit.Absolute),
                        GradientStops = new GradientStops
                        {
                            new GradientStop(Color.FromRgb((byte)(52 * shade), (byte)(88 * shade), (byte)(150 * shade)), 0),
                            new GradientStop(Color.FromRgb((byte)(26 * shade), (byte)(48 * shade), (byte)(102 * shade)), 1),
                        }
                    };
                    var cellPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 40, 60, 90)), 0.8);
                    ctx.DrawGeometry(cellBrush, cellPen, RoundedRect(cx, y0, cell, cell, 8));

                    // 细栅线
                    var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(140, 216, 226, 238)), 0.6);
                    for (double gx = cx + 4; gx < cx + cell - 2; gx += 5)
                    {
                        ctx.DrawLine(gridPen, new Point(gx, y0 + 3), new Point(gx, y0 + cell - 3));
                    }

                    // 随机裂纹短线
                    if (rnd() < 0.22)
                    {
                        var crackPen = new Pen(new SolidColorBrush(Color.FromArgb(191, 70, 36, 30)), 1.3);
                        double sx = cx + 4 + rnd() * (cell - 8), sy = y0 + 4 + rnd() * (cell - 8);
                        ctx.DrawLine(crackPen, new Point(sx, sy),
                            new Point(sx + (rnd() - 0.5) * 20, sy + (rnd() - 0.5) * 20));
                    }
                }

                // 焊带：3 根横向贯穿整串
                var ribbonPen = new Pen(new SolidColorBrush(Color.FromArgb(242, 238, 242, 250)), 3);
                for (int k = 0; k < 3; k++)
                {
                    double yy = y0 + cell * (0.25 + 0.25 * k);
                    ctx.DrawLine(ribbonPen, new Point(margin, yy), new Point(W - margin, yy));
                }
            }
            return bmp;
        }

        public static Bitmap DrawEL(string serial)
        {
            var rnd = mulberry32(hashStr(serial));
            var bmp = new RenderTargetBitmap(new PixelSize(W, H), new Vector(96, 96));
            using (var ctx = bmp.CreateDrawingContext())
            {
                ctx.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x03, 0x05, 0x08)), null, new Rect(0, 0, W, H));

                const int n = 12, gap = 4, margin = 8, y0 = 12;
                double cell = (W - margin * 2 - gap * (n - 1)) / (double)n;
                for (int i = 0; i < n; i++)
                {
                    double cx = margin + i * (cell + gap);
                    byte b = (byte)(116 + Math.Round(rnd() * 46));
                    var cellBrush = new SolidColorBrush(Color.FromRgb(b, b, (byte)(b + 10)));
                    ctx.DrawGeometry(cellBrush, null, RoundedRect(cx, y0, cell, cell, 8));

                    // 栅线（暗）
                    var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(77, 0, 0, 0)), 0.8);
                    for (double gx = cx + 2; gx < cx + cell; gx += 5)
                    {
                        ctx.DrawLine(gridPen, new Point(gx, y0), new Point(gx, y0 + cell));
                    }

                    double d = rnd();
                    if (d < 0.45)
                    {
                        // 裂纹折线
                        var crackPen = new Pen(new SolidColorBrush(Color.FromArgb(242, 2, 4, 7)), 1.7);
                        double px = cx + cell * rnd(), py = y0 + cell * rnd();
                        var start = new Point(px, py);
                        for (int k = 0; k < 6; k++)
                        {
                            px += (rnd() - 0.5) * cell * 0.6;
                            py += (rnd() - 0.5) * cell * 0.6;
                            ctx.DrawLine(crackPen, start, new Point(px, py));
                        }
                    }
                    else if (d < 0.72)
                    {
                        // 暗斑
                        var spotBrush = new SolidColorBrush(Color.FromArgb(224, 2, 4, 7));
                        double sx = cx + cell * rnd(), sy = y0 + cell * rnd(), sr = 4 + rnd() * 12;
                        ctx.DrawEllipse(spotBrush, null, new Point(sx, sy), sr, sr);
                    }
                }

                // 焊带：EL 下更亮
                var ribbonPen = new Pen(new SolidColorBrush(Color.FromArgb(217, 230, 240, 252)), 3);
                for (int k = 0; k < 3; k++)
                {
                    double yy = y0 + cell * (0.25 + 0.25 * k);
                    ctx.DrawLine(ribbonPen, new Point(margin, yy), new Point(W - margin, yy));
                }
            }
            return bmp;
        }

        private static StreamGeometry RoundedRect(double x, double y, double w, double h, double r)
        {
            var g = new StreamGeometry();
            using (var ctx = g.Open())
            {
                ctx.BeginFigure(new Point(x + r, y), false);
                ctx.ArcTo(new Point(x + w, y + r), new Size(r, r), 0, false, SweepDirection.Clockwise);
                ctx.LineTo(new Point(x + w, y + h - r));
                ctx.ArcTo(new Point(x + w - r, y + h), new Size(r, r), 0, false, SweepDirection.Clockwise);
                ctx.LineTo(new Point(x + r, y + h));
                ctx.ArcTo(new Point(x, y + h - r), new Size(r, r), 0, false, SweepDirection.Clockwise);
                ctx.LineTo(new Point(x, y + r));
                ctx.ArcTo(new Point(x + r, y), new Size(r, r), 0, false, SweepDirection.Clockwise);
                ctx.EndFigure(true);
            }
            return g;
        }

        private static uint hashStr(string s)
        {
            uint h = 2166136261u;
            for (int i = 0; i < s.Length; i++)
            {
                h ^= s[i];
                h = h * 16777619u;
            }
            return h;
        }

        private static Func<double> mulberry32(uint a)
        {
            return () =>
            {
                a += 0x6D2B79F5u;
                uint t = a ^ (a >> 15);
                t = t * (t | 1u);
                t = (t + (t ^ (t >> 7)) * (61u | t)) ^ t;
                t = t ^ (t >> 14);
                return t / 4294967296.0;
            };
        }
    }
}
