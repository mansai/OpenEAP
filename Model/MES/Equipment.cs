using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
/********************************************
 * 公司名称：营口金辰机械股份有限公司
 * 创 建 人：wdd
 * 创建日期：2024/05/22
 * 说    明：
 * 版    本：1.0.0.0
 ********************************************/

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TEMS_Equipment")]
    public class Equipment : CreationAuditedEntity
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
        //public JC.AI.MES.SYS.Org? Org { get; set; }

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
        public string? AssetNumber { get; set; }
        /// <summary>
        /// 设备品牌
        /// </summary>
        public long? BrandId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? UserId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(UserId))]
        //public Emp? Emp { get; set; }

        /// <summary>
        /// 设备类型1:物理设备2:虚拟设备
        /// </summary>
        public SByte? TypeId { get; set; }

        public bool IsDeleted { get; set; }

        /// <summary>
        /// 设备组
        /// </summary>
        public long? EquipmentGroupId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(EquipmentGroupId))]
        //public EquipmentGroup? EquipmentGroup { get; set; }

        /// <summary>
        /// 区域
        /// </summary>
        public long? LocationId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(LocationId))]
        //public Localtion? Localtion { get; set; }

        /// <summary>
        /// 生产线
        /// </summary>
        public long? ProductionLineId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(ProductionLineId))]
        //public ProductionLine? ProductionLine { get; set; }

        /// <summary>
        /// 设备 虚拟设备时，需要选择
        /// </summary>
        public long? RealEquipmentId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(RealEquipmentId))]
        //public Equipment? Equipments { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        public long? EquipmentStateId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(EquipmentStateId))]
        //public EquipmentState? EquipmentState { get; set; }

        /// <summary>
        /// 运行速率
        /// </summary>
        public decimal? RunRate { get; set; }
        /// <summary>
        /// 平均时间
        /// </summary>
        public decimal? AverageTime { get; set; }
        /// <summary>
        /// 每小时加工量
        /// </summary>
        public Decimal? WPH { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? IsBatch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? ImageSrc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? Remark { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 禁用状态0有效1、禁用状态
        /// </summary>
        public byte ForbidStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ForbidderId { get; set; }
        /// <summary>
        /// 禁用时间
        /// </summary>
        public DateTime? ForbidDate { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(EquipmentProperties.EquipmentId))]
        //public List<EquipmentProperties> Details { get; set; }

        ///// <summary>
        ///// 设置主键
        ///// </summary>
        ///// <param name="key"></param>
        //public void SetKey(long key)
        //{
        //    Id = key;
        //}
    }
}