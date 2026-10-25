using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TFormRoleUser")]
    public partial class FormRoleUserEntity : Entities.Entity
    {
        public FormRoleUserEntity() { }
        public FormRoleUserEntity(long id) : base(id) { }

        public long UserId { get; set; }

        public long FormRoleId { get; set; }
    }
}
