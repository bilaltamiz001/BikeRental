using BikeRental.Data.Models;

namespace BikeRental.Services
{
    /// <summary>
    /// Service interface for beach cruiser operations.
    /// </summary>
    public interface IBeachCruiserService
    {
        /// <summary>
        /// Gets all beach cruisers.
        /// </summary>
        List<BeachCruiser> GetAll();

        /// <summary>
        /// Gets a beach cruiser by ID.
        /// </summary>
        BeachCruiser? GetById(int id);

        /// <summary>
        /// Rents a beach cruiser.
        /// </summary>
        bool RentBike(int bikeId);

        /// <summary>
        /// Restores all bikes to their original availability states.
        /// </summary>
        void ResetToDefaults();
    }

    /// <summary>
    /// Service interface for mountain bike operations.
    /// </summary>
    public interface IMountainBikeService
    {
        /// <summary>
        /// Gets all mountain bikes.
        /// </summary>
        List<MountainBike> GetAll();

        /// <summary>
        /// Gets a mountain bike by ID.
        /// </summary>
        MountainBike? GetById(int id);

        /// <summary>
        /// Rents a mountain bike.
        /// </summary>
        bool RentBike(int bikeId);

        /// <summary>
        /// Restores all bikes to their original availability states.
        /// </summary>
        void ResetToDefaults();
    }

    /// <summary>
    /// Service interface for accessory operations.
    /// </summary>
    public interface IAccessoryService
    {
        /// <summary>
        /// Gets all accessories.
        /// </summary>
        List<Accessory> GetAll();

        /// <summary>
        /// Gets accessories compatible with a specific bike type.
        /// </summary>
        List<Accessory> GetCompatibleWith(string bikeType);

        /// <summary>
        /// Processes an accessory order.
        /// </summary>
        AccessoryRequestResult ProcessOrder(Dictionary<int, int> quantities);

        /// <summary>
        /// Restores all accessories to their original stock levels.
        /// </summary>
        void ResetToDefaults();
    }
}
