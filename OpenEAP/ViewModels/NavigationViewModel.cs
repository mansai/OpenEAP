using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Dock.Model.ReactiveUI.Controls;
using Masuit.Tools.Extension.Object;
using OpenEAP.Common;
using OpenEAP.Helper;
using ReactiveUI.Reactive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace OpenEAP.ViewModels
{
    public class NavigationViewModel : Tool
    {
        private bool _isInlineCollapsed = true;

        public bool IsInlineCollapsed { get => _isInlineCollapsed; set => this.RaiseAndSetIfChanged(ref _isInlineCollapsed, value); }
        private ObservableCollection<INavMenuNode> menuItems = new();

        public ObservableCollection<INavMenuNode> MenuItems { get => menuItems; set => this.RaiseAndSetIfChanged(ref menuItems, value); }
        public ObservableCollection<INavMenuNode> SourceMenuItems { get; set; } = new();

        public NavigationViewModel()
        {
            MenuHelper menuHelper = new MenuHelper();
            menuHelper._tool = this;
            MenuItems = menuHelper.LoadMenu();
            SourceMenuItems = MenuItems.DeepClone();
            Localizer.Localizer.Instance.PropertyChanged += (s, e) =>
            {
                // 根据你的 Localizer 实现判断何时刷新（例如 culture 改变或索引器变化）
                menuHelper.LoadMenu();
                SourceMenuItems = MenuItems.DeepClone();
            };

        }
    }
}
