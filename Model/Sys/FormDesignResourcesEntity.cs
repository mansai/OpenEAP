using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenEAP.Model
{
    /// <summary>
    /// 图片资源文件
    /// </summary>
    [Serializable]
    [Table("TFormDesignResources")]
    public class FormDesignResourcesEntity : AuditedEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }
        /// <summary>
        /// 表单id
        /// </summary>
        public long FormDesignDataId { get; set; }
        /// <summary>
        /// 组件id
        /// </summary>
        public string ControlId { get; set; }
        /// <summary>
        /// 附件名称
        /// </summary>
        public string AttachmentName { get; set; }
        /// <summary>
        /// MD5值
        /// </summary>
        public string MD5 { get; set; }
        /// <summary>
        /// 资源数据
        /// </summary>
        public byte[] ResData { get; set; }

    }
}