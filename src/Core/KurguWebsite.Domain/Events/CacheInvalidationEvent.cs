using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Events
{
    public class CacheInvalidationEvent : DomainEvent
    {
        public string[] CacheKeys { get; }
        public CacheInvalidationEvent(params string[] cacheKeys)
        {
            CacheKeys = cacheKeys;
        }
    }
}