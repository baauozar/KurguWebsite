using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime? LastModifiedDate { get; private set; }
        public string? CreatedBy { get; private set; }
        public string? LastModifiedBy { get; private set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }

        public void SetCreatedBy(string userId)
        {
            CreatedBy = userId;
        }

        public void SetModifiedBy(string userId)
        {
            LastModifiedBy = userId;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}
