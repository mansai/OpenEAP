using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using ReactiveUI;

namespace OpenEAP.Models.FormDesigner
{
    /// <summary>表单设计器支持的控件类型。</summary>
    public enum FormControlKind
    {
        Label = 0,
        TextBox = 1,
        MultilineTextBox = 2,
        ComboBox = 3,
        CheckBox = 4,
        Button = 5,
        DropdownTable = 6,
        DataGrid = 7,
        DatePicker = 8,
        Divider = 9
    }

    /// <summary>设计器内置的演示数据源。</summary>
    public enum FormDataSourceKind
    {
        None = 0,
        Devices = 1,
        Products = 2,
        Customers = 3,
        QualityRecords = 4
    }

    /// <summary>表单模型基类，属性变化会同步到画布和属性检查器。</summary>
    public abstract class FormModelBase : ReactiveObject
    {
        protected bool Set<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>控件的内置事件及其 C# 脚本。</summary>
    public sealed class FormEventBinding : FormModelBase
    {
        private string _name = string.Empty;
        private string _code = string.Empty;

        public FormEventBinding()
        {
        }

        public FormEventBinding(string name, string code = "")
        {
            _name = name;
            _code = code;
        }

        [Browsable(false)]
        public string Name
        {
            get => _name;
            set => Set(ref _name, value ?? string.Empty);
        }

        [Browsable(false)]
        public string Code
        {
            get => _code;
            set => Set(ref _code, value ?? string.Empty);
        }

        public FormEventBinding Clone() => new(Name, Code);
    }

    /// <summary>表格列定义。列标题会在创建时按当前语言生成。</summary>
    public sealed class FormColumnDefinition : FormModelBase
    {
        private string _key = string.Empty;
        private string _title = string.Empty;
        private double _width;
        private bool _searchable = true;

        [Category("Data")]
        [DisplayName("Column key")]
        public string Key
        {
            get => _key;
            set => Set(ref _key, value ?? string.Empty);
        }

        [Category("Data")]
        [DisplayName("Header")]
        public string Title
        {
            get => _title;
            set => Set(ref _title, value ?? string.Empty);
        }

        [Category("Data")]
        [DisplayName("Width")]
        public double Width
        {
            get => _width;
            set => Set(ref _width, Math.Max(0, value));
        }

        [Category("Data")]
        [DisplayName("Searchable")]
        public bool Searchable
        {
            get => _searchable;
            set => Set(ref _searchable, value);
        }

        public FormColumnDefinition()
        {
        }

        public FormColumnDefinition(string key, string title, double width = 0, bool searchable = true)
        {
            _key = key ?? string.Empty;
            _title = title ?? string.Empty;
            _width = width;
            _searchable = searchable;
        }

        public FormColumnDefinition Clone()
            => new(Key, Title, Width, Searchable);
    }

    /// <summary>设计器示例数据/表格行。Values 按列索引保存，便于 ProDataGrid 动态列绑定。</summary>
    public sealed class FormDataRow
    {
        public List<string> Values { get; set; } = new();

        public FormDataRow()
        {
        }

        public FormDataRow(IEnumerable<string> values)
        {
            Values = values.ToList();
        }

        public FormDataRow(params string[] values)
        {
            Values = values.ToList();
        }

        [JsonIgnore]
        public string FirstValue => Values.FirstOrDefault() ?? string.Empty;

        public string GetValue(int index) => index >= 0 && index < Values.Count ? Values[index] : string.Empty;
    }

