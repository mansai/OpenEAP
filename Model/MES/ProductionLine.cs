using OpenEAP.Model.Entities.Auditing;
/********************************************
 * 公司名称：营口金辰机械股份有限公司
 * 创 建 人：杨勇兵
 * 创建日期：2024-05-10
 * 说    明：
 * 版    本：1.0.0.0
 ********************************************/

namespace OpenEAP.Model
{
    [Serializable]
    [System.ComponentModel.DataAnnotations.Schema.Table("TBAS_ProductionLine")]
    public class ProductionLine : CreationAuditedEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid? TenantId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 区域id
        /// </summary>
        public long LocaltionId { get; set; }

        /// <summary>
        /// 区域对象
        /// </summary>
        //[Navigate(nameof(LocaltionId))]
        //public virtual Localtion Localtion { get; set; }

        /// <summary>
        /// 等级（无尘车间、保密等)
        /// </summary>
        public long GradeId { get; set; }

        public long? WorkOrderId { get; set; }

        //[Navigate(nameof(WorkOrderId))]
        //public virtual WorkOrder? WorkOrder { get; set; }

        /// <summary>
        /// 启用日期
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 停用日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 状态:1、新增、2、审核中3、已审核、4退回
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// 禁用状态0有效1、禁用状态
        /// </summary>
        public bool ForbidStatus { get; set; }

        /// <summary>
        /// 车间负责人1
        /// </summary>
        public Guid? Manager1Id { get; set; }

        /// <summary>
        /// 车间负责人2
        /// </summary>
        public Guid? Manager2Id { get; set; }

        /// <summary>
        /// 车间负责人3
        /// </summary>
        public Guid? Manager3Id { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 禁用人
        /// </summary>
        public Guid? ForbidderId { get; set; }

        /// <summary>
        /// 禁用时间
        /// </summary>
        public DateTime? ForbidDate { get; set; }
    }
}