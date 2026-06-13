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
    /// Async mountain bike repository with caching and JSON persistence.
    /// Consolidates async operations for mountain bike data access.
    /// </summary>
    public class MountainBikeAsyncRepository : GenericAsyncRepository<MountainBike>, IMountainBikeAsyncRepository
    {
        private readonly Services.Caching.ICacheService _cacheService;
        private const string CacheKeyPrefix = "mountain_bikes";
        private const int CacheExpirationMinutes = 30;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public MountainBikeAsyncRepository(
            string filePath,
            ILogger<MountainBikeAsyncRepository> logger,
            Services.Caching.ICacheService cacheService)
            : base(filePath, logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        /// <summary>
        /// Gets all mountain bikes asynchronously with caching.
        /// </summary>
        public override async Task<List<MountainBike>> GetAllAsync()
        {
            try
            {
                var cacheKey = CacheKeyPrefix;
                var cached = await _cacheService.GetAsync<List<MountainBike>>(cacheKey);

                if (cached != null)
                {
                    _logger.LogInformation("Retrieved mountain bikes from cache");
                    return cached;
                }

                ValidateFileExists();

                var bikes = await Task.Run(() =>
                {
                    var cachePath = _filePath + ".cache.json";
                    if (BinaryFormatterCache.IsFresh(cachePath, _filePath))
                        return BinaryFormatterCache.Read<List<MountainBike>>(cachePath);

                    var loadedBikes = IsolatedDataLoader.LoadMountainBikes(_filePath);
                    BinaryFormatterCache.Write(cachePath, loadedBikes);
                    return loadedBikes;
                });

                await _cacheService.SetAsync(cacheKey, bikes, TimeSpan.FromMinutes(CacheExpirationMinutes));
                _logger.LogInformation("Retrieved and cached {BikeCount} mountain bikes", bikes.Count);

                return bikes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading mountain bikes");
                throw new InvalidOperationException($"Failed to load mountain bikes from {_filePath}", ex);
            }
        }

        /// <summary>
        /// Gets a mountain bike by ID asynchronously.
        /// </summary>
        public override async Task<MountainBike?> GetByIdAsync(int id)
        {
            var bikes = await GetAllAsync();
            return bikes.FirstOrDefault(b => b.BikeID == id);
        }

        /// <summary>
        /// Saves mountain bikes asynchronously and updates cache.
        /// </summary>
        public override async Task SaveAsync(List<MountainBike> bikes)
        {
            try
            {
                await Task.Run(() =>
                {
                    var json = JsonSerializer.Serialize(bikes, _jsonOptions);
                    File.WriteAllText(_filePath, json, Encoding.UTF8);
                    BinaryFormatterCache.Write(_filePath + ".cache.json", bikes);
                });

                // Invalidate cache
                await _cacheService.RemoveAsync(CacheKeyPrefix);
                _logger.LogInformation("Saved {BikeCount} mountain bikes and invalidated cache", bikes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving mountain bikes");
                throw;
            }
        }
    }
}
