using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Table(Name = "TSysRoleUser")]
    [Serializable]
    public class RoleUserEntity : Entity
    {
        public RoleUserEntity() { }
        public RoleUserEntity(long id) : base(id) { }

        public long UserId { get; set; }

        public long RoleId { get; set; }

        #region 导航属性

        public UserEntity User { get; set; }

        public RoleEntity Role { get; set; }

        #endregion
    }
}
