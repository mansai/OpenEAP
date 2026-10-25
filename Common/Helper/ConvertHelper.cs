using System;
using System.Threading;

namespace OpenEAP.Helper
{
    public static class ConvertHelper
    {
        public static object ChangeType(object obj, Type conversionType)
        {
            return ChangeType(obj, conversionType, Thread.CurrentThread.CurrentCulture);
        }

        private static object ChangeType(object obj, Type conversionType, IFormatProvider provider)
        {
            Type nullableType = Nullable.GetUnderlyingType(conversionType);

            if (nullableType != null)
            {
                if (obj == null)
                {
                    return null;
                }
                return Convert.ChangeType(obj, nullableType, provider);
            }

            if (typeof(Enum).IsAssignableFrom(conversionType))
            {
                return Enum.Parse(conversionType, obj.ToString());
            }
            return Convert.ChangeType(obj, conversionType, provider);
        }
    }
}