using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia.Reactive;

namespace OpenEAP.Views;

public partial class MenuView : ReactiveUserControl<ViewModels.MenuViewModel>
{
    public MenuView()
    {
        InitializeComponent();
    }
}