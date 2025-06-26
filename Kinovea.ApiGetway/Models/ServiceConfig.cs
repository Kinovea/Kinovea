namespace Kinovea.ApiGetway.Models
{
    public class ServiceConfig
    {
        public string Url { get; set; }
        public string HealthCheck { get; set; } = "/health";

        public string Name { get; set; }
    }
}
