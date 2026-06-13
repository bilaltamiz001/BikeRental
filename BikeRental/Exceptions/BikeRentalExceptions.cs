namespace BikeRental.Exceptions
{
    /// <summary>
    /// Base exception for BikeRental application.
    /// </summary>
    public class BikeRentalException : Exception
    {
        public BikeRentalException(string message) : base(message) { }
        public BikeRentalException(string message, Exception innerException) 
            : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a resource is not found.
    /// </summary>
    public class ResourceNotFoundException : BikeRentalException
    {
        public string? ResourceType { get; }
        public object? ResourceId { get; }

        public ResourceNotFoundException(string resourceType, object resourceId)
            : base($"{resourceType} with ID {resourceId} not found.")
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }

        public ResourceNotFoundException(string message)
            : base(message) { }
    }

    /// <summary>
    /// Exception thrown when validation fails.
    /// </summary>
    public class ValidationException : BikeRentalException
    {
        public IDictionary<string, string[]>? Errors { get; }

        public ValidationException(string message)
            : base(message) { }

        public ValidationException(string message, IDictionary<string, string[]> errors)
            : base(message)
        {
            Errors = errors;
        }
    }

    /// <summary>
    /// Exception thrown when a resource conflict occurs.
    /// </summary>
    public class ConflictException : BikeRentalException
    {
        public ConflictException(string message)
            : base(message) { }
    }

    /// <summary>
    /// Exception thrown when data access fails.
    /// </summary>
    public class DataAccessException : BikeRentalException
    {
        public DataAccessException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
