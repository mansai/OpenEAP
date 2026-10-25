using Avalonia.Data.Converters;
using OpenEAP.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Converters;

public class ConfigValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is List<ConfigValueEntity> configList && parameter is string configName)
        {
            return configList.FirstOrDefault(x => x.Name == configName)?.Value;
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
