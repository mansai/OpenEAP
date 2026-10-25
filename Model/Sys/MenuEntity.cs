using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;

namespace OpenEAP.Model
{
    /// <summary>
    /// 菜单实体类
    /// </summary>
    [Serializable]
    [Table(Name = "TSysMenu")]
    public partial class MenuEntity : FullAuditedEntity
    {
        /// <summary>
        /// 启用?
        /// </summary>		
        public virtual bool Enable { get; set; } = true;

        /// <summary>
        /// 窗体
        /// </summary>		
        public virtual string Form { get; set; }

        /// <summary>
        /// 图像
        /// </summary>
        [Column(DbType = "image")]
        public virtual byte[] Image { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public virtual string Icon { get; set; }

        /// <summary>
        /// 图像链接
        /// </summary>		
        public virtual string ImageUrl { get; set; }

        /// <summary>
        /// 菜单类型
        /// </summary>
        [Column(DbType = "nvarchar(20)")]
        public virtual string ItemType { get; set; }

        /// <summary>
        /// 类型枚举
        /// </summary>
        [Column(IsIgnore = true)]
        public virtual Dictionary<object, object> ItemTypeEnum { get; set; }

        /// <summary>
        /// 名称
        /// </summary>		
        public virtual string Name { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>		
        public virtual string Namespace { get; set; }

        /// <summary>
        /// 归属
        /// </summary>
        [Column(DbType = "nvarchar(20)")]
        public virtual string Owner { get; set; }

        /// <summary>
        /// 父级
        /// </summary>		
        public virtual long? ParentId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>		
        public virtual int? Sort { get; set; } = 0;

        /// <summary>
        /// 权限编码
        /// </summary>
        public virtual string RightCode { get; set; }

        /// <summary>
        /// 多语言标识
        /// </summary>
        public virtual string LanguageKey { get; set; }

        /// <summary>
        /// 是否报表    @Zane 20260429
        /// </summary>
        //public virtual bool? IsReport { get; set; } = false;
        public long? IsReport { get; set; }

        /// <summary>
        /// 忽略缓存
        /// </summary>
        public virtual bool? IgnoreKeepAlive { get; set; }
        /// <summary>
        /// 内嵌iframe的地址 
        /// </summary>
        public virtual string FrameSrc { get; set; }
        /// <summary>
        /// 当前路由不再菜单显示
        /// </summary>
        public virtual bool? HideMenu { get; set; }
        /// <summary>
        /// 组件路径
        /// </summary>
        public virtual string Component { get; set; }
        /// <summary>
        /// 路由名称
        /// </summary>
        public virtual string RouteName { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        public virtual string Path { get; set; }
        /// <summary>
        /// 快捷键
        /// </summary>
        public virtual string ShortcutKey { get; set; }
        /// <summary>
        /// 图标大小
        /// </summary>
        public virtual string IconSize { get; set; }
        /// <summary>
        /// 显示方式：Content文本，ContentAndGlyph文本和图标
        /// </summary>
        public virtual string DisplayMode { get; set; }
        /// <summary>
        /// 工具提示信息
        /// </summary>
        public virtual string ToolTip { get; set; }
        /// <summary>
        /// 分隔（菜单或按钮分组标记)
        /// </summary>
        public virtual string Separator { get; set; }

        public MenuEntity() { }

        public MenuEntity(long id) : base(id) { }

        /// <summary>
        /// 菜单扩展
        /// </summary>
        /// <remarks>桌面显示配置</remarks>
        [Column(IsIgnore = true)]
        public virtual DesktopEntity MenuExt { get; set; }
    }
}
