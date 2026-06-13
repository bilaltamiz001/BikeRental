using BikeRental.Data.Models;

namespace BikeRental.Services
{
    /// <summary>
    /// Async service interface for beach cruiser operations.
    /// </summary>
    public interface IBeachCruiserAsyncService
    {
        /// <summary>
        /// Gets all beach cruisers asynchronously.
        /// </summary>
        Task<List<BeachCruiser>> GetAllAsync();

        /// <summary>
        /// Gets a beach cruiser by ID asynchronously.
        /// </summary>
        Task<BeachCruiser?> GetByIdAsync(int id);

        /// <summary>
        /// Rents a beach cruiser asynchronously.
        /// </summary>
        Task<bool> RentBikeAsync(int bikeId);

        /// <summary>
        /// Restores all bikes to their original availability states asynchronously.
        /// </summary>
        Task ResetToDefaultsAsync();
    }

    /// <summary>
    /// Async service interface for mountain bike operations.
    /// </summary>
    public interface IMountainBikeAsyncService
    {
        /// <summary>
        /// Gets all mountain bikes asynchronously.
        /// </summary>
        Task<List<MountainBike>> GetAllAsync();

        /// <summary>
        /// Gets a mountain bike by ID asynchronously.
        /// </summary>
        Task<MountainBike?> GetByIdAsync(int id);

        /// <summary>
        /// Rents a mountain bike asynchronously.
        /// </summary>
        Task<bool> RentBikeAsync(int bikeId);

        /// <summary>
        /// Restores all bikes to their original availability states asynchronously.
        /// </summary>
        Task ResetToDefaultsAsync();
    }

    /// <summary>
    /// Async service interface for accessory operations.
    /// </summary>
    public interface IAccessoryAsyncService
    {
        /// <summary>
        /// Gets all accessories asynchronously.
        /// </summary>
        Task<List<Accessory>> GetAllAsync();

        /// <summary>
        /// Gets accessories compatible with a specific bike type asynchronously.
        /// </summary>
        Task<List<Accessory>> GetCompatibleWithAsync(string bikeType);

        /// <summary>
        /// Processes an accessory order asynchronously.
        /// </summary>
        Task<AccessoryRequestResult> ProcessOrderAsync(Dictionary<int, int> quantities);

        /// <summary>
        /// Restores all accessories to their original stock levels asynchronously.
        /// </summary>
        Task ResetToDefaultsAsync();
    }
}
