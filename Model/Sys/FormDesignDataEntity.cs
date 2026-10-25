using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenEAP.Model
{
    [Serializable]
    [FreeSql.DataAnnotations.Table(Name = "TFormDesignData")]
    public class FormDesignDataEntity
    {
        #region 实体属性
        /// <summary>
        /// 唯一主键
        /// </summary>
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 父级表单
        /// </summary>
        public virtual int? ParentId { get; set; }

        /// <summary>
        /// 表单id
        /// </summary>
        public long FormDesignId { get; set; }
        /// <summary>
        /// 窗体代码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 表单数据 mysql数据大小默认1mb限制，大于1mb报错：MySqlException: Packets larger than max_allowed_packet are not allowed.，使用命令查看限制show VARIABLES like '%max_allowed_packet%'; 在服务器上打开MySQL选项文件 - my.ini在以下部分mysqld添加以下行：max_allowed_packet=200M,重启mysql
        /// </summary>
        public byte[] FormData { get; set; }
        /// <summary>
        /// xml格式数据
        /// </summary>
        public string XmlData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? EnableDate { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatorId { get; set; }
        

        /// <summary>
        /// 启用权限
        /// </summary>
        public string Auth { get; set; } = "N";
        /// <summary>
        /// 有效状态
        /// </summary>
        public string Enable { get; set; } = "N";

        /// <summary>
        /// 删除标记
        /// </summary>
        public string IsDeleted { get; set; } = "N";

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? ModificationTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public long? ModifierId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 1;

        /// <summary>
        /// 窗体类型：1主窗体
        /// </summary>
        public int? DataType { get; set; } = 2;

        /// <summary>
        /// 关联菜单
        /// </summary>
        public virtual long? MenuId { get; set; }

        #endregion

    }
}
