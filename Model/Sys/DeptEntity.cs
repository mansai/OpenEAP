using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using FreeSql.DataAnnotations;

using OpenEAP.Model.Entities.Auditing;

namespace OpenEAP.Model
{
    /// <summary>
    /// 部门
    /// </summary>
    [Serializable]
    [Table(Name = "TSysDept")]
    public class DeptEntity : FullAuditedEntity
    {
        #region 构造方法
        /// <summary>
        /// 
        /// </summary>
        public DeptEntity() {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public DeptEntity(long id) : base(id) { }

        #endregion

        #region 实体属性
        /// <summary>
        /// 归属组织
        /// </summary>
        public long? OrgId { get; set; }

        /// <summary>
        /// 父级
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        [Column(DbType = "varchar(38)")]
        public string Code { get; set; }

        /// <summary>
        /// 简称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 全称
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 部门主管
        /// </summary>
        public long? ManagerId { get; set; }

        /// <summary>
        /// 部门助理
        /// </summary>
        public long AssistantId { get; set; }

        /// <summary>
        /// 有效状态
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 第三方Id
        /// </summary>
        [Column(DbType = "varchar(38)")]
        public string SourceId { get; set; }

        #endregion

        #region 导航属性

        [JsonIgnore]
        public UserEntity Manager { get; set; }

        [JsonIgnore]
        public UserEntity Assistant { get; set; }

        [JsonIgnore]
        [Navigate(nameof(OrgId))]
        public OrgEntity Organization { get; set; }

        [JsonIgnore]
        public List<DeptEntity> Childs { get; set; }

        #endregion
    }
}
