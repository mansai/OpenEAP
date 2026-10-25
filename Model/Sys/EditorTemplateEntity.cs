using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;

namespace OpenEAP.Model
{
    /// <summary>
    /// 模板编辑器
    /// </summary>
    [Serializable]
    [Table(Name = "TEditorTemplate")]
    public partial class EditorTemplateEntity : FullAuditedEntity
    {
        public EditorTemplateEntity() { }

        public EditorTemplateEntity(long id) : base(id) { }

        /// <summary>
        /// 类别
        /// </summary>
        public virtual long Category { get; set; } = 4;

        /// <summary>
        /// 描述
        /// </summary>        
        public virtual string Description { get; set; } = string.Empty;

        /// <summary>
        /// 启用?
        /// </summary>        
        public virtual bool Enable { get; set; } = true;

        /// <summary>
        /// 缩略图
        /// </summary>
        [Column(DbType = "image")]
        public virtual byte[] Thumbnail { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public virtual string Title { get; set; } = string.Empty;

        #region 扩展属性

        /// <summary>
        /// 分类枚举
        /// </summary>
        [Column(IsIgnore = true)]
        public virtual EnumEntity CategoryEnum { get; set; }

        /// <summary>
        /// 分类集合
        /// </summary>
        [Column(IsIgnore = true)]
        public virtual ObservableCollection<object> Categories { get; set; } 

        #endregion

    }
}
