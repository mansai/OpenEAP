using System.Collections.Generic;

namespace OpenEAP.Model
{
    /// <summary>
    /// 编号规则格式
    /// </summary>
    public class BarcodeRuleFormatModel
    {
        #region 实体成员
        /// <summary>
        /// 项目类型
        /// </summary>		
        public virtual int ItemType { get; set; }
        /// <summary>
        /// 格式化字符串
        /// </summary>		
        public virtual string FormatStr { get; set; }
        /// <summary>
        /// 步长
        /// </summary>		
        public virtual int? StepValue { get; set; }
        /// <summary>
        /// 初始值
        /// </summary>		
        public virtual int? InitValue { get; set; }
        /// <summary>
        /// 备注
        /// </summary>		
        public virtual string Remarks { get; set; }
        /// <summary>
        /// 特殊转换
        /// </summary>
        public virtual string SpecialConvert { get; set; }

        #endregion

        #region 扩展操作
        public virtual string ItemTypeName { get; set; }
        #endregion
    }
}
