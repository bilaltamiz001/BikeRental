using BikeRental.Data.Models;
using BikeRental.Data.Repositories;

namespace BikeRental.Data.UnitOfWork
{
    /// <summary>
    /// Interface for Unit of Work pattern implementation.
    /// Coordinates multiple repositories and provides transactional consistency.
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable
    {
        /// <summary>
        /// Gets the beach cruiser repository.
        /// </summary>
        IAsyncRepository<BeachCruiser> BeachCruisers { get; }

        /// <summary>
        /// Gets the mountain bike repository.
        /// </summary>
        IAsyncRepository<MountainBike> MountainBikes { get; }

        /// <summary>
        /// Gets the accessory repository.
        /// </summary>
        IAsyncRepository<Accessory> Accessories { get; }

        /// <summary>
        /// Saves all changes made to repositories.
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Rolls back all pending changes.
        /// </summary>
        Task RollbackAsync();
    }
}
