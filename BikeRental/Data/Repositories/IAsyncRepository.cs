namespace BikeRental.Data.Repositories
{
    /// <summary>
    /// Generic async repository interface for data access abstraction.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public interface IAsyncRepository<T> where T : class
    {
        /// <summary>
        /// Gets all entities asynchronously.
        /// </summary>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Gets an entity by ID asynchronously.
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Saves entities asynchronously.
        /// </summary>
        Task SaveAsync(List<T> entities);
    }
}
