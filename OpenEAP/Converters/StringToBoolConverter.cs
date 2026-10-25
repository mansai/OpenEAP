using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace OpenEAP.Converters;
public class StringToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string v)
        {
            return v == "N" ? false : true;
        }
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool v)
        {
            return v ? "Y" : "N";
        }
        return "N";
    }
}