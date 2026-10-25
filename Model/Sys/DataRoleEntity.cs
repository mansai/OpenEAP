using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    /// <summary>
    /// 数据角色
    /// </summary>
    [Serializable]
    [FreeSql.DataAnnotations.Table(Name = "TSysDataRole")]
    public class DataRoleEntity:AuditedEntity
    {
        /// <summary>
        /// 数据角色编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 数据角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; } = "1";

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 1;

        /// <summary>
        /// 有效状态
        /// </summary>
        public string Enable { get; set; } = "Y";

        /// <summary>
        /// 删除标记
        /// </summary>
        public string IsDeleted { get; set; } = "N";

        /// <summary>
        /// 默认
        /// </summary>
        public string IsDefault { get; set; } = "N";

        /// <summary>
        /// 数据规则列表
        /// </summary>
        [Navigate(nameof(DataAuthEntity.DataRoleId))]
        public virtual List<DataAuthEntity> DataAuthList { get; set; }
    }
}