    /// <summary>表单中的一个控件。坐标与尺寸使用设计画布坐标。</summary>
    public sealed class FormControlModel : FormModelBase
    {
        private string _id = Guid.NewGuid().ToString("N");
        private FormControlKind _kind = FormControlKind.TextBox;
        private string _name = "control";
        private string _text = string.Empty;
        private string _placeholder = string.Empty;
        private string _optionsText = string.Empty;
        private double _x;
        private double _y;
        private double _width = 180;
        private double _height = 34;
        private double _fontSize = 13;
        private string _textColor = "#1F2937";
        private string _background = "#FFFFFF";
        private string _borderColor = "#CBD5E1";
        private double _cornerRadius = 6;
        private bool _isRequired;
        private bool _isEnabled = true;
        private bool _isChecked;
        private FormDataSourceKind _dataSource;
        private string _displayColumn = "Name";
        private string _valueColumn = "Code";
        private bool _isSelected;

        public FormControlModel()
        {
            AttachColumnNotifications(_columns);
        }

        [Browsable(false)]
        public string Id
        {
            get => _id;
            set => Set(ref _id, value ?? Guid.NewGuid().ToString("N"));
        }

        [Category("Basic")]
        [DisplayName("Control type")]
        [TypeConverter(typeof(FormControlKindConverter))]
        public FormControlKind Kind
        {
            get => _kind;
            set
            {
                if (Set(ref _kind, value))
                {
                    if (value == FormControlKind.Divider)
                    {
                        Height = Math.Max(2, Math.Min(Height, 4));
                    }
                    else if (value == FormControlKind.Button && Width < 80)
                    {
                        Width = 120;
                    }

                    if (value is FormControlKind.DropdownTable or FormControlKind.DataGrid && Columns.Count == 0)
                    {
                        if (DataSource == FormDataSourceKind.None) DataSource = FormDataSourceKind.Devices;
                        else ApplyDefaultColumns();
                    }
                }
            }
        }

        [Category("Basic")]
        [DisplayName("Name")]
        public string Name
        {
            get => _name;
            set => Set(ref _name, value ?? string.Empty);
        }

        [Category("Content")]
        [DisplayName("Text")]
        public string Text
        {
            get => _text;
            set => Set(ref _text, value ?? string.Empty);
        }

        [Category("Content")]
        [DisplayName("Placeholder")]
        public string Placeholder
        {
            get => _placeholder;
            set => Set(ref _placeholder, value ?? string.Empty);
        }

        /// <summary>下拉选项，使用分号、逗号或换行分隔。</summary>
        [Category("Content")]
        [DisplayName("Options")]
        public string OptionsText
        {
            get => _optionsText;
            set => Set(ref _optionsText, value ?? string.Empty);
        }

        [Category("Layout")]
        [DisplayName("X")]
        public double X
        {
            get => _x;
            set => Set(ref _x, Math.Max(0, value));
        }

        [Category("Layout")]
        [DisplayName("Y")]
        public double Y
        {
            get => _y;
            set => Set(ref _y, Math.Max(0, value));
        }

        [Category("Layout")]
        [DisplayName("Width")]
        public double Width
        {
            get => _width;
            set => Set(ref _width, Math.Max(16, value));
        }

        [Category("Layout")]
        [DisplayName("Height")]
        public double Height
        {
            get => _height;
            set => Set(ref _height, Math.Max(2, value));
        }

        [Category("Appearance")]
        [DisplayName("Font size")]
        public double FontSize
        {
            get => _fontSize;
            set => Set(ref _fontSize, Math.Clamp(value, 9, 48));
        }

        [Category("Appearance")]
        [DisplayName("Text color")]
        public string TextColor
        {
            get => _textColor;
            set => Set(ref _textColor, value ?? "#1F2937");
        }

        [Category("Appearance")]
        [DisplayName("Background")]
        public string Background
        {
            get => _background;
            set => Set(ref _background, value ?? "#FFFFFF");
        }

        [Category("Appearance")]
        [DisplayName("Border color")]
        public string BorderColor
        {
            get => _borderColor;
            set => Set(ref _borderColor, value ?? "#CBD5E1");
        }

        [Category("Appearance")]
        [DisplayName("Corner radius")]
        public double CornerRadius
        {
            get => _cornerRadius;
            set => Set(ref _cornerRadius, Math.Clamp(value, 0, 32));
        }

