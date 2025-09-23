using KurguWebsite.Domain.Entities;
using System;

namespace KurguWebsite.Domain.Events
{
    public class CompanyInfoUpdatedEvent : GenericDomainEvent<CompanyInfo>
    {
        public CompanyInfoUpdatedEvent(Guid companyInfoId) : base(companyInfoId) { }
    }
}