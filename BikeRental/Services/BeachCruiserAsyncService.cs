using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BikeRental.Data;
using BikeRental.Data.Models;
using BikeRental.Exceptions;
using Microsoft.Extensions.Logging;

namespace BikeRental.Services
{
    /// <summary>
    /// Async service for beach cruiser operations.
    /// Provides async/await support for improved scalability.
    /// </summary>
    public class BeachCruiserAsyncService : IBeachCruiserAsyncService
    {
        private readonly IBeachCruiserAsyncRepository _repo;
        private readonly ILogger<BeachCruiserAsyncService> _logger;
        private List<BeachCruiser>? _defaults;

        public BeachCruiserAsyncService(IBeachCruiserAsyncRepository repo, ILogger<BeachCruiserAsyncService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all beach cruisers asynchronously.
        /// </summary>
        public async Task<List<BeachCruiser>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all beach cruisers");
                return await _repo.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching beach cruisers");
                throw new DataAccessException("Failed to fetch beach cruisers", ex);
            }
        }

        /// <summary>
        /// Gets a beach cruiser by ID asynchronously.
        /// </summary>
        public async Task<BeachCruiser?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching beach cruiser with ID {BikeId}", id);
            var bike = await _repo.GetByIdAsync(id);
            if (bike == null)
                _logger.LogWarning("Beach cruiser with ID {BikeId} not found", id);
            return bike;
        }

        /// <summary>
        /// Rents a beach cruiser asynchronously.
        /// </summary>
        public async Task<bool> RentBikeAsync(int bikeId)
        {
            var bike = await GetByIdAsync(bikeId);

            if (bike == null)
            {
                _logger.LogWarning("Attempted to rent non-existent beach cruiser {BikeId}", bikeId);
                throw new ResourceNotFoundException("BeachCruiser", bikeId);
            }

            if (!bike.available)
            {
                _logger.LogWarning("Attempted to rent unavailable beach cruiser {BikeId}", bikeId);
                throw new ConflictException($"Beach cruiser {bikeId} is not available for rental");
            }

            bike.available = false;
            var allBikes = await GetAllAsync();
            await _repo.SaveAsync(allBikes);
            _logger.LogInformation("Successfully rented beach cruiser {BikeId}", bikeId);
            return true;
        }

        /// <summary>
        /// Restores all bikes to their original availability states asynchronously.
        /// </summary>
        public async Task ResetToDefaultsAsync()
        {
            try
            {
                _logger.LogInformation("Resetting beach cruisers to defaults");
                _defaults ??= await GetAllAsync();
                await _repo.SaveAsync(new List<BeachCruiser>(_defaults));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting beach cruisers to defaults");
                throw;
            }
        }
    }
}