        [Category("Behavior")]
        [DisplayName("Required")]
        public bool IsRequired
        {
            get => _isRequired;
            set => Set(ref _isRequired, value);
        }

        [Category("Behavior")]
        [DisplayName("Enabled")]
        public bool IsEnabled
        {
            get => _isEnabled;
            set => Set(ref _isEnabled, value);
        }

        [Category("Content")]
        [DisplayName("Checked")]
        public bool IsChecked
        {
            get => _isChecked;
            set => Set(ref _isChecked, value);
        }

        [Category("Data")]
        [DisplayName("Data source")]
        [TypeConverter(typeof(FormDataSourceKindConverter))]
        public FormDataSourceKind DataSource
        {
            get => _dataSource;
            set
            {
                if (!Set(ref _dataSource, value)) return;
                if (Kind is FormControlKind.DropdownTable or FormControlKind.DataGrid)
                {
                    ApplyDefaultColumns();
                }
            }
        }

        [Category("Data")]
        [DisplayName("Display column")]
        public string DisplayColumn
        {
            get => _displayColumn;
            set => Set(ref _displayColumn, value ?? "Name");
        }

        [Category("Data")]
        [DisplayName("Value column")]
        public string ValueColumn
        {
            get => _valueColumn;
            set => Set(ref _valueColumn, value ?? "Code");
        }

        private ObservableCollection<FormColumnDefinition> _columns = new();

        [Category("Data")]
        [DisplayName("Columns")]
        public ObservableCollection<FormColumnDefinition> Columns
        {
            get => _columns;
            set
            {
                if (ReferenceEquals(_columns, value)) return;
                DetachColumnNotifications(_columns);
                _columns = value ?? new ObservableCollection<FormColumnDefinition>();
                AttachColumnNotifications(_columns);
                this.RaisePropertyChanged(nameof(Columns));
            }
        }

        [Browsable(false)]
        public ObservableCollection<FormEventBinding> Events { get; set; } = new();

        [Browsable(false)]
        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        [Browsable(false)]
        [JsonIgnore]
        public string KindDisplayName => Kind.ToString();

        [Browsable(false)]
        [JsonIgnore]
        public bool HasDataSource => Kind is FormControlKind.DropdownTable or FormControlKind.DataGrid;

        public string GetEventCode(string eventName)
        {
            return Events.FirstOrDefault(e => string.Equals(e.Name, eventName, StringComparison.OrdinalIgnoreCase))?.Code ?? string.Empty;
        }

        public void SetEventCode(string eventName, string code)
        {
            var item = Events.FirstOrDefault(e => string.Equals(e.Name, eventName, StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                item = new FormEventBinding(eventName);
                Events.Add(item);
            }
            item.Code = code ?? string.Empty;
        }

        /// <summary>解析下拉选项，供设计画布和运行时控件使用。</summary>
        public IReadOnlyList<string> ParseOptions()
        {
            return (OptionsText ?? string.Empty)
                .Split(new[] { ';', ',', '，', '；', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)
                .Distinct()
                .ToList();
        }

        public void ApplyDefaultColumns()
        {
            Columns.Clear();
            foreach (var column in FormSampleData.GetColumns(DataSource))
            {
                Columns.Add(column.Clone());
            }
            this.RaisePropertyChanged(nameof(Columns));
        }

        private void AttachColumnNotifications(ObservableCollection<FormColumnDefinition> columns)
        {
            columns.CollectionChanged += OnColumnCollectionChanged;
            foreach (var column in columns)
            {
                column.PropertyChanged += OnColumnPropertyChanged;
            }
        }

        private void DetachColumnNotifications(ObservableCollection<FormColumnDefinition> columns)
        {
            columns.CollectionChanged -= OnColumnCollectionChanged;
            foreach (var column in columns)
            {
                column.PropertyChanged -= OnColumnPropertyChanged;
            }
        }

        private void OnColumnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (FormColumnDefinition column in e.NewItems)
                {
                    column.PropertyChanged += OnColumnPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (FormColumnDefinition column in e.OldItems)
                {
                    column.PropertyChanged -= OnColumnPropertyChanged;
                }
            }
            this.RaisePropertyChanged(nameof(Columns));
        }

        private void OnColumnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            => this.RaisePropertyChanged(nameof(Columns));

        public FormControlModel Clone(bool newId = false)
        {
            var clone = new FormControlModel
            {
                Id = newId ? Guid.NewGuid().ToString("N") : Id,
                Kind = Kind,
                Name = Name,
                Text = Text,
                Placeholder = Placeholder,
                OptionsText = OptionsText,
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                FontSize = FontSize,
                TextColor = TextColor,
                Background = Background,
                BorderColor = BorderColor,
                CornerRadius = CornerRadius,
                IsRequired = IsRequired,
                IsEnabled = IsEnabled,
                IsChecked = IsChecked,
                DataSource = DataSource,
                DisplayColumn = DisplayColumn,
                ValueColumn = ValueColumn,
                Columns = new ObservableCollection<FormColumnDefinition>(Columns.Select(c => c.Clone())),
                Events = new ObservableCollection<FormEventBinding>(Events.Select(e => e.Clone())),
            };
            return clone;
        }
    }

