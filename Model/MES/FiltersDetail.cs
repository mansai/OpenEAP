using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
/********************************************
 * 公司名称：营口金辰机械股份有限公司
 * 创 建 人：满赛
 * 创建日期：2024/04/19
 * 说    明：
 * 版    本：1.0.0.0
 ********************************************/

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TSYS_FiltersDetail")]
    public class FiltersDetail : CreationAuditedEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public long? FilterId { get; set; }
        /// <summary>
        /// 1为系统,2扩展字段,3为用户自定义字段
        /// </summary>
        public int? FilterFiledType { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public string? DataType { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }
        /// <summary>
        /// 字段名称
        /// </summary>
        public string? FiledName { get; set; }
        /// <summary>
        /// 操作符
        /// </summary>
        public string? Operator { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string? DefaultValue { get; set; }
        /// <summary>
        /// 是否默认显示
        /// </summary>
        public string? IsDafaultFilter { get; set; }
        /// <summary>
        /// 高级搜索值显示类型，文本框，下拉列表，复选框等
        /// </summary>
        public string? ValueType { get; set; }
        /// <summary>
        /// 关联字典
        /// </summary>
        public string? EnumCode { get; set; }
        /// <summary>
        /// 下拉默认值，如果配置字典，默认字典优先
        /// </summary>
        public string? ValueOptions { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
        /// <summary>
        /// 启用?
        /// </summary>
        public string? Enable { get; set; }
        /// <summary>
        /// 显示行索引
        /// </summary>
        public int? RowIndex { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double? Width { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? IsRequired { get; set; }

        /// <summary>
        /// 设置主键
        /// </summary>
        /// <param name="key"></param>
        public void SetKey(long key)
        {
            Id = key;
        }

        /// <summary>
        /// Enable = Y / N
        /// </summary>
        public bool IsEnabled => string.Equals(Enable, "Y", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// IsRequired = Y / N
        /// </summary>
        public bool IsRequiredBool => string.Equals(IsRequired, "Y", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// IsDafaultFilter = Y / N
        /// </summary>
        public bool IsDefaultFilterBool => string.Equals(IsDafaultFilter, "Y", StringComparison.OrdinalIgnoreCase);
    }
}
