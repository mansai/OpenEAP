using AtomUI.Controls;
using Avalonia.Data.Converters;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace OpenEAP.Converters;

public class StringToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is Icon icon)
        {
            return GetIcon(icon.ToString());
        }
        else
        {
            var key = value as string;
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }
            else
            {
                return GetIcon(key);
            }
        }      
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    public Icon GetIcon(string kind)
    {
        try
        {
            var fullTypeName = kind.StartsWith("AtomUI.Icons.AntDesign.")? kind : $"AtomUI.Icons.AntDesign.{kind}";
            var type = typeof(AtomUI.Icons.AntDesign.AntDesignIconKind).Assembly.GetType(fullTypeName)
                        ?? Assembly.GetExecutingAssembly().GetType(fullTypeName);

            if (type == null)
            {
                throw new InvalidOperationException($"{fullTypeName} not exist");
            }

            var icon = (Icon?)Activator.CreateInstance(type);
            Debug.Assert(icon != null);
            return icon;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"create icon {kind} failed", ex);
        }
    }
}
