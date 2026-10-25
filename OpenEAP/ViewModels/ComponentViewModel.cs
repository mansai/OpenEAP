using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Masuit.Tools;
using Masuit.Tools.Extension.Object;
using OpenEAP.Common;
using OpenEAP.Helper;
using OpenEAP.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Windows.Input;
using Yitter.IdGenerator;

namespace OpenEAP.ViewModels;

public class ComponentViewModel : ViewModelBase
{
    private ObservableCollection<ComponentEntity> componentEntity;
    private ObservableCollection<ComponentPropertyEntity> componentPropertyEntity;

    private int componentEntitySelectedIndex;
    private AtomUI.Desktop.Controls.DataGrid componentDataGrid;
    private AtomUI.Desktop.Controls.DataGrid componentPropertyDataGrid;
    private GridLength leftPanelWidth;
    private GridLength rightPanelWidth;
    public GridLength LeftPanelWidth { get => leftPanelWidth; set => this.RaiseAndSetIfChanged(ref leftPanelWidth, value); }
    public GridLength RightPanelWidth { get => rightPanelWidth; set => this.RaiseAndSetIfChanged(ref rightPanelWidth, value); }
    public ObservableCollection<string> AutoCompleteBoxItemsSource { get => autoCompleteBoxItemsSource; set => this.RaiseAndSetIfChanged(ref autoCompleteBoxItemsSource, value); }
    public ObservableCollection<ComponentEntity> Component { get => componentEntity; set => this.RaiseAndSetIfChanged(ref componentEntity, value); }
    public ObservableCollection<ComponentPropertyEntity> ComponentPropertyEntity { get => componentPropertyEntity; set => this.RaiseAndSetIfChanged(ref componentPropertyEntity, value); }
    public int ComponentSelectedIndex { get => componentEntitySelectedIndex; set => this.RaiseAndSetIfChanged(ref componentEntitySelectedIndex, value); }
    public int ComponentPropertySelectedIndex { get => componentPropertySelectedIndex; set => this.RaiseAndSetIfChanged(ref componentPropertySelectedIndex, value); }
    private ObservableCollection<ComponentEntity> ComponentSource;
    public ComponentEntity SelectedItem { get => selectedItem; set => this.RaiseAndSetIfChanged(ref selectedItem, value); }
    public ComponentViewModel()
    {
        //string[] arr = GetSplitterValue(nameof(ComponentView));
        //if (arr.Length == 0)
        //{
        //    LeftPanelWidth = new GridLength(6, GridUnitType.Star);
        //    RightPanelWidth = new GridLength(4, GridUnitType.Star);
        //}
        //else
        //{
        //    LeftPanelWidth = GridLength.Parse(arr[0]);
        //    RightPanelWidth = GridLength.Parse(arr[1]);
        //}
        // 获取当前NameScope
        //var nameScope = NameScope.GetNameScope(this);
        var components = DbHelp.fsql.Select<ComponentEntity>().ToList();
        //AutoCompleteBoxItemsSource = new ObservableCollection<string>(enums.Select(t => t.Code).ToList());
        componentEntity = new ObservableCollection<ComponentEntity>(components);
        var componentPropertys = DbHelp.fsql.Select<ComponentPropertyEntity>().Where(t => t.ComponentId == components.First().Id).ToList();
        ComponentPropertyEntity = new ObservableCollection<ComponentPropertyEntity>(componentPropertys);
        ComponentSource = componentEntity.DeepClone();


    }
    public void Component_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (componentDataGrid == null)
        {
            componentDataGrid = e.Source as AtomUI.Desktop.Controls.DataGrid;
        }
    }
    public void ComponentProperty_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (componentPropertyDataGrid == null)
        {
            componentPropertyDataGrid = e.Source as AtomUI.Desktop.Controls.DataGrid;
        }
    }


    public void SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            var row = e.AddedItems[0] as ComponentEntity;
            var componentProperty = DbHelp.fsql.Select<ComponentPropertyEntity>().Where(t => t.ComponentId == row.Id).ToList();
            ComponentPropertyEntity = new ObservableCollection<ComponentPropertyEntity>(componentProperty);
        }
    }
    ComponentEntity AddComponentEntity;
    public void AddComponent()
    {
        AddComponentEntity = new ComponentEntity { Enable = "Y" };
        Component.Insert(0, AddComponentEntity);
        ComponentSelectedIndex = 0;
        componentDataGrid.BeginEdit(new RoutedEventArgs());
    }
    public async void CopyComponent()
    {
        if (selectedItem == null)
        {
            UIMessage.ShowError(UIMessage.SelectRowError);
        }
        else
        {
            ComponentEntity componentEntity = selectedItem.DeepClone();
            componentEntity.CreationTime = DateTime.Now;
            componentEntity.CreatorId = GlobalHelper.UserId;
            componentEntity.ComponentCode = string.Empty;
            componentEntity.ComponentName = string.Empty;
            componentEntity.Id = YitIdHelper.NextId();
            var componentProperty = DbHelp.fsql.Select<ComponentPropertyEntity>().Where(t => t.ComponentId == selectedItem.Id).ToList();
            foreach (var item in componentProperty)
            {
                item.Id = YitIdHelper.NextId();
                item.ComponentId = componentEntity.Id;
                item.CreationTime = DateTime.Now;
                item.CreatorId = GlobalHelper.UserId;
            }
            //ComponentPropertyEntity = new ObservableCollection<ComponentPropertyEntity>(componentProperty);
            DbHelp.fsql.Insert(componentEntity).ExecuteAffrows();
            DbHelp.fsql.Insert(componentProperty).ExecuteAffrows();
            var index = Component.IndexOf(selectedItem) + 1;
            Component.Insert(index, componentEntity);
            ComponentSelectedIndex = index;
            componentDataGrid.BeginEdit(new RoutedEventArgs());
            UIMessage.Notification(UIMessage.SaveOK);
        }
    }
    public async void DeleteComponent()
    {

        if (selectedItem == null)
        {
            UIMessage.ShowError(UIMessage.SelectRowError);
        }
        else
        {
           
            var result = await UIMessage.ShowConfirm(UIMessage.DeleteConfirm);
            if (result)
            {
                DbHelp.fsql.Delete<ComponentEntity>(selectedItem).ExecuteAffrows();
                DbHelp.fsql.Delete<ComponentPropertyEntity>().Where(t => t.ComponentId == selectedItem.Id).ExecuteAffrows();
                Component.Remove(selectedItem);
                UIMessage.Notification(UIMessage.DeleteOK);
            }
        }
    }
    public async void UploadComponent()
    {

        if (selectedItem == null)
        {
            UIMessage.ShowError(UIMessage.SelectRowError);
        }
        else
        {
            if (selectedItem.ComponentType != "PLC")
            {
                var keyMsg = Localizer.Localizer.Instance["Error.SelectType"];
                UIMessage.ShowError(keyMsg);
            }
            else
            {
                if (selectedItem.Excel != null)
                {
                    var confirmMsg = Localizer.Localizer.Instance["Error.ExcelExits"];
                    var result = await UIMessage.ShowConfirm(confirmMsg);
                    if (result)
                    {
                        UploadFile();
                    }
                }
                else
                {
                    UploadFile();
                }
            }
        }
    }
    private async void UploadFile()
    {
        var ops = new Avalonia.Platform.Storage.FilePickerOpenOptions();
        ops.FileTypeFilter = new[] { new FilePickerFileType("Excel(*.xls|*.xlsx)") { Patterns = new[] { "*.xls", "*.xlsx" } } };
        ops.AllowMultiple = false;
        var result = await UIMessage.FindViableOwner().StorageProvider.OpenFilePickerAsync(ops);
        if (result.Count > 0)
        {
            var filePath = result[0].Path.LocalPath;
            
            //using var fileBytes = FileHelper.OpenFile(filePath);
            //selectedItem.Excel = fileBytes.ToArray();
            //DbHelp.fsql.Update<ComponentEntity>().SetSource(selectedItem).ExecuteAffrows();
            //UIMessage.Notification();
        }
    }
    public async void OpenFile()
    {
        if (selectedItem == null)
        {
            UIMessage.ShowError(UIMessage.SelectRowError);
        }
        else {
            if (selectedItem.Excel==null)
            {
                var msg = Localizer.Localizer.Instance["Error.ExcelNotExits"];
                UIMessage.ShowError(msg);
            }
            else
            {
                GlobalHelper.Excel=selectedItem.Excel;
                //OpenExcelView openExcelView = new OpenExcelView();
                //openExcelView.Show();
                //var openView = nameof(OpenExcel);
                //UIHelper.OpenView(openView);
            }            
        }        
    }
    public async void DownloadFile()
    {
        if (selectedItem == null)
        {
            UIMessage.ShowError(UIMessage.SelectRowError);
        }
        else
        {
            if (selectedItem.Excel == null)
            {
                var msg = Localizer.Localizer.Instance["Error.ExcelNotExits"];
                UIMessage.ShowError(msg);
            }
            else
            {
                GlobalHelper.Excel = selectedItem.Excel;
                var ops = new FilePickerSaveOptions()
                {
                    Title = "Save Excel File",
                    SuggestedFileName = "PLC对接表.xlsx",
                    FileTypeChoices = new[] { new FilePickerFileType("Excel(*.xls|*.xlsx)") { Patterns = new[] { "*.xls", "*.xlsx" } } }
                };
                var result = await UIMessage.FindViableOwner().StorageProvider.SaveFilePickerAsync(ops);
                if (result != null)
                {
                    await System.IO.File.WriteAllBytesAsync(result.Path.LocalPath, GlobalHelper.Excel);
                }
            }
        }
    }
    public async void DataMonitor()
    {
        if (selectedItem == null)
        {
            UIMessage.ShowError(UIMessage.SelectRowError);
        }
        else
        {
            if (selectedItem.Excel == null)
            {
                var msg = Localizer.Localizer.Instance["Error.ExcelNotExits"];
                UIMessage.ShowError(msg);
            }
            else
            {
                GlobalHelper.Excel = selectedItem.Excel;
                //var form = new MonitorView();
                //form.DataContext = new MonitorViewModel();
                //ToolViewModel.dockViewModel.AddDocument(selectedItem.ComponentCode, selectedItem.ComponentName, form);
                //var monitor = new Monitor();
                //monitor.Show();
                //monitor.Title = selectedItem.ComponentName;
            }
        }
    }    
    public void SaveComponent()
    {
        for (int i = 0; i < Component.Count; i++)
        {
            Component[i].Sort = i + 1;
        }
        int result = DbHelp.fsql.InsertOrUpdate<ComponentEntity>().SetSource(Component).ExecuteAffrows();
        UIMessage.Notification(UIMessage.SaveOK);
    }
    List<ComponentPropertyEntity> AddComponentItem = new List<ComponentPropertyEntity>();
    private int componentPropertySelectedIndex;
    private ObservableCollection<string> autoCompleteBoxItemsSource;
    private ComponentEntity selectedItem;

    public void SaveComponentProperty()
    {
        DbHelp.fsql.InsertOrUpdate<ComponentPropertyEntity>().SetSource(ComponentPropertyEntity).ExecuteAffrows();
        UIMessage.Notification(UIMessage.SaveOK);
    }
    public void AddComponentProperty()
    {
        ComponentPropertyEntity addComponentPropertyEntity = new ComponentPropertyEntity();
        addComponentPropertyEntity.ComponentId = Component[ComponentSelectedIndex].Id;
        AddComponentItem.Add(addComponentPropertyEntity);
        ComponentPropertyEntity.Add(addComponentPropertyEntity);
        ComponentPropertySelectedIndex = ComponentPropertyEntity.Count - 1;
        componentPropertyDataGrid.BeginEdit(new RoutedEventArgs());
    }
    public void LineEdit_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is LineEdit lineEdit)
        {
            if (string.IsNullOrWhiteSpace(lineEdit.Text))
            {
                Component = ComponentSource;
            }
            else
            {
                var fileter = ComponentSource.Where(t => t.ComponentCode.Contains(lineEdit.Text, StringComparison.OrdinalIgnoreCase) || t.ComponentName.Contains(lineEdit.Text, StringComparison.OrdinalIgnoreCase));
                Component = new ObservableCollection<ComponentEntity>(fileter);
            }
        }
    }
    public void GridSplitter_DragCompleted(object? sender, Avalonia.Input.VectorEventArgs e)
    {
        if (((Avalonia.StyledElement)e.Source).Parent is Grid grid)
        {
           // SaveGridSplitter(grid, nameof(ComponentView));
        }
    }
}