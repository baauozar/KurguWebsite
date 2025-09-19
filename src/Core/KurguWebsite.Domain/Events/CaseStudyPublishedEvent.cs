using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Events
{
    public class CaseStudyPublishedEvent : DomainEvent
    {
        public string Title { get; }
        public string ClientName { get; }

        public CaseStudyPublishedEvent(Guid caseStudyId, string title, string clientName)
            : base(caseStudyId)
        {
            Title = title;
            ClientName = clientName;
        }
    }
}