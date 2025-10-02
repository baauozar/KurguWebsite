using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class ServiceFeature : AuditableEntity
    {
        public Guid ServiceId { get;  set; }
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string? IconClass { get; private set; }
        public int DisplayOrder { get; private set; }

        // Navigation
        public virtual Service Service { get;  set; } = null!;

        private ServiceFeature() { }

        public static ServiceFeature Create(
     Guid serviceId,
     string title,
     string description,
     string? iconClass = null,
     int displayOrder = 0)                 // <— added
        {
            return new ServiceFeature
            {
                ServiceId = serviceId,
                Title = title,
                Description = description,
                IconClass = iconClass,
                DisplayOrder = displayOrder       // <— set it
            };
        }
        public void SetDisplayOrder(int order)
        {
            if (order < 0)
                throw new ArgumentException("Display order cannot be negative", nameof(order));
            DisplayOrder = order;
        }
        public void Update(string title, string description, string? iconClass)
        {
            Title = title;
            Description = description;
            IconClass = iconClass;
        }
    }
}