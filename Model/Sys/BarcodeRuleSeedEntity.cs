using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenEAP.Model
{
    /// <summary>
    /// 编号规则种子
    /// </summary>
    [Table("TCodeRuleSeed")]
    public class BarcodeRuleSeedEntity : AuditedEntity
    {
        #region 实体成员
        /// <summary>
        /// 编码规则主键
        /// </summary>
        /// <returns></returns>
        public virtual long RuleId { get; set; }
        /// <summary>
        /// 用户主键
        /// </summary>
        /// <returns></returns>
        public virtual long? UserId { get; set; }
        /// <summary>
        /// 种子值
        /// </summary>
        /// <returns></returns>
        public virtual int? SeedValue { get; set; } = 0;
        #endregion

        #region 扩展操作
        public virtual string Variable { get; set; }

        public virtual int InUse { get; set; } = 0;
        #endregion
    }
}
