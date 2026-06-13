namespace BikeRental.Data
{
    /// <summary>
    /// Generic repository interface for data access abstraction.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public interface IRepository<T>
    {
        /// <summary>
        /// Gets all entities.
        /// </summary>
        List<T> GetAll();

        /// <summary>
        /// Saves entities.
        /// </summary>
        void Save(List<T> entities);

        /// <summary>
        /// Gets the entity by ID.
        /// </summary>
        T? GetById(int id);
    }

    /// <summary>
    /// Repository interface for beach cruisers.
    /// </summary>
    public interface IBeachCruiserRepository : IRepository<BikeRental.Data.Models.BeachCruiser>
    {
    }

    /// <summary>
    /// Repository interface for mountain bikes.
    /// </summary>
    public interface IMountainBikeRepository : IRepository<BikeRental.Data.Models.MountainBike>
    {
    }

    /// <summary>
    /// Repository interface for accessories.
    /// </summary>
    public interface IAccessoryRepository : IRepository<BikeRental.Data.Models.Accessory>
    {
    }
}
