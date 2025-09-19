using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Events
{
    public abstract class DomainEvent
    {
        public DateTime OccurredOn { get; }
        public Guid AggregateId { get; }

        protected DomainEvent(Guid aggregateId)
        {
            OccurredOn = DateTime.UtcNow;
            AggregateId = aggregateId;
        }
    }
}
