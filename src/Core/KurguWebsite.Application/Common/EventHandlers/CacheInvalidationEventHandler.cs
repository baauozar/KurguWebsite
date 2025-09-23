using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.EventHandlers
{
    public class CacheInvalidationEventHandler : INotificationHandler<CacheInvalidationEvent>
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheInvalidationEventHandler> _logger;

        public CacheInvalidationEventHandler(ICacheService cacheService, ILogger<CacheInvalidationEventHandler> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task Handle(CacheInvalidationEvent notification, CancellationToken cancellationToken)
        {
            foreach (var key in notification.CacheKeys)
            {
                await _cacheService.RemoveAsync(key);
                _logger.LogInformation("CACHE INVALIDATED: Key = {CacheKey}", key);
            }
        }
    }
}