using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Helper
{
    public static class DataTableHelper
    {
        public static List<T> ToList<T>(this DataTable dt)
        {
            var dataColumn = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

            var properties = typeof(T).GetProperties();
            string columnName = string.Empty;

            return dt.AsEnumerable().Select(row =>
            {
                var t = Activator.CreateInstance<T>();
                foreach (var p in properties)
                {
                    columnName = p.Name;
                    if (dataColumn.Contains(columnName))
                    {
                        if (!p.CanWrite)
                            continue;

                        object value = row[columnName];
                        Type type = p.PropertyType;

                        if (value != DBNull.Value)
                        {
                            p.SetValue(t, Convert.ChangeType(value, type), null);
                        }
                    }
                }
                return t;
            }).ToList();
        }
        public static DataTable ToDataTable<T>(this List<T> items)
        {
            DataTable dataTable = new DataTable();

            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                dataTable.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (T obj in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(obj, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
        public static List<T> DataTableToList<T>(DataTable dt) where T : class, new()
        {
            // 定义集合 
            List<T> ts = new List<T>();
            //定义一个临时变量 
            string tempName = string.Empty;
            //遍历DataTable中所有的数据行 
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性 
                PropertyInfo[] propertys = t.GetType().GetProperties();
                //遍历该对象的所有属性 
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;//将属性名称赋值给临时变量 
                                       //检查DataTable是否包含此列（列名==对象的属性名）  
                    if (dt.Columns.Contains(tempName))
                    {
                        //取值 
                        object value = dr[tempName];
                        //如果非空，则赋给对象的属性 
                        if (value != DBNull.Value)
                        {
                            if (pi.PropertyType != typeof(string))
                            {

                                if (pi.PropertyType.BaseType==typeof(Enum))
                                {
                                    pi.SetValue(t, Enum.Parse(pi.PropertyType,value.ToString()), null);
                                }
                                else
                                {
                                    pi.SetValue(t, Convert.ChangeType(value, pi.PropertyType), null);
                                }
                            }
                            else
                            {

                                pi.SetValue(t, value, null);
                            }
                        }
                    }
                }
                //对象添加到泛型集合中 
                ts.Add(t);
            }
            return ts;
        }
    }
}
