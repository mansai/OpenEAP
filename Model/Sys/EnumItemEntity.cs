using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    /// <summary>
    /// 枚举明细
    /// </summary>
    [Serializable]
    [Table(Name = "TSysEnumItem")]
    public class EnumItemEntity : AuditedEntity
    {
        public EnumItemEntity() { }
        public EnumItemEntity(long id) : base(id) { }

        /// <summary>
        /// 启用?
        /// </summary>
        public bool? Enable { get; set; } = true;

        /// <summary>
        /// 父Id
        /// </summary>
        public long EnumId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string Remark { get; set; }
        public bool Selected { get; set; }
        public bool IsEnable { get; set; } = true;

        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; } = 0;

        /// <summary>
        /// 值
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string Value { get; set; }

        [Navigate(nameof(EnumId))]
        public EnumEntity Enum { get; set; }
    }
}
