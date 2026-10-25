using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
/********************************************
 * 公司名称：营口金辰机械股份有限公司
 * 创 建 人：杨勇兵
 * 创建日期：2024-05-25
 * 说    明：
 * 版    本：1.0.0.0
 ********************************************/

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TENG_Processes")]
    public class Processes : CreationAuditedEntity
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
        //public virtual JC.AI.MES.SYS.Org Org { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 0:工序类型
        /// </summary>
        public long TypeId { get; set; }

        /// <summary>
        /// 不良原因
        /// </summary>
        public long? DefectReasonGroupId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(DefectReasonGroupId))]
        //public virtual ReasonGroup DefectReasonGroup { get; set; }

        /// <summary>
        /// 报废原因
        /// </summary>
        public long? ScrapReasonGroupId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(ScrapReasonGroupId))]
        //public virtual ReasonGroup ScrapReasonGroup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 时长（秒）
        /// </summary>
        public long? Times { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

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

        //[FreeSql.DataAnnotations.Navigate(nameof(ProcessesParamter.ProcessesId))]
        //public virtual List<ProcessesParamter> ParamterDetails { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(ProcessesEquipment.ProcessesId))]
        //public virtual List<ProcessesEquipment> EquipmentDetails { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(ProcessesAttr.ProcessesId))]
        //public List<ProcessesAttr> AttrDetails { get; set; }

        /// <summary>
        /// 设置主键
        /// </summary>
        /// <param name="key"></param>
        public void SetKey(long key) { Id = key; }
    }
}