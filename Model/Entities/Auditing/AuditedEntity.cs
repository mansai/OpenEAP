using FreeSql.DataAnnotations;
using System;

namespace OpenEAP.Model.Entities.Auditing
{
    [Serializable]
    public abstract class AuditedEntity : CreationAuditedEntity
    {
        /// <summary>
        /// 修改时间
        /// </summary>
        [Column(Position = -5)]
        public virtual DateTime? ModificationTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Column(Position = -4)]
        public virtual long? ModifierId { get; set; }

        public AuditedEntity() { }

        public AuditedEntity(long id) : base(id) { }

        public void Modify(long? userId = null)
        {
            ModificationTime = DateTime.Now;
            ModifierId = userId ?? GetUserId();
        }
    }
}
