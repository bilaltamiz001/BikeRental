using System.Collections.Generic;
using System.Linq;
using BikeRental.Data;
using BikeRental.Data.Models;
using BikeRental.Exceptions;
using Microsoft.Extensions.Logging;

namespace BikeRental.Services
{
    /// <summary>
    /// Service for beach cruiser bike operations.
    /// </summary>
    public class BeachCruiserService : IBeachCruiserService
    {
        private readonly IBeachCruiserRepository _repo;
        private readonly ILogger<BeachCruiserService> _logger;
        private readonly List<BeachCruiser> _defaults;

        public BeachCruiserService(IBeachCruiserRepository repo, ILogger<BeachCruiserService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                _defaults = new List<BeachCruiser>(_repo.GetAll());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load beach cruiser defaults");
                throw new DataAccessException("Failed to initialize beach cruiser service", ex);
            }
        }

        /// <summary>
        /// Gets all beach cruisers.
        /// </summary>
        public List<BeachCruiser> GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all beach cruisers");
                return _repo.GetAll();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching beach cruisers");
                throw new DataAccessException("Failed to fetch beach cruisers", ex);
            }
        }

        /// <summary>
        /// Gets a beach cruiser by ID.
        /// </summary>
        public BeachCruiser? GetById(int id)
        {
            _logger.LogInformation("Fetching beach cruiser with ID {BikeId}", id);
            var bike = _repo.GetById(id);
            if (bike == null)
                _logger.LogWarning("Beach cruiser with ID {BikeId} not found", id);
            return bike;
        }

        /// <summary>
        /// Rents a beach cruiser by ID.
        /// </summary>
        public bool RentBike(int bikeId)
        {
            var bike = GetById(bikeId);

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
            _repo.Save(GetAll());
            _logger.LogInformation("Successfully rented beach cruiser {BikeId}", bikeId);
            return true;
        }

        /// <summary>
        /// Restores all bikes to their original availability states.
        /// </summary>
        public void ResetToDefaults()
        {
            _logger.LogInformation("Resetting beach cruisers to defaults");
            _repo.Save(_defaults);
        }
    }
}
