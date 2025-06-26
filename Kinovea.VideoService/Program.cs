using Kinovea.VideoService.Models;
using Kinovea.VideoService.Services.Implementations;
using Kinovea.VideoService.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Kinovea.VideoService
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
            // 注册视频服务
            services.AddSingleton<IVideoReaderFactory, VideoReaderFactory>();
            services.AddScoped<FFmpegVideoReader>();
            // 配置 FFmpeg
            // 配置 FFmpeg 选项
            services.Configure<FFmpegOptions>(options =>
            {
                options.FFmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg");
                options.TempPath = Path.Combine(AppContext.BaseDirectory, "temp");
                options.EnableHardwareAcceleration = true;
            });
        }
    }

   
}
