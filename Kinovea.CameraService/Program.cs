using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kinovea.CameraService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ��������Docker��Kestrel
            var configuration = builder.Configuration;
            // ���� Kestrel
            builder.WebHost.ConfigureKestrel(options =>
            {
                // Kestrel ���Զ��������ļ���ȡ Endpoints ����
                // ����Ҫ��ʽ���ã���Ϊ�� appsettings.json �� appsettings.Development.json ���Ѿ�����
            });

            // ��ӽ������
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy());

            // ��ӿ�����
            builder.Services.AddControllers();
            
            // ���Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // ����������ܵ�
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ��ӽ������˵�
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
