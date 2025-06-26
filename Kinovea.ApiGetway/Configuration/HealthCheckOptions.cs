namespace Kinovea.ApiGetway.Configuration
{
    /// <summary>
    /// 配置健康检查选项。
    /// </summary>
    public class HealthCheckOptions
    {
        public string EndpointPath { get; set; } = "/health";
        public string UiPath { get; set; } = "/health-ui";
        public string ApiPath { get; set; } = "/health-api";
    }
}
