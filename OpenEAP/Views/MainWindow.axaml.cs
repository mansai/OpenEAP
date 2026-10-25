using Avalonia.Controls;
using OpenEAP.ViewModels;

namespace OpenEAP.Views;

public partial class MainWindow : AtomUI.Desktop.Controls.ReactiveWindow<MainViewModel>
{
    public MainWindow()
    {
        InitializeComponent();  // 监听窗口关闭事件
        this.Closing += OnWindowClosing;
    }
    private void OnWindowClosing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        // 取消默认的关闭行为
        e.Cancel = true;
        // 最小化窗口
        this.WindowState = Avalonia.Controls.WindowState.Minimized;
    }
}