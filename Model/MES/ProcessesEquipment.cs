using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenEAP.Model
{
    /// <summary>
    /// 工序关联设备
    /// </summary>
    [Serializable]
    [Table("TENG_ProcessesEquipment")]
    public class ProcessesEquipment : CreationAuditedEntity
    {
        /// <summary>
        /// 租户Id
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 组织Id
        /// </summary>
        public Guid? OrgId { get; set; }

        /// <summary>
        /// 工序Id
        /// </summary>
        public long? ProcessesId { get; set; }

        /// <summary>
        /// 设备Id
        /// </summary>
        public long? EquipmentId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

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
