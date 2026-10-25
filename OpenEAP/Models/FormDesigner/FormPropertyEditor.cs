using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OpenEAP.Models.FormDesigner
{
    /// <summary>
    /// 将表单模型包装为 PropertyGrid 可编辑对象，并把属性名称/分类按当前语言显示。
    /// </summary>
    public sealed class FormPropertyEditor : ICustomTypeDescriptor
    {
        private static readonly Dictionary<string, string> PropertyKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Name"] = "FormPropName",
            ["Kind"] = "FormPropKind",
            ["Text"] = "FormPropText",
            ["Placeholder"] = "FormPropPlaceholder",
            ["OptionsText"] = "FormPropOptions",
            ["X"] = "FormPropX",
            ["Y"] = "FormPropY",
            ["Width"] = "FormPropWidth",
            ["Height"] = "FormPropHeight",
            ["FontSize"] = "FormPropFontSize",
            ["TextColor"] = "FormPropTextColor",
            ["Background"] = "FormPropBackground",
            ["BorderColor"] = "FormPropBorderColor",
            ["CornerRadius"] = "FormPropCornerRadius",
            ["IsRequired"] = "FormPropRequired",
            ["IsEnabled"] = "FormPropEnabled",
            ["IsChecked"] = "FormPropChecked",
            ["DataSource"] = "FormPropDataSource",
            ["DisplayColumn"] = "FormPropDisplayColumn",
            ["ValueColumn"] = "FormPropValueColumn",
            ["Title"] = "FormPropTitle",
            ["Description"] = "FormPropDescription",
            ["Columns"] = "FormPropColumns",
            ["Key"] = "FormPropColumnKey",
            ["Searchable"] = "FormPropSearchable",
        };

        private static readonly Dictionary<string, string> CategoryKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Basic"] = "FormCatBasic",
            ["Layout"] = "FormCatLayout",
            ["Content"] = "FormCatContent",
            ["Appearance"] = "FormCatAppearance",
            ["Behavior"] = "FormCatBehavior",
            ["Data"] = "FormCatData",
        };

        public FormPropertyEditor(object target)
        {
            Target = target;
        }

        public object Target { get; }

        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(Target);
        public string? GetClassName() => TypeDescriptor.GetClassName(Target);
        public string? GetComponentName() => TypeDescriptor.GetComponentName(Target);
        public TypeConverter? GetConverter() => TypeDescriptor.GetConverter(Target);
        public EventDescriptor? GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(Target);
        public PropertyDescriptor? GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(Target);
        public object? GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(Target, editorBaseType);

        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(Target);
        public EventDescriptorCollection GetEvents(Attribute[]? attributes) => TypeDescriptor.GetEvents(Target, attributes);

        public PropertyDescriptorCollection GetProperties()
            => GetProperties(null);

        public PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
        {
            var descriptors = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(Target))
            {
                if (!descriptor.IsBrowsable) continue;
                if (descriptor.Name is "Id" or "IsSelected" or "KindDisplayName" or "HasDataSource") continue;
                if (!IsRelevant(Target, descriptor.Name)) continue;
                descriptors.Add(new LocalizedPropertyDescriptor(descriptor, Target));
            }
            return new PropertyDescriptorCollection(descriptors.ToArray(), true);
        }

        public object? GetPropertyOwner(PropertyDescriptor? pd) => Target;

        private static bool IsRelevant(object target, string propertyName)
        {
            if (target is FormPageModel)
            {
                return propertyName is nameof(FormPageModel.Name) or nameof(FormPageModel.Title)
                    or nameof(FormPageModel.Description) or nameof(FormPageModel.Width)
                    or nameof(FormPageModel.Height) or nameof(FormPageModel.Background);
            }

            if (target is FormColumnDefinition)
            {
                return propertyName is nameof(FormColumnDefinition.Key) or nameof(FormColumnDefinition.Title)
                    or nameof(FormColumnDefinition.Width) or nameof(FormColumnDefinition.Searchable);
            }

            if (target is not FormControlModel control) return true;

            var isDataControl = control.Kind is FormControlKind.DropdownTable or FormControlKind.DataGrid;
            var isInput = control.Kind is FormControlKind.TextBox or FormControlKind.MultilineTextBox
                or FormControlKind.ComboBox or FormControlKind.DropdownTable or FormControlKind.DatePicker;
            var hasText = control.Kind is not (FormControlKind.DataGrid or FormControlKind.Divider);
            var hasFont = control.Kind is not FormControlKind.Divider;

            return propertyName switch
            {
                nameof(FormControlModel.Kind) or nameof(FormControlModel.Name) => true,
                nameof(FormControlModel.Text) => hasText,
                nameof(FormControlModel.Placeholder) => isInput,
                nameof(FormControlModel.OptionsText) => control.Kind == FormControlKind.ComboBox,
                nameof(FormControlModel.X) or nameof(FormControlModel.Y)
                    or nameof(FormControlModel.Width) or nameof(FormControlModel.Height) => true,
                nameof(FormControlModel.FontSize) => hasFont,
                nameof(FormControlModel.TextColor) => hasText,
                nameof(FormControlModel.Background) or nameof(FormControlModel.BorderColor) => true,
                nameof(FormControlModel.CornerRadius) => control.Kind is FormControlKind.Button
                    or FormControlKind.Divider or FormControlKind.Label,
                nameof(FormControlModel.IsRequired) => isInput,
                nameof(FormControlModel.IsEnabled) => control.Kind is not (FormControlKind.Label or FormControlKind.Divider),
                nameof(FormControlModel.IsChecked) => control.Kind == FormControlKind.CheckBox,
                nameof(FormControlModel.DataSource) or nameof(FormControlModel.DisplayColumn)
                    or nameof(FormControlModel.ValueColumn) or nameof(FormControlModel.Columns) => isDataControl,
                _ => true,
            };
        }

        private sealed class LocalizedPropertyDescriptor : PropertyDescriptor
        {
            private readonly PropertyDescriptor _inner;
            private readonly object _target;

            public LocalizedPropertyDescriptor(PropertyDescriptor inner, object target)
                : base(inner, null)
            {
                _inner = inner;
                _target = target;
            }

            public override AttributeCollection Attributes => _inner.Attributes;
            public override Type ComponentType => _inner.ComponentType;
            public override bool IsReadOnly => _inner.IsReadOnly;
            public override Type PropertyType => _inner.PropertyType;

            public override string DisplayName => Localize("FormProp_" + _inner.Name,
                PropertyKeys.TryGetValue(_inner.Name, out var key) ? key : null,
                _inner.DisplayName);

            public override string? Category => Localize("FormCat_" + _inner.Category,
                CategoryKeys.TryGetValue(_inner.Category ?? "Basic", out var key) ? key : null,
                _inner.Category);

            public override string? Description
            {
                get
                {
                    var description = _inner.Description;
                    if (!string.IsNullOrWhiteSpace(description)) return description;

                    var key = "FormProp" + _inner.Name + "Desc";
                    var localized = Localizer.Localizer.Instance[key];
                    return string.Equals(localized, key, StringComparison.Ordinal) ? null : localized;
                }
            }

            public override bool CanResetValue(object component) => _inner.CanResetValue(Resolve(component));
            public override object? GetValue(object? component) => _inner.GetValue(Resolve(component));
            public override void ResetValue(object component) => _inner.ResetValue(Resolve(component));
            public override void SetValue(object? component, object? value) => _inner.SetValue(Resolve(component), value);
            public override bool ShouldSerializeValue(object component) => _inner.ShouldSerializeValue(Resolve(component));

            public override void AddValueChanged(object component, EventHandler handler)
                => _inner.AddValueChanged(Resolve(component), handler);

            public override void RemoveValueChanged(object component, EventHandler handler)
                => _inner.RemoveValueChanged(Resolve(component), handler);

            private object Resolve(object? component)
            {
                return component is FormPropertyEditor editor ? editor.Target : component ?? _target;
            }

            private static string Localize(string fallbackKey, string? key, string? fallback)
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    var value = Localizer.Localizer.Instance[key];
                    if (!string.IsNullOrWhiteSpace(value) && value != key) return value;
                }

                var defaultText = Localizer.Localizer.Instance[fallbackKey];
                if (!string.IsNullOrWhiteSpace(defaultText) && defaultText != fallbackKey) return defaultText;
                return fallback ?? string.Empty;
            }
        }
    }
}
