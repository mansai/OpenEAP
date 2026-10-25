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
    [FreeSql.DataAnnotations.Table(Name = "TFormDesign")]
    public class FormDesignEntity : AuditedEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否启动
        /// </summary>
        public bool IsStart { get; set; } = false;
        /// <summary>
        /// 保存xml
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 1.表单设计优先，2.Table优先
        /// </summary>
        public int DesignType { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public int? Version { get; set; } = 1;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 界面主题样式
        /// </summary>
        public string ThemeName { get; set; }
        
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

        //// 导航属性，OneToOne
        //[Navigate(nameof(CreatorId))]
        //public virtual UserEntity Creator { get; set; }

        //[Navigate(nameof(ModifierId))]
        //public virtual UserEntity Modifier { get; set; }
    }
}
