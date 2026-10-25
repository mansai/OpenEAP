using AtomUI.Desktop.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Masuit.Tools;
using Masuit.Tools.Extension.Object;
using OpenEAP.Common;
using OpenEAP.Model;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Yitter.IdGenerator;

namespace OpenEAP.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private ObservableCollection<MenuEntity> menuEntity;
        private int selectedIndex = -1;
        public ObservableCollection<MenuEntity> MenuEntity { get => menuEntity; set => this.RaiseAndSetIfChanged(ref menuEntity, value); }
        /// <summary>
        /// 选中的行索引
        /// </summary>
        public int SelectedIndex { get => selectedIndex; set => this.RaiseAndSetIfChanged(ref selectedIndex, value); }
        public ToolViewModel Tool { get; set; }

        public MenuViewModel()
        {
            var menus = DbHelp.fsql.Select<MenuEntity>().OrderBy(x => x.Sort).ToList();
            MenuEntity = new ObservableCollection<MenuEntity>(menus);
            // 订阅消息
            //MessageBus.Current.Listen<ToolViewModelMessage>()
            //    .Subscribe(msg =>
            //    {
            //        Tool = msg.ToolViewModel;
            //    });
        }

        public void Delete_Confirmed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (selectedIndex == -1)
            {
                UIMessage.NotificationError(UIMessage.SelectRowError);
            }
            else
            {

                Delete();
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        public void Add()
        {
            var addMenu = new MenuEntity();
            addMenu.ItemType = "menu";
            MenuEntity.Insert(0, addMenu);
            SelectedIndex = 0;
            //dataGrid.BeginEdit(new RoutedEventArgs());
        }

        public void DataGrid_RowReordering(object? sender, DataGridRowReorderingEventArgs e)
        {
            var grid = e.Row.FindAncestorOfType<DataGrid>();
            if (grid != null)
            {
                grid.CommitEdit();
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async void Save()
        {
            var index = 0;
            foreach (var item in MenuEntity)
            {
                index++;
                item.Sort = index;
            }
            DbHelp.fsql.InsertOrUpdate<MenuEntity>().SetSource(MenuEntity).ExecuteAffrows();
            if (Tool != null)
            {
                //Tool.LoadMenu();
            }
            UIMessage.Notification(UIMessage.SaveOK);
        }
        public async void Copy()
        {
            var selectedItem = MenuEntity[selectedIndex].DeepClone();
            selectedItem.Id = YitIdHelper.NextId();
            selectedItem.Name = "";
            MenuEntity.Insert(selectedIndex, selectedItem);
            DbHelp.fsql.InsertOrUpdate<MenuEntity>().SetSource(MenuEntity).ExecuteAffrows();
            if (Tool != null)
            {
              //  Tool.LoadMenu();
            }
            UIMessage.Notification(UIMessage.CopyOK);
        }
        /// <summary>
        /// 删除
        /// </summary>
        public void Delete()
        {
            var selectedItem = MenuEntity[selectedIndex];
            MenuEntity.Remove(selectedItem);
            DbHelp.fsql.Delete<MenuEntity>().Where(t => t.Id == selectedItem.Id).ExecuteAffrows();
            if (Tool != null)
            {
                //Tool.LoadMenu();
            }
            UIMessage.Notification(UIMessage.DeleteOK);
        }
    }
}
