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

        private async Task<bool> IsRateLimitExceeded(string key)
        {
            // 获取当前请求计数
            var countBytes = await _cache.GetAsync(key);
            int currentCount = 0;

            if (countBytes != null)
            {
                currentCount = BitConverter.ToInt32(countBytes);
            }

            // 增加计数并设置过期时间
            currentCount++;
            await _cache.SetAsync(
                key,
                BitConverter.GetBytes(currentCount),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) // 1分钟过期
                });

            // 检查是否超过限制 (这里设置为每分钟100次请求)
            int rateLimit = 100;

            // 记录日志
            _logger.LogDebug($"Rate limit for {key}: {currentCount}/{rateLimit}");

            return currentCount > rateLimit;
        }

        private string GetRateLimitKey(HttpContext context)
        {
            return $"ratelimit_{context.Request.Path}_{context.Connection.RemoteIpAddress}";
        }
    }
}
