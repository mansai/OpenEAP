using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using OpenEAP.Common;
using OpenEAP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;

namespace OpenEAP.Views
{
    [SupportedOSPlatform("windows")]
    public partial class FlowPrintPreview : AtomUI.Desktop.Controls.Window
    {
        private readonly byte[] _png = Array.Empty<byte>();
        private readonly string _documentName = L("FlowDocumentDefault");
        private readonly bool _autoLandscape;
        private IReadOnlyList<string> _printers = Array.Empty<string>();
        private string? _selectedPrinter;
        private Bitmap? _previewBitmap;

        /// <summary>供 XAML 运行时加载器使用；实际打印请使用带预览图的重载。</summary>
        public FlowPrintPreview()
        {
            InitializeComponent();
        }

        public FlowPrintPreview(byte[] png, string documentName)
        {
            InitializeComponent();
            _png = png;
            _documentName = string.IsNullOrWhiteSpace(documentName) ? L("FlowDocumentDefault") : documentName;
            Title = string.Format(L("FlowPrintDialogTitle"), _documentName);

            using (var stream = new MemoryStream(png, writable: false))
            {
                _previewBitmap = new Bitmap(stream);
            }
            PreviewImage.Source = _previewBitmap;
            _autoLandscape = _previewBitmap.PixelSize.Width >= _previewBitmap.PixelSize.Height;

            _printers = FlowPrintingService.GetPrinters();
            PrinterList.ItemsSource = _printers;
            if (_printers.Count > 0)
            {
                PrinterList.SelectedIndex = 0;
                _selectedPrinter = _printers[0];
                PrinterNameText.Text = _selectedPrinter;
            }
            else
            {
                PrinterNameText.Text = L("FlowNoPrinter");
                PrinterButton.IsEnabled = false;
                PrintButton.IsEnabled = false;
                StatusText.Text = L("FlowNoPrinterHint");
            }

            Opened += (_, _) => Dispatcher.UIThread.Post(UpdatePage, DispatcherPriority.Background);
        }

        private bool IsLandscape()
        {
            return LandscapeOrientation.IsChecked == true ||
                   (AutoOrientation.IsChecked == true && _autoLandscape);
        }

        private void OnOrientationChanged(object? sender, RoutedEventArgs e)
        {
            UpdatePage();
        }

        private void OnPrinterSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (PrinterList.SelectedItem is not string printer) return;
            _selectedPrinter = printer;
            PrinterNameText.Text = printer;
            PrinterButton.Flyout?.Hide();
        }

        private void OnPageHostSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdatePage();
        }

        private void UpdatePage()
        {
            if (PageHost == null || PageBorder == null) return;
            var availableWidth = PageHost.Bounds.Width - 20;
            var availableHeight = PageHost.Bounds.Height - 20;
            if (availableWidth < 60 || availableHeight < 60) return;

            var landscape = IsLandscape();
            var ratio = landscape ? 297.0 / 210.0 : 210.0 / 297.0;
            var pageWidth = availableWidth;
            var pageHeight = pageWidth / ratio;
            if (pageHeight > availableHeight)
            {
                pageHeight = availableHeight;
                pageWidth = pageHeight * ratio;
            }

            PageBorder.Width = Math.Max(80, pageWidth);
            PageBorder.Height = Math.Max(80, pageHeight);
            PageInfoText.Text = landscape ? L("FlowA4Landscape") : L("FlowA4Portrait");
            if (PrintButton.IsEnabled)
            {
                StatusText.Text = L("FlowPrintPreviewStatus");
            }
        }

        private void OnPrintClick(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedPrinter))
            {
                StatusText.Text = L("FlowSelectPrinterHint");
                return;
            }

            try
            {
                PrintButton.IsEnabled = false;
                FlowPrintingService.PrintPng(_png, _documentName, _selectedPrinter, IsLandscape());
                StatusText.Text = string.Format(L("FlowPrintSent"), _selectedPrinter);
            }
            catch (Exception ex)
            {
                StatusText.Text = string.Format(L("FlowPrintFailed"), ex.Message);
                UIMessage.NotificationError(string.Format(L("FlowPrintFailed"), ex.Message));
            }
            finally
            {
                PrintButton.IsEnabled = _printers.Count > 0;
            }
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private static string L(string key) => Localizer.Localizer.Instance[key];

        protected override void OnClosed(EventArgs e)
        {
            PreviewImage.Source = null;
            _previewBitmap?.Dispose();
            _previewBitmap = null;
            base.OnClosed(e);
        }
    }
}
