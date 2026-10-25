using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia.Reactive;

namespace OpenEAP.Views;

public partial class LanguageView : ReactiveUserControl<ViewModels.LanguageViewModel>
{
    public LanguageView()
    {
        InitializeComponent();
    }
}