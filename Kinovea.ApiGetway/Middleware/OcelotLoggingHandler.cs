using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kinovea.ApiGetway.Middleware
{
    /// <summary>
    /// Ocelot HTTP消息处理程序 处理 Ocelot 转发到下游服务的请求
    /// </summary>
    public class OcelotLoggingHandler : DelegatingHandler
    {
        private readonly ILogger<OcelotLoggingHandler> _logger;

        public OcelotLoggingHandler(ILogger<OcelotLoggingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation(
                    "Ocelot转发请求: {Method} {Uri}",
                    request.Method,
                    request.RequestUri);

                var response = await base.SendAsync(request, cancellationToken);

                var elapsed = DateTime.UtcNow - requestTime;
                _logger.LogInformation(
                    "Ocelot请求完成: {Method} {Uri} - 状态码: {StatusCode}, 耗时: {Elapsed}ms",
                    request.Method,
                    request.RequestUri,
                    response.StatusCode,
                    elapsed.TotalMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ocelot请求发生错误: {Method} {Uri}",
                    request.Method,
                    request.RequestUri);
                throw;
            }
        }
    }
}
