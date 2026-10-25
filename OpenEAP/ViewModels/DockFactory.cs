using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using DocumentFormat.OpenXml.Office2010.Excel;
using OpenEAP.Views;
using Org.BouncyCastle.Asn1.X509;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenEAP.ViewModels
{
    public class DockFactory : Factory
    {
        private int _documentIndex = 1;
        private ToolViewModel _explorerTool;
        private NavigationViewModel _navigationTool;
        LogViewModel output;
        DocumentDock _rootDocumentDock;
        public ISplitViewDock? SplitViewDock { get; private set; }
        public IRootDock _rootDock;

        public override IRootDock CreateLayout()
        {
            var homeView = new HomeView();
            homeView._viewModel = new HomeViewModel();
            homeView.DataContext = homeView._viewModel;
            var homeTitle = Localizer.Localizer.Instance["Home"];
            var homeDocument = new DocumentViewModel() { Id = "Home", Title = homeTitle, CanClose = false, Content = homeView };
            _navigationTool = new NavigationViewModel
            {
                Id = "Navigation",
                Title = "Navigation"
            };
            var documentDock = new DocumentDock
            {
                Id = "Documents",
                Title = "Documents",
                IsCollapsable = false,
                CanCreateDocument = false,
                AllowedDropOperations = DockOperationMask.Fill
            };            

            documentDock.DocumentFactory = () => CreateDocument($"Doc{_documentIndex}", "New document content.");

            var firstDocument = CreateDocument("Welcome", "This document dock only allows tabbed docking (Fill). Splits are blocked by docking restrictions.");

            documentDock.VisibleDockables = CreateList<IDockable>(homeDocument);
            documentDock.ActiveDockable = homeDocument;
            _rootDocumentDock = documentDock;
            _explorerTool = CreateTool("Explorer", "Restricted to left docking or floating.");
            _explorerTool.AllowedDockOperations = DockOperationMask.Left | DockOperationMask.Fill | DockOperationMask.Window;
            var logService = Locator.Current.GetService<ILogService>();
            var errorListTitle = Localizer.Localizer.Instance["ErrorList"];
            output = new LogViewModel(logService) { Id = "log", Title = errorListTitle };
            var outputTool = CreateTool("Output", "Restricted to bottom docking or floating.");
            outputTool.AllowedDockOperations = DockOperationMask.Bottom | DockOperationMask.Fill | DockOperationMask.Window;

            var propertyTool = CreateTool("Inspector", "Restricted to right docking or floating.");
            propertyTool.AllowedDockOperations = DockOperationMask.Right | DockOperationMask.Fill | DockOperationMask.Window;

            var floatOnlyTool = CreateTool("Float Only", "This tool can only be floated.");
            floatOnlyTool.AllowedDockOperations = DockOperationMask.Window;

            var leftToolDock = new ToolDock
            {
                Id = "LeftTools",
                Title = "Left Tools",
                Alignment = Alignment.Left,
                Proportion = 0.1,
                VisibleDockables = CreateList<IDockable>(_explorerTool),
                ActiveDockable = _explorerTool
            };

            var rightToolDock = new ToolDock
            {
                Id = "RightTools",
                Title = "Right Tools",
                Alignment = Alignment.Right,
                Proportion = 0.15,
                VisibleDockables = CreateList<IDockable>(propertyTool),
                ActiveDockable = propertyTool
            };
            var bottomToolDock = new ToolDock
            {
                Id = "BottomTools",
                Title = "Bottom Tools",
                Alignment = Alignment.Bottom,
                Proportion = 0.15,
                VisibleDockables = CreateList<IDockable>(output),
                ActiveDockable = output
            };
            var centerDock = new ProportionalDock
            {
                Orientation = Orientation.Vertical,
                VisibleDockables = CreateList<IDockable>(
                    documentDock,
                    new ProportionalDockSplitter(),
                    bottomToolDock)
            };

            var mainLayout = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = CreateList<IDockable>(
                    centerDock)
            };

            //var rootDock = CreateRootDock();
            //rootDock.IsCollapsable = false;
            //rootDock.ActiveDockable = mainLayout;
            //rootDock.DefaultDockable = mainLayout;
            //rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);
            // Create root dock
            // Create the SplitViewDock
            SplitViewDock = new SplitViewDock
            {
                Id = "SplitViewDock",
                Title = "Main",
                DisplayMode = SplitViewDisplayMode.CompactOverlay,
                IsPaneOpen = false,
                CompactPaneLength = 48,
                OpenPaneLength = 200,
                PanePlacement = SplitViewPanePlacement.Left,
                PaneDockable = _navigationTool,
                ContentDockable = mainLayout
            };
            _rootDock = new RootDock
            {
                Id = "Root",
                Title = "Root",
                IsCollapsable = false,
                VisibleDockables = CreateList<IDockable>(SplitViewDock),
                ActiveDockable = SplitViewDock,
                DefaultDockable = SplitViewDock
            };            
            return _rootDock;
        }
        public void SetTitle()
        {
            foreach (var item in _rootDocumentDock.VisibleDockables)
            {
                item.Title = Localizer.Localizer.Instance[item.Id];
            }
            output.Title = Localizer.Localizer.Instance["ErrorList"];
        }
        // 添加文档的方法
        public void AddDocument(string id, string title, Avalonia.Controls.UserControl control, Icon icon = null)
        {
            var documentDock = _rootDocumentDock;
            if (documentDock != null)
            {

                var document = new DocumentViewModel()
                {
                    Id = id,
                    Title = title,
                    Icon = icon,
                    Content = control
                };
                var doc = documentDock.VisibleDockables.OfType<DocumentViewModel>().FirstOrDefault(d => d.Id == id);
                if (doc != null)
                {
                    documentDock.ActiveDockable = doc;
                }
                else
                {
                    // 使用 Factory 添加文档
                    documentDock.Factory?.AddDockable(documentDock, document);
                    documentDock.ActiveDockable = document;
                }
            }
        }
        private DocumentViewModel CreateDocument(string title, string body)
        {
            var index = _documentIndex++;
            return new DocumentViewModel
            {
                Id = $"doc-{index}",
                Title = title
            };
        }

        private ToolViewModel CreateTool(string title, string description)
        {
            return new ToolViewModel
            {
                Id = $"tool-{title.ToLowerInvariant().Replace(' ', '-')}",
                Title = title,                
            };
        }
        public void RemoveSplitViewDock()
        {
            var splitViewDock = _rootDock.VisibleDockables.FirstOrDefault(t => t.Id == "SplitViewDock");
            if (splitViewDock is SplitViewDock splitView)
            {
                var _mainLayout = new ProportionalDock
                {
                    Orientation = Orientation.Horizontal,
                    VisibleDockables = CreateList<IDockable>(
                   splitView.ContentDockable)
                };
                _rootDock.VisibleDockables = CreateList<IDockable>(_mainLayout);
                _rootDock.ActiveDockable = _mainLayout;
                _rootDock.DefaultDockable = _mainLayout;
                SplitViewDock = splitView;
            }
        }
        public void ShowSplitViewDock()
        {
            ReSplitViewDock();
        }
        public void ReSplitViewDock()
        {
            var splitViewDock = _rootDock.VisibleDockables.FirstOrDefault(t => t.Id == "SplitViewDock");
            if (splitViewDock is SplitViewDock splitView)
            {
                var _mainLayout = new ProportionalDock
                {
                    Orientation = Orientation.Horizontal
                };
                _rootDock.VisibleDockables = CreateList<IDockable>(_mainLayout);
                _rootDock.ActiveDockable = _mainLayout;
                _rootDock.DefaultDockable = _mainLayout;
                SplitViewDock = splitView;
            }
            var homeView = new HomeView();
            homeView._viewModel = new HomeViewModel();
            homeView.DataContext = homeView._viewModel;
            var homeTitle = Localizer.Localizer.Instance["Home"];
            var homeDocument = new DocumentViewModel() { Id = "Home", Title = homeTitle, CanClose = false, Content = homeView };
            var documentDock = new DocumentDock
            {
                Id = "Documents",
                Title = "Documents",
                IsCollapsable = false,
                CanCreateDocument = false,
                AllowedDropOperations = DockOperationMask.Fill
            };
            documentDock.VisibleDockables = CreateList<IDockable>(homeDocument);
            documentDock.ActiveDockable = homeDocument;
            var centerDock = new ProportionalDock
            {
                Orientation = Orientation.Vertical,
                VisibleDockables = CreateList<IDockable>(
                    documentDock)
            };
            _rootDocumentDock = documentDock;
            _navigationTool = new NavigationViewModel
            {
                Id = "Navigation",
                Title = "Navigation"
            };            
            var mainLayout = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = CreateList<IDockable>(
                   centerDock)
            };
            SplitViewDock = new SplitViewDock
            {
                Id = "SplitViewDock",
                Title = "Main",
                DisplayMode = SplitViewDisplayMode.CompactOverlay,
                IsPaneOpen = false,
                CompactPaneLength = 48,
                OpenPaneLength = 200,
                PanePlacement = SplitViewPanePlacement.Left,
                PaneDockable = _navigationTool,
                ContentDockable = mainLayout
            };
            _rootDock.VisibleDockables = CreateList<IDockable>(SplitViewDock);
            _rootDock.ActiveDockable = SplitViewDock;
            _rootDock.DefaultDockable = SplitViewDock;
        }
        public void ShowLeftTool()
        {
            var leftToolDock = new ToolDock
            {
                Id = "LeftTools",
                Title = "Left Tools",
                Alignment = Alignment.Left,
                Proportion = 0.1,
                VisibleDockables = CreateList<IDockable>(_explorerTool),
                ActiveDockable = _explorerTool
            };
            var _mainLayout = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = CreateList<IDockable>(
                    leftToolDock, new ProportionalDockSplitter(),
                   SplitViewDock.ContentDockable)
            };
            _rootDock.ActiveDockable = _mainLayout;
            _rootDock.DefaultDockable = _mainLayout;
        }
        public void MenuCollapsed()
        {
           
            //_explorerTool.IsInlineCollapsed = !_explorerTool.IsInlineCollapsed;
            _navigationTool.IsInlineCollapsed = !_navigationTool.IsInlineCollapsed;
            SplitViewDock.IsPaneOpen = !SplitViewDock.IsPaneOpen;           
        }
        public void MenuCollapse()
        {
            if (SplitViewDock.IsPaneOpen)
            {
                _navigationTool.IsInlineCollapsed = !_navigationTool.IsInlineCollapsed;
                SplitViewDock.IsPaneOpen = !SplitViewDock.IsPaneOpen;
            }
        }
    }
}
