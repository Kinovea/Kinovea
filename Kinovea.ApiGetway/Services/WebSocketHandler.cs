namespace Kinovea.ApiGetway.Services
{
    public class WebSocketHandler
    {
        private readonly ILogger<WebSocketHandler> _logger;
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        public WebSocketHandler(ILogger<WebSocketHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
        {
            var socketId = Guid.NewGuid().ToString();
            _sockets.TryAdd(socketId, webSocket);

            try
            {
                await ProcessWebSocketMessages(socketId, webSocket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket处理错误");
            }
            finally
            {
                _sockets.TryRemove(socketId, out _);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "连接关闭", CancellationToken.None);
            }
        }
    }
}
