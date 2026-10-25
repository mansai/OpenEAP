using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.ReactiveUI.Controls;
using Masuit.Tools.Extension.Object;
using OpenEAP.Helper;
using OpenEAP.Model;
using OpenEAP.ViewModels;
using OpenEAP.Views;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenEAP.Common
{
    public class MenuHelper
    {
        public Tool _tool;
        public ObservableCollection<INavMenuNode> LoadMenu()
        {
            var menuList = DbHelp.fsql.Select<Model.MenuEntity>().WhereDynamic(GlobalHelper.WhereEnable).Where(t => t.ItemType == "menu" && t.Enable).OrderBy(t => t.Sort).ToList();

            List<INavMenuNode> menuTree = new List<INavMenuNode>();
            var menu = BulidTreeByRecursive(menuList, menuTree, null);
            ObservableCollection<INavMenuNode> MenuItems = new ObservableCollection<INavMenuNode>(menu);
            return MenuItems;
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

            var name = $"{typeof(MainWindow).Assembly.GetName().Name}.Views";;
            var type = typeof(MainWindow).Assembly.GetType($"{menuEntity.Namespace?? name}.{menuEntity.Form}");            
            if (navMenuNode.Children.Count == 0 && type != null && _tool != null && _tool.Factory is DockFactory _factory)
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
                    if (navMenuNode.Icon is Icon icon)
                    {

                    }
                    _factory.AddDocument(menuEntity.LanguageKey, title, form, GetIcon(menuEntity.Icon));
                    if (_tool is NavigationViewModel navigationViewModel)
                    {
                        _factory.MenuCollapse();
                    }
                }
            }
        }
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
