using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace OpenEAP.Helper
{
    public class AttributeHelper
    {
        /// <summary>
        /// 通过反射设置指定对象属性上的Attribute值
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="attri">Attributte Type</param>
        /// <param name="attributeField">Attribute字段</param>
        /// <param name="value">设置值</param>
        public static void SetAttributeFieldValue(object source, string propertyName, Type attri, string attributeField, object value)
        {
            var prop = TypeDescriptor.GetProperties(source)[propertyName];
            if (prop is null) return;

            //反射获取BrowsableAttribute类中名称含有browsable的字段
            var field = attri.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(f => f.Name.Contains(attributeField, StringComparison.OrdinalIgnoreCase));
            if (field is null)
                return;

            //设置新值
            foreach (var item in prop.Attributes)
            {
                if (item is DisplayAttribute)
                {
                    field.SetValue(prop.Attributes[attri], value);
                }
            }
        }
    }
}