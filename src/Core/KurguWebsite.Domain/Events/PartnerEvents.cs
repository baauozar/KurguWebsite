using KurguWebsite.Domain.Entities;
using System;

namespace KurguWebsite.Domain.Events
{
    public class PartnerCreatedEvent : GenericDomainEvent<Partner>
    {
        public PartnerCreatedEvent(Guid partnerId) : base(partnerId) { }
    }

    public class PartnerUpdatedEvent : GenericDomainEvent<Partner>
    {
        public PartnerUpdatedEvent(Guid partnerId) : base(partnerId) { }
    }

    public class PartnerDeletedEvent : GenericDomainEvent<Partner>
    {
        public PartnerDeletedEvent(Guid partnerId) : base(partnerId) { }
    }
}