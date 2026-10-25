using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace OpenEAP.Model
{
    #region 20220112 bo.liang
    ///// <summary>
    ///// 桌面显示设置
    ///// </summary>
    //[Serializable]
    //[Table(Name = "TSysDesktop")]
    //public class DesktopEntity : INotifyPropertyChanged
    //{
    //    #region 字段

    //    private int? sort;
    //    private string remark = string.Empty;
    //    private string path;
    //    private byte[] iconPhoto;
    //    private DateTime creationTime = DateTime.Now;
    //    private string enable = "Y";

    //    #endregion

    //    #region 实体属性

    //    /// <summary>
    //    /// 创建时间
    //    /// </summary>
    //    public DateTime CreationTime { get => creationTime; set { creationTime = value; NotifyPropertyChanged(); } }

    //    /// <summary>
    //    /// 创建人
    //    /// </summary>
    //    public long CreatorId { get; set; }

    //    /// <summary>
    //    /// 有效状态
    //    /// </summary>
    //    public string Enable { get => enable; set { enable = value; NotifyPropertyChanged(); } }

    //    /// <summary>
    //    /// 图标路径
    //    /// </summary>
    //    public string Icon { get; set; } = string.Empty;

    //    /// <summary>
    //    /// 图标文件
    //    /// </summary>
    //    public byte[] IconPhoto { get => iconPhoto; set { iconPhoto = value; NotifyPropertyChanged(); } }

    //    /// <summary>
    //    /// 是否允许移动位置
    //    /// </summary>
    //    public bool? IsDrag { get; set; } = true;

    //    /// <summary>
    //    /// 隐藏
    //    /// </summary>
    //    public bool? IsHidden { get; set; } = false;

    //    /// <summary>
    //    /// 是否打开
    //    /// </summary>
    //    public bool? IsOpen { get; set; } = false;

    //    /// <summary>
    //    /// 是否允许重命名
    //    /// </summary>
    //    public bool? IsReName { get; set; } = true;

    //    /// <summary>
    //    /// 窗口路径
    //    /// </summary>
    //    public string Path { get => path; set { path = value; NotifyPropertyChanged(); } }

    //    /// <summary>
    //    /// 说明
    //    /// </summary>
    //    public string Remark { get => remark; set { remark = value; NotifyPropertyChanged(); } }

    //    /// 修改时间
    //    /// </summary>
    //    public DateTime? ModificationTime { get; set; }

    //    /// <summary>
    //    /// 修改人
    //    /// </summary>
    //    public long? ModifierId { get; set; }

    //    /// <summary>
    //    /// x轴位置
    //    /// </summary>
    //    public decimal? PositionX { get; set; }

    //    /// <summary>
    //    /// y轴位置
    //    /// </summary>
    //    public decimal? PositionY { get; set; }

    //    /// <summary>
    //    /// 资源Key(多语言，TSysResources)
    //    /// </summary>
    //    public string ResourceKey { get; set; }

    //    /// <summary>
    //    /// 排序
    //    /// </summary>
    //    public int? Sort
    //    {
    //        get => sort;
    //        set { sort = value; NotifyPropertyChanged(); }
    //    }

    //    /// <summary>
    //    /// 系统Id
    //    /// </summary>
    //    [Key]
    //    public int SysId { get; set; }

    //    /// <summary>
    //    /// 菜单编号
    //    /// </summary>
    //    /// <remarks>外键</remarks>
    //    public long MenuId { get; set; }

    //    #endregion

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    /// <summary>
    //    /// 通知客户端属性值已更改
    //    /// </summary>
    //    /// <param name="propertyName"></param>
    //    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //} 
    #endregion

    /// <summary>
    /// 菜单扩展
    /// </summary>
    /// <remarks>桌面显示设置</remarks>
    [Serializable]
    [Table(Name = "TSysDesktop")]
    public class DesktopEntity : AuditedEntity
    {
        #region 实体属性

        /// <summary>
        /// 唯一编号
        /// </summary>
        [Column(Name = "SysId",
            IsIdentity = true, IsPrimary = true)]
        override public long Id { get; set; }

        /// <summary>
        /// 有效状态
        /// </summary>
        public virtual string Enable { get; set; }

        /// <summary>
        /// 图标路径
        /// </summary>
        public virtual string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 图标文件
        /// </summary>
        public virtual byte[] IconPhoto { get; set; }

        /// <summary>
        /// 是否允许移动位置
        /// </summary>
        public virtual bool? IsDrag { get; set; } = true;

        /// <summary>
        /// 隐藏
        /// </summary>
        public virtual bool? IsHidden { get; set; } = false;

        /// <summary>
        /// 是否打开
        /// </summary>
        public virtual bool? IsOpen { get; set; } = false;

        /// <summary>
        /// 是否允许重命名
        /// </summary>
        public virtual bool? IsReName { get; set; } = true;

        /// <summary>
        /// 窗口路径
        /// </summary>
        [Column(Name = "Path")]
        public virtual string FullPath { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public virtual string Remark { get; set; }

        /// <summary>
        /// x轴位置
        /// </summary>
        public virtual decimal? PositionX { get; set; }

        /// <summary>
        /// y轴位置
        /// </summary>
        public virtual decimal? PositionY { get; set; }

        /// <summary>
        /// 资源Key(多语言，TSysResources)
        /// </summary>
        [Column(Name = "ResourceKey")]
        public virtual string LanguageKey { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public virtual int? Sort { get; set; } = 0;

        /// <summary>
        /// 菜单编号
        /// </summary>
        /// <remarks>外键</remarks>
        public virtual long MenuId { get; set; }

        #endregion
    }
}
