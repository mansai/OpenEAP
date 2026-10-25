using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TSYS_Enum")]
    public class Enum
    {
        /// <summary>
        /// 字典群组
        /// </summary>
        public int EnumGroup { get; set; }

        public Guid Id { get; set; }

        public Guid? OrgId { get; set; }

        //数据类型
        public string EnumDataType { get; set; } = string.Empty;

        /// <summary>
        /// 1系统2用户自定义
        /// </summary>
        public SByte? SysType { get; set; }

        /// <summary>
        /// 字典类型
        /// </summary>
        public int EnumType { get; set; } = 1;

        /// <summary>
        /// 字典代码
        /// </summary>
        [StringLength(100)]
        public string EnumCode { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        [StringLength(100)]
        public string EnumDesc { get; set; }

        public string EnumSql { get; set; }

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

        [FreeSql.DataAnnotations.Navigate(nameof(EnumItem.EnumId))]
        public virtual List<EnumItem> Details { get; set; }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        protected Enum(Guid id) { Id = id; }

        /// <summary>
        /// 带参构造
        /// </summary>
        public Enum(Guid id, int enumGroup, int enumType, string enumCode, string enumDesc, string enumSql, string remark, byte status, byte forbidStatus, Guid? forbidderId, DateTime? forbidDate, Guid? approverId, DateTime? approveDate, List<EnumItem> details) : base()
        {
            EnumGroup = enumGroup;
            EnumType = enumType;
            EnumCode = enumCode;
            EnumDesc = enumDesc;
            EnumSql = enumSql;
            Remark = remark;
            Status = status;
            ForbidStatus = forbidStatus;
            ForbidderId = forbidderId;
            ForbidDate = forbidDate;
            ApproverId = approverId;
            ApproveDate = approveDate;
            Details = details;
        }
    }
}
