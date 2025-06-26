using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kinovea.ApiGetway.Configuration;

namespace Kinovea.ApiGetway.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddConfigurationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 使用Options模式绑定强类型配置
            services.Configure<GatewayOptions>(
                configuration.GetSection("Gateway"));
                
            services.Configure<ServiceRegistryOptions>(
                configuration.GetSection("ServiceRegistry"));
                
            services.Configure<SwaggerOptions>(
                configuration.GetSection("Swagger"));
                
            services.Configure<HealthCheckOptions>(
                configuration.GetSection("HealthChecks"));

            return services;
        }
    }
}
