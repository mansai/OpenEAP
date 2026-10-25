using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using OpenEAP.Common;
using OpenEAP.Model;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenEAP.ViewModels;

public class DataSourceViewModel : ViewModelBase
{
    private ObservableCollection<DataSourceEntity> itemData = new ObservableCollection<DataSourceEntity>();
    private DataSourceEntity selectedItem;
    private int selectedIndex;
    private AtomUI.Desktop.Controls.DataGrid dataGrid;
    public ObservableCollection<DataSourceEntity> ItemData { get => itemData; set => this.RaiseAndSetIfChanged(ref itemData, value); }

    /// <summary>
    /// 选中的行索引
    /// </summary>
    public int SelectedIndex { get => selectedIndex; set => this.RaiseAndSetIfChanged(ref selectedIndex, value); }
    /// <summary>
    /// 选中的数据
    /// </summary>
    public DataSourceEntity SelectedItem { get => selectedItem; set => this.RaiseAndSetIfChanged(ref selectedItem, value); }
    public string[] DataTypes { get; set; }= System.Enum.GetNames(typeof(FreeSql.DataType));
    public DataSourceViewModel()
    {
        var list= DbHelp.fsql.Select<DataSourceEntity>().ToList();
        ItemData = new ObservableCollection<DataSourceEntity>(list);
    }
    public void DataGrid_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (dataGrid == null)
        {
            dataGrid = e.Source as AtomUI.Desktop.Controls.DataGrid;
        }
    }
    /// <summary>
    /// 新增
    /// </summary>
    public void Add()
    {
        ItemData.Insert(0, new DataSourceEntity());
        SelectedIndex = 0;
        //dataGrid.BeginEdit(new RoutedEventArgs());
    }
    /// <summary>
    /// 保存
    /// </summary>
    public void Save()
    {
        try
        {
            DbHelp.fsql.InsertOrUpdate<DataSourceEntity>().SetSource(ItemData).ExecuteAffrows();
            UIMessage.Notification(UIMessage.SaveOK);
        }
        catch (System.Exception ex)
        {

            UIMessage.ShowError(ex.Message);
        } 
    }
    /// <summary>
    /// 删除
    /// </summary>
    public void Delete()
    {
        ItemData.Remove(SelectedItem);
        DbHelp.fsql.Delete<DataSourceEntity>(SelectedItem).ExecuteAffrows();
        UIMessage.Notification(UIMessage.DeleteOK);
    }
}
