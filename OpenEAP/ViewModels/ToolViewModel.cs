using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Masuit.Tools.Extension.Object;
using OpenEAP.Common;
using OpenEAP.Helper;
using OpenEAP.Model;
using OpenEAP.Views;
using ReactiveUI.Reactive;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenEAP.ViewModels
{
    public class ToolViewModel : Tool
    {
        private bool _isInlineCollapsed;
        public bool IsInlineCollapsed { get => _isInlineCollapsed; set => this.RaiseAndSetIfChanged(ref _isInlineCollapsed, value); }
        private ObservableCollection<INavMenuNode> menuItems = new();

        public ObservableCollection<INavMenuNode> MenuItems { get => menuItems; set => this.RaiseAndSetIfChanged(ref menuItems, value); }
        public ObservableCollection<INavMenuNode> SourceMenuItems { get; set; } = new();
        public ToolViewModel()
        {
            MenuHelper menuHelper = new MenuHelper();
            menuHelper._tool = this;
            MenuItems = menuHelper.LoadMenu();
            SourceMenuItems = MenuItems.DeepClone();
            // LoadMenu();
             var icon2 = (Icon?)new AtomUI.Icons.AntDesign.LaptopOutlined();
            Localizer.Localizer.Instance.PropertyChanged += (s, e) =>
            {
                // 根据你的 Localizer 实现判断何时刷新（例如 culture 改变或索引器变化）
                MenuItems = menuHelper.LoadMenu();
                SourceMenuItems = MenuItems.DeepClone();
            };
        }
        public bool IsExpanded { get; set; } = true; // true为展开
        public double MenuWidth => IsExpanded ? 180 : 50;
        public string CollapseButtonContent => IsExpanded ? "<<" : ">>";
        public void LoadMenu()
        {
            var menuList = DbHelp.fsql.Select<Model.MenuEntity>().WhereDynamic(GlobalHelper.WhereEnable).Where(t => t.ItemType == "menu" && t.Enable).OrderBy(t => t.Sort).ToList();

            List<INavMenuNode> menuTree = new List<INavMenuNode>();
            var menu = BulidTreeByRecursive(menuList, menuTree, null);
            MenuItems = new ObservableCollection<INavMenuNode>(menu);
            SourceMenuItems = MenuItems.DeepClone();
        }
        public IList<INavMenuNode> BulidTreeByRecursive(IEnumerable<MenuEntity> treeNodes, List<INavMenuNode> resps, long? pId)
        {
            //通过权限ID和菜单Id获取
            resps = new();
            var tempList = treeNodes.Where(t => t.ParentId.Equals(pId)).OrderBy(t => t.Sort).ToList();
            foreach (var item in tempList)
            {
                var node = new NavMenuNode
                {
                    Header = Localizer.Localizer.Instance[item.LanguageKey],
                    //IsVisible = true,
                    ItemKey = item.Id.ToString(),
                    Icon = string.IsNullOrEmpty(item.Icon) ? null : GetIcon(item.Icon),
                    Children = BulidTreeByRecursive(treeNodes, resps, item.Id),
                };
                node.Command = new RelayCommand(() =>
                {
                    // 点击时执行的逻辑
                    Node_NavMenuItemClick(item, node);
                });
                resps.Add(node);
            }
            return resps;
        }

        private void Node_NavMenuItemClick(MenuEntity menuEntity, NavMenuNode navMenuNode)
        {
            var name = typeof(MainWindow).Assembly.GetName().Name;
            var type = typeof(MainWindow).Assembly.GetType($"{menuEntity.Namespace}.{menuEntity.Form}");
            if (navMenuNode.Children.Count == 0 && type != null && base.Factory is DockFactory factory)
            {
                var obj = Activator.CreateInstance(type);
                if (obj != null)
                {
                    UserControl form = obj as UserControl;
                    var baseType = type.BaseType;
                    if (baseType != null && baseType.IsGenericType)
                    {
                        var vmType = baseType.GetGenericArguments()[0];
                        form.DataContext = Activator.CreateInstance(vmType);
                    }
                    var title = Localizer.Localizer.Instance[menuEntity.LanguageKey];
                    factory.AddDocument(menuEntity.LanguageKey, title, form, GetIcon(menuEntity.Icon));
                }
            }
        }

        //public void Filter(string query)
        //{
        //    // 如果没有菜单或没有过滤条件则重置全部显示
        //    if (MenuItems == null) return;

        //    if (string.IsNullOrWhiteSpace(query))
        //    {
        //        foreach (var root in MenuItems)
        //            ResetNodeState(root);
        //        return;
        //    }

        //    var q = query.Trim();

        //    foreach (var root in MenuItems)
        //    {
        //        // 对每个根节点运行过滤，FilterNode 会设置 Visibility / IsSubMenuOpen
        //        FilterNode(root, q);
        //    }
        //}

        /// <summary>
        /// 递归过滤单个节点：若节点或任一子孙匹配 query，则返回 true 并将该节点设为可见；否则返回 false 并隐藏该节点。
        /// 不会修改 ItemsSource 集合或把控件移动到其它父节点，因此不会触发 "already has a parent" 异常。
        /// </summary>
        private bool FilterNode(INavMenuItem node, string query)
        {
            if (node == null) return false;

            // 先递归处理子节点
            var children = node.SubItems?.OfType<INavMenuItem>().ToList() ?? new List<INavMenuItem>();
            bool anyChildMatches = false;
            foreach (var child in children)
            {
                if (FilterNode(child, query))
                    anyChildMatches = true;
            }

            // 判断自身是否匹配（Header 可能为 object，先判空）
            bool selfMatches = false;
            if (node.Node.Header != null)
            {
                var headerText = node.Node.Header.ToString();
                if (!string.IsNullOrEmpty(headerText))
                    selfMatches = headerText.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            bool keep = selfMatches || anyChildMatches;

            // 修改可见性与展开状态（不改变父子关系，不会导致 parent 错误）

            // 如果有子节点匹配，展开父节点以便用户看到匹配的子节点
            node.IsSubMenuOpen = anyChildMatches;

            return keep;
        }

        /// <summary>
        /// 过滤为空时重置节点状态：显示节点并收起/展开子菜单（根据需要调整）。
        /// </summary>
        //private void ResetNodeState(NavMenuItem node)
        //{
        //    if (node == null) return;

        //    node.IsVisible = true;

        //    // 重置展开状态：这里选择收起，若你希望默认全部展开可以改为 true
        //    node.IsSubMenuOpen = false;

        //    var children = node.ItemsSource?.OfType<NavMenuItem>().ToList() ?? new List<NavMenuItem>();
        //    foreach (var child in children)
        //        ResetNodeState(child);
        //}

        //private void Node_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        //{
        //    if (sender is NavMenuItem item)
        //    {
        //        if (item.ItemCount == 0 && item.Tag is MenuEntity menuEntity)
        //        {
        //            var type = typeof(MainWindow).Assembly.GetType($"{menuEntity.Namespace}.{menuEntity.Form}");
        //            if (type != null)
        //            {
        //                var obj = Activator.CreateInstance(type);
        //                if (obj != null)
        //                {
        //                    UserControl form = obj as UserControl;
        //                    var baseType = type.BaseType;
        //                    if (baseType != null && baseType.IsGenericType)
        //                    {
        //                        var vmType = baseType.GetGenericArguments()[0];
        //                        form.DataContext = Activator.CreateInstance(vmType);
        //                    }
        //                    var title = Localizer.Localizer.Instance[menuEntity.LanguageKey];
        //                    if (menuEntity.Form == "MenuView")
        //                    {
        //                        MessageBus.Current.SendMessage(new ToolViewModelMessage { ToolViewModel = this });
        //                    }
        //                    _dock.AddDocument(menuEntity.LanguageKey, title, form, item.Icon);
        //                }
        //            }
        //        }
        //    }
        //}
        public Icon? GetIcon(string kind)
        {
            try
            {
                var fullTypeName = $"AtomUI.Icons.AntDesign.{kind}";
                var type = typeof(AtomUI.Icons.AntDesign.AntDesignIconKind).Assembly.GetType(fullTypeName)
                            ?? Assembly.GetExecutingAssembly().GetType(fullTypeName);

                if (type == null)
                {
                    throw new InvalidOperationException($"{fullTypeName} not exist");
                }

                var icon = (Icon?)Activator.CreateInstance(type);
                Debug.Assert(icon != null);
                return icon;
            }
            catch (Exception ex)
            {
                Log.Error($"create icon {kind} failed");
                return null;
                //throw new InvalidOperationException($"create icon {kind} failed", ex);
            }
        }
    }
}
