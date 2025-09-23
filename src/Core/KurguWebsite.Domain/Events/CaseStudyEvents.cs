using KurguWebsite.Domain.Entities;
using System;

namespace KurguWebsite.Domain.Events
{
    public class CaseStudyCreatedEvent : GenericDomainEvent<CaseStudy>
    {
        public CaseStudyCreatedEvent(Guid caseStudyId) : base(caseStudyId) { }
    }

    public class CaseStudyUpdatedEvent : GenericDomainEvent<CaseStudy>
    {
        public CaseStudyUpdatedEvent(Guid caseStudyId) : base(caseStudyId) { }
    }

    public class CaseStudyDeletedEvent : GenericDomainEvent<CaseStudy>
    {
        public CaseStudyDeletedEvent(Guid caseStudyId) : base(caseStudyId) { }
    }
}