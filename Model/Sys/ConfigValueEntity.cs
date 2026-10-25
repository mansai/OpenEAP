using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenEAP.Model
{
    /// <summary>
    /// 配置项明细
    /// </summary>
    [Serializable]
    [Table("TSysConfigValue")]
    public class ConfigValueEntity : AuditedEntity
    {
        #region 实体属性

        public long ConfigId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ConfigType { get; set; } = string.Empty;

        /// <summary>
        /// 启用?
        /// </summary>
        public string Enable { get; set; } = "Y";

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; } = 1;

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; } = string.Empty;

        #endregion

        #region 导航属性

        [FreeSql.DataAnnotations.Navigate(nameof(ConfigId))]
        public ConfigEntity Config { get; set; }

        #endregion
    }
}
