namespace BikeRental.Services.Caching
{
    /// <summary>
    /// Abstraction for distributed caching operations.
    /// Supports both in-memory and Redis caching implementations.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a value from cache asynchronously.
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Sets a value in cache asynchronously with optional expiration.
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Removes a value from cache asynchronously.
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Removes all values matching a pattern from cache asynchronously.
        /// </summary>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// Checks if a key exists in cache asynchronously.
        /// </summary>
        Task<bool> ExistsAsync(string key);
    }
}
