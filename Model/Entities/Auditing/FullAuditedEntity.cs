using FreeSql.DataAnnotations;
using System;

namespace OpenEAP.Model.Entities.Auditing
{
    [Serializable]
    public abstract class FullAuditedEntity : AuditedEntity
    {
        /// <summary>
        /// 删除标记
        /// </summary>
        [Column(Position = -1)]
        public virtual bool IsDeleted { get; set; } = false;

        public FullAuditedEntity()
        {
        }
        public FullAuditedEntity(long id) : base(id)
        {
        }
    }
}
