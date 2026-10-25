using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Serializable]
    [Table(Name = "TSysWebLog")]
    public class WebLogEntity : AuditedEntity
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        [Column(StringLength = 255)]
        public string ApplicationName { get; set; }

        
        public DateTime? ExecutionTime { get; set; }
        public int ExecutionDuration { get; set; }       
        public string SendData { get; set; }
        public string ReceiveData { get; set; }
        public string HttpMethod { get; set; }
        public string Url { get; set; }
        /// <summary>
        /// 异常
        /// </summary>
        public string Exceptions { get; set; }
        public string HttpStatusCode { get; set; }
    }
}
