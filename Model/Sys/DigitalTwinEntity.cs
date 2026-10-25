using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenEAP.Model
{
    /// <summary>
    /// 枚举
    /// </summary>
    [Serializable]
    [Table(Name = "TDigitalTwin")]
    public partial class DigitalTwinEntity : FullAuditedEntity
    {
        public DigitalTwinEntity() { }
        public DigitalTwinEntity(long id) : base(id) { }

        /// <summary>
        /// 编号
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string Remark { get; set; }
        /// <summary>
        /// 前端用户
        /// </summary>
        public string WebUserId { get; set; }= "3d";
        /// <summary>
        /// PLC端用户Id
        /// </summary>
        public string PlcUserId { get; set; } = "plc";
        /// <summary>
        /// Plc扫描周期
        /// </summary>
        public int CycleTime { get; set; } = 10;
        /// <summary>
        /// 发送给web端的数据
        /// </summary>
        public string SendData { get; set; }
        public DateTime? SendTime { get; set; }
        /// <summary>
        /// 启用?
        /// </summary>
        public bool? Enable { get; set; } = true;

        /// <summary>
        /// 类别
        /// </summary>
        public long Type { get; set; }
    }
}
