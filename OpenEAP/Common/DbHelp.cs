using Avalonia.Controls;
using FreeSql;
using OpenEAP.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;

namespace OpenEAP
{
    public partial class DbHelp
    {

        /// <summary>
        /// 获取数据库连接对象
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static IFreeSql CreateFreeSql(string configName)
        {
            ConnectionStringSettings? conn = ConfigHelper.GetConnectionString(configName);
            if (conn == null)
            {
                if (!Design.IsDesignMode)
                {
                    throw new Exception($"配置文件中未找到连接字符串：{configName}");
                }
                else
                {
                    conn=new ConnectionStringSettings();
                    conn.ConnectionString = @"Data Source=F:\OpenEAP\OpenEAP\DB\eap.db;";
                    conn.ProviderName = "Sqlite";
                }                
            }
            if (!Enum.TryParse(conn.ProviderName, true, out DataType dbType)) { throw new Exception($"不支持的 providerName：{conn.ProviderName}"); }
            // |DataDirectory| 解析为程序目录，使连接串可移植
            var connStr = conn.ConnectionString.Replace("|DataDirectory|", AppContext.BaseDirectory);
            return new FreeSqlBuilder().UseConnectionString(dbType, connStr).UseLazyLoading(false)
#if DEBUG
            .UseMonitorCommand(cmd => Debug.WriteLine(cmd.CommandText))
#endif
            .Build();
        }

        /// <summary>
        /// SQLite 数据库
        /// </summary>
        public static IFreeSql fsql = CreateFreeSql("DB");

        /// <summary>
        /// SQL Server 数据库
        /// </summary>
        public static IFreeSql fsqlSqlServer = CreateFreeSql("DB");
    }
}
