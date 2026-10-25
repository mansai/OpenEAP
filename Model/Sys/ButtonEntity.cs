using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;

namespace OpenEAP.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [Table(Name = "TSysButton")]
    public partial class ButtonEntity : AuditedEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public ButtonEntity() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public ButtonEntity(long id) : base(id) { }

        /// <summary>
        /// 按钮类型
        /// </summary>		
        public int? ButtonType { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        [Column(DbType = "varchar(38)")]
        public string Code { get; set; }

        /// <summary>
        /// 描述
        /// </summary>		
        public string Description { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string Icon { get; set; }

        /// <summary>
        /// 启用
        /// </summary>		
        public bool? IsEnabled { get; set; } = true;

        /// <summary>
        /// 菜单编号
        /// </summary>		
        public long? MenuId { get; set; }

        /// <summary>
        /// 菜单对象
        /// </summary>
        public MenuEntity Menu { get; set; }

        /// <summary>
        /// 模块编号
        /// </summary>		
        public long? ModuleId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>		
        public string Name { get; set; }

        /// <summary>
        /// 父级
        /// </summary>		
        public long? ParentId { get; set; } = 0;

        /// <summary>
        /// 位置
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string Position { get; set; }

        /// <summary>
        /// 原生类型
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string PrimitiveType { get; set; }

        /// <summary>
        /// 排序
        /// </summary>		
        public int? Sort { get; set; } = 0;

        /// <summary>
        /// 主题
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string Theme { get; set; }

        /// <summary>
        /// 显示
        /// </summary>
        [Column(DbType = "varchar(38)")]
        public string Visibility { get; set; }

        /// <summary>
        /// 权限
        /// </summary>
        public string Permission { get; set; }
    }
}
