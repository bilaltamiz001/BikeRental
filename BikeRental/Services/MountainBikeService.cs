using System.Collections.Generic;
using System.Linq;
using BikeRental.Data;
using BikeRental.Data.Models;
using BikeRental.Exceptions;
using Microsoft.Extensions.Logging;

namespace BikeRental.Services
{
    /// <summary>
    /// Service for mountain bike operations.
    /// </summary>
    public class MountainBikeService : IMountainBikeService
    {
        private readonly IMountainBikeRepository _repo;
        private readonly ILogger<MountainBikeService> _logger;
        private readonly List<MountainBike> _defaults;

        public MountainBikeService(IMountainBikeRepository repo, ILogger<MountainBikeService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                _defaults = new List<MountainBike>(_repo.GetAll());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load mountain bike defaults");
                throw new DataAccessException("Failed to initialize mountain bike service", ex);
            }
        }

        /// <summary>
        /// Gets all mountain bikes.
        /// </summary>
        public List<MountainBike> GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all mountain bikes");
                return _repo.GetAll();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching mountain bikes");
                throw new DataAccessException("Failed to fetch mountain bikes", ex);
            }
        }

        /// <summary>
        /// Gets a mountain bike by ID.
        /// </summary>
        public MountainBike? GetById(int id)
        {
            _logger.LogInformation("Fetching mountain bike with ID {BikeId}", id);
            var bike = _repo.GetById(id);
            if (bike == null)
                _logger.LogWarning("Mountain bike with ID {BikeId} not found", id);
            return bike;
        }

        /// <summary>
        /// Rents a mountain bike by ID.
        /// </summary>
        public bool RentBike(int bikeId)
        {
            var bike = GetById(bikeId);

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
            _repo.Save(GetAll());
            _logger.LogInformation("Successfully rented mountain bike {BikeId}", bikeId);
            return true;
        }

        /// <summary>
        /// Restores all bikes to their original availability states.
        /// </summary>
        public void ResetToDefaults()
        {
            _logger.LogInformation("Resetting mountain bikes to defaults");
            _repo.Save(_defaults);
        }
    }
}
