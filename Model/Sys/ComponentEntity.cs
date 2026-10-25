using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenEAP.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [Table(Name = "TSysComponent")]
    public partial class ComponentEntity : AuditedEntity
    {

        /// <summary>
        /// 品牌
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName { get; set; } = string.Empty;

        /// <summary>
        /// 组件代码
        /// </summary>
        public string ComponentCode { get; set; }

        /// <summary>
        /// 组件类型
        /// </summary>
        public string ComponentType { get; set; } = string.Empty;
        /// <summary>
        /// PLC对接表
        /// </summary>
        public byte[] Excel { get; set; }


        /// <summary>
        /// 有效状态
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
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 1;

        /// <summary>
        /// 组件属性
        /// </summary>
        public List<ComponentPropertyEntity> Properties { get; set; }

        //TODO 为了配合组态界面 暂时未删除
        [Column(IsIgnore = true)]
        public string CommunicationValue { get; set; }
    }
}