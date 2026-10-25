using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
/********************************************
 * 公司名称：营口金辰机械股份有限公司
 * 创 建 人：杨勇兵
 * 创建日期：2024-06-13
 * 说    明：
 * 版    本：1.0.0.0
 ********************************************/
namespace OpenEAP.Model
{
    [Serializable]
    [Table("TPRD_WorkOrder")]
    public class WorkOrder : CreationAuditedEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? OrgId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(OrgId))]
        //public JC.AI.MES.SYS.Org Org { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }


        /// <summary>
        /// 批次编码规则
        /// </summary>
        public long? LotSequenceId { get; set; }


        /// <summary>
        /// 计划完成日期
        /// </summary>
        public DateTime PlanFinishDate { get; set; }

        /// <summary>
        /// 关闭状态
        /// </summary>
        //public EnumWorkOrderCloseType CloseStatus { get; set; }
        public byte CloseStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? CloserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? CloseDate { get; set; }

        /// <summary>
        /// 工单取消
        /// </summary>
        public byte CancelStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? CancellerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? CancelDate { get; set; }

        /// <summary>
        /// 申请日期
        /// </summary>
        public DateTime ApplyDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? Applicant { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? ApproverId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? ApproveDate { get; set; }

        /// <summary>
        /// 工单类型1正常2返工
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// 0：低 1：中 2：高
        /// </summary>
        public byte? Priority { get; set; }

        /// <summary>
        /// 物料内码
        /// </summary>
        public long MaterialId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(MaterialId))]
        //public virtual MaterialInfo MaterialInfo { get; set; }

        /// <summary>
        /// 关联BOMid
        /// </summary>
        public long? BomId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(BomId))]

        //public BOM Bom { get; set; }

        /// <summary>
        /// 工单创批状态
        /// </summary>
        public byte LotCreateState { get; set; }
        /// <summary>
        /// 工单数量
        /// </summary>
        public Decimal Qty { get; set; }
        /// <summary>
        /// 在制数量
        /// </summary>
        public Decimal WipQty { get; set; }
        /// <summary>
        /// 报废数量
        /// </summary>
        public Decimal ScrappedQty { get; set; }
        /// <summary>
        /// 完工数量
        /// </summary>
        public Decimal FinishedQty { get; set; }
        /// <summary>
        /// 剩余数量
        /// </summary>
        public Decimal LeftQty { get; set; }
        /// <summary>
        /// 重工数量
        /// </summary>
        public Decimal ReworkQty { get; set; }
        /// <summary>
        /// 维修数量
        /// </summary>
        public Decimal RepairQty { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 税收类型（1保税/2完税）Taxation type
        /// </summary>
        public int? TaxType { get; set; }
        /// <summary>
        /// 车间
        /// </summary>
        public long LocaltionId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(LocaltionId))]
        //public virtual Localtion Localtion { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        public long? SaleOrderId { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        public string? SaleOrder { get; set; }

        /// <summary>
        /// 装玻类型 1:单玻 Single glass 2:双玻 Double glass
        /// </summary>
        public int? GlazeType { get; set; }

        /// <summary>
        /// 税号
        /// </summary>
        public string TaxNumber { get; set; }

        /// <summary>
        /// 领料状态(0:未领料;1:部分领料;2:全部领料;3:超领料)
        /// </summary>
        public int PickStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ERPSet { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ERPSetCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 状态:1、新增、2、审核中3、已审核、4退回
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// 禁用状态0有效1、禁用状态
        /// </summary>
        public bool ForbidStatus { get; set; }

        /// <summary>
        /// 
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
        /// 排序顺序
        /// </summary>
        public int? Sort { get; set; }

        public WorkOrder()
        {
            LotCreateState = 0;
        }


        /// <summary>
        /// 工单物料清单
        /// </summary>
        //[FreeSql.DataAnnotations.Navigate(nameof(WorkOrderBom.WorkOrderId))]
        //public virtual List<WorkOrderBom> WorkOrderBomDetails { get; set; }

        /// <summary>
        /// 工单产品
        /// </summary>
        //[FreeSql.DataAnnotations.Navigate(nameof(WorkOrderProduct.WorkOrderId))]
        //public virtual List<WorkOrderProduct> WorkOrderProductDetails { get; set; }

        /// <summary>
        /// 工单线别
        /// </summary>
        //[FreeSql.DataAnnotations.Navigate(nameof(WorkOrderProductionLine.WorkOrderId))]
        //public virtual List<WorkOrderProductionLine> WorkOrderProductionLineDetails { get; set; }

        /// <summary>
        /// 工单工艺路线
        /// </summary>
        //[FreeSql.DataAnnotations.Navigate(nameof(WorkOrderTechnology.WorkOrderId))]
        //public virtual List<WorkOrderTechnology> WorkOrderTechnologyDetails { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(WorkOrderTechnology.WorkOrderId))]
        //public List<WorkOrderAttr> WorkOrderAttrDetails { get; set; }
    }


    ///// <summary>
    ///// 工单优先级。
    ///// </summary>
    //public enum EnumWorkOrderPriority
    //{
    //    /// <summary>
    //    /// 高
    //    /// </summary>
    //    [Display(Name = "EnumWorkOrderPriority_High")]
    //    High = 10,
    //    /// <summary>
    //    /// 正常
    //    /// </summary>
    //    [Display(Name = "EnumWorkOrderPriority_Normal")]
    //    Normal = 5,
    //    /// <summary>
    //    /// 低
    //    /// </summary>
    //    [Display(Name = "EnumWorkOrderPriority_Lower")]
    //    Lower = 1
    //}

    ///// <summary>
    ///// 工单状态。
    ///// </summary>
    //public enum EnumWorkOrderOrderState
    //{
    //    /// <summary>
    //    /// 正在创建（批次未创建）
    //    /// </summary>
    //    [Display(Name = "EnumWorkOrderOrderState_CRTE")]
    //    CRTE = 0,
    //    /// <summary>
    //    /// 批次部分创建
    //    /// </summary>
    //    [Display(Name = "EnumWorkOrderOrderState_PART")]
    //    PART = 1,
    //    /// <summary>    
    //    /// 批次创建完成 
    //    /// </summary>   
    //    [Display(Name = "EnumWorkOrderOrderState_REST")]
    //    REST = 2
    //}

    ///// <summary>
    ///// 工单关闭类型。
    ///// </summary>
    //public enum EnumWorkOrderCloseType
    //{
    //    /// <summary>
    //    /// 未关闭
    //    /// </summary>
    //    [Display(Name = "EnumWorkOrderCloseType_None")]
    //    None = 0,
    //    /// <summary>
    //    /// 正常关闭
    //    /// </summary>
    //    [Display(Name = "EnumWorkOrderCloseType_Normal")]
    //    Normal = 1,
    //    /// <summary>
    //    /// 手动关闭
    //    /// </summary>
    //    [Display(Name = "EnumWorkOrderCloseType_Manual")]
    //    Manual = 2,
    //    /// <summary>
    //    /// 其他关闭
    //    /// </summary>
    //    [Display(Name = "EnumWorkOrderCloseType_Other")]
    //    Other = 3,
    //    /// <summary>
    //    /// 禁用关闭
    //    /// </summary>
    //    [Display(Name = "EnumWorkOrderCloseType_Stop")]
    //    Stop = 4
    //}

}