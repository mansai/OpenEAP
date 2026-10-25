using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TFormRole")]
    public partial class FormRoleEntity : Entities.Auditing.FullAuditedEntity
    {
        public FormRoleEntity() { }
        public FormRoleEntity(long id) : base(id) { }

        /// <summary>
        /// 角色编号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 备注
        /// </summary>		
        public string Remark { get; set; }

        public bool IsSys { get; set; } = false;

        /// <summary>
        /// 排序
        /// </summary>		
        public int? Sort { get; set; } = 0;

        /// <summary>
        /// 启动?
        /// </summary>		
        public bool Enable { get; set; } = true;

        public UserEntity Creator { get; set; }

        public UserEntity User { get; set; }
    }
}
