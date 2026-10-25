using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Localization;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using DynamicData;
using OpenEAP.Common;
using OpenEAP.Helper;
using OpenEAP.Views;
using ReactiveUI.Reactive;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace OpenEAP.ViewModels;

public class MainViewModel : ReactiveObject
{
    private bool drawerOpen;
    public bool DrawerOpen { get => drawerOpen; set => this.RaiseAndSetIfChanged(ref drawerOpen, value); }
    public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
    private readonly ILanguageManager _languageManager;
    private readonly IThemeManager _themeManager;
    private readonly DockWorkspaceManager _workspaceManager;
    private readonly DockFactory _factory;
    private IRootDock? _layout;
    private bool _isDockingEnabled = true;
    private bool _isWorkspaceDirty;
    private string _workspaceStatus = "Workspace: Unsaved";
    private DockWorkspace? _defaultWorkspace;
    private DockWorkspace? _workspaceA;
    private DockWorkspace? _workspaceB;
    private bool isStatusBar = false;
    private bool isShowSearch = false;

    public bool IsShowStatusBar { get => isStatusBar; set => this.RaiseAndSetIfChanged(ref isStatusBar, value); }
    public bool IsShowSearch { get => isShowSearch; set => this.RaiseAndSetIfChanged(ref isShowSearch, value); }
    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public IFactory Factory { get; }

    public bool IsDockingEnabled
    {
        get => _isDockingEnabled;
        set => this.RaiseAndSetIfChanged(ref _isDockingEnabled, value);
    }

    public bool IsWorkspaceDirty
    {
        get => _isWorkspaceDirty;
        set => this.RaiseAndSetIfChanged(ref _isWorkspaceDirty, value);
    }

    public string WorkspaceStatus
    {
        get => _workspaceStatus;
        set => this.RaiseAndSetIfChanged(ref _workspaceStatus, value);
    }

    public ReactiveCommand<Unit, Unit> SaveWorkspaceA { get; }
    public ReactiveCommand<Unit, Unit> LoadWorkspaceA { get; }
    public ReactiveCommand<Unit, Unit> ResetLayout { get; }
    public string StatusMessage { get; } = "All changes saved";
    ILogger _logger => Log.ForContext<MainViewModel>();
    public MainViewModel()
    {
        _languageManager = Avalonia.Application.Current?.GetLanguageManager();
        _themeManager = Avalonia.Application.Current?.GetThemeManager();
        _factory = new DockFactory();
        Factory = _factory;
        _workspaceManager = new DockWorkspaceManager(new Dock.Serializer.DockSerializer());
        _workspaceManager.TrackFactory(_factory);
        _workspaceManager.WorkspaceDirtyChanged += (_, _) => UpdateWorkspaceState();
        SaveWorkspaceA = ReactiveCommand.Create(() =>
        {
            _workspaceA = SaveWorkspace("A");
        });
        LoadWorkspaceA = ReactiveCommand.Create(() => LoadWorkspace(_workspaceA));
        ResetLayout = ReactiveCommand.Create(ResetLayoutImpl);

        Layout = _factory.CreateLayout();
       _defaultWorkspace = SaveWorkspace("Default", "Default");
        UpdateWorkspaceState();
        Localizer.Localizer.Instance.PropertyChanged += (s, e) =>
        {
            _factory.SetTitle();
        };
        Log.Information($"显示到前端");
        Log.Error($"显示到前端错误类型");
        _logger.Information("不显示到前端");
    }
    public async Task Theme(string newTheme)
    {
        List<ThemeAlgorithm> themeAlgorithm = new List<ThemeAlgorithm>();
        var currentState = _themeManager.CurrentTheme;
        if (currentState.Algorithms.First().ToString() != newTheme)
        {
            switch (newTheme)
            {
                case "DaybreakBlue":
                    themeAlgorithm.Add(ThemeAlgorithm.Default);
                    if (currentState.Algorithms.Any(t => t == ThemeAlgorithm.Compact))
                    {
                        themeAlgorithm.Add(ThemeAlgorithm.Compact);
                    }
                    break;
                case "Dark":
                    themeAlgorithm.Add(ThemeAlgorithm.Dark);
                    if (currentState.Algorithms.Any(t => t == ThemeAlgorithm.Compact))
                    {
                        themeAlgorithm.Add(ThemeAlgorithm.Compact);
                    }
                    break;
                case "Compact":
                    themeAlgorithm.Add(currentState.Algorithms.First());
                    themeAlgorithm.Add(ThemeAlgorithm.Compact);
                    break;
                case "Normal":
                    themeAlgorithm.Add(currentState.Algorithms.First());
                    break;
            }
            var themeRequest = new ThemeRequest(
         IThemeManager.DEFAULT_THEME_ID,
         new ThemeConfigBuilder()
             .WithAlgorithms(themeAlgorithm.ToArray())
             .Build(),
         ThemeTransitionReason.UserRequest
     );
            await _themeManager.ApplyThemeAsync(themeRequest);
        }

    }
    public void LineEdit_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is LineEdit lineEdit)
        {
            if (string.IsNullOrWhiteSpace(lineEdit.Text))
            {

            }
            else
            {

            }
        }
    }
    public void Language(string newLanguage)
    {
        if (Localizer.Localizer.Instance.Language != newLanguage)
        {
            ConfigHelper.Set("language", newLanguage);
            GlobalHelper.Language = newLanguage;
            Localizer.Localizer.Instance.LoadLanguage(newLanguage);
        }
        Dispatcher.UIThread.Post(() =>
        {
            if (newLanguage == "zh-Hans")
            {
                _languageManager.ChangeLanguage(LanguageTags.ZhCN);
                var uri = new Uri("avares://OpenEAP/Styles/DockStrings.zh.axaml");
                var rd = new ResourceInclude(uri);
                rd.Source = uri;
                Application.Current?.Resources.MergedDictionaries.Add(rd);
            }
            else
            {
                _languageManager.ChangeLanguage(LanguageTags.EnUS);
                var include = Application.Current?.Resources.MergedDictionaries.OfType<ResourceInclude>().FirstOrDefault(i => i.Source != null && i.Source.ToString().Contains("DockStrings.zh.axaml"));
                if (include != null)
                {
                    Application.Current?.Resources.MergedDictionaries.Remove(include);
                }
            }
        });
    }
    public void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 直接退出
            desktop.Shutdown();
        }
    }
    public void MediaPlayer()
    {
        new Views.MediaPlayer().Show();
    }
    public void PdfViewer()
    {
        new Views.PdfViewer().Show();
    }
    public void Excel()
    {
        new Views.Excel().Show();
    }

    private DockWorkspace? SaveWorkspace(string id, string? name = null)
    {
        if (Layout is not IDock layout)
        {
            return null;
        }

        var workspaceName = string.IsNullOrWhiteSpace(name) ? $"Workspace {id}" : name;
        var workspace = _workspaceManager.Capture(id, layout, includeState: true, name: workspaceName);
        UpdateWorkspaceState();
        return workspace;
    }

    private void LoadWorkspace(DockWorkspace? workspace)
    {
        if (workspace is null)
        {
            return;
        }
        var restored = _workspaceManager.Restore(workspace);
        if (restored is IRootDock root)
        {
            Layout = root;
            _factory._rootDock = root;
            _factory.ReSplitViewDock();
        }
        UpdateWorkspaceState();
    }

    private void ResetLayoutImpl()
    {
        //if (_defaultWorkspace is not null)
        //{
        //    LoadWorkspace(_defaultWorkspace);
        //    return;
        //}
        Layout = _factory.CreateLayout();
        _workspaceManager.MarkClean();
        UpdateWorkspaceState();
    }

    private void UpdateWorkspaceState()
    {
        var workspace = _workspaceManager.ActiveWorkspace;
        var label = workspace?.Name ?? workspace?.Id ?? "Unsaved";
        var status = _workspaceManager.IsDirty ? $"Workspace: {label} *" : $"Workspace: {label}";

        IsWorkspaceDirty = _workspaceManager.IsDirty;
        WorkspaceStatus = status;
    }
    public async Task OpenLayout()
    {

        var storageProvider = UIMessage.FindViableOwner()?.StorageProvider;
        if (storageProvider is null)
        {
            return;
        }

        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open layout",
            FileTypeFilter = [new FilePickerFileType("Json") { Patterns = ["*.json"] }, new FilePickerFileType("All") { Patterns = ["*.*"] }],
            AllowMultiple = false
        });

        var file = result.FirstOrDefault();

        if (file is not null)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                stream.Position = 0;
                string json = await reader.ReadToEndAsync();
                var dockWorkspace = new DockWorkspace("a", json, new DockState());
                LoadWorkspace(dockWorkspace);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public async Task SaveLayout()
    {

        var storageProvider = UIMessage.FindViableOwner()?.StorageProvider;
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save layout",
            FileTypeChoices = [new FilePickerFileType("Json") { Patterns = ["*.json"] }, new FilePickerFileType("All") { Patterns = ["*.*"] }],
            SuggestedFileName = "layout",
            DefaultExtension = "json",
            ShowOverwritePrompt = true
        });

        if (file is not null)
        {
            try
            {
                await using var stream = await file.OpenWriteAsync();
                byte[] bytes = Encoding.UTF8.GetBytes(_workspaceA.Layout);
                await stream.WriteAsync(bytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
    public void Setting()
    {
        DrawerOpen = true;
    }
    public void MenuSetting(NavMenuNode navMenuNode)
    {
        switch (navMenuNode.ItemKey.Value.Value)
        {
            case "Menu":
                var menuTitle = Localizer.Localizer.Instance["CustomMenu"];
                var view = new Views.MenuView();
                view.DataContext = new ViewModels.MenuViewModel();
                _factory.AddDocument("CustomMenu", menuTitle, view);
                break;
            case "LanguageView":
                var languageView = new Views.LanguageView();
                var languageTitle = Localizer.Localizer.Instance["CustomLanguage"];
                languageView.DataContext = new ViewModels.LanguageViewModel();
                _factory.AddDocument("CustomLanguage", languageTitle, languageView);
                break;
            default:
                break;
        }
        DrawerOpen = false;
    }
    public void RemoveSplitViewDock()
    {
        _factory.RemoveSplitViewDock();
    }
    public void ShowSplitViewDock()
    {
        _factory.ShowSplitViewDock();
    }
    public void ShowLeftTool()
    {
        _factory.ShowLeftTool();
    }
    public void ToggleInlineCollapsed()
    {
        _factory.MenuCollapsed();
    }
    public void CloseLayout()
    {
        if (Layout is IDock dock && dock.Close.CanExecute(null))
        {
            dock.Close.Execute(null);
        }
    }
    public void StatusBar()
    {
        IsShowStatusBar = !IsShowStatusBar;
    }
    public void ShowSearch()
    {
        IsShowSearch = !IsShowSearch;
    }
}
