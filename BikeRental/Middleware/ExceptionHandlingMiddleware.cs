using BikeRental.Exceptions;
using BikeRental.Models.Api;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Linq;

namespace BikeRental.Middleware
{
    /// <summary>
    /// Global exception handling middleware for consistent error responses.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {ExceptionMessage}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode = exception switch
            {
                ResourceNotFoundException => (int)HttpStatusCode.NotFound,
                ValidationException => (int)HttpStatusCode.BadRequest,
                ConflictException => (int)HttpStatusCode.Conflict,
                DataAccessException => (int)HttpStatusCode.InternalServerError,
                _ => (int)HttpStatusCode.InternalServerError
            };

            ApiResponse<object> apiResponse;

            switch (exception)
            {
                case ResourceNotFoundException ex:
                    apiResponse = ApiResponse<object>.FailureResponse(ex.Message);
                    break;
                case ValidationException ex:
                    var validationErrors = ex.Errors?.SelectMany(kv => kv.Value.Select(v => $"{kv.Key}: {v}")) ?? Enumerable.Empty<string>();
                    apiResponse = ApiResponse<object>.FailureResponse(ex.Message, validationErrors);
                    break;
                case ConflictException ex:
                    apiResponse = ApiResponse<object>.FailureResponse(ex.Message);
                    break;
                case DataAccessException ex:
                    apiResponse = ApiResponse<object>.FailureResponse("A data access error occurred.", new[] { ex.Message });
                    break;
                default:
                    apiResponse = ApiResponse<object>.FailureResponse("An internal server error occurred.", new[] { exception.Message });
                    break;
            }

            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(apiResponse);
        }
    }
}
