using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using DocumentFormat.OpenXml.Presentation;
using OpenEAP.ViewModels;
using System.IO;
using System.Threading.Tasks;

namespace OpenEAP.Views;

public partial class MediaPlayer :Window
{
    public MediaPlayer()
    {
        InitializeComponent();
        mediaPlayerControl.DoubleTapped += MediaPlayerControl_DoubleTapped;


    }

    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        Open();
    }

    private void MediaPlayerControl_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        Open();
    }
    public async void Open()
    {
        // 获取当前窗口的 TopLevel
        var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        // 配置文件选择器选项
        var options = new FilePickerOpenOptions
        {
            Title = "选择媒体文件",
            AllowMultiple = false, // 是否允许多选
            FileTypeFilter = new[]
            {
                new FilePickerFileType("媒体文件")
                {
                    Patterns = new[] { "*.mp4","*.avi", "*.mp3","*.wav" }
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
            var stream = await files[0].OpenReadAsync();
            mediaPlayerControl.Source = new StreamSource(stream);
        }
    }
}