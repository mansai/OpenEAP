using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace OpenEAP.Extensions
{
    /// <summary>
    /// 表达式所有者
    /// </summary>
    /// <remarks>
    /// 可以选择将表达式附加到类。表达式附加到的类称为表达式所有者。
    /// 附加后，表达式的行为类似于所有者类的实例方法，因此可以访问其所有静态、实例、公共和非公共成员。
    /// 使用表达式所有者的主要优点是：
    /// 您可以引用所有者的实例成员 访问字段和属性比使用变量更快
    /// 您可以访问私有成员，而不会受到反射的性能影响 所有者类不必是公共的
    /// 您可以在编译表达式后更改所有者的实例，从而允许您更改表达式的“上下文”
    /// 具有所有者的表达式还可以访问与所有者位于同一模块中的内部类型的公共成员
    /// </remarks>
    public class CustomExpressionOwner
    {
        private int MyField;

        public int MyFunc(int value)
        {
            return 2 * value + 3;
        }

        public void SetFieldValue(int value)
        {
            MyField = value;
        }

        public int MyProperty
        {
            get { return 11; }
        }

        public string Remove(int index, string arrData)
        {
            var tpmData = arrData.Split(",").ToList();
            tpmData.RemoveAt(index);
            return string.Join(",", tpmData);
        }
    }
}
