using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
/********************************************
 * 公司名称：营口金辰机械股份有限公司
 * 说    明：异常原因编码定义
 * 版    本：1.0.0.0
 ********************************************/

namespace OpenEAP.Model
{
    [Serializable]
    [Table(Name = "TSys_JudgReasonCode")]
    public class JudgReasonCodeEntity : CreationAuditedEntity
    {
        /// <summary>
        /// 默认编码与原因（0 为删除键）
        /// </summary>
        public static readonly (string Code, string Reason)[] Defaults =
        {
            ("0", "删除键"), ("1", "虚焊"), ("2", "并片"), ("3", "断栅"), ("4", "划伤"),
            ("5", "隐裂"), ("6", "片短路"), ("7", "掉串"), ("8", "加电失败"), ("9", "原料"),
        };

        public JudgReasonCodeEntity() { }
        public JudgReasonCodeEntity(long id) : base(id) { }

        /// <summary>
        /// 数字编码（0 为删除键）
        /// </summary>
        [Column(StringLength = 8)]
        public string Code { get; set; }

        /// <summary>
        /// 编码对应异常原因
        /// </summary>
        [Column(StringLength = 100)]
        public string Reason { get; set; }

        /// <summary>
        /// 有效状态
        /// </summary>
        public string Enable { get; set; } = "Y";

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }
    }
}
