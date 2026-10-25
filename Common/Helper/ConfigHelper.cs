using System.Configuration;

namespace OpenEAP.Helper
{
    public class ConfigHelper
    {
        static Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);        

        // 加载数据库连接字段
        public static ConnectionStringSettings GetConnectionString(string key)
        {
            ConnectionStringSettings connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[key];
            return connectionString;
        }

        public static string Get(string key)
        {
            //var getKey = cfa.AppSettings.Settings[key];
            KeyValueConfigurationElement getKey = cfa.AppSettings.Settings[key];
            if (getKey == null)
            {
                return null;
            }
            else
            {
                return getKey.Value;
            }
        }
        public static void Set(string key,string value)
        {            
            cfa.AppSettings.Settings[key].Value = value;
            cfa.Save();
        }
        public static void Add(string key, string value)
        {
            var check = cfa.AppSettings.Settings.AllKeys.Any(s => s == key);
            if (check)
            {
                cfa.AppSettings.Settings[key].Value = value;
            }
            else
            {
                cfa.AppSettings.Settings.Add(key, value);
            }
            cfa.Save();
        }
        public static void Remove(string key)
        {
            cfa.AppSettings.Settings.Remove(key);
            cfa.Save();
        }
    }
}
