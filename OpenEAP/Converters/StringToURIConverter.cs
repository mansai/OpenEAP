using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OpenEAP.Converters;

public class StringToURIConverter : IValueConverter
{
    public static readonly string avares = "avares";
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        value ??= parameter;

        if (!value.ToString().StartsWith(avares))
        {
            value = $"avares://JC.AI.Pro{value}";
        }

        try
        {
            return new Uri(value.ToString());
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
