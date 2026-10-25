using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Serializable]
    [Table(Name = "TSysResources")]
    public class ResourcesEntity : AuditedEntity // FullAuditedEntity
    {
        public ResourcesEntity() { }
        public ResourcesEntity(long id) : base(id) { }

        public long ModuleId { get; set; }

        /// <summary>
        /// 语种
        /// </summary>
        [Column(StringLength = 255)]
        public string Culture { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        [Column(StringLength = 100)]
        public string Key { get; set; }

        /// <summary>
        /// 显示值
        /// </summary>
        [Column(StringLength = 255)]
        public string Value { get; set; }

        /// <summary>
        /// 类型
        /// 0菜单1按钮(11标题)2单头(21标题)3单身(31标题)99共用
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 删除标记
        /// </summary>
        public string IsDeleted { get; set; } = "N";

        /// <summary>
        /// 类型名称 额外字段
        /// </summary>
        public EnumItemEntity TypeEnum { get; set; }

        /// <summary>
        /// 模块名称 额外字段
        /// </summary>
        [Navigate(nameof(ModuleId))]
        public virtual MenuEntity Module { get; set; }
    }
}
