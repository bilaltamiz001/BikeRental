using Microsoft.AspNetCore.Mvc.Filters;

namespace BikeRental.Attributes
{
    /// <summary>
    /// Action filter for HTTP response caching.
    /// Automatically adds cache headers to responses.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CacheableResponseAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _durationInSeconds;
        private readonly string? _varyByQueryKeys;

        /// <summary>
        /// Initializes a new instance of CacheableResponseAttribute.
        /// </summary>
        /// <param name="durationInSeconds">Cache duration in seconds.</param>
        /// <param name="varyByQueryKeys">Comma-separated query parameter keys for cache variation.</param>
        public CacheableResponseAttribute(int durationInSeconds = 300, string? varyByQueryKeys = null)
        {
            _durationInSeconds = durationInSeconds;
            _varyByQueryKeys = varyByQueryKeys;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var response = context.HttpContext.Response;

            // Set cache control headers
            response.Headers.CacheControl = $"public, max-age={_durationInSeconds}";
            response.Headers.Pragma = "cache";

            if (!string.IsNullOrEmpty(_varyByQueryKeys))
            {
                response.Headers.Vary = _varyByQueryKeys;
            }

            // Add ETag support
            response.Headers.Add("X-Cache-Duration", _durationInSeconds.ToString());

            await next();
        }
    }

    /// <summary>
    /// Extension methods for cache control.
    /// </summary>
    public static class CacheControlExtensions
    {
        /// <summary>
        /// Adds cache control headers to the response.
        /// </summary>
        public static void SetCacheHeaders(this HttpResponse response, int durationInSeconds, bool isPublic = true)
        {
            var visibility = isPublic ? "public" : "private";
            response.Headers.CacheControl = $"{visibility}, max-age={durationInSeconds}";
            response.Headers.Pragma = "cache";
        }

        /// <summary>
        /// Disables caching for the response.
        /// </summary>
        public static void DisableCaching(this HttpResponse response)
        {
            response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            response.Headers.Pragma = "no-cache";
            response.Headers.Expires = "0";
        }
    }
}
