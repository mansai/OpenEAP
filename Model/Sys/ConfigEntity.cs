using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenEAP.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [Table("TSysConfig")]
    public class ConfigEntity : AuditedEntity
    {
        #region 实体属性
       

        /// <summary>
        /// 编号
        /// </summary>
        [Key]
        public string ConfigCode { get; set; } = string.Empty;

        /// <summary>
        /// 名称
        /// </summary>
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string EditorType { get; set; } = string.Empty;

        /// <summary>
        /// 设置的值
        /// </summary>
        public string ConfigValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        public object ConfigImage { get; set; }

        /// <summary>
        /// 复选框
        /// </summary>
        [FreeSql.DataAnnotations.Navigate(nameof(ConfigCode))]
        public List<ConfigValueEntity> ListValue { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public List<object> ListValueSelected { get; set; }

        //public List<object> ListValue
        //{
        //	get
        //	{
        //		return configValue;
        //	}
        //	set 
        //	{
        //		configValue = value;
        //	}
        //}

        /// <summary>
        /// 启用?
        /// </summary>
        public string Enable { get; set; } = "Y";

        /// <summary>
        /// 删除标记
        /// </summary>
        public string IsDeleted { get; set; } = "N";

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public int? Sort { get; set; } = 1;

        /// <summary>
        /// 分类
        /// </summary>
        public short? SysType { get; set; }

        #endregion
    }
}
