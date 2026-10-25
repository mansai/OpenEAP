using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Masuit.Tools;
using Masuit.Tools.Extension.Object;
using OpenEAP.Common;
using OpenEAP.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenEAP.ViewModels;

public class ConfigViewModel : ViewModelBase
{
    private ObservableCollection<ConfigEntity> config;
    private ObservableCollection<ConfigValueEntity> configValueEntity;
    private int configSelectedIndex;
    private AtomUI.Desktop.Controls.DataGrid ConfigDataGrid;
    private AtomUI.Desktop.Controls.DataGrid configValueDataGrid;
    private GridLength leftPanelWidth;
    private GridLength rightPanelWidth;

    public ObservableCollection<ConfigEntity> Config { get => config; set => this.RaiseAndSetIfChanged(ref config, value); }
    public ObservableCollection<ConfigValueEntity> ConfigValueEntity { get => configValueEntity; set => this.RaiseAndSetIfChanged(ref configValueEntity, value); }
    public int ConfigSelectedIndex { get => configSelectedIndex; set => this.RaiseAndSetIfChanged(ref configSelectedIndex, value); }
    public int ConfigValuesSelectedIndex { get => configValueSelectedIndex; set => this.RaiseAndSetIfChanged(ref configValueSelectedIndex, value); }
    public GridLength LeftPanelWidth { get => leftPanelWidth; set => this.RaiseAndSetIfChanged(ref leftPanelWidth, value); }
    public GridLength RightPanelWidth { get => rightPanelWidth; set => this.RaiseAndSetIfChanged(ref rightPanelWidth, value); }
    private ObservableCollection<ConfigEntity> ConfigSource;
    public ConfigViewModel()
    {
        // 获取当前NameScope
        //var nameScope = NameScope.GetNameScope(this);
        var configList = DbHelp.fsql.Select<ConfigEntity>().ToList();
        config = new ObservableCollection<ConfigEntity>(configList);
        ConfigSource = config.DeepClone();
        var enumItems = DbHelp.fsql.Select<ConfigValueEntity>().Where(t => t.ConfigId == configList.First().Id).ToList();
        ConfigValueEntity = new ObservableCollection<ConfigValueEntity>(enumItems);
        //string[] arr = GetSplitterValue(nameof(ConfigView));
        //if (arr.Length == 0)
        //{
        //    LeftPanelWidth = new GridLength(5, GridUnitType.Star);
        //    RightPanelWidth = new GridLength(5, GridUnitType.Star);
        //}
        //else
        //{
        //    LeftPanelWidth = GridLength.Parse(arr[0]);
        //    RightPanelWidth = GridLength.Parse(arr[1]);
        //}
    }
    public void Config_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ConfigDataGrid == null)
        {
            ConfigDataGrid = e.Source as AtomUI.Desktop.Controls.DataGrid;
        }
    }
    public void ConfigValue_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (configValueDataGrid == null)
        {
            configValueDataGrid = e.Source as AtomUI.Desktop.Controls.DataGrid;
        }
    }


    public void SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            var row = e.AddedItems[0] as ConfigEntity;
            var enumItem = DbHelp.fsql.Select<ConfigValueEntity>().Where(t => t.ConfigId == row.Id).ToList();
            ConfigValueEntity = new ObservableCollection<ConfigValueEntity>(enumItem);
        }
    }
    ConfigEntity AddConfigEntity;
    public void AddConfig()
    {
        AddConfigEntity = new ConfigEntity {  };
        Config.Insert(0, AddConfigEntity);
        ConfigSelectedIndex = 0;
        ConfigDataGrid.BeginEdit(new RoutedEventArgs());
        //ConfigDataGrid.ItemsSource = Config;
        //ConfigDataGrid.InvalidateVisual();
        //ConfigDataGrid.UpdateLayout();
        //ConfigDataGrid.BeginInit();
        //ConfigDataGrid.InvalidateVisual();
        //ConfigDataGrid.UpdateLayout();
        //ConfigDataGrid.EndInit();
        //ConfigDataGrid.BeginInit();
        //ConfigDataGrid.UpdateLayout();
        //ConfigDataGrid.Columns[0].IsReadOnly = true;

    }
    public void SaveConfig()
    {
        for (int i = 0; i < Config.Count; i++)
        {
            Config[i].Sort = i + 1;
        }
        int result = DbHelp.fsql.InsertOrUpdate<ConfigEntity>().SetSource(Config).ExecuteAffrows();
        UIMessage.Notification(UIMessage.SaveOK);
    }
    List<ConfigValueEntity> AddConfigValue = new List<ConfigValueEntity>();
    private int configValueSelectedIndex;

    public void SaveConfigValues()
    {
        for (int i = 0; i < ConfigValueEntity.Count; i++)
        {
            ConfigValueEntity[i].Sort = i + 1;
        }
        DbHelp.fsql.InsertOrUpdate<ConfigValueEntity>().SetSource(ConfigValueEntity).ExecuteAffrows();
        UIMessage.Notification(UIMessage.SaveOK);
    }
    public void AddConfigValues()
    {
        ConfigValueEntity addConfigValueEntity = new ConfigValueEntity();
        addConfigValueEntity.ConfigId = config[configSelectedIndex].Id;
        AddConfigValue.Add(addConfigValueEntity);
        ConfigValueEntity.Add(addConfigValueEntity);
        ConfigValuesSelectedIndex = ConfigValueEntity.Count- 1;
        configValueDataGrid.BeginEdit(new RoutedEventArgs());
    }
    public void LineEdit_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is LineEdit autoCompleteBox)
        {
            if (string.IsNullOrWhiteSpace(autoCompleteBox.Text))
            {
                Config = ConfigSource;
            }
            else
            {
                var fileter = ConfigSource.Where(t => t.ConfigCode.Contains(autoCompleteBox.Text, StringComparison.OrdinalIgnoreCase) || t.ConfigName.Contains(autoCompleteBox.Text, StringComparison.OrdinalIgnoreCase));
                Config = new ObservableCollection<ConfigEntity>(fileter);
            }
        }
    }
    public void GridSplitter_DragCompleted(object? sender, Avalonia.Input.VectorEventArgs e)
    {
        if (((Avalonia.StyledElement)e.Source).Parent is Grid grid)
        {
            //SaveGridSplitter(grid, nameof(ConfigView));
        }
    }
}
