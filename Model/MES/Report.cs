using OpenEAP.Model.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
/********************************************
 * 公司名称：营口金辰机械股份有限公司
 * 创 建 人：王丁丁
 * 创建日期：2024/06/29
 * 说    明：
 * 版    本：1.0.0.0
 ********************************************/

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TSYS_Report")]
    public class Report : CreationAuditedEntity
    {
        /// <summary>
        /// 报表分类
        /// </summary>
        public long? CategoryId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(CategoryId))]
        //public virtual ReportCategory? ReportCategory { get; set; }

        /// <summary>
        /// 菜单编号
        /// </summary>
        public long? MenuId { get; set; }

        [FreeSql.DataAnnotations.Navigate(nameof(MenuId))]
        //public virtual Menu? Menu { get; set; }

        /// <summary>
        /// 报表编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 报表名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
        /// <summary>
        /// 报表文件路径
        /// </summary>
        public string? Path { get; set; }
        /// <summary>
        /// 报表序列化xml
        /// </summary>
        public string? Xml { get; set; }
        /// <summary>
        /// 报表数据sql
        /// </summary>
        public string SqlText { get; set; }
        /// <summary>
        /// 导出文件名（带径给客户端使用）
        /// </summary>
        public string? FileName { get; set; }
        /// 图片路径
        public string? ImagePath { get; set; }
        /// 图片名称
        public string? ImageName { get; set; }
        /// 自动加载
        public string AutoLoad { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? Sort { get; set; }

        public long? FilterId { get; set; }

        //[FreeSql.DataAnnotations.Navigate(nameof(FilterId))]
        //public virtual Filters? Filters { get; set; }

        public string? ExportFormat { get; set; }
        /// <summary>
        /// 禁用状态0有效1、禁用状态
        /// </summary>
        public byte? ForbidStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ForbidderId { get; set; }
        /// <summary>
        /// 禁用时间
        /// </summary>
        public DateTime? ForbidDate { get; set; }

        public bool IsDeleted { get; set; }
        /// <summary>
        /// 超时
        /// </summary>
        public int Timeout { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public string? ReportOrderBy { get; set; }
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