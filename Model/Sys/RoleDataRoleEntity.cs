using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Table("TSysRoleDataRole")]
    public partial class RoleDataRoleEntity : Entities.Entity
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        public virtual long RoleId { get; set; }

        /// <summary>
        /// 数据角色编号
        /// </summary>
        public virtual long DataRoleId { get; set; }
    }
}
