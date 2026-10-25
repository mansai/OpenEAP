using FreeSql.DataAnnotations;

namespace OpenEAP.Model.Entities.Auditing
{
    [Serializable]
    public abstract class CreationAuditedEntity : Entity
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Position = -3, CanUpdate = false)]
        public virtual DateTime? CreationTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建人
        /// </summary>
        [Column(Position = -2, CanUpdate = false)]
        public virtual long? CreatorId { get; set; } = GetUserId();

        public CreationAuditedEntity() { }

        public CreationAuditedEntity(long id) : base(id) { }
    }
}
