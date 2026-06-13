using System.Text.Json;
using StackExchange.Redis;

namespace BikeRental.Services.Caching
{
    /// <summary>
    /// Redis-based distributed cache service implementation.
    /// Provides high-performance caching with optional expiration.
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly IServer _server;
        private readonly ILogger<RedisCacheService> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public RedisCacheService(IConnectionMultiplexer multiplexer, ILogger<RedisCacheService> logger)
        {
            _db = multiplexer.GetDatabase();
            _server = multiplexer.GetServer(multiplexer.GetEndPoints().FirstOrDefault() ?? throw new InvalidOperationException("No Redis endpoints available"));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a value from Redis cache asynchronously.
        /// </summary>
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var value = await _db.StringGetAsync(key);
                if (!value.HasValue)
                {
                    _logger.LogDebug("Cache miss for key: {CacheKey}", key);
                    return null;
                }

                _logger.LogDebug("Cache hit for key: {CacheKey}", key);
                return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from cache for key: {CacheKey}", key);
                return null;
            }
        }

        /// <summary>
        /// Sets a value in Redis cache asynchronously with optional expiration.
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                await _db.StringSetAsync(key, json, expiration);
                _logger.LogDebug("Cache set for key: {CacheKey} with expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {CacheKey}", key);
            }
        }

        /// <summary>
        /// Removes a value from Redis cache asynchronously.
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
                _logger.LogDebug("Cache key removed: {CacheKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {CacheKey}", key);
            }
        }

        /// <summary>
        /// Removes all keys matching a pattern from Redis cache asynchronously.
        /// </summary>
        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var keys = _server.Keys(pattern: pattern).ToArray();
                if (keys.Length > 0)
                {
                    await _db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Removed {KeyCount} cache keys matching pattern: {Pattern}", keys.Length, pattern);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache keys by pattern: {Pattern}", pattern);
            }
        }

        /// <summary>
        /// Checks if a key exists in Redis cache asynchronously.
        /// </summary>
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache key existence: {CacheKey}", key);
                return false;
            }
        }
    }
}
