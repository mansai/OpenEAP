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
    [Table("TSYS_Filters")]
    public class Filters : CreationAuditedEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid? TenantId { get; set; }
        /// <summary>
        /// 1系统2用户自定义
        /// </summary>
        public int FilterType { get; set; } = 2;
        /// <summary>
        /// 布局：1、默认一个文本框2表单搜索
        /// </summary>
        public int Layout { get; set; } = 1;
        /// <summary>
        /// 标签宽度
        /// </summary>
        public string? LabelWidth { get; set; }
        /// <summary>
        /// 0全局1公司2部门3自己
        /// </summary>
        public int FilterScope { get; set; } = 3;
        /// <summary>
        /// 编号
        /// </summary>
        public string? Code { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 默认搜索列
        /// </summary>
        public string? DefaultFileds { get; set; }
        /// <summary>
        /// 自定义搜索条件
        /// </summary>
        public string? UserFilter { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? Sort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 是否导出（0：否，1：是）
        /// </summary>
        public byte? IsExport { get; set; }
        /// <summary>
        /// 导出格式
        /// </summary>
        public string? ExportFormat { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(FiltersDetail.FilterId))]
        //public virtual List<FiltersDetail> Details { get; set; }

        /// <summary>
        /// 设置主键
        /// </summary>
        /// <param name="key"></param>
        public void SetKey(long key)
        {
            Id = key;
        }
    }
}
