using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenEAP.Model
{
    [Table("TLifeDate")]
    public class LifeDateEntity : FullAuditedEntity
    {
        /// <summary>
        /// 部件名称
        /// </summary>
        public virtual string PartName { get; set; }
        /// <summary>
        /// 策略类型
        /// </summary>
        public virtual string PolicyType { get; set; } = "5";
        /// <summary>
        /// 预警值
        /// </summary>
        public virtual string AlertValue { get; set; }
        /// <summary>
        /// 预警消息
        /// </summary>
        public virtual string AlertMsg { get; set; }
        /// <summary>
        /// 停机值
        /// </summary>
        public virtual string AlarmValue { get; set; }
        /// <summary>
        /// 停机指令
        /// </summary>
        public virtual string AlarmCommand { get; set; } = "0";
        /// <summary>
        /// 停机消息
        /// </summary>
        public virtual string AlarmMsg { get; set; }
        /// <summary>
        /// 是否需要停机
        /// </summary>
        public virtual bool IsAlarm { get; set; } = false;

        /// <summary>
        /// 实际值
        /// </summary>
        public virtual string RealValue { get; set; }

        /// <summary>
        /// 管控类型
        /// </summary>
        /// <remarks>
        /// 1:周期管理 2.……
        /// </remarks>
        public virtual string Type { get; set; } = "1";

        /// <summary>
        /// 逻辑表达式
        /// </summary>
        public virtual string LogicExp { get; set; }
        /// <summary>
        /// Cron表达式
        /// </summary>
        public virtual string Cron { get; set; }

        public virtual bool Enabled { get; set; } = true;

        public virtual long TaskId { get; set; }

        public virtual string LogicType { get; set; }
        /// <summary>
        /// 所属工站
        /// </summary>
        public virtual string Station { get; set; }
        /// <summary>
        /// 流程编号
        /// </summary>
        public int ChartId { get; set; }
    }
}