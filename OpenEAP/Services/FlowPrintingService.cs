using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.Versioning;

namespace OpenEAP.Services
{
    /// <summary>Windows 打印服务。位图按页面比例缩放后发送到所选打印机。</summary>
    [SupportedOSPlatform("windows")]
    public static class FlowPrintingService
    {
        public static IReadOnlyList<string> GetPrinters()
        {
            var result = new List<string>();
            try
            {
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (!string.IsNullOrWhiteSpace(printer)) result.Add(printer);
                }
            }
            catch
            {
                // 没有打印后台服务时返回空列表，由预览窗口显示提示。
            }
            return result;
        }

        public static void PrintPng(byte[] png, string documentName, string? printerName, bool landscape)
        {
            if (png is not { Length: > 0 }) throw new ArgumentException(L("FlowPrintingEmptyImage"), nameof(png));

            using var imageStream = new MemoryStream(png, writable: false);
            using var image = Image.FromStream(imageStream);
            using var document = new PrintDocument();
            document.DocumentName = string.IsNullOrWhiteSpace(documentName) ? L("FlowDocumentDefault") : documentName;
            document.PrintController = new StandardPrintController();

            if (!string.IsNullOrWhiteSpace(printerName))
            {
                document.PrinterSettings.PrinterName = printerName;
            }
            if (!document.PrinterSettings.IsValid)
            {
                throw new InvalidOperationException(L("FlowNoAvailablePrinter"));
            }

            document.DefaultPageSettings.Landscape = landscape;
            document.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);
            document.PrintPage += (_, args) =>
            {
                var bounds = args.MarginBounds;
                if (bounds.Width <= 0 || bounds.Height <= 0) bounds = args.PageBounds;
                if (bounds.Width <= 0 || bounds.Height <= 0) return;
                if (args.Graphics is not { } graphics) return;

                var scale = Math.Min((double)bounds.Width / image.Width, (double)bounds.Height / image.Height);
                var width = Math.Max(1, (int)Math.Round(image.Width * scale));
                var height = Math.Max(1, (int)Math.Round(image.Height * scale));
                var x = bounds.X + (bounds.Width - width) / 2;
                var y = bounds.Y + (bounds.Height - height) / 2;

                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, new Rectangle(x, y, width, height));
                args.HasMorePages = false;
            };

            document.Print();
        }
        private static string L(string key) => OpenEAP.Localizer.Localizer.Instance[key];

    }
}