    /// <summary>一个表单窗体/页面。一个文档可以同时设计多个页面。</summary>
    public sealed class FormPageModel : FormModelBase
    {
        private string _id = Guid.NewGuid().ToString("N");
        private string _name = "page1";
        private string _title = string.Empty;
        private string _description = string.Empty;
        private double _width = 960;
        private double _height = 620;
        private string _background = "#FFFFFF";
        private bool _isSelected;

        [Browsable(false)]
        public string Id
        {
            get => _id;
            set => Set(ref _id, value ?? Guid.NewGuid().ToString("N"));
        }

        [Category("Basic")]
        [DisplayName("Page name")]
        public string Name
        {
            get => _name;
            set => Set(ref _name, value ?? string.Empty);
        }

        [Category("Basic")]
        [DisplayName("Title")]
        public string Title
        {
            get => _title;
            set => Set(ref _title, value ?? string.Empty);
        }

        [Category("Basic")]
        [DisplayName("Description")]
        public string Description
        {
            get => _description;
            set => Set(ref _description, value ?? string.Empty);
        }

        [Category("Layout")]
        [DisplayName("Width")]
        public double Width
        {
            get => _width;
            set => Set(ref _width, Math.Clamp(value, 360, 4000));
        }

        [Category("Layout")]
        [DisplayName("Height")]
        public double Height
        {
            get => _height;
            set => Set(ref _height, Math.Clamp(value, 240, 4000));
        }

        [Category("Appearance")]
        [DisplayName("Background")]
        public string Background
        {
            get => _background;
            set => Set(ref _background, value ?? "#FFFFFF");
        }

        [Browsable(false)]
        public ObservableCollection<FormControlModel> Controls { get; set; } = new();

        [Browsable(false)]
        public ObservableCollection<FormEventBinding> Events { get; set; } = new();

        [Browsable(false)]
        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        [Browsable(false)]
        [JsonIgnore]
        public int ControlCount => Controls.Count;

        public void NotifyControlCountChanged()
        {
            this.RaisePropertyChanged(nameof(ControlCount));
        }

