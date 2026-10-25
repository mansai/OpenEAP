using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Converters
{
    // IsCollapsed -> 宽度（像素）
    public class BoolToWidthConverter : IValueConverter
    {
        // parameter 可用来传 collapsedWidth;expandedWidth 等，可选
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var isCollapsed = value as bool? ?? false;
            return isCollapsed ? 48.0 : 200.0; // 折叠时 48px，展开时 200px
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
