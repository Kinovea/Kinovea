using Kinovea.VideoService.Models;
using Kinovea.VideoService.Services.Implementations;
using Kinovea.VideoService.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Kinovea.VideoService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ����log4net
            var logRepository = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));


            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;
            var port = configuration.GetValue<int>("Kestrel:Endpoints:Http:Port", 5001);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(port);
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            ConfigureServices(builder.Services);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            // ע��MinIO����
            services.AddScoped<IMinioService, MinioService>();

            // ע����Ƶ����
            services.AddScoped<BasicVideoReader>();
            services.AddScoped<IVideoFileOperations>(provider => provider.GetRequiredService<BasicVideoReader>());
            services.AddScoped<IVideoPlayback>(provider => provider.GetRequiredService<BasicVideoReader>());

            // ���VideoTypeManager��ʼ��
            services.AddSingleton<IVideoTypeManagerService, VideoTypeManagerService>();

        }
    }
}
