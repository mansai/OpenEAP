using FreeSql.DataAnnotations;
using OpenEAP.Model.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OpenEAP.Model
{
    [Serializable]
    [Table(Name = "TSysUser")]
    public partial class UserEntity : FullAuditedEntity
    {
        public UserEntity() { }
        public UserEntity(long id) : base(id) { }

        /// <summary>
        /// 住址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        [Column(DbType = "date")]
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 电子邮箱
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string Email { get; set; }

        /// <summary>
        /// 有效状态
        /// </summary>

        public bool Enable { get; set; } = true;

        /// <summary>
        /// 姓名
        /// </summary>
        [Column(DbType = "nvarchar(128)")]
        public string Name { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string Password { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [Column(DbType = "varchar(20)")]
        public string Phone { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// 在职部门
        /// </summary>
        public long? DeptId { get; set; }

        /// <summary>
        /// 第三方Id
        /// </summary>
        [Column(DbType = "varchar(38)")]
        public string SourceId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>		
        public string UserName { get; set; }

        /// <summary>
        /// 照片
        /// </summary>
        public byte[] Photo { get; set; }

        #region 导航属性

        /// <summary>
        /// 在职部门
        /// </summary>
        [JsonIgnore]
        [FreeSql.DataAnnotations.Navigate("DeptId")]
        public DeptEntity Department { get; set; }

        /// <summary>
        /// 拥有角色
        /// </summary>
        [JsonIgnore]
        public ICollection<RoleUserEntity> RoleUsers { get; set; }

        [JsonIgnore]
        [Navigate(ManyToMany = typeof(RoleUserEntity))]
        public virtual ICollection<RoleEntity> Roles { get; set; }

        [JsonIgnore]
        public virtual object RolesOwned { get; set; }

        #endregion

        #region 扩展属性

        /// <summary>
        /// 超级管理员?
        /// </summary>
        public bool IsSuperAdmin => this.UserName.ToUpper().Equals("ADMIN");
        /// <summary>
        /// 管理员?
        /// </summary>
        public bool IsAdmin => this.UserName.ToUpper().Equals("ADMIN");

        /// <summary>
        /// 权限菜单
        /// </summary>
        [JsonIgnore]
        public List<MenuEntity> Menus { get; set; }

        /// <summary>
        /// 数据权限
        /// </summary>
        [JsonIgnore]
        public List<DataRoleUserEntity> DataRoleUsers { get; set; }

        /// <summary>
        /// 数据角色
        /// </summary>
        [JsonIgnore]
        public List<DataRoleEntity> DataRoles { get; set; }

        public virtual object DataRolesOwned { get; set; }

        /// <summary>
        /// 数据权限
        /// </summary>
        [JsonIgnore]
        public List<FormRoleUserEntity> FormRoleUsers { get; set; }

        /// <summary>
        /// 数据角色
        /// </summary>
        [JsonIgnore]
        public List<FormRoleEntity> FormRoles { get; set; }

        [JsonIgnore]
        public virtual object FormRolesOwned { get; set; }

        #endregion
    }
}
