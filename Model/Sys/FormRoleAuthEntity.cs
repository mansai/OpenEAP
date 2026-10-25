using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    [Serializable]
    [Table("TFormRoleAuth")]
    public partial class FormRoleAuthEntity : Entities.Entity
    {
        public long FormRoleId { get; set; }
        
        public long FormDesignId { get; set; }

        public long FormDesignDataId { get; set; }
        public string ControlType { get; set; }

        public string ControlId { get; set; }
    }
}
