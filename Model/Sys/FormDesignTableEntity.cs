using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Serializable]
    [FreeSql.DataAnnotations.Table(Name = "FormDesignTable")]
    public class FormDesignTableEntity : AuditedEntity
    {
        /// <summary>
        /// 表单id
        /// </summary>
        public int FormDesignId { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 1.单头2明细
        /// </summary>
        public int FormType { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public string FormKey { get; set; }
        
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 有效状态
        /// </summary>
        public string Enable { get; set; } = "Y";

        /// <summary>
        /// 删除标记
        /// </summary>
        public string IsDeleted { get; set; } = "N";
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 1;
        public int Version { get; set; } = 1;


        // 导航属性，OneToOne
        //[Navigate(nameof(CreatorId))]
        //public virtual UserEntity Creator { get; set; }

        //[Navigate(nameof(ModifierId))]
        //public virtual UserEntity Modifier { get; set; }
    }
}
