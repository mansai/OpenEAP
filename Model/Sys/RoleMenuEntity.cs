using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Serializable]
    [Table(Name = "TSysRoleMenu")]
    public partial class RoleMenuEntity : Entity
    {
        public RoleMenuEntity() { }
        public RoleMenuEntity(long id) : base(id) { }

        /// <summary>
        /// 菜单编号
        /// </summary>
        public long MenuId { get; set; }
        public MenuEntity Menu { get; set; }
        
        /// <summary>
        /// 角色编号
        /// </summary>
        public long RoleId { get; set; }
        public RoleEntity Role { get; set; }
    }
}
