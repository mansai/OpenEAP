using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using OpenEAP.Common;
using OpenEAP.ViewModels;
using System.IO;
using System.Threading.Tasks;

namespace OpenEAP.Views;

public partial class PdfViewer :Window
{
    public PdfViewer()
    {
        InitializeComponent();        
    }

    private async void ButtonOpen_Click(object? sender, RoutedEventArgs e)
    {
        // 获取当前窗口的 TopLevel
        var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        // 配置文件选择器选项
        var options = new FilePickerOpenOptions
        {
            Title = "选择PDF文件",
            AllowMultiple = false, // 是否允许多选
            FileTypeFilter = new[]
            {
                new FilePickerFileType("PDF")
                {
                    Patterns = new[] { "*.pdf", "*.doc" }
                },
                new FilePickerFileType("所有文件")
                {
                    Patterns = new[] { "*" }
                }
            }
        };
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        if (files.Count >= 1)
        {
            string filePath = files[0].Path.LocalPath;
            Viewer.DocumentSource = filePath;
            Save.IsEnabled=true;
        }
    }
    private async void ButtonSave_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "保存pdf文件",
            SuggestedFileName = "pdf.pdf",          // 建议的文件名
            DefaultExtension = "pdf",                  // 默认扩展名
            ShowOverwritePrompt = true,                // 覆盖前提示
            FileTypeChoices = new[]
            {
                new FilePickerFileType("pdf文件") { Patterns = new[] { "*.pdf" } },
                FilePickerFileTypes.All                // 内置的“所有文件”类型
            }
        });
        if (file is null) return;

        await using var stream = await file.OpenWriteAsync();
        await Viewer.SaveDocumentAsync(stream);
        UIMessage.Notification(UIMessage.SaveOK);
    }
}