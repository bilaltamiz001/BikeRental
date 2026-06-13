using BikeRental.Data.Models;
using BikeRental.Data.Repositories;

namespace BikeRental.Data
{
    /// <summary>
    /// Async repository interface for beach cruisers.
    /// </summary>
    public interface IBeachCruiserAsyncRepository : IAsyncRepository<BeachCruiser>
    {
    }

    /// <summary>
    /// Async repository interface for mountain bikes.
    /// </summary>
    public interface IMountainBikeAsyncRepository : IAsyncRepository<MountainBike>
    {
    }

    /// <summary>
    /// Async repository interface for accessories.
    /// </summary>
    public interface IAccessoryAsyncRepository : IAsyncRepository<Accessory>
    {
    }
}
