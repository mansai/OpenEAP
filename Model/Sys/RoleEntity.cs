using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Serializable]
    [Table(Name = "TSysRole")]
    public partial class RoleEntity : FullAuditedEntity
    {
        public RoleEntity() { }
        public RoleEntity(long id) : base(id) { }

        /// <summary>
        /// 角色编号
        /// </summary>
        [Column(DbType = "varchar(38)")]
        public string Code { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        [Column(DbType = "nvarchar(50)")]
        public string Name { get; set; }

        /// <summary>
        /// 备注
        /// </summary>		
        public string Remark { get; set; }

        /// <summary>
        /// 排序
        /// </summary>		
        public int? Sort { get; set; } = 0;

        /// <summary>
        /// 启动?
        /// </summary>		
        public bool Enable { get; set; } = true;

        #region 导航属性
        public ICollection<RoleMenuEntity> RoleMenus { get; set; }

        [FreeSql.DataAnnotations.Navigate("RoleId")]
        public virtual List<RoleDataRoleEntity> RoleDataRoles { get; set; }

        #endregion
    }
}
