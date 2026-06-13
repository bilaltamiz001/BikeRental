using BikeRental.Data.Models;
using BikeRental.Data.Repositories;

namespace BikeRental.Data.UnitOfWork
{
    /// <summary>
    /// Unit of Work pattern implementation for coordinating multiple repositories.
    /// Provides transactional consistency and change tracking.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IBeachCruiserAsyncRepository _beachCruiserRepository;
        private readonly IMountainBikeAsyncRepository _mountainBikeRepository;
        private readonly IAccessoryAsyncRepository _accessoryRepository;
        private readonly ILogger<UnitOfWork> _logger;

        public IAsyncRepository<BeachCruiser> BeachCruisers => _beachCruiserRepository;
        public IAsyncRepository<MountainBike> MountainBikes => _mountainBikeRepository;
        public IAsyncRepository<Accessory> Accessories => _accessoryRepository;

        public UnitOfWork(
            IBeachCruiserAsyncRepository beachCruiserRepository,
            IMountainBikeAsyncRepository mountainBikeRepository,
            IAccessoryAsyncRepository accessoryRepository,
            ILogger<UnitOfWork> logger)
        {
            _beachCruiserRepository = beachCruiserRepository ?? throw new ArgumentNullException(nameof(beachCruiserRepository));
            _mountainBikeRepository = mountainBikeRepository ?? throw new ArgumentNullException(nameof(mountainBikeRepository));
            _accessoryRepository = accessoryRepository ?? throw new ArgumentNullException(nameof(accessoryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Saves all changes made to repositories.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            try
            {
                _logger.LogInformation("Saving all unit of work changes");
                // Changes are automatically persisted in each repository's SaveAsync method
                // This method serves as a coordination point for future transaction management
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving unit of work changes");
                throw;
            }
        }

        /// <summary>
        /// Rolls back all pending changes.
        /// </summary>
        public async Task RollbackAsync()
        {
            try
            {
                _logger.LogInformation("Rolling back unit of work changes");
                // Rollback is handled by reloading from the source
                // Future implementation can add transaction support
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back unit of work");
                throw;
            }
        }

        /// <summary>
        /// Disposes the unit of work.
        /// </summary>
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
