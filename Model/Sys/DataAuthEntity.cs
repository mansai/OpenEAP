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
    /// 数据权限配置
    /// </summary>
    [Serializable]
    [FreeSql.DataAnnotations.Table(Name = "TSysDataAuth")]
    public class DataAuthEntity : AuditedEntity
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        public long DataRoleId { get; set; }

        /// <summary>
        /// 权限类型 1允许，0禁止
        /// </summary>
        public int Type { get; set; } = 1;

        /// <summary>
        /// 数据权限编码
        /// </summary>
        public string AuthCode { get; set; }

        /// <summary>
        /// 权限表达式
        /// </summary>
        public virtual string AuthRule { get; set; }

        /// <summary>
        /// 权限表达式 展示数据
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// 局部页面权限ControllerName.ActionName
        /// </summary>
        [FreeSql.DataAnnotations.Column(Name ="ActionPath")]
        public string FormClass { get; set; }

        /// <summary>
        /// 表单名称
        /// </summary>
        [FreeSql.DataAnnotations.Column(Name = "ActionPathName")]
        public virtual string FormClassName { get; set; }

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
    }

    /// <summary>
    /// 数据权限规则
    /// </summary>
    public class DataRuleViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public IEnumerable<int> Ids { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public IEnumerable<string> Names { get; set; }

        /// <summary>
        /// sql
        /// </summary>
        public IEnumerable<string> Sql { get; set; }

        /// <summary>
        /// api
        /// </summary>
        public IEnumerable<string> Api { get; set; }
    }

    public class DataAuthDTO
    {
        public virtual string DataAuthCode { get; set; }
        public virtual string DataAuthRule { get; set; }
        public virtual string ActionPath { get; set; }
    }
}
