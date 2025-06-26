using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kinovea.CameraService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 配置用于Docker的Kestrel
            var configuration = builder.Configuration;
            // 配置 Kestrel
            builder.WebHost.ConfigureKestrel(options =>
            {
                // Kestrel 会自动从配置文件读取 Endpoints 配置
                // 不需要显式配置，因为在 appsettings.json 和 appsettings.Development.json 中已经定义
            });

            // 添加健康检查
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy());

            // 添加控制器
            builder.Services.AddControllers();
            
            // 添加Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // 配置请求处理管道
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 添加健康检查端点
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true
            });

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
