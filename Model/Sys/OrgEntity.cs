using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using OpenEAP.Model.Entities.Auditing;

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TSysOrg")]
    public class OrgEntity : FullAuditedEntity
    {
        public OrgEntity() { }
        public OrgEntity(long id) : base(id) { }

        /// <summary>
        /// 父级
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        [Required(ErrorMessage = "请输入编号")]
        public string Code { get; set; }

        /// <summary>
        /// 简称
        /// </summary>
        [Required(ErrorMessage = "请输入简称")]
        public string Name { get; set; }

        /// <summary>
        /// 全称
        /// </summary>
        [Required(ErrorMessage = "请输入全称")]
        public string FullName { get; set; }

        /// <summary>
        /// 邮件地址
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 对外电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 传真
        /// </summary>
        public string Fax { get; set; }

        /// <summary>
        /// 联系地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 来源编号
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// 有效状态
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;

        [JsonIgnore]
        [FreeSql.DataAnnotations.Navigate(nameof(ParentId))]
        public List<OrgEntity> Childs { get; set; }

        [JsonIgnore]
        [FreeSql.DataAnnotations.Navigate(nameof(DeptEntity.OrgId))]
        public IEnumerable<DeptEntity> Depts { get; set; }
    }
}
