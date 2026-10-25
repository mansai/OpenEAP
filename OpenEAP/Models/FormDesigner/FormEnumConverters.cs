using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace OpenEAP.Models.FormDesigner
{
    /// <summary>将枚举值显示为当前语言下的控件名称。</summary>
    public sealed class FormControlKindConverter : EnumConverter
    {
        private static readonly Dictionary<FormControlKind, string> Keys = new()
        {
            [FormControlKind.Label] = "FormWidgetLabel",
            [FormControlKind.TextBox] = "FormWidgetTextBox",
            [FormControlKind.MultilineTextBox] = "FormWidgetMultiline",
            [FormControlKind.ComboBox] = "FormWidgetComboBox",
            [FormControlKind.CheckBox] = "FormWidgetCheckBox",
            [FormControlKind.Button] = "FormWidgetButton",
            [FormControlKind.DropdownTable] = "FormWidgetDropdownTable",
            [FormControlKind.DataGrid] = "FormWidgetDataGrid",
            [FormControlKind.DatePicker] = "FormWidgetDatePicker",
            [FormControlKind.Divider] = "FormWidgetDivider",
        };

        public FormControlKindConverter() : base(typeof(FormControlKind))
        {
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
            Type destinationType)
        {
            if (destinationType == typeof(string) && value is FormControlKind kind &&
                Keys.TryGetValue(kind, out var key))
            {
                return Localizer.Localizer.Instance[key];
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string text)
            {
                foreach (var pair in Keys)
                {
                    if (string.Equals(Localizer.Localizer.Instance[pair.Value], text, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(pair.Key.ToString(), text, StringComparison.OrdinalIgnoreCase))
                    {
                        return pair.Key;
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <summary>将演示数据源枚举显示为当前语言名称。</summary>
    public sealed class FormDataSourceKindConverter : EnumConverter
    {
        private static readonly Dictionary<FormDataSourceKind, string> Keys = new()
        {
            [FormDataSourceKind.None] = "FormDataSourceNone",
            [FormDataSourceKind.Devices] = "FormDataSourceDevices",
            [FormDataSourceKind.Products] = "FormDataSourceProducts",
            [FormDataSourceKind.Customers] = "FormDataSourceCustomers",
            [FormDataSourceKind.QualityRecords] = "FormDataSourceQuality",
        };

        public FormDataSourceKindConverter() : base(typeof(FormDataSourceKind))
        {
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
            Type destinationType)
        {
            if (destinationType == typeof(string) && value is FormDataSourceKind kind &&
                Keys.TryGetValue(kind, out var key))
            {
                return Localizer.Localizer.Instance[key];
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string text)
            {
                foreach (var pair in Keys)
                {
                    if (string.Equals(Localizer.Localizer.Instance[pair.Value], text, StringComparison.OrdinalIgnoreCase))
                    {
                        return pair.Key;
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
