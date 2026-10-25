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
	[Table(Name = "TSysRoleButton")]
	public partial class RoleButtonEntity : Entity
    {
        public RoleButtonEntity() { }
        public RoleButtonEntity(long id) : base(id) { }

		/// <summary>
		/// 按钮编号
		/// </summary>
		public long ButtonId { get; set; }

		/// <summary>
		/// 角色编号
		/// </summary>
		public long RoleId { get; set; }
	}
}
