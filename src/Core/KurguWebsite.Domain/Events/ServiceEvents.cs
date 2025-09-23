using KurguWebsite.Domain.Entities;
using System;

namespace KurguWebsite.Domain.Events
{
    public class ServiceCreatedEvent : GenericDomainEvent<Service>
    {
        public ServiceCreatedEvent(Guid serviceId) : base(serviceId) { }
    }

    public class ServiceUpdatedEvent : GenericDomainEvent<Service>
    {
        public ServiceUpdatedEvent(Guid serviceId) : base(serviceId) { }
    }

    public class ServiceDeletedEvent : GenericDomainEvent<Service>
    {
        public ServiceDeletedEvent(Guid serviceId) : base(serviceId) { }
    }
}