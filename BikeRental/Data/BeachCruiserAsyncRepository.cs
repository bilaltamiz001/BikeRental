using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using BikeRental.Data.Models;
using BikeRental.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace BikeRental.Data
{
    /// <summary>
    /// Async beach cruiser repository with caching and XML persistence.
    /// Consolidates async operations for beach cruiser data access.
    /// </summary>
    public class BeachCruiserAsyncRepository : GenericAsyncRepository<BeachCruiser>, IBeachCruiserAsyncRepository
    {
        private readonly Services.Caching.ICacheService _cacheService;
        private const string CacheKeyPrefix = "beach_cruisers";
        private const int CacheExpirationMinutes = 30;

        public BeachCruiserAsyncRepository(
            string filePath,
            ILogger<BeachCruiserAsyncRepository> logger,
            Services.Caching.ICacheService cacheService)
            : base(filePath, logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        /// <summary>
        /// Gets all beach cruisers asynchronously with caching.
        /// </summary>
        public override async Task<List<BeachCruiser>> GetAllAsync()
        {
            try
            {
                var cacheKey = CacheKeyPrefix;
                var cached = await _cacheService.GetAsync<List<BeachCruiser>>(cacheKey);

                if (cached != null)
                {
                    _logger.LogInformation("Retrieved beach cruisers from cache");
                    return cached;
                }

                ValidateFileExists();

                var bikes = await Task.Run(() =>
                {
                    var cachePath = _filePath + ".cache.json";
                    if (BinaryFormatterCache.IsFresh(cachePath, _filePath))
                        return BinaryFormatterCache.Read<List<BeachCruiser>>(cachePath);

                    var loadedBikes = IsolatedDataLoader.LoadBeachCruisers(_filePath);
                    BinaryFormatterCache.Write(cachePath, loadedBikes);
                    return loadedBikes;
                });

                await _cacheService.SetAsync(cacheKey, bikes, TimeSpan.FromMinutes(CacheExpirationMinutes));
                _logger.LogInformation("Retrieved and cached {BikeCount} beach cruisers", bikes.Count);

                return bikes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading beach cruisers");
                throw new InvalidOperationException($"Failed to load beach cruisers from {_filePath}", ex);
            }
        }

        /// <summary>
        /// Gets a beach cruiser by ID asynchronously.
        /// </summary>
        public override async Task<BeachCruiser?> GetByIdAsync(int id)
        {
            var bikes = await GetAllAsync();
            return bikes.FirstOrDefault(b => b.bike_id == id);
        }

        /// <summary>
        /// Saves beach cruisers asynchronously and updates cache.
        /// </summary>
        public override async Task SaveAsync(List<BeachCruiser> bikes)
        {
            try
            {
                await Task.Run(() =>
                {
                    var doc = new XDocument(
                        new XDeclaration("1.0", "utf-8", null),
                        new XElement("beach_cruisers",
                            bikes.Select(b => new XElement("bike",
                                new XElement("bike_id", b.bike_id),
                                new XElement("bike_name", b.bike_name ?? string.Empty),
                                new XElement("color", b.color ?? string.Empty),
                                new XElement("frame_size", b.frame_size ?? string.Empty),
                                new XElement("price_per_day", b.price_per_day.ToString(CultureInfo.InvariantCulture)),
                                new XElement("available", b.available.ToString().ToLower()),
                                new XElement("description", b.description ?? string.Empty)
                            ))
                        )
                    );
                    doc.Save(_filePath);
                    BinaryFormatterCache.Write(_filePath + ".cache.json", bikes);
                });

                // Invalidate cache
                await _cacheService.RemoveAsync(CacheKeyPrefix);
                _logger.LogInformation("Saved {BikeCount} beach cruisers and invalidated cache", bikes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving beach cruisers");
                throw;
            }
        }
    }
}
