using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenEAP.Model
{
    /// <summary>
    /// 编号规则
    /// </summary>
    [Table("TCodeRule")]
    public class BarcodeRuleEntity : FullAuditedEntity
    {
        #region  实体成员
        [FreeSql.DataAnnotations.Column(IsIgnore = true)]
        public long RuleId { get => this.Id; }

        /// <summary>
        /// 编号
        /// </summary>
        /// <returns></returns>
        public virtual string EnCode { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        /// <returns></returns>
        public virtual string FullName { get; set; }
        /// <summary>
        /// 当前流水号
        /// </summary>
        /// <returns></returns>
        public virtual string CurrentNumber { get; set; }
        /// <summary>
        /// 规则格式格式Json
        /// </summary>
        /// <returns></returns>
        public virtual string RuleFormatJson { get; set; }
        /// <summary>
        /// 排序码
        /// </summary>
        /// <returns></returns>
        public virtual int? SortCode { get; set; } = 0;
        /// <summary>
        /// 有效标志
        /// </summary>
        /// <returns></returns>
        public virtual bool Enabled { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        /// <returns></returns>
        public virtual string Remarks { get; set; }
        #endregion

        #region  扩展操作

        #endregion
    }
}
