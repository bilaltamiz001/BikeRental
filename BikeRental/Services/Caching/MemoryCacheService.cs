using Microsoft.Extensions.Caching.Memory;

namespace BikeRental.Services.Caching
{
    /// <summary>
    /// In-memory cache service implementation.
    /// Used for local development or as fallback when distributed caching is unavailable.
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly HashSet<string> _keys = new();
        private readonly object _lockObj = new();

        public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a value from memory cache asynchronously.
        /// </summary>
        public Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var exists = _cache.TryGetValue(key, out T? value);
                if (exists)
                {
                    _logger.LogDebug("Memory cache hit for key: {CacheKey}", key);
                    return Task.FromResult(value);
                }

                _logger.LogDebug("Memory cache miss for key: {CacheKey}", key);
                return Task.FromResult<T?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from memory cache for key: {CacheKey}", key);
                return Task.FromResult<T?>(null);
            }
        }

        /// <summary>
        /// Sets a value in memory cache asynchronously with optional expiration.
        /// </summary>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions();
                if (expiration.HasValue)
                {
                    cacheOptions.AbsoluteExpirationRelativeToNow = expiration;
                }

                _cache.Set(key, value, cacheOptions);

                lock (_lockObj)
                {
                    _keys.Add(key);
                }

                _logger.LogDebug("Memory cache set for key: {CacheKey} with expiration: {Expiration}", key, expiration);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting memory cache for key: {CacheKey}", key);
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Removes a value from memory cache asynchronously.
        /// </summary>
        public Task RemoveAsync(string key)
        {
            try
            {
                _cache.Remove(key);

                lock (_lockObj)
                {
                    _keys.Remove(key);
                }

                _logger.LogDebug("Memory cache key removed: {CacheKey}", key);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing memory cache key: {CacheKey}", key);
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Removes all keys matching a pattern from memory cache asynchronously.
        /// </summary>
        public Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                lock (_lockObj)
                {
                    var matchingKeys = _keys.Where(k => k.Contains(pattern)).ToList();
                    foreach (var key in matchingKeys)
                    {
                        _cache.Remove(key);
                        _keys.Remove(key);
                    }

                    _logger.LogDebug("Removed {KeyCount} memory cache keys matching pattern: {Pattern}", matchingKeys.Count, pattern);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing memory cache keys by pattern: {Pattern}", pattern);
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Checks if a key exists in memory cache asynchronously.
        /// </summary>
        public Task<bool> ExistsAsync(string key)
        {
            try
            {
                return Task.FromResult(_cache.TryGetValue(key, out _));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking memory cache key existence: {CacheKey}", key);
                return Task.FromResult(false);
            }
        }
    }
}
