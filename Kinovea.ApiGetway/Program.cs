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
                // ��������
                builder.Configuration
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();

                // ���ѡ������
                builder.Services.AddConfigurationServices(builder.Configuration);

                // ���Ocelot����
                builder.Services
                                 .AddOcelot(builder.Configuration)
                                 .AddDelegatingHandler<OcelotLoggingHandler>(true);

                // ��ӽ������
                builder.Services.AddCustomHealthChecks(builder.Configuration);

                // ���Swagger
                builder.Services.AddCustomSwagger(builder.Configuration);

                // ��ӷֲ�ʽ����
                builder.Services.AddDistributedMemoryCache();

                // ���CORS
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

                // ������������
                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseCustomSwaggerUI();
                }

                // ���ý������˵�
                app.UseCustomHealthChecks();

                // ����ָ����
                app.UseMetricServer();
                app.UseHttpMetrics();

                // Ӧ���м��
                app.UseMiddleware<LoggingMiddleware>(); // ʹ��ASP.NET Core��־�м��
                app.UseCors("AllowAll");
                app.UseHttpsRedirection();

                // ʹ��Ocelot
                app.UseOcelot().Wait();

                app.Run();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine("Ocelot��������ʱ����:");
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    Console.WriteLine($"���ش���: {loaderException.Message}");
                    Console.WriteLine($"��ջ����: {loaderException.StackTrace}");
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"��������ʱ����: {ex.Message}");
                Console.WriteLine($"��ջ����: {ex.StackTrace}");
                throw;
            }
        }
    }
}
