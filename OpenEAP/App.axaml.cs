using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Localization;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Common.Helper;
using OpenEAP.Common;
using OpenEAP.Helper;
using OpenEAP.Services;
using OpenEAP.ViewModels;
using OpenEAP.Views;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Serilog;
using System;

namespace OpenEAP;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        // ReactiveCommand 的 CanExecute 通知须在 UI 线程派发，否则 Button 订阅在后台线程访问 UI 对象抛异常
        RxSchedulers.MainThreadScheduler = AvaloniaScheduler.Instance;
        this.UseAtomUI(builder =>
        {
            builder.UseUserThemeDirectory();
            builder.UseLanguages(LanguageTags.ZhCN, [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW]);
            builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID, new ThemeConfigBuilder()
             .WithAlgorithms(ThemeAlgorithm.Dark)
             .Build());
            builder.UseAlibabaSansFont();
            builder.UseDesktopControls();
            builder.UseDesktopDataGrid();
        });
        var language = ConfigHelper.Get("language");
        if (string.IsNullOrWhiteSpace(language))
        {
            var localName = System.Globalization.CultureInfo.CurrentUICulture.Name;
            if (localName.StartsWith("zh-"))
            {
                language = "zh-Hans";                
            }
            else
            {
                language = "en-US";
            }
        }
        GlobalHelper.Language = language;
        HomeLocalizationSeed.Ensure();
        FlowChartLocalizationSeed.Ensure();
        if (language == "zh-Hans")
        {
            var uri = new Uri("avares://OpenEAP/Styles/DockStrings.zh.axaml");
            var rd = new ResourceInclude(uri);
            rd.Source = uri;
            this.Resources.MergedDictionaries.Add(rd);
        }
        Localizer.Localizer.Instance.LoadLanguage(language);
    }
    public override void OnFrameworkInitializationCompleted()
    {
        var mainWindowViewModel = new MainViewModel();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            var mainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };

            mainWindow.Closing += (_, _) => mainWindowViewModel.CloseLayout();
            desktopLifetime.Exit += (_, _) => mainWindowViewModel.CloseLayout();
            desktopLifetime.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
    private void TrayIcon_Clicked(object? sender, EventArgs e)
    {
        if (UIMessage.FindViableOwner().WindowState == WindowState.Minimized) {
            UIMessage.FindViableOwner().WindowState = WindowState.Maximized;
        }        
    }
    private void NativeMenuItem_Click(object? sender, System.EventArgs e)
    {
        if (sender is NativeMenuItem nativeMenuItem)
        {
            // UIHelper.OpenView(nativeMenuItem.CommandParameter.ToString());
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // 直接退出
                desktop.Shutdown();
            }
        }
    }
}