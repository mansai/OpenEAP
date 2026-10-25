using AngleSharp.Dom;
using Avalonia.Data.Converters;
using OpenEAP.Helper;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Converters;
public class DateTimeFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            // 使用参数传递格式，或者使用默认格式
            var lang=Localizer.Localizer.Instance.Language;            
            string format;
            if (lang == "zh-Hans")
            {
                format = GlobalHelper.DateFormatCn;
            }
            else
            {
                format = GlobalHelper.DateFormatEn;
            }
            if (parameter != null)
            {
                format = parameter as string;
            }
            return dateTime.ToString(format);
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 如果需要双向绑定，实现反向转换
        if (DateTime.TryParse(value?.ToString(), out DateTime result))
        {
            return result;
        }
        return null;
    }
}

