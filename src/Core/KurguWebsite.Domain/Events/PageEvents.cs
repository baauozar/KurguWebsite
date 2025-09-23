using KurguWebsite.Domain.Entities;
using System;

namespace KurguWebsite.Domain.Events
{
    public class PageUpdatedEvent : GenericDomainEvent<Page>
    {
        public PageUpdatedEvent(Guid pageId) : base(pageId) { }
    }
}