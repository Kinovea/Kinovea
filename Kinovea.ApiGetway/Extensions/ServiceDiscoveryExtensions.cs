using Consul;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using Kinovea.ApiGetway.Configuration;
using Kinovea.ApiGetway.ServiceRegistry;

namespace Kinovea.ApiGetway.Extensions
{
    public static class ServiceDiscoveryExtensions
    {
        public static IServiceCollection AddConsulServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 注册Consul客户端
            services.AddSingleton<IConsulClient>(p => new ConsulClient(cfg =>
            {
                var consulConfig = configuration.GetSection("Consul");
                cfg.Address = new Uri(consulConfig["Url"] ?? "http://localhost:8500");
            }));

            // 注册服务注册适配器
            services.AddSingleton<IServiceRegistryAdapter, ConsulServiceRegistry>();

            // 注册本地服务注册表 (用于备份)
            services.AddSingleton<ServiceRegistry.ServiceRegistry>();

            return services;
        }

        public static IApplicationBuilder UseConsulServiceDiscovery(
            this IApplicationBuilder app,
            IHostApplicationLifetime lifetime,
            IConfiguration configuration)
        {
            var serviceRegistry = app.ApplicationServices.GetRequiredService<IServiceRegistryAdapter>();
            var gatewayConfig = configuration.GetSection("Gateway").Get<GatewayOptions>();

            if (gatewayConfig == null)
            {
                throw new InvalidOperationException("未找到Gateway配置节");
            }

            // 生成服务ID
            var serviceId = $"{gatewayConfig.ServiceName}-{Guid.NewGuid()}";

            // 注册网关服务
            serviceRegistry.RegisterServiceAsync(
                gatewayConfig.ServiceName,
                $"http://{gatewayConfig.ServiceAddress}:{gatewayConfig.ServicePort}",
                new Dictionary<string, string> {
                    { "type", "gateway" },
                    { "api", "true" }
                })
                .GetAwaiter()
                .GetResult();

            // 应用停止时注销服务
            lifetime.ApplicationStopping.Register(() =>
            {
                serviceRegistry.DeregisterServiceAsync(serviceId)
                    .GetAwaiter()
                    .GetResult();
            });

            return app;
        }
    }
}
