using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Serializable]
    [Table(Name = "TSysLocalization")]
    public partial class LocalizationEntity:AuditedEntity
    {
        /// <summary>
        /// 语言标识
        /// </summary>
        [Key]
        [Column(DbType = "nvarchar(255)")]
        public string Culture { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [Column(DbType = "nvarchar(255)")]
        public string Icon { get; set; }
        /// <summary>
        /// 语言名称
        /// </summary>
        [Column(DbType = "nvarchar(255)")]
        public string CultureName { get; set; }

        /// <summary>
        /// 删除标记
        /// </summary>
        [Column(DbType = "nvarchar(1)")]
        public string IsDeleted { get; set; } = "N";

        /// <summary>
        /// 有效状态
        /// </summary>
        public string Enable { get; set; } = "N";

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// 是否默认语言
        /// </summary>
        public string IsDefault { get; set; } = "N";
    }
}
