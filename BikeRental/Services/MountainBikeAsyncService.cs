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
    /// Async service for mountain bike operations.
    /// Provides async/await support for improved scalability.
    /// </summary>
    public class MountainBikeAsyncService : IMountainBikeAsyncService
    {
        private readonly IMountainBikeAsyncRepository _repo;
        private readonly ILogger<MountainBikeAsyncService> _logger;
        private List<MountainBike>? _defaults;

        public MountainBikeAsyncService(IMountainBikeAsyncRepository repo, ILogger<MountainBikeAsyncService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all mountain bikes asynchronously.
        /// </summary>
        public async Task<List<MountainBike>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all mountain bikes");
                return await _repo.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching mountain bikes");
                throw new DataAccessException("Failed to fetch mountain bikes", ex);
            }
        }

        /// <summary>
        /// Gets a mountain bike by ID asynchronously.
        /// </summary>
        public async Task<MountainBike?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching mountain bike with ID {BikeId}", id);
            var bike = await _repo.GetByIdAsync(id);
            if (bike == null)
                _logger.LogWarning("Mountain bike with ID {BikeId} not found", id);
            return bike;
        }

        /// <summary>
        /// Rents a mountain bike asynchronously.
        /// </summary>
        public async Task<bool> RentBikeAsync(int bikeId)
        {
            var bike = await GetByIdAsync(bikeId);

            if (bike == null)
            {
                _logger.LogWarning("Attempted to rent non-existent mountain bike {BikeId}", bikeId);
                throw new ResourceNotFoundException("MountainBike", bikeId);
            }

            if (!bike.IsAvailable)
            {
                _logger.LogWarning("Attempted to rent unavailable mountain bike {BikeId}", bikeId);
                throw new ConflictException($"Mountain bike {bikeId} is not available for rental");
            }

            bike.IsAvailable = false;
            var allBikes = await GetAllAsync();
            await _repo.SaveAsync(allBikes);
            _logger.LogInformation("Successfully rented mountain bike {BikeId}", bikeId);
            return true;
        }

        /// <summary>
        /// Restores all bikes to their original availability states asynchronously.
        /// </summary>
        public async Task ResetToDefaultsAsync()
        {
            try
            {
                _logger.LogInformation("Resetting mountain bikes to defaults");
                _defaults ??= await GetAllAsync();
                await _repo.SaveAsync(new List<MountainBike>(_defaults));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting mountain bikes to defaults");
                throw;
            }
        }
    }
}
