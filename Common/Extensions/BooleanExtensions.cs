using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OpenEAP.Extensions
{
    /// <summary>
    /// 转布尔类型静态拓展
    /// </summary>
    public static class BooleanExtensions
    {
        /// <summary>
        /// 字符串转布尔类型静态拓展方法，
        /// 要转换的字符串，true,1,y,ok,success,succeed,good,enable转为True,其他转为false
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ToBoolean(this string value)
        {
            switch (value.ToLower()) {
                case "true":
                case "1":
                case "y":
                case "yes":
                case "ok":
                case "success":
                case "succeed":
                case "good":
                case "enable":
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 数字转布尔类型静态拓展，0==>false;1==true
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ToBoolean(this int value)
        {
            return value == 0 ? false : true;
        }
        /// <summary>
        /// 数字转布尔类型静态拓展，0==>false;1==true
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ToBoolean(this int? value)
        {
            return (value is null || value == 0) ? false : true;
        }
        /// <summary>
        /// short类型转Boolean，1为true,其他为false
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ToBoolean(this short? value)
        {
            return (value is null || value != 1) ? false : true;
        }

        public static bool ToBoolean(this bool value)
        {
            return value;
        }
        /// <summary>
        /// 布尔值转Y或N
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToYorN(this bool value)
        {
            return value ? "Y" : "N";
        }
        /// <summary>
        /// 布尔值转Yes或No
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToYesOrNo(this bool value)
        {
            return value ? "Yes" : "No";
        }
        /// <summary>
        /// 布尔值转OK或NG
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToOKorNG(this bool value)
        {
            return value ? "OK" : "NG";
        }
    }
}
