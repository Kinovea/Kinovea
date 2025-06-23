using HealthChecks.UI.Client;
using Kinovea.ApiGetway.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Prometheus;

namespace Kinovea.ApiGetway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 健康检查配置
            builder.Services.AddHealthChecks()
                            .AddCheck("self", () => HealthCheckResult.Healthy())
                            .AddUrlGroup(new Uri("http://localhost:5001/health"), name: "video-service")
                            .AddUrlGroup(new Uri("http://localhost:5002/health"), name: "camera-service");

            // 健康检查UI配置
            builder.Services.AddHealthChecksUI()
                            .AddInMemoryStorage();

            // Swagger配置
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Kinovea API Gateway",
                    Version = "v1"
                });

                // 修改为正确的 OpenApiInfo 类型
                c.SwaggerDoc("video-service", new OpenApiInfo
                {
                    Title = "Video Service API",
                    Version = "v1",
                    Description = "API for managing video services in Kinovea",
                    Contact = new OpenApiContact
                    {
                        Name = "Video Service Team",
                        Email = "video@kinovea.org"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "GNU GPLv2",
                        Url = new Uri("http://www.gnu.org/licenses/")
                    }
                });

                c.SwaggerDoc("camera-service", new OpenApiInfo
                {
                    Title = "Camera Service API",
                    Version = "v1",
                    Description = "API for managing camera services in Kinovea",
                    Contact = new OpenApiContact
                    {
                        Name = "Camera Service Team",
                        Email = "camera@kinovea.org"
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
                    Url = "http://localhost:5001",
                    Description = "Video Service"
                });

                c.AddServer(new OpenApiServer
                {
                    Url = "http://localhost:5002",
                    Description = "Camera Service"
                });
            });

            // 其他服务配置
            builder.Services.AddMetrics();
            builder.Services.AddDistributedMemoryCache();

            // Ocelot 配置
            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
            builder.Services.AddOcelot(builder.Configuration);

            // CORS 配置
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // 配置中间件管道
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kinovea API Gateway v1");
                    c.SwaggerEndpoint("/swagger/video-service/swagger.json", "Video Service API v1");
                    c.SwaggerEndpoint("/swagger/camera-service/swagger.json", "Camera Service API v1");

                    // 配置 Swagger UI
                    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                    c.DefaultModelsExpandDepth(-1); // 隐藏 Models 部分
                    c.RoutePrefix = "swagger"; // 可以通过 /swagger 访问
                });
            }

            // 健康检查端点
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapHealthChecksUI(options =>
            {
                options.UIPath = "/health-ui";
                options.ApiPath = "/health-api";
            });

            // 其他中间件配置
            app.UseMetricServer();
            app.UseHttpMetrics();
            app.UseMiddleware<LoggingMiddleware>();
            app.UseCors("AllowAll");
            app.UseMiddleware<RateLimitMiddleware>();
            app.UseHttpsRedirection();

            // Ocelot 中间件
            app.UseOcelot().Wait();

            app.Run();
        }
    }
}
