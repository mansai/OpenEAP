using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model.Sys
{
    /// <summary>
    /// 自动编号
    /// </summary>
    [Serializable]
    [FreeSql.DataAnnotations.Table(Name = "TSysSequence")]
    public class SequenceEntity : AuditedEntity
    {
        /// <summary>
        /// 构造方法-依赖注入
        /// </summary>
        public SequenceEntity()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("SequenceId")]
        override public long Id { get; set; }

        /// <summary>
        /// 序列名称
        /// </summary>
        public string SequenceName { get; set; }

        /// <summary>
        /// 开始序号
        /// </summary>
        public int? StartSequence { get; set; } = 1;

        /// <summary>
        /// 当前序号
        /// </summary>
        public long CurrentValue { get; set; }

        /// <summary>
        /// 显示格式
        /// </summary>
        public string Format { get; set; } = "YMDN";

        /// <summary>
        /// 应用名称
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// 前缀
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// 后缀
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// 序列长度
        /// </summary>
        public int? SequenceLenght { get; set; } = 3;

        /// <summary>
        /// 步长
        /// </summary>
        public int? Increment { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public int? MaxValue { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        /// <summary>
        /// 日期格式
        /// </summary>
        public string DateFormat { get; set; } = "yy";

        /// <summary>
        /// 获取数量
        /// </summary>
        public byte? GetAmount { get; set; }

        /// <summary>
        /// 自定义
        /// </summary>
        public string CustomValues { get; set; }

        /// <summary>
        /// 周期
        /// </summary>
        public string Cycle { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 有效状态
        /// </summary>
        public string Enable { get; set; } = "Y";

        /// <summary>
        /// 删除标记
        /// </summary>
        public string IsDeleted { get; set; } = "N";

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 1;
    }
}
