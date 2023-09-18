using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TemperatureApi.Configurations;

namespace TemperatureApi.Services
{
    public class RedisCacheClient : ICacheClient
    {
        private readonly DistributedCacheEntryOptions defaultCacheEntryOptions;
        private readonly IDistributedCache distributedCache;
        private readonly ILogger<RedisCacheClient> logger;

        public RedisCacheClient(IDistributedCache distributedCache,
            ILogger<RedisCacheClient> logger, IOptions<RedisConfig> options)
        {
            this.distributedCache = distributedCache;
            this.logger = logger;

            defaultCacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(options.Value.DefaultExpirationInMinutes)
            };
        }

        public async Task<T?> Get<T>(string cacheKey, CancellationToken cancellationToken)
        {
            logger.LogInformation("Attempting to get value from cache with key {cacheKey}", cacheKey);

            var value = await distributedCache.GetStringAsync(cacheKey, cancellationToken);

            if (value is not null)
            {
                logger.LogInformation("Value found in cache with key {cacheKey}", cacheKey);

                try
                {
                    return JsonSerializer.Deserialize<T>(value);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to deserialize cached value: {value}", value);
                }
            }

            return default;
        }

        public async Task Set<T>(string cacheKey, T value, CancellationToken cancellationToken)
        {
            logger.LogInformation("Attempting to add value to cache with key {cacheKey}", cacheKey);
            await distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(value),
                defaultCacheEntryOptions, cancellationToken);
        }

        public async Task Remove(string cacheKey, CancellationToken cancellationToken)
        {
            logger.LogInformation("Attempting to remove cache value with key {cacheKey}", cacheKey);
            await distributedCache.RemoveAsync(cacheKey, cancellationToken);
        }
    }
}
