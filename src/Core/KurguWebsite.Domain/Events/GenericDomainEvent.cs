using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Events
{
    public abstract class GenericDomainEvent<T> : DomainEvent
    {
        public Guid AggregateId { get; }

        protected GenericDomainEvent(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
