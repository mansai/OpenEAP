using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    /// <summary>
    /// 缓存管理
    /// </summary>
    [Table(Name = "TCacheManagement")]
    [Serializable]
    public class CacheManagementEntity : AuditedEntity
    {
        /// <summary>
        /// 程序集
        /// </summary>
        public virtual string AssemblyName { get; set; }

        /// <summary>
        /// 类名
        /// </summary>
        public virtual string ClassName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// 缓存类型
        /// </summary>
        /// <remarks>
        /// 1:Redis 2:RabbitMQ
        /// </remarks>
        public virtual int CacheType { get; set; } = 2;

        /// <summary>
        /// 有效状态
        /// </summary>
        public virtual bool Enable { get; set; } = true;

        [Column(IsIgnore = true)]
        public virtual Dictionary<int, string> CacheTypeEnum => new() { { 1, "Redis" }, { 2, "RabbitMQ" } };

    }
}
