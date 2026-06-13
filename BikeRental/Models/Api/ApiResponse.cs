namespace BikeRental.Models.Api
{
    /// <summary>
    /// Generic API response wrapper for consistent error handling and responses.
    /// </summary>
    /// <typeparam name="T">Type of the response data.</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message or error description.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The actual response data.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Any errors that occurred.
        /// </summary>
        public IEnumerable<string>? Errors { get; set; }

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "Success") =>
            new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };

        /// <summary>
        /// Creates a failed response.
        /// </summary>
        public static ApiResponse<T> FailureResponse(string message, IEnumerable<string>? errors = null) =>
            new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };

        /// <summary>
        /// Creates a failed response with exception details.
        /// </summary>
        public static ApiResponse<T> ErrorResponse(Exception ex, string message = "An error occurred") =>
            new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = new[] { ex.Message, ex.InnerException?.Message ?? string.Empty }
                    .Where(e => !string.IsNullOrEmpty(e))
            };
    }

    /// <summary>
    /// Generic API response for non-generic endpoints.
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Indicates if the request was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Any errors that occurred.
        /// </summary>
        public IEnumerable<string>? Errors { get; set; }

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        public static ApiResponse SuccessResponse(string message = "Success") =>
            new ApiResponse { Success = true, Message = message };

        /// <summary>
        /// Creates a failed response.
        /// </summary>
        public static ApiResponse FailureResponse(string message, IEnumerable<string>? errors = null) =>
            new ApiResponse { Success = false, Message = message, Errors = errors };
    }
}
