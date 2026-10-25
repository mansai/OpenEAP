using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia.Reactive;

namespace OpenEAP.Views;

public partial class LogView : ReactiveUserControl<ViewModels.LogViewModel>
{
    private ScrollViewer _scrollViewer;
    public LogView()
    {
        InitializeComponent();
    }
}