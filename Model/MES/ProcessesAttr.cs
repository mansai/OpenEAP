using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TENG_ProcessesAttr")]
    public class ProcessesAttr : CreationAuditedEntity
    {
        /// <summary>
        /// 工序Id
        /// </summary>
        public long? ProcessesId { get; set; }

        /// <summary>
        /// 业务属性Id
        /// </summary>
        public long? PropertyId { get; set; }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string? AttributeCode { get; set; }

        /// <summary>
        /// 属性类型：字符串、整型、布尔
        /// </summary>
        public string? DataType { get; set; }

        /// <summary>
        /// 属性值
        /// </summary>
        public string? AttributeValue { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 状态：1新增 2审核中 3已审核 4退回
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; }

        /// <summary>
        /// 禁用状态 0有效 1禁用
        /// </summary>
        public bool? ForbidStatus { get; set; }

        /// <summary>
        /// 禁用人Id
        /// </summary>
        public Guid? ForbidderId { get; set; }

        /// <summary>
        /// 禁用时间
        /// </summary>
        public DateTime? ForbidDate { get; set; }

        /// <summary>
        /// 扩展属性
        /// </summary>
        public string? ExtraProperties { get; set; }

        /// <summary>
        /// 并发戳
        /// </summary>
        public string? ConcurrencyStamp { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// 最后修改人Id
        /// </summary>
        public Guid? LastModifierId { get; set; }
    }
}