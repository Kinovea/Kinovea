using Microsoft.Extensions.Caching.Distributed;

namespace Kinovea.ApiGetway.Middleware
{
    /// <summary>
    /// 中间件用于实现API速率限制
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        private readonly ILogger<RateLimitMiddleware> _logger;

        public RateLimitMiddleware(RequestDelegate next, IDistributedCache cache, ILogger<RateLimitMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var key = GetRateLimitKey(context);
            if (await IsRateLimitExceeded(key))
            {
                context.Response.StatusCode = 429; // Too Many Requests
                return;
            }

            await _next(context);
        }

        private string GetRateLimitKey(HttpContext context)
        {
            return $"ratelimit_{context.Request.Path}_{context.Connection.RemoteIpAddress}";
        }
    }
}
