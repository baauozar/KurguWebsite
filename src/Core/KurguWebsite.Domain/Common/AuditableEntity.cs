using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
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
