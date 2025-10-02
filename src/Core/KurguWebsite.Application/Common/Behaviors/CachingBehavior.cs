using KurguWebsite.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Behaviors
{
    public interface ICacheableQuery<TResponse> : IRequest<TResponse>
    {
        string CacheKey { get; }
        TimeSpan? CacheTime { get; }
    }

    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICacheableQuery<TResponse>
    {
        private readonly ICacheService _cache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var cachedResponse = await _cache.GetAsync<TResponse>(request.CacheKey);

            if (cachedResponse != null)
            {
                _logger.LogInformation($"Fetched from cache: {request.CacheKey}");
                return cachedResponse;
            }

            // Get or create a lock for this cache key
            var semaphore = _locks.GetOrAdd(request.CacheKey, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync(cancellationToken);
            try
            {
                // Double-check after acquiring lock
                cachedResponse = await _cache.GetAsync<TResponse>(request.CacheKey);
                if (cachedResponse != null)
                {
                    _logger.LogInformation($"Fetched from cache after lock: {request.CacheKey}");
                    return cachedResponse;
                }

                var response = await next();
                await _cache.SetAsync(request.CacheKey, response, request.CacheTime);

                _logger.LogInformation($"Added to cache: {request.CacheKey}");
                return response;
            }
            finally
            {
                semaphore.Release();

                // Cleanup: Remove lock if no one is waiting
                if (semaphore.CurrentCount == 1)
                {
                    _locks.TryRemove(request.CacheKey, out _);
                }
            }
        }
    }
}