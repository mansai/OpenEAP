using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using OpenEAP.Common;
using OpenEAP.Controls;
using OpenEAP.Models.FlowChartDesigner;
using OpenEAP.Services;
using OpenEAP.ViewModels;
using ReactiveUI.Avalonia.Reactive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenEAP.Views
{
    public partial class HomeView : UserControl
    {
        private static readonly FilePickerFileType FlowFileType = new(L("FlowFileTypeFlow"))
        {
            Patterns = new[] { "*.flow.json", "*.json" },
            MimeTypes = new[] { "application/json" }
        };

        private static readonly FilePickerFileType BpmnFileType = new(L("FlowFileTypeBpmn"))
        {
            Patterns = new[] { "*.bpmn", "*.xml" },
            MimeTypes = new[] { "application/xml", "text/xml" }
        };

        private static readonly FilePickerFileType VisioFileType = new(L("FlowFileTypeVisio"))
        {
            Patterns = new[] { "*.vsdx" }
        };

        private static readonly FilePickerFileType PngFileType = new(L("FlowFileTypePng"))
        {
            Patterns = new[] { "*.png" },
            MimeTypes = new[] { "image/png" }
        };

        private static readonly FilePickerFileType SvgFileType = new(L("FlowFileTypeSvg"))
        {
            Patterns = new[] { "*.svg" },
            MimeTypes = new[] { "image/svg+xml" }
        };

        private static readonly FilePickerFileType ImageFileType = new(L("FlowFileTypeImage"))
        {
            Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.webp" },
            MimeTypes = new[] { "image/png", "image/jpeg", "image/bmp", "image/gif", "image/webp" }
        };

        public HomeViewModel _viewModel;
        private bool _syncingTool;
        private bool _closeConfirmed;
        private TopLevel _topLevel;

        public HomeView()
        {
            InitializeComponent();           
            this.Loaded += HomeView_Loaded;
        }

        private void HomeView_Loaded(object? sender, RoutedEventArgs e)
        {
            _topLevel = TopLevel.GetTopLevel(this);
            DesignerCanvas.Attach(_viewModel);
            DesignerCanvas.ToolChanged += OnCanvasToolChanged;
            _viewModel.SelectionDoubleClicked += OnSelectionDoubleClicked;
            SyncToolSegment();
        }

        private void OnUndoClick(object? sender, RoutedEventArgs e)
        {
            if (_viewModel.Undo())
            {
                DesignerCanvas.Focus();
            }
        }

        private void OnRedoClick(object? sender, RoutedEventArgs e)
        {
            if (_viewModel.Redo())
            {
                DesignerCanvas.Focus();
            }
        }

        private void OnToolSegmentChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_syncingTool || sender is not Segmented segmented || DesignerCanvas == null) return;
            var tool = segmented.SelectedIndex switch
            {
                1 => FlowToolMode.Connect,
                2 => FlowToolMode.Pan,
                _ => FlowToolMode.Select
            };
            DesignerCanvas.SetTool(tool);
            DesignerCanvas.Focus();
        }

        private void OnCanvasToolChanged(object? sender, EventArgs e)
        {
            SyncToolSegment();
            _viewModel.StatusText = DesignerCanvas.Tool switch
            {
                FlowToolMode.Connect => L("FlowToolConnectStatus"),
                FlowToolMode.Pan => L("FlowToolPanStatus"),
                _ => L("FlowToolSelectStatus")
            };
        }

        private void SyncToolSegment()
        {
            if (ToolSegment == null) return;
            _syncingTool = true;
            ToolSegment.SelectedIndex = (int)DesignerCanvas.Tool;
            _syncingTool = false;
        }

        private async void OnPaletteShapeClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Avalonia.Controls.Button { DataContext: FlowShapeDefinition definition })
            {
                if (definition.Kind == FlowNodeKind.Image)
                {
                    await ImportImageShapeAsync(null);
                    return;
                }
                DesignerCanvas.AddShape(definition.Kind);
                DesignerCanvas.Focus();
            }
        }

        private void OnAddProcessClick(object? sender, RoutedEventArgs e)
        {
            DesignerCanvas.AddShape(FlowNodeKind.Process);
            DesignerCanvas.Focus();
        }

        private async void OnImportImageShapeClick(object? sender, RoutedEventArgs e)
        {
            await ImportImageShapeAsync(null);
        }

        private async void OnChangeNodeImageClick(object? sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedNode == null) return;
            await ImportImageShapeAsync(_viewModel.SelectedNode);
        }

        private void OnResetRotationClick(object? sender, RoutedEventArgs e)
        {
            _viewModel.ResetSelectedNodeRotation();
            DesignerCanvas.Focus();
        }

        private async Task ImportImageShapeAsync(FlowNode? replaceNode)
        {
            try
            {
                var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = Localizer.Localizer.Instance[replaceNode == null ? "FlowDialogImportImage" : "FlowDialogChangeImage"],
                    AllowMultiple = false,
                    FileTypeFilter = new[] { ImageFileType }
                });
                if (files.Count == 0) return;

                var file = files[0];
                var bytes = await ReadStorageFileBytesAsync(file);
                if (bytes.Length == 0)
                {
                    UIMessage.NotificationError(Localizer.Localizer.Instance["FlowImageEmpty"]);
                    return;
                }

                int pixelWidth;
                int pixelHeight;
                using (var stream = new MemoryStream(bytes))
                using (var bitmap = new Bitmap(stream))
                {
                    pixelWidth = bitmap.PixelSize.Width;
                    pixelHeight = bitmap.PixelSize.Height;
                }

                var scale = Math.Min(240.0 / Math.Max(1, pixelWidth), 170.0 / Math.Max(1, pixelHeight));
                var width = Math.Max(1, Math.Round(pixelWidth * scale, 2));
                var height = Math.Max(1, Math.Round(pixelHeight * scale, 2));
                var base64 = Convert.ToBase64String(bytes);
                var mime = GetImageMime(file.Name);

                if (replaceNode != null)
                {
                    _viewModel.SetSelectedNodeImage(base64, mime, file.Name, width, height);
                }
                else
                {
                    DesignerCanvas.AddImageShape(base64, mime, file.Name, width, height);
                }
                UIMessage.Notification(Localizer.Localizer.Instance["FlowImageImported"]);
                DesignerCanvas.Focus();
            }
            catch (Exception ex)
            {
                UIMessage.NotificationError(string.Format(Localizer.Localizer.Instance["FlowImageImportFailed"], ex.Message));
            }
        }

        private static async Task<byte[]> ReadStorageFileBytesAsync(IStorageFile file)
        {
            await using var stream = await file.OpenReadAsync();
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            return buffer.ToArray();
        }

        private static string GetImageMime(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".bmp" => "image/bmp",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/png"
            };
        }

        private void OnAlignClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Avalonia.Controls.Button { Tag: string tag } &&
                Enum.TryParse<FlowAlignMode>(tag, out var mode))
            {
                DesignerCanvas.AlignSelection(mode);
                DesignerCanvas.Focus();
            }
        }

        private void OnAutoLayoutClick(object? sender, RoutedEventArgs e)
        {
            _viewModel.AutoLayout();
            DesignerCanvas.Focus();
        }

        private void OnDuplicateSelectionClick(object? sender, RoutedEventArgs e)
        {
            if (DesignerCanvas.SelectedNodes.Count == 0)
            {
                _viewModel.StatusText = L("FlowSelectNodesFirst");
                return;
            }
            DesignerCanvas.DuplicateSelection();
            DesignerCanvas.Focus();
        }

        private void OnDeleteSelectionClick(object? sender, RoutedEventArgs e)
        {
            if (DesignerCanvas.SelectedNodes.Count == 0 && DesignerCanvas.SelectedLink == null)
            {
                _viewModel.StatusText = L("FlowSelectDeleteFirst");
                return;
            }
            DesignerCanvas.DeleteSelection();
            DesignerCanvas.Focus();
        }

        private void OnZoomInClick(object? sender, RoutedEventArgs e)
        {
            DesignerCanvas.ZoomIn();
            DesignerCanvas.Focus();
        }

        private void OnZoomOutClick(object? sender, RoutedEventArgs e)
        {
            DesignerCanvas.ZoomOut();
            DesignerCanvas.Focus();
        }

        private void OnResetZoomClick(object? sender, RoutedEventArgs e)
        {
            DesignerCanvas.ResetZoom();
            DesignerCanvas.Focus();
        }

        private void OnFitClick(object? sender, RoutedEventArgs e)
        {
            DesignerCanvas.FitToContent();
            DesignerCanvas.Focus();
        }

        private void OnCenterSelectionClick(object? sender, RoutedEventArgs e)
        {
            DesignerCanvas.CenterOnSelection();
            DesignerCanvas.Focus();
        }

        private void OnTogglePreviewClick(object? sender, RoutedEventArgs e)
        {
            _viewModel.IsPreviewMode = !_viewModel.IsPreviewMode;
            if (!_viewModel.IsPreviewMode) DesignerCanvas.Focus();
        }

        private void OnToggleShapePanelClick(object? sender, RoutedEventArgs e)
        {
            _viewModel.IsShapePanelVisible = !_viewModel.IsShapePanelVisible;
        }

        private void OnToggleInspectorClick(object? sender, RoutedEventArgs e)
        {
            _viewModel.IsInspectorVisible = !_viewModel.IsInspectorVisible;
        }

        private void OnToggleMiniMapClick(object? sender, RoutedEventArgs e)
        {
            _viewModel.ShowMiniMap = !_viewModel.ShowMiniMap;
        }

        private async void OnImportBpmnClick(object? sender, RoutedEventArgs e)
        {
            await ImportBpmnAsync();
        }

        private async void OnImportVisioClick(object? sender, RoutedEventArgs e)
        {
            await ImportVisioAsync();
        }

        private async void OnExportPngClick(object? sender, RoutedEventArgs e)
        {
            await ExportPngAsync();
        }

        private async void OnExportSvgClick(object? sender, RoutedEventArgs e)
        {
            await ExportSvgAsync();
        }

        private async void OnPrintPreviewClick(object? sender, RoutedEventArgs e)
        {
            await ShowPrintPreviewAsync();
        }

        private void OnFillSwatchClick(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Border border || border.DataContext is not string color) return;
            if (!e.GetCurrentPoint(border).Properties.IsLeftButtonPressed) return;
            _viewModel.NodeFill = color;
            e.Handled = true;
        }

        private void OnTextSwatchClick(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Border border || border.DataContext is not string color) return;
            if (!e.GetCurrentPoint(border).Properties.IsLeftButtonPressed) return;
            _viewModel.NodeTextColor = color;
            e.Handled = true;
        }

        private void OnLinkSwatchClick(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Border border || border.DataContext is not string color) return;
            if (!e.GetCurrentPoint(border).Properties.IsLeftButtonPressed) return;
            _viewModel.LinkColor = color;
            e.Handled = true;
        }

        private void OnSelectionDoubleClicked(object? sender, EventArgs e)
        {
            if (!_viewModel.HasNodeSelection) return;
            Dispatcher.UIThread.Post(() =>
            {
                if (!TitleEdit.IsVisible) return;
                TitleEdit.Focus();
                TitleEdit.SelectAll();
            }, DispatcherPriority.Background);
        }

        private async void OnNewClick(object? sender, RoutedEventArgs e)
        {
            if (!await ConfirmDiscardAsync(L("FlowActionNew"))) return;
            _viewModel.NewDocument();
            DesignerCanvas.Focus();
        }

        private async void OnBlankDocumentClick(object? sender, RoutedEventArgs e)
        {
            if (!await ConfirmDiscardAsync(L("FlowActionNewBlank"))) return;
            _viewModel.NewDocument();
            DesignerCanvas.Focus();
        }

        private async void OnLoadSampleClick(object? sender, RoutedEventArgs e)
        {
            if (!await ConfirmDiscardAsync(L("FlowActionLoadSample"))) return;
            _viewModel.LoadSample();
            DesignerCanvas.Focus();
        }

        private async void OnOpenClick(object? sender, RoutedEventArgs e)
        {
            await OpenAsync();
        }

        private async void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            await SaveAsync(false);
        }

        private async void OnSaveAsClick(object? sender, RoutedEventArgs e)
        {
            await SaveAsync(true);
        }

        private async Task OpenAsync()
        {
            if (!await ConfirmDiscardAsync(L("FlowActionOpenOther"))) return;

            var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = L("FlowDialogOpen"),
                AllowMultiple = false,
                FileTypeFilter = new[] { FlowFileType }
            });
            if (files.Count == 0) return;

            try
            {
                var file = files[0];
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var json = await reader.ReadToEndAsync();
                var document = _viewModel.DeserializeDocument(json);
                _viewModel.LoadDocument(document, file.TryGetLocalPath() ?? file.Name);
                _viewModel.StatusText = L("FlowStatusOpened");
                UIMessage.Notification(string.Format(L("FlowNotifyOpened"), document.Title));
                DesignerCanvas.Focus();
            }
            catch (Exception ex)
            {
                UIMessage.NotificationError(string.Format(L("FlowErrorOpen"), ex.Message));
            }
        }

        private async Task<bool> SaveAsync(bool saveAs)
        {
            var json = _viewModel.SerializeDocument();
            var currentPath = _viewModel.CurrentFilePath;

            try
            {
                if (!saveAs && !string.IsNullOrWhiteSpace(currentPath))
                {
                    var directory = Path.GetDirectoryName(currentPath);
                    if (string.IsNullOrWhiteSpace(directory) || Directory.Exists(directory))
                    {
                        await File.WriteAllTextAsync(currentPath, json, new UTF8Encoding(false));
                        _viewModel.MarkSaved(currentPath);
                        UIMessage.Notification(string.Format(L("FlowNotifySaved"), Path.GetFileName(currentPath)));
                        return true;
                    }
                }

                var suggestedName = string.IsNullOrWhiteSpace(_viewModel.DocumentTitle)
                    ? L("FlowUntitled")
                    : _viewModel.DocumentTitle.Trim();
                foreach (var invalid in Path.GetInvalidFileNameChars())
                {
                    suggestedName = suggestedName.Replace(invalid, '_');
                }

                var file = await _topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = L("FlowDialogSave"),
                    SuggestedFileName = suggestedName + ".json",
                    DefaultExtension = "json",
                    ShowOverwritePrompt = true,
                    FileTypeChoices = new[] { FlowFileType }
                });
                if (file == null) return false;

                await using (var stream = await file.OpenWriteAsync())
                {
                    stream.SetLength(0);
                    await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
                    await writer.WriteAsync(json);
                }

                var path = file.TryGetLocalPath();
                _viewModel.MarkSaved(path ?? file.Name);
                UIMessage.Notification(string.Format(L("FlowNotifySaved"), file.Name));
                return true;
            }
            catch (Exception ex)
            {
                UIMessage.NotificationError(string.Format(L("FlowErrorSave"), ex.Message));
                return false;
            }
        }

        private async Task ImportBpmnAsync()
        {
            if (!await ConfirmDiscardAsync(L("FlowActionImportBpmn"))) return;
            var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = L("FlowDialogImportBpmn"),
                AllowMultiple = false,
                FileTypeFilter = new[] { BpmnFileType }
            });
            if (files.Count == 0) return;

            try
            {
                var file = files[0];
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                var xml = await reader.ReadToEndAsync();
                var document = FlowBpmnImporter.Import(xml, file.Name);
                _viewModel.LoadDocument(document, null, false);
                _viewModel.StatusText = string.Format(L("FlowStatusBpmnImported"), document.Nodes.Count, document.Links.Count);
                UIMessage.Notification(string.Format(L("FlowNotifyBpmnImported"), document.Title));
                DesignerCanvas.Focus();
            }
            catch (Exception ex)
            {
                UIMessage.NotificationError(string.Format(L("FlowErrorBpmnImport"), ex.Message));
            }
        }

        private async Task ImportVisioAsync()
        {
            if (!await ConfirmDiscardAsync(L("FlowActionImportVisio"))) return;
            var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = L("FlowDialogImportVisio"),
                AllowMultiple = false,
                FileTypeFilter = new[] { VisioFileType }
            });
            if (files.Count == 0) return;

            try
            {
                var file = files[0];
                await using var stream = await file.OpenReadAsync();
                var document = FlowVisioImporter.Import(stream, file.Name);
                _viewModel.LoadDocument(document, null, false);
                _viewModel.StatusText = string.Format(L("FlowStatusVisioImported"), document.Nodes.Count, document.Links.Count);
                UIMessage.Notification(string.Format(L("FlowNotifyVisioImported"), document.Title));
                DesignerCanvas.Focus();
            }
            catch (Exception ex)
            {
                UIMessage.NotificationError(string.Format(L("FlowErrorVisioImport"), ex.Message));
            }
        }

        private async Task ExportPngAsync()
        {
            try
            {
                var bytes = DesignerCanvas.ExportPng(2);
                var file = await _topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = L("FlowDialogExportPng"),
                    SuggestedFileName = BuildSuggestedFileName("png"),
                    DefaultExtension = "png",
                    ShowOverwritePrompt = true,
                    FileTypeChoices = new[] { PngFileType }
                });
                if (file == null) return;

                await WriteBytesAsync(file, bytes);
                UIMessage.Notification(string.Format(L("FlowNotifyPngExported"), file.Name));
            }
            catch (Exception ex)
            {
                UIMessage.NotificationError(string.Format(L("FlowErrorPngExport"), ex.Message));
            }
        }

        private async Task ExportSvgAsync()
        {
            try
            {
                var svg = FlowSvgExporter.Export(_viewModel.Document);
                var bytes = new UTF8Encoding(false).GetBytes(svg);
                var file = await _topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = L("FlowDialogExportSvg"),
                    SuggestedFileName = BuildSuggestedFileName("svg"),
                    DefaultExtension = "svg",
                    ShowOverwritePrompt = true,
                    FileTypeChoices = new[] { SvgFileType }
                });
                if (file == null) return;

                await WriteBytesAsync(file, bytes);
                UIMessage.Notification(string.Format(L("FlowNotifySvgExported"), file.Name));
            }
            catch (Exception ex)
            {
                UIMessage.NotificationError(string.Format(L("FlowErrorSvgExport"), ex.Message));
            }
        }

        private async Task ShowPrintPreviewAsync()
        {
            if (!OperatingSystem.IsWindows())
            {
                UIMessage.NotificationError(L("FlowPrintUnsupported"));
                return;
            }
            if (_viewModel.Document.Nodes.Count == 0)
            {
                _viewModel.StatusText = L("FlowPrintEmpty");
                return;
            }

            try
            {
                var png = DesignerCanvas.ExportPng(2);
                var preview = new FlowPrintPreview(png, _viewModel.DocumentTitle);
                var window = this.FindLogicalAncestorOfType<Avalonia.Controls.Window>();
                await preview.ShowDialog(window);
            }
            catch (Exception ex)
            {
                UIMessage.NotificationError(string.Format(L("FlowErrorPrintPreview"), ex.Message));
            }
        }

        private string BuildSuggestedFileName(string extension)
        {
            var name = string.IsNullOrWhiteSpace(_viewModel.DocumentTitle)
                ? L("FlowUntitled")
                : _viewModel.DocumentTitle.Trim();
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalid, '_');
            }
            return name + "." + extension;
        }

        private static async Task WriteBytesAsync(IStorageFile file, byte[] bytes)
        {
            var localPath = file.TryGetLocalPath();
            if (!string.IsNullOrWhiteSpace(localPath))
            {
                await File.WriteAllBytesAsync(localPath, bytes);
                return;
            }

            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);
            await stream.WriteAsync(bytes);
        }

        private async Task<bool> ConfirmDiscardAsync(string action)
        {
            if (!_viewModel.IsDirty) return true;
            return await UIMessage.ShowConfirm(
                string.Format(L("FlowUnsavedAction"), action),
                L("FlowUnsavedTitle"),
                DialogStandardButton.No);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled) return;

            if (e.Key == Key.F5)
            {
                _viewModel.IsPreviewMode = !_viewModel.IsPreviewMode;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && _viewModel.IsPreviewMode)
            {
                _viewModel.IsPreviewMode = false;
                DesignerCanvas.Focus();
                e.Handled = true;
                return;
            }

            if (!e.KeyModifiers.HasFlag(KeyModifiers.Control)) return;
            var focusedTextBox =_topLevel.FocusManager?.GetFocusedElement() is Avalonia.Controls.TextBox;
            switch (e.Key)
            {
                case Key.S:
                    _ = SaveAsync(e.KeyModifiers.HasFlag(KeyModifiers.Shift));
                    e.Handled = true;
                    break;
                case Key.O:
                    _ = OpenAsync();
                    e.Handled = true;
                    break;
                case Key.P:
                    _ = ShowPrintPreviewAsync();
                    e.Handled = true;
                    break;
                case Key.N:
                    _ = NewDocumentFromKeyboardAsync();
                    e.Handled = true;
                    break;
                case Key.Z when !focusedTextBox:
                    _viewModel.Undo();
                    e.Handled = true;
                    break;
                case Key.Y when !focusedTextBox:
                    _viewModel.Redo();
                    e.Handled = true;
                    break;
            }
        }

        private async Task NewDocumentFromKeyboardAsync()
        {
            if (!await ConfirmDiscardAsync(L("FlowActionNew"))) return;
            _viewModel.NewDocument();
            DesignerCanvas.Focus();
        }

        private static string L(string key) => Localizer.Localizer.Instance[key];
    }
}
