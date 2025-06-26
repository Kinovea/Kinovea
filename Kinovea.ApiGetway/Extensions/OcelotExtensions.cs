using Ocelot.DependencyInjection;
using Ocelot.Provider.Consul;


namespace Kinovea.ApiGetway.Extensions
{
    public static class OcelotExtensions
    {
        public static IServiceCollection AddOcelotWithConsul(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 添加Ocelot基础服务
            return services
                .AddOcelot(configuration)
                .AddConsul()              // 添加Consul支持
                .AddConfigStoredInConsul() // 配置存储在Consul中
                .Services;
        }
    }
}
