using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Events
{
    public class ServiceCreatedEvent : DomainEvent
    {
        public string ServiceTitle { get; }
        public string Category { get; }

        public ServiceCreatedEvent(Guid serviceId, string serviceTitle, string category)
            : base(serviceId)
        {
            ServiceTitle = serviceTitle;
            Category = category;
        }
    }

    public class ServiceUpdatedEvent : DomainEvent
    {
        public string ServiceTitle { get; }

        public ServiceUpdatedEvent(Guid serviceId, string serviceTitle)
            : base(serviceId)
        {
            ServiceTitle = serviceTitle;
        }
    }
}
