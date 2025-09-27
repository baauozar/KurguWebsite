using KurguWebsite.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Interfaces
{
    public interface IDomainEventDispatcher
    {
        Task Dispatch(DomainEvent domainEvent);
    }
}
