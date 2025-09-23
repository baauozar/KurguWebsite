  

namespace KurguWebsite.Domain.Common
{
    public abstract class AuditableEntity : BaseEntity
    {
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedDate { get; private set; }
        public string? DeletedBy { get; private set; }

        public void SoftDelete(string userId)
        {
            IsDeleted = true;
            DeletedDate = DateTime.UtcNow;
            DeletedBy = userId;

        }

        public void Restore()
        {
            IsDeleted = false;
            DeletedDate = null;
            DeletedBy = null;
        }
    }
}
