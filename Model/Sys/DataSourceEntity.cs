using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace OpenEAP.Model
{
    /// <summary>
    /// 数据源实体类
    /// </summary>
    [Serializable]
    [Table(Name = "TDataSource")]
    public partial class DataSourceEntity : AuditedEntity, INotifyPropertyChanged
    {
        public DataSourceEntity()
        {
            DataTypes = new Dictionary<string, dynamic>
            {
                {"MySql",("Data Source=[Host];Port=[Port];User ID=[UserId];Password=[Password]; Initial Catalog=[DbName];Charset=utf8; SslMode=none;Min pool size=1","3306","root","" ) },
                {"SqlServer",("Data Source=[Host];User Id=[UserId];Password=[Password];Initial Catalog=[DbName];Pooling=true;Min Pool Size=1","1433","","" )},
                {"Oracle",("user id=[UserId];password=[Password]; data source=//[Host]:[Port]/XE;Pooling=true;Min Pool Size=1","1521","","")},
                {"PostgreSQL",("Host=[Host];Port=[Port];Username=[UserId];Password=[Password]; Database=[DbName];Pooling=true;Minimum Pool Size=1","5432","postgres","postgres")},
                {"Sqlite",(@"Data Source=|DataDirectory|\document.db; Attachs=xxxtb.db; Pooling=true;Min Pool Size=1","","","")},
                {"MonogDB",("mongodb://fred:foobar@localhost/baz","","","")},
                {"Odbc",("Driver={SQL Server};Server=[Host];Persist Security Info=False; Trusted_Connection=Yes;Integrated Security=True; DATABASE=[DbName]; Pooling=true;Min pool size=1","","","")}
            };
        }

        /// <summary>
        /// 连接名
        /// </summary>
        private string connectionName;
        public string Name
        {
            get => connectionName;
            set
            {
                connectionName = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        private string dbType;
        public string DbType
        {
            get => dbType;
            set
            {
                dbType = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 数据库
        /// </summary>
        private string dbName = string.Empty;
        public string DbName
        {
            get => dbName;
            set
            {
                dbName = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 主机
        /// </summary>
        private string host;
        public string Host
        {
            get => host;
            set
            {
                host = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 端口
        /// </summary>
        private string port;
        public string Port
        {
            get => port;
            set
            {
                port = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 用户名
        /// </summary>
        private string userId;
        public string UserId
        {
            get => userId;
            set
            {
                userId = value;
                NotifyPropertyChanged();
            }
        }
        public long CreatorId { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        private string password;
        public string Password
        {
            get => password;
            set
            {
                password = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 连接字符
        /// </summary>
        private string connectionStrings = string.Empty;
        public string ConnectionStrings
        {
            get => connectionStrings;
            set
            {
                connectionStrings = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 自动同步实体结构到数据库
        /// </summary>
        /// <remarks>
        ///【开发环境必备】自动同步实体结构到数据库，程序运行中检查实体创建或修改表结构
        /// </remarks>
        public bool AutoSyncStructure { get; set; } = false;

        /// <summary>
        /// 开启延时加载功能
        /// </summary>		
        public bool LazyLoading { get; set; } = true;

        /// <summary>
        /// 不使用命令参数化执行
        /// </summary>
        /// <remarks>
        /// 不使用命令参数化执行，针对 Insert/Update，也可临时使用 IInsert/IUpdate.NoneParameter()
        /// </remarks>
        public bool NoneCommandParameter { get; set; } = true;

        [XmlIgnore]
        public Dictionary<string, dynamic> DataTypes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知客户端属性值已更改
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 保存
        /// </summary>

        public void Save()
        {
            try
            {
                ConnectionStrings = (((string, string, string, string))DataTypes.FirstOrDefault(x => x.Key.Equals(dbType, StringComparison.Ordinal)).Value).Item1;
                foreach (PropertyInfo item in GetType().GetProperties())
                {
                    if (item.Name.Equals("Error", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    ConnectionStrings = ConnectionStrings.Replace($"[{item.Name}]", item.GetValue(this).ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}