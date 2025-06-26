using HealthChecks.UI.Client;
using Kinovea.ApiGetway.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kinovea.ApiGetway.Extensions
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddCustomHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var healthChecks = services.AddHealthChecks();

            // 添加基础健康检查
            healthChecks.AddCheck("gateway-self", () => HealthCheckResult.Healthy(), tags: new[] { "gateway" });

            // 添加微服务健康检查
            var serviceSection = configuration.GetSection("ServiceRegistry:Services");
            var serviceList = serviceSection.Get<List<ServiceConfig>>();

            if (serviceList != null)
            {
                foreach (var serviceConfig in serviceList)
                {
                    if (string.IsNullOrEmpty(serviceConfig.Name))
                    {
                        continue;
                    }

                    var healthCheckUrl = $"{serviceConfig.Url.TrimEnd('/')}{serviceConfig.HealthCheck}";
                    healthChecks.AddUrlGroup(
                        new Uri(healthCheckUrl),
                        name: $"{serviceConfig.Name}-health", // 使用服务名称作为健康检查名称
                        failureStatus: HealthStatus.Degraded,
                        tags: new[] { "microservice" });
                }
            }

            // 添加健康检查UI和存储
            services
                .AddHealthChecksUI(setup =>
                {
                    setup.SetEvaluationTimeInSeconds(30);
                    setup.MaximumHistoryEntriesPerEndpoint(60);

                    // 添加网关自身的健康检查
                    setup.AddHealthCheckEndpoint("Gateway", "/health");

                    // 添加各个微服务的健康检查
                    if (serviceList != null)
                    {
                        foreach (var service in serviceList)
                        {
                            setup.AddHealthCheckEndpoint(
                                service.Name,
                                $"{service.Url.TrimEnd('/')}{service.HealthCheck}");
                        }
                    }
                })
                .AddInMemoryStorage();

            return services;
        }

        public static WebApplication UseCustomHealthChecks(this WebApplication app)
        {
            // 配置健康检查端点
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // 配置健康检查UI
            app.MapHealthChecksUI(options =>
            {
                options.UIPath = "/health-ui";
                options.ApiPath = "/health-api";
            });

            return app;
        }
    }
}