        public FormControlModel? FindControl(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return Controls.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public string GetEventCode(string eventName)
        {
            return Events.FirstOrDefault(e => string.Equals(e.Name, eventName, StringComparison.OrdinalIgnoreCase))?.Code ?? string.Empty;
        }

        public void SetEventCode(string eventName, string code)
        {
            var item = Events.FirstOrDefault(e => string.Equals(e.Name, eventName, StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                item = new FormEventBinding(eventName);
                Events.Add(item);
            }
            item.Code = code ?? string.Empty;
        }

        public FormPageModel Clone(bool newIds = false)
        {
            var clone = new FormPageModel
            {
                Id = newIds ? Guid.NewGuid().ToString("N") : Id,
                Name = Name,
                Title = Title,
                Description = Description,
                Width = Width,
                Height = Height,
                Background = Background,
                Events = new ObservableCollection<FormEventBinding>(Events.Select(e => e.Clone())),
                Controls = new ObservableCollection<FormControlModel>(Controls.Select(c => c.Clone(newIds))),
            };
            return clone;
        }
    }

    /// <summary>表单设计文档，可包含多个窗体页面。</summary>
    public sealed class FormDesignerDocument
    {
        public string Name { get; set; } = "Form";
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
        public ObservableCollection<FormPageModel> Pages { get; set; } = new();

        public static FormDesignerDocument Create(string name)
        {
            var document = new FormDesignerDocument { Name = name };
            document.Pages.Add(new FormPageModel
            {
                Name = "page1",
                Title = name,
            });
            return document;
        }

        public void EnsureDefaults()
        {
            Pages ??= new ObservableCollection<FormPageModel>();
            if (Pages.Count == 0)
            {
                Pages.Add(new FormPageModel { Name = "page1", Title = Name });
            }

            var pageIndex = 1;
            foreach (var page in Pages)
            {
                page.Controls ??= new ObservableCollection<FormControlModel>();
                page.Events ??= new ObservableCollection<FormEventBinding>();
                if (string.IsNullOrWhiteSpace(page.Name))
                {
                    page.Name = $"page{pageIndex}";
                }
                if (string.IsNullOrWhiteSpace(page.Title))
                {
                    page.Title = page.Name;
                }

                foreach (var control in page.Controls)
                {
                    control.Columns ??= new ObservableCollection<FormColumnDefinition>();
                    control.Events ??= new ObservableCollection<FormEventBinding>();
                }
                pageIndex++;
            }
        }

        public FormDesignerDocument Clone()
        {
            var clone = new FormDesignerDocument
            {
                Name = Name,
                Description = Description,
                CreatedAt = CreatedAt,
                ModifiedAt = ModifiedAt,
                Pages = new ObservableCollection<FormPageModel>(Pages.Select(p => p.Clone())),
            };
            return clone;
        }
    }

    /// <summary>工具箱中的组件定义。</summary>
    public sealed class FormPaletteItem
    {
        public FormControlKind Kind { get; init; }
        public string Key { get; init; } = string.Empty;
        public string Icon { get; init; } = string.Empty;
        public string DisplayName => Localizer.Localizer.Instance[Key];

        public string Hint => Localizer.Localizer.Instance[Key + "Hint"];
    }

    /// <summary>内置事件目录。事件名称会写入模型，用户可为每个事件配置 C# 代码。</summary>
    public static class FormEventCatalog
    {
        public static IReadOnlyList<string> GetEvents(FormControlKind kind)
        {
            return kind switch
            {
                FormControlKind.Button => new[] { "Click", "Loaded" },
                FormControlKind.TextBox => new[] { "TextChanged", "GotFocus", "LostFocus" },
                FormControlKind.MultilineTextBox => new[] { "TextChanged", "GotFocus", "LostFocus" },
                FormControlKind.ComboBox => new[] { "SelectionChanged", "Loaded" },
                FormControlKind.CheckBox => new[] { "CheckedChanged", "Loaded" },
                FormControlKind.DropdownTable => new[] { "SelectionChanged", "SearchChanged", "Loaded" },
                FormControlKind.DataGrid => new[] { "SelectionChanged", "CellDoubleClick", "Loaded" },
                FormControlKind.DatePicker => new[] { "SelectedDateChanged", "Loaded" },
                _ => new[] { "Loaded" }
            };
        }

        public static IReadOnlyList<string> PageEvents { get; } = new[] { "Loaded" };
    }
}
