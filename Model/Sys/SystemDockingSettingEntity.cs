using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;

namespace OpenEAP.Model
{
    /// <summary>
    /// 系统对接设置实体类
    /// </summary>
    [Table(Name = "TSds")]
    [Serializable]
    public partial class SDSEntity : FullAuditedEntity
    {
        /// <summary>
        /// 对接名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 详情描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 接口类型
        /// </summary>
        /// <remarks>
        /// 1:WebAPI 2:WebService 3.Tcp/Ip
        /// </remarks>
        public virtual int ApiType { get; set; }

        [Column(IsIgnore = true)]
        public virtual Dictionary<int, string> ApiTypeEnum { get; set; }
        //=> new()
        //{
        //  { 1, "WebAPI" }, { 2, "WebService" }, { 3, "Tcp/Ip" }
        //};

        /// <summary>
        /// 请求类型
        /// </summary>
        /// <remarks>
        /// Get Or Post
        /// </remarks>
        public virtual string RequestType { get; set; }

        [Column(IsIgnore = true)]
        public virtual Dictionary<int, string> RequestTypeEnum => new() { { 1, "Get" }, { 2, "Post" } };

        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType { get; set; }

        [Column(IsIgnore = true)]
        public virtual Dictionary<int, Dictionary<int, string>> ContentTypeEnums { get; set; }
        //=>new()
        //{
        //    { 1, new() { { 0, "application/json" } } },
        //    { 2, new() { { 0, "application/x-www-form-urlencoded" }, { 1, "application/soap+xml;charset=utf-8" }, { 2, "text/xml;charset=utf-8" } } }
        //};

        [Column(IsIgnore = true)]
        public virtual Dictionary<int, string> ContentTypeEnum { get; set; }

        /// <summary>
        /// 占位符
        /// </summary>
        public virtual string Raw { get; set; }

        /// <summary>
        /// 请求地址
        /// </summary>
        public virtual string Url { get; set; }

        /// <summary>
        /// 有效状态
        /// </summary>
        public virtual bool? Enable { get; set; } = true;

        /// <summary>
        /// 结果类型
        /// </summary>
        /// <remarks>
        /// 1:XML 2:Json 3:Text
        /// </remarks>
        public virtual int? ResultType { get; set; } = 2;

        [Column(IsIgnore = true)]
        public virtual Dictionary<int, string> ResultTypeEnum => new() { { 1, "Xml" }, { 2, "Json" }, { 3, "Text" } };

        /// <summary>
        /// 结果是否分页
        /// </summary>
        public virtual bool? ResultIsPage { get; set; } = false;

        /// <summary>
        /// 分页信息映射
        /// </summary>
        public virtual string PageInfo { get; set; }

        /// <summary>
        /// 分页替换关系
        /// </summary>
        public virtual string PageReplaceRelation { get; set; }

        /// <summary>
        /// 结果节点
        /// </summary>
        public virtual string ValueNode { get; set; }

        /// <summary>
        /// 结果映射
        /// </summary>
        public virtual string ResultMapper { get; set; }

        /// <summary>
        /// 关联键字段
        /// </summary>
        public virtual string KeyFiled { get; set; } = "SourceId";

        /// <summary>
        /// 本地存储方式
        /// </summary>
        /// <remarks>
        /// 1:Assembly 2:Sql 3:Text
        /// </remarks>
        public virtual int LocalStorageType { get; set; }

        [Column(IsIgnore = true)]
        public virtual Dictionary<int, string> LocalStorageTypeEnum => new() { { 1, "程序集" }, { 2, "Sql Or Procedure" }, { 3, "文本" } };

        [Column(IsIgnore = true)]
        public SDSToAssemblyEntity AssemblyModel { get; set; }

        [Column(IsIgnore = true)]
        public SDSToSqlEntity SqlModel { get; set; }
    }

    /// <summary>
    /// 本地存储之程序集实体类
    /// </summary>
    [Table(Name = "TSdsToAssembly")]
    [Serializable]
    public partial class SDSToAssemblyEntity : Entity
    {
        public virtual long SdsId { get; set; }

        /// <summary>
        /// 程序集
        /// </summary>
        public virtual string AssemblyName { get; set; }

        /// <summary>
        /// 类名
        /// </summary>
        public virtual string ClassName { get; set; }

        /// <summary>
        /// 方法名
        /// </summary>
        public virtual string MethodName { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public virtual string Params { get; set; }

     
    }

    /// <summary>
    /// 本地存储之Sql实体类
    /// </summary>
    [Table(Name = "TSdsToSql")]
    [Serializable]
    public partial class SDSToSqlEntity : Entity
    {
        public virtual long SdsId { get; set; }

        /// <summary>
        /// 命令类型
        /// </summary>
        /// <remarks>
        /// sql or procedure
        /// </remarks>
        public virtual int? CommandType { get; set; }

        [Column(IsIgnore = true)]
        public virtual Dictionary<int, string> CommandTypeEnum => new() { { 1, "T-Sql" }, { 2, "Procedure" } };

        /// <summary>
        /// Sql语句
        /// </summary>
        public virtual string SqlString { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public virtual string Params { get; set; }
     
    }
}
