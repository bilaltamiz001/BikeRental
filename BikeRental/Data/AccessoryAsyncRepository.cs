using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BikeRental.Data.Models;
using BikeRental.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace BikeRental.Data
{
    /// <summary>
    /// Async accessory repository with caching and JSON persistence.
    /// Consolidates async operations for accessory data access.
    /// </summary>
    public class AccessoryAsyncRepository : GenericAsyncRepository<Accessory>, IAccessoryAsyncRepository
    {
        private readonly Services.Caching.ICacheService _cacheService;
        private const string CacheKeyPrefix = "accessories";
        private const int CacheExpirationMinutes = 30;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public AccessoryAsyncRepository(
            string filePath,
            ILogger<AccessoryAsyncRepository> logger,
            Services.Caching.ICacheService cacheService)
            : base(filePath, logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        /// <summary>
        /// Gets all accessories asynchronously with caching.
        /// </summary>
        public override async Task<List<Accessory>> GetAllAsync()
        {
            try
            {
                var cacheKey = CacheKeyPrefix;
                var cached = await _cacheService.GetAsync<List<Accessory>>(cacheKey);

                if (cached != null)
                {
                    _logger.LogInformation("Retrieved accessories from cache");
                    return cached;
                }

                ValidateFileExists();

                var accessories = await Task.Run(() =>
                {
                    var cachePath = _filePath + ".cache.json";
                    if (BinaryFormatterCache.IsFresh(cachePath, _filePath))
                        return BinaryFormatterCache.Read<List<Accessory>>(cachePath);

                    var loadedAccessories = IsolatedDataLoader.LoadAccessories(_filePath);
                    BinaryFormatterCache.Write(cachePath, loadedAccessories);
                    return loadedAccessories;
                });

                await _cacheService.SetAsync(cacheKey, accessories, TimeSpan.FromMinutes(CacheExpirationMinutes));
                _logger.LogInformation("Retrieved and cached {AccessoryCount} accessories", accessories.Count);

                return accessories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accessories");
                throw new InvalidOperationException($"Failed to load accessories from {_filePath}", ex);
            }
        }

        /// <summary>
        /// Gets an accessory by ID asynchronously.
        /// </summary>
        public override async Task<Accessory?> GetByIdAsync(int id)
        {
            var accessories = await GetAllAsync();
            return accessories.FirstOrDefault(a => a.AccessoryID == id);
        }

        /// <summary>
        /// Saves accessories asynchronously and updates cache.
        /// </summary>
        public override async Task SaveAsync(List<Accessory> accessories)
        {
            try
            {
                await Task.Run(() =>
                {
                    var json = JsonSerializer.Serialize(accessories, _jsonOptions);
                    File.WriteAllText(_filePath, json, Encoding.UTF8);
                    BinaryFormatterCache.Write(_filePath + ".cache.json", accessories);
                });

                // Invalidate cache
                await _cacheService.RemoveAsync(CacheKeyPrefix);
                _logger.LogInformation("Saved {AccessoryCount} accessories and invalidated cache", accessories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving accessories");
                throw;
            }
        }
    }
}
