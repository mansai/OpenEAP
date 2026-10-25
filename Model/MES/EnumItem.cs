using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpenEAP.Model.Entities.Auditing;

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TSYS_EnumItem")]
    public class EnumItem : CreationAuditedEntity
    {
        /// <summary>
        /// 主表key
        /// </summary>
        public Guid EnumId { get; set; }

        /// <summary>
        /// 显示值
        /// </summary>
        [StringLength(100)]
        public string EnumItemName { get; set; }
        /// <summary>
        /// 存储值
        /// </summary>
        [StringLength(100)]
        public string EnumItemValue { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        [StringLength(100)]
        public string EnumItemDesc { get; set; }
        /// <summary>
        /// 是否选中
        /// </summary>
        public string Selected { get; set; }
        /// <summary>
        /// 行号
        /// </summary>
        public int Sort { get; set; } = 1;
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remark { get; set; }
        /// <summary>
        /// 数据状态 0：未提交，1：审核中，2：已审核
        /// </summary>
        public byte Status { get; set; }
        /// <summary>
        /// 禁用状态
        /// </summary>
        public byte ForbidStatus { get; set; }
        /// <summary>
        /// 禁用人
        /// </summary>
        public Guid? ForbidderId { get; set; }
        /// <summary>
        /// 禁用日期
        /// </summary>
        public DateTime? ForbidDate { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public Guid? ApproverId { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary>
        public DateTime? ApproveDate { get; set; }


        /// <summary>
        /// 设置主键
        /// </summary>
        /// <param name="key"></param>
        public void SetKey(long key)
        {
            Id = key;
        }
    }
}
