using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Serializable]
    [Table(Name = "TSysLogs")]
    public class LogEntity : Entity
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        [Column(StringLength = 255)]
        public string ApplicationName { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public int UserId { get; set; }
        public string UserName { get; set; }

        public int ImpersonatorUserId { get; set; }
        public DateTime? ExecutionTime { get; set; }
        public int ExecutionDuration { get; set; }
        public string ClientIpAddress { get; set; }
        public string ClientName { get; set; }
        public int ClientId { get; set; }
        public int CorrelationId { get; set; }
        public string BrowserInfo { get; set; }
        public string HttpMethod { get; set; }
        public string Url { get; set; }
        public long CreatorId { get; set; }
        /// <summary>
        /// 异常
        /// </summary>
        public string Exceptions { get; set; }
        public string Comments { get; set; }
        public string HttpStatusCode { get; set; }
        /// <summary>
        /// 拓展属性
        /// </summary>
        public string ExtraProperties { get; set; }
        public string ConcurrencyStamp { get; set; }
    }
}
