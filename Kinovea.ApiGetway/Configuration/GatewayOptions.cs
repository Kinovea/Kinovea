namespace Kinovea.ApiGetway.Configuration
{
    /// <summary>
    /// API 网关配置选项
    /// </summary>
    public class GatewayOptions
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; } = "kinovea-gateway";

        /// <summary>
        /// 服务地址
        /// </summary>
        public string ServiceAddress { get; set; } = "localhost";

        /// <summary>
        /// 服务端口
        /// </summary>
        public int ServicePort { get; set; } = 5000;

        /// <summary>
        /// 路由前缀
        /// </summary>
        public string RoutePrefix { get; set; } = "api";

        /// <summary>
        /// 是否启用HTTPS
        /// </summary>
        public bool UseHttps { get; set; } = false;

        /// <summary>
        /// 请求超时时间（秒）
        /// </summary>
        public int RequestTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 全局请求重试次数
        /// </summary>
        public int GlobalRetryCount { get; set; } = 3;

        /// <summary>
        /// 获取网关的基础URL
        /// </summary>
        public string BaseUrl => $"{(UseHttps ? "https" : "http")}://{ServiceAddress}:{ServicePort}";
    }
}
