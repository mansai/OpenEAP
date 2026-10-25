using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DocumentFormat.OpenXml.Drawing.Charts;
using Masuit.Tools;
using Masuit.Tools.Extension.Object;
using OpenEAP.Common;
using OpenEAP.Helper;
using OpenEAP.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.ViewModels
{
    public class LanguageViewModel : ViewModelBase
    {
        private ObservableCollection<LocalizationEntity> localization;
        private ObservableCollection<ResourcesEntity> resourcesEntity;
        private int localizationSelectedIndex;        
        private int resourcesSelectedIndex;
        private ColumnDefinitions gridSplitterColumnDefinitions;
        private Dimension leftPanelWidth;
        private Dimension rightPanelWidth;
        private AtomUI.Desktop.Controls.DataGrid localizationDataGrid;
        private AtomUI.Desktop.Controls.DataGrid resourcesDataGrid;

        public ObservableCollection<LocalizationEntity> Localization { get => localization; set => this.RaiseAndSetIfChanged(ref localization, value); }
        public ObservableCollection<ResourcesEntity> ResourcesEntity { get => resourcesEntity; set => this.RaiseAndSetIfChanged(ref resourcesEntity, value); }
        public int LocalizationSelectedIndex { get => localizationSelectedIndex; set => this.RaiseAndSetIfChanged(ref localizationSelectedIndex, value); }
        public int ResourcesSelectedIndex { get => resourcesSelectedIndex; set => this.RaiseAndSetIfChanged(ref resourcesSelectedIndex, value); }
        public ColumnDefinitions GridSplitterColumnDefinitions { get => gridSplitterColumnDefinitions; set => this.RaiseAndSetIfChanged(ref gridSplitterColumnDefinitions, value); }
        public Dimension LeftPanelWidth { get => leftPanelWidth; set => this.RaiseAndSetIfChanged(ref leftPanelWidth, value); }
        public Dimension RightPanelWidth { get => rightPanelWidth; set => this.RaiseAndSetIfChanged(ref rightPanelWidth, value); }
        public ObservableCollection<ResourcesEntity> ResourcesEntitySource { get; set; }        
        public LanguageViewModel()
        {
            var lans = DbHelp.fsql.Select<LocalizationEntity>().WhereDynamic(GlobalHelper.WhereEnable).OrderBy(t => t.Sort).ToList();
            Localization = new ObservableCollection<LocalizationEntity>(lans);
            var culture = lans.First(s => s.IsDefault == GlobalHelper.Y).Culture;
            var resources = DbHelp.fsql.Select<ResourcesEntity>().Where(t => t.IsDeleted == GlobalHelper.N && t.Value != null && t.Culture == culture).ToList();
            ResourcesEntity = new ObservableCollection<ResourcesEntity>(resources);
            ResourcesEntitySource = ResourcesEntity.DeepClone();
            LeftPanelWidth = new Dimension(30, DimensionUnitType.Percentage);
            RightPanelWidth = new Dimension(70, DimensionUnitType.Percentage);
        }

        public void Splitter_ResizeCompleted(object? sender, SplitterResizeEventArgs e)
        {
            var total = e.Sizes.Sum();
            double leftRaw = e.Sizes.First() / total * 100.0;
            int leftPercent = (int)Math.Round(leftRaw);     
            int rightPercent = 100 - leftPercent;   // 保证和为 100
        }

        public void Grid_Loaded(object? sender, RoutedEventArgs e)
        {
            if (e.Source is Grid grid)
            {
                GridSplitterColumnDefinitions = grid.ColumnDefinitions;
            }
        }
        public void GridSplitter_DragCompleted(object? sender, Avalonia.Input.VectorEventArgs e)
        {
            if (((Avalonia.StyledElement)e.Source).Parent is Grid grid)
            {
                //SaveGridSplitter(grid, nameof(LanguageView));
            }
        }

        public void Localization_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (localizationDataGrid == null)
            {
                localizationDataGrid = e.Source as AtomUI.Desktop.Controls.DataGrid;
            }
        }
        public void Resources_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (resourcesDataGrid == null)
            {
                resourcesDataGrid = e.Source as AtomUI.Desktop.Controls.DataGrid;
            }
        }


        public void SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var row = e.AddedItems[0] as LocalizationEntity;
                var resources = DbHelp.fsql.Select<ResourcesEntity>().Where(t => t.IsDeleted == GlobalHelper.N && t.Culture == row.Culture && t.Value != null).ToList();
                ResourcesEntity = new ObservableCollection<ResourcesEntity>(resources);
                ResourcesEntitySource = ResourcesEntity.DeepClone();
            }
            //if (localizationDataGrid != null && localizationDataGrid.SelectedItem != null)
            //{
            //    // 创建 RoutedEventArgs 并调用 BeginEdit
            //    //var routedEventArgs = new RoutedEventArgs();
            //    //localizationDataGrid.BeginEdit(routedEventArgs);

            //    //或者延迟执行，确保 UI 更新完成
            //    Dispatcher.UIThread.Post(() =>
            //    {
            //        localizationDataGrid.BeginEdit(new RoutedEventArgs());
            //    });
            //}
        }
        public void ResourcesSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            //if (resourcesDataGrid != null && resourcesDataGrid.SelectedItem != null)
            //{
            //    Dispatcher.UIThread.Post(() =>
            //    {
            //        resourcesDataGrid.BeginEdit(new RoutedEventArgs());
            //    });
            //}
        }
        LocalizationEntity AddLocalizationEntity;
        public void AddLocalization()
        {
            AddLocalizationEntity = new LocalizationEntity { Enable = "Y" };
            Localization.Insert(0, AddLocalizationEntity);
            LocalizationSelectedIndex = 0;
        }
        public void SaveLocalization()
        {
            for (int i = 0; i < localization.Count; i++)
            {
                localization[i].Sort = i + 1;
            }
            int result = DbHelp.fsql.InsertOrUpdate<LocalizationEntity>().SetSource(Localization).ExecuteAffrows();
            if (result > 0 && AddLocalizationEntity != null)
            {
                //var lans = DbHelp.fsql.Select<LocalizationEntity>().WhereDynamic(GlobalHelper.WhereEnable).OrderBy(t=>t.Sort).ToList();
                //Localization = new ObservableCollection<LocalizationEntity>(lans);
                var resources = DbHelp.fsql.Select<ResourcesEntity>().Where(t => t.IsDeleted == GlobalHelper.N && t.Culture == "zh-Hans").ToList(t => new ResourcesEntity { Culture = AddLocalizationEntity.Culture, Key = t.Key });
                result = DbHelp.fsql.Insert<ResourcesEntity>(resources).ExecuteAffrows();
                ResourcesEntity = new ObservableCollection<ResourcesEntity>(resources);
                AddLocalizationEntity = null;
            }
            UIMessage.Notification(UIMessage.SaveOK);
        }
        List<ResourcesEntity> AddResource = new List<ResourcesEntity>();
        public void SaveResources()
        {

            AddResource = AddResource.Where(t => !string.IsNullOrWhiteSpace(t.Key)).ToList();
            if (AddResource.Count > 0)
            {
                var arr = AddResource.Select(s => s.Key).ToArray();
                var check = DbHelp.fsql.Queryable<ResourcesEntity>().Where(t => t.Culture == AddResource.First().Culture && arr.Contains(t.Key)).ToList();
                if (check != null && check.Count() > 0)
                {
                    var keyMsg = Localizer.Localizer.Instance["Key"];
                    var repeatMsg = Localizer.Localizer.Instance["Repeat"];
                    var item = string.Join(", ", check.Select(t => t.Key));
                    UIMessage.NotificationError($"{keyMsg}:{item}{repeatMsg}");
                    return;
                }
            }
            DbHelp.fsql.InsertOrUpdate<ResourcesEntity>().SetSource(ResourcesEntity).ExecuteAffrows();
            if (AddResource.Count > 0)
            {
                foreach (var item in localization)
                {
                    if (item.Culture != AddResource[0].Culture)
                    {
                        var insertResources = AddResource.Select(t => new ResourcesEntity { Culture = item.Culture, Key = t.Key });
                        DbHelp.fsql.Insert<ResourcesEntity>(insertResources).ExecuteAffrows();
                    }
                }
                AddResource.Clear();
            }
            Localizer.Localizer.Instance.LoadLanguage(Localizer.Localizer.Instance.Language);
            UIMessage.Notification(UIMessage.SaveOK);
        }
        public void AddResources()
        {
            ResourcesEntity addResourcesEntity = new ResourcesEntity() { Culture = ResourcesEntity.First().Culture };
            AddResource.Add(addResourcesEntity);
            ResourcesEntity.Insert(0, addResourcesEntity);
            ResourcesSelectedIndex = 0;
            //resourcesDataGrid.BeginEdit();
        }
        public void LineEdit_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (e.Source is LineEdit lineEdit)
            {
                if (string.IsNullOrWhiteSpace(lineEdit.Text))
                {
                    ResourcesEntity = ResourcesEntitySource;
                }
                else
                {
                    try
                    {
                        var filter = ResourcesEntity.Where(t => t.Key.Contains(lineEdit.Text, StringComparison.OrdinalIgnoreCase) || t.Value.Contains(lineEdit.Text, StringComparison.OrdinalIgnoreCase));
                        if (filter.Any())
                        {
                            ResourcesEntity = new ObservableCollection<ResourcesEntity>(filter);
                        }
                        else
                        {
                            ResourcesEntity = new ObservableCollection<ResourcesEntity>();
                        }
                    }
                    catch (Exception ex)
                    {
                        ResourcesEntity = new ObservableCollection<ResourcesEntity>();
                    }
                }
            }
        }
    }
    public class ResourcesEntityQuery
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
