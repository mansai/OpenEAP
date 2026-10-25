using FreeSql.DataAnnotations;
using FreeSql.Internal.Model;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenEAP.Model
{
    /// <summary>
    /// 高级搜索
    /// </summary>
    [Table(Name = "TAdvancedSearch")]
    public class AdvancedSearchEntity : AuditedEntity, INotifyPropertyChanged
    {
        #region 字段

        private long menuId;
        private string name = string.Empty;
        private DynamicFilterLogic operation = DynamicFilterLogic.And;
        private string filters = string.Empty;
        private bool? isDefault = false;

        #endregion

        #region 属性

        /// <summary>
        /// 表单编号
        /// </summary>
        public long MenuId
        {
            get => this.menuId;
            set
            {
                if (value != this.menuId)
                {
                    this.menuId = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get => this.name;
            set
            {
                if (value != this.name)
                {
                    this.name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 操作类型
        /// </summary>
        public DynamicFilterLogic Operation
        {
            get => this.operation;
            set
            {
                if (value != this.operation)
                {
                    this.operation = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 搜索条件
        /// </summary>
        public string Filters
        {
            get =>filters;
            set
            {
                if (value != filters)
                {
                    filters = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 默认?
        /// </summary>
        public bool? IsDefault
        {
            get => this.isDefault;
            set
            {
                if (value != this.isDefault)
                {
                    this.isDefault = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        #region 导航属性

        #endregion

        #region 构造函数-依赖注入

        public AdvancedSearchEntity() { }

        public AdvancedSearchEntity(long id) : base(id) { }

        #endregion

        #region 事件

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region 方法

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class Filter : INotifyPropertyChanged
    {
        private string _field;
        private string _operator;
        private object _value;

        public string Field
        {
            get => _field;
            set
            {
                if (value != this._field)
                {
                    this._field = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Operator
        {
            get => _operator;
            set
            {
                if (value != this._operator)
                {
                    this._operator = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public object Value
        {
            get => _value;
            set
            {
                if (value != this._value)
                {
                    this._value = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
