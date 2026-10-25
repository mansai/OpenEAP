using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;

namespace OpenEAP.Model
{
    [Serializable]
    [Table(Name = "TProductionType")]
    public partial class ProductTypeEntity : AuditedEntity
    {

        /// <summary>
        /// 名称
        /// </summary>
        public virtual string TypeName { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public virtual string TypeCode { get; set; }

        /// <summary>
        /// 图标文件
        /// </summary>
        public virtual byte[] Image { get; set; }

        /// <summary>
        /// 有效状态
        /// </summary>
        public virtual string Enable { get; set; } = "1";

        /// <summary>
        /// 说明
        /// </summary>
        public virtual string Remark { get; set; }

    }
}
