namespace BikeRental.Data.Repositories
{
    /// <summary>
    /// Generic async repository base class for data access abstraction.
    /// Provides common CRUD operations with async/await support.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public abstract class GenericAsyncRepository<T> : IAsyncRepository<T> where T : class
    {
        protected readonly ILogger<GenericAsyncRepository<T>> _logger;
        protected readonly string _filePath;

        protected GenericAsyncRepository(string filePath, ILogger<GenericAsyncRepository<T>> logger)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all entities asynchronously.
        /// </summary>
        public abstract Task<List<T>> GetAllAsync();

        /// <summary>
        /// Gets an entity by ID asynchronously.
        /// </summary>
        public abstract Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Saves entities asynchronously.
        /// </summary>
        public abstract Task SaveAsync(List<T> entities);

        /// <summary>
        /// Validates if file exists and throws if not.
        /// </summary>
        protected void ValidateFileExists()
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"Data file not found: {_filePath}");
            }
        }
    }
}
