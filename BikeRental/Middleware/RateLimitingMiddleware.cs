using System.Collections.Concurrent;
using System.Net;

namespace BikeRental.Middleware
{
    /// <summary>
    /// Rate limiting middleware for throttling API requests per client.
    /// Helps protect against abuse and ensures fair resource allocation.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly int _requestsPerMinute;
        private readonly ConcurrentDictionary<string, ClientRateLimit> _clients =
            new ConcurrentDictionary<string, ClientRateLimit>();

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, int requestsPerMinute = 100)
        {
            _next = next;
            _logger = logger;
            _requestsPerMinute = requestsPerMinute;

            // Cleanup task to remove expired entries
            _ = Task.Run(CleanupExpiredEntriesAsync);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientId(context);
            var now = DateTime.UtcNow;

            if (!_clients.TryGetValue(clientId, out var rateLimit))
            {
                rateLimit = new ClientRateLimit { FirstRequestTime = now, RequestCount = 1 };
                _clients.AddOrUpdate(clientId, rateLimit, (key, old) => rateLimit);
            }
            else
            {
                var timeDifference = (now - rateLimit.FirstRequestTime).TotalSeconds;

                if (timeDifference > 60)
                {
                    // Reset if the minute window has passed
                    rateLimit.FirstRequestTime = now;
                    rateLimit.RequestCount = 1;
                }
                else if (rateLimit.RequestCount >= _requestsPerMinute)
                {
                    // Rate limit exceeded
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.ContentType = "application/json";
                    _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);

                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Rate limit exceeded",
                        retryAfter = 60 - (int)timeDifference,
                        limit = _requestsPerMinute,
                        window = "60 seconds"
                    });
                    return;
                }
                else
                {
                    rateLimit.RequestCount++;
                }
            }

            await _next(context);
        }

        /// <summary>
        /// Gets the client identifier from request context.
        /// </summary>
        private string GetClientId(HttpContext context)
        {
            // Try to use user ID if authenticated
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
                return userId;

            // Fall back to IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        /// <summary>
        /// Periodically cleans up expired rate limit entries.
        /// </summary>
        private async Task CleanupExpiredEntriesAsync()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));

                    var now = DateTime.UtcNow;
                    var expiredKeys = _clients
                        .Where(kvp => (now - kvp.Value.FirstRequestTime).TotalMinutes > 5)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in expiredKeys)
                    {
                        _clients.TryRemove(key, out _);
                    }

                    if (expiredKeys.Count > 0)
                    {
                        _logger.LogDebug("Cleaned up {ExpiredCount} expired rate limit entries", expiredKeys.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in rate limit cleanup task");
                }
            }
        }

        /// <summary>
        /// Client rate limit tracking.
        /// </summary>
        private class ClientRateLimit
        {
            public DateTime FirstRequestTime { get; set; }
            public int RequestCount { get; set; }
        }
    }

    /// <summary>
    /// Extension methods for rate limiting middleware.
    /// </summary>
    public static class RateLimitingMiddlewareExtensions
    {
        /// <summary>
        /// Adds rate limiting middleware to the pipeline.
        /// </summary>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, int requestsPerMinute = 100)
        {
            return app.UseMiddleware<RateLimitingMiddleware>(requestsPerMinute);
        }
    }
}
