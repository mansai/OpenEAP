using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;

namespace OpenEAP.Model
{
    [Serializable]
    [Table(Name = "TSysComponentProperty")]
    public partial class ComponentPropertyEntity : AuditedEntity
    {

        public string ComponentCode { get; set; } = string.Empty;

        public long ComponentId { get; set; }

        /// <summary>
        /// 有效状态
        /// </summary>
        public string Enable { get; set; } = "Y";

        /// <summary>
        /// 删除标记
        /// </summary>
        public string IsDeleted { get; set; } = "N";

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 1;
        

        public string PropertyGroup { get; set; } = string.Empty;

        public string PropertyName { get; set; } = string.Empty;

        public string PropertyValue { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = string.Empty;
    }
}