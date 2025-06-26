using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Kinovea.ApiGetway.Middleware
{
    /// <summary>
    /// ASP.NET Core 日志中间件 处理进入网关的原始请求
    /// </summary>
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation(
                    "开始处理请求: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await _next(context);

                var elapsed = DateTime.UtcNow - requestTime;
                _logger.LogInformation(
                    "请求处理完成: {Method} {Path} - 状态码: {StatusCode}, 耗时: {Elapsed}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "请求处理发生错误: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                throw;
            }
        }
    }
}
