using Kinovea.ApiGetway.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;

namespace Kinovea.ApiGetway.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddCustomSwagger(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Kinovea API Gateway",
                    Version = "v1"
                });

                // 从配置中读取服务信息
                var serviceSection = configuration.GetSection("ServiceRegistry:Services");
                var serviceList = serviceSection.Get<List<ServiceConfig>>();

                if (serviceList != null)
                {
                    foreach (var service in serviceList)
                    {
                        //var serviceName = new Uri(service.Url).Host;
                        var serviceName = service.Name;
                        c.SwaggerDoc(serviceName, new OpenApiInfo
                        {
                            Title = $"{serviceName} API",
                            Version = "v1",
                            Description = $"API for {serviceName} in Kinovea",
                            Contact = new OpenApiContact
                            {
                                Name = $"{serviceName} Team",
                                Email = $"{serviceName.ToLower()}@kinovea.org"
                            },
                            License = new OpenApiLicense
                            {
                                Name = "GNU GPLv2",
                                Url = new Uri("http://www.gnu.org/licenses/")
                            }
                        });

                        // 添加服务器配置
                        c.AddServer(new OpenApiServer
                        {
                            Url = service.Url,
                            Description = serviceName
                        });
                    }
                }
            });

            return services;
        }

        public static WebApplication UseCustomSwaggerUI(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kinovea API Gateway v1");

                // 从配置中读取服务信息
                var serviceSection = app.Configuration.GetSection("ServiceRegistry:Services");
                var serviceList = serviceSection.Get<List<ServiceConfig>>();

                if (serviceList != null)
                {
                    foreach (var service in serviceList)
                    {
                        var serviceName = new Uri(service.Url).Host;
                        c.SwaggerEndpoint($"/swagger/{serviceName}/swagger.json", $"{serviceName} API v1");
                    }
                }

                // 配置 Swagger UI
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelsExpandDepth(-1); // 隐藏 Models 部分
                c.RoutePrefix = "swagger"; // 可以通过 /swagger 访问
            });

            return app;
        }
    }
}
