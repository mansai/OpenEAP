using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenEAP.Model
{
    /// <summary>
    /// 枚举
    /// </summary>
    [Serializable]
    [Table(Name = "TSysEnum")]
    public partial class EnumEntity : FullAuditedEntity
    {
        public EnumEntity() { }
        public EnumEntity(long id) : base(id) { }

        /// <summary>
        /// 编号
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string Remark { get; set; }

        /// <summary>
        /// 系统枚举
        /// </summary>
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// 启用?
        /// </summary>
        public bool? Enable { get; set; } = true;
        public int Sort { get; set; }

        /// <summary>
        /// 字典类别
        /// </summary>
        public int EnumType { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public string EnumDataType { get; set; }
        /// <summary>
        /// 自定义sql
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// 枚举明细
        /// </summary>
        [FreeSql.DataAnnotations.Navigate(nameof(EnumItemEntity.EnumId))]
        public List<EnumItemEntity> EnumItems { get; set; }
    }


    /// <summary>
    /// 枚举视图
    /// </summary>
    [Serializable]
    [Table(Name ="vEnum")]
    public partial class EnumViewEntity : Entity
    {
        /// <summary>
        /// 唯一编码
        /// </summary>
        public virtual string Code { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public virtual string Remark { get; set; }
    }
}
