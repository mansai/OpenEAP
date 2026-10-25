using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using OpenEAP.Model;
using OpenEAP.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace OpenEAP.Localizer
{
    public class LExtension : MarkupExtension
    {
        public LExtension(string key)
        {
            this.Key = key;
        }

        public string Key { get; set; }

        public string Context { get; set; }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Localizer))]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget))!;
            var targetObject = target.TargetObject;
            var targetProperty = target.TargetProperty as AvaloniaProperty;
            var targetType = targetProperty?.PropertyType;
            var keyToUse = Key;
            if (!string.IsNullOrWhiteSpace(Context))
                keyToUse = $"{Context}/{Key}";

            var binding = new ReflectionBindingExtension($"[{keyToUse}]")
            {
                Mode = BindingMode.OneWay,
                Source = Localizer.Instance,
            };
#if DEBUG
            var checkedKey = DbHelp.fsql.Select<ResourcesEntity>().Any(t => t.Key == keyToUse);
            if (!checkedKey)
            {
                var lans = DbHelp.fsql.Select<LocalizationEntity>().WhereDynamic(GlobalHelper.WhereEnable).ToList();
                foreach (var item in lans)
                {
                    ResourcesEntity resourcesEntity = new ResourcesEntity
                    {
                        Culture = item.Culture,
                        Key = keyToUse,
                        Value = keyToUse
                    };
                    DbHelp.fsql.Insert(resourcesEntity).ExecuteAffrows();
                }
            }
#endif
            return binding.ProvideValue(serviceProvider);
        }
    }
    public static class LExtensionMethods
    {
        public static object ProvideValueForCode(this LExtension lExtension)
        {
            var keyToUse = !string.IsNullOrWhiteSpace(lExtension.Context)
                ? $"{lExtension.Context}/{lExtension.Key}"
                : lExtension.Key;

            // 模拟XAML中的行为
            var binding = new Binding($"[{keyToUse}]")
            {
                Source = Localizer.Instance,
                Mode = BindingMode.OneWay
            };

#if DEBUG
            // 调试代码
            var checkedKey = DbHelp.fsql.Select<ResourcesEntity>().Any(t => t.Key == keyToUse);
            if (!checkedKey)
            {
                var lans = DbHelp.fsql.Select<LocalizationEntity>().WhereDynamic(GlobalHelper.WhereEnable).ToList();
                foreach (var item in lans)
                {
                    ResourcesEntity resourcesEntity = new ResourcesEntity
                    {
                        Culture = item.Culture,
                        Key = keyToUse,
                        Value = keyToUse
                    };
                    DbHelp.fsql.Insert(resourcesEntity).ExecuteAffrows();
                }
            }
#endif

            return binding;
        }
    }
}
