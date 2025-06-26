using Kinovea.ApiGetway.Extensions;
using Kinovea.ApiGetway.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Prometheus;
using System;
using System.Reflection;

namespace Kinovea.ApiGetway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppContext.SetSwitch("Microsoft.AspNetCore.Http.UseSystemWeb", true);

            var builder = WebApplication.CreateBuilder(args);

            try
            {
                // 基础配置
                builder.Configuration
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();

                // 添加选项配置
                builder.Services.AddConfigurationServices(builder.Configuration);

                // 添加Ocelot服务
                builder.Services
                                 .AddOcelot(builder.Configuration)
                                 .AddDelegatingHandler<OcelotLoggingHandler>(true);

                // 添加健康检查
                builder.Services.AddCustomHealthChecks(builder.Configuration);

                // 添加Swagger
                builder.Services.AddCustomSwagger(builder.Configuration);

                // 添加分布式缓存
                builder.Services.AddDistributedMemoryCache();

                // 添加CORS
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", cors =>
                    {
                        cors.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
                });

                var app = builder.Build();
                var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

                // 开发环境配置
                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseCustomSwaggerUI();
                }

                // 配置健康检查端点
                app.UseCustomHealthChecks();

                // 配置指标监控
                app.UseMetricServer();
                app.UseHttpMetrics();

                // 应用中间件
                app.UseMiddleware<LoggingMiddleware>(); // 使用ASP.NET Core日志中间件
                app.UseCors("AllowAll");
                app.UseHttpsRedirection();

                // 使用Ocelot
                app.UseOcelot().Wait();

                app.Run();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine("Ocelot加载类型时出错:");
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    Console.WriteLine($"加载错误: {loaderException.Message}");
                    Console.WriteLine($"堆栈跟踪: {loaderException.StackTrace}");
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序启动时出错: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                throw;
            }
        }
    }
}
