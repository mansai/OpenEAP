using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TBAS_Property")]
    public class Property : CreationAuditedEntity
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
        /// 编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 属性类型：1 工序属性 2 工步属性 3 工单属性
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 数据类型：1 整型 2 布尔 3 文本 4 浮点数
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// 编辑类型
        /// </summary>
        public string? EditType { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否显示下一步，默认N
        /// </summary>
        public string? IsShowNext { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 状态：1新增 2审核中 3已审核 4退回
        /// </summary>
        public byte Status { get; set; }

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

        /// <summary>
        /// 构造函数
        /// </summary>
        public Property()
        {
            Code = string.Empty;
            Name = string.Empty;
            Type = string.Empty;
            DataType = string.Empty;
            DefaultValue = string.Empty;
        }
    }
}
