using Consul;
using Kinovea.ApiGetway.Configuration;


namespace Kinovea.ApiGetway.ServiceRegistry
{
    /// <summary>
    /// Consul服务注册适配器实现
    /// </summary>
    public class ConsulServiceRegistry : IServiceRegistryAdapter
    {
        private readonly IConsulClient _consulClient;
        private readonly ILogger<ConsulServiceRegistry> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="consulClient">Consul客户端</param>
        /// <param name="logger">日志记录器</param>
        public ConsulServiceRegistry(IConsulClient consulClient, ILogger<ConsulServiceRegistry> logger)
        {
            _consulClient = consulClient ?? throw new ArgumentNullException(nameof(consulClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        public async Task<bool> RegisterServiceAsync(string name, string url, Dictionary<string, string> metadata)
        {
            try
            {
                var serviceId = $"{name}-{Guid.NewGuid()}";
                var uri = new Uri(url);
                
                // 创建服务注册信息
                var registration = new AgentServiceRegistration
                {
                    ID = serviceId,
                    Name = name,
                    Address = uri.Host,
                    Port = uri.Port,
                    Tags = metadata?.Keys.ToArray() ?? Array.Empty<string>(),
                    Meta = metadata ?? new Dictionary<string, string>(),
                    Check = new AgentServiceCheck
                    {
                        HTTP = $"{url}/health",
                        Interval = TimeSpan.FromSeconds(30),
                        Timeout = TimeSpan.FromSeconds(5),
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
                    }
                };

                // 注册服务
                await _consulClient.Agent.ServiceRegister(registration);
                _logger.LogInformation("服务 {ServiceName} 已注册到Consul，ID: {ServiceId}", name, serviceId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册服务 {ServiceName} 到Consul失败", name);
                return false;
            }
        }

        /// <summary>
        /// 注销服务
        /// </summary>
        public async Task<bool> DeregisterServiceAsync(string serviceId)
        {
            try
            {
                await _consulClient.Agent.ServiceDeregister(serviceId);
                _logger.LogInformation("服务 {ServiceId} 已从Consul注销", serviceId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从Consul注销服务 {ServiceId} 失败", serviceId);
                return false;
            }
        }

        /// <summary>
        /// 获取所有服务
        /// </summary>
        public async Task<IEnumerable<Models.ServiceInfo>> GetServicesAsync()
        {
            try
            {
                var services = new List<Models.ServiceInfo>();
                var consulServices = await _consulClient.Agent.Services();
                
                foreach (var service in consulServices.Response)
                {
                    var checks = await _consulClient.Health.Service(service.Value.Service);
                    var status = checks.Response.Any(c => c.Checks.Any(check => check.Status == HealthStatus.Passing)) 
                        ? "Running" 
                        : "Unhealthy";
                    
                    services.Add(new Models.ServiceInfo
                    {
                        Name = service.Value.Service,
                        Url = $"http://{service.Value.Address}:{service.Value.Port}",
                        Status = status,
                        LastHeartbeat = DateTime.UtcNow,
                        Metadata = (Dictionary<string, string>)(service.Value.Meta ?? new Dictionary<string, string>())
                    });
                }
                
                return services;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从Consul获取服务列表失败");
                return Enumerable.Empty<Models.ServiceInfo>();
            }
        }

        /// <summary>
        /// 获取服务健康状态
        /// </summary>
        public async Task<bool> IsServiceHealthyAsync(string serviceName)
        {
            try
            {
                var healthChecks = await _consulClient.Health.Service(serviceName);
                return healthChecks.Response.Any(s => s.Checks.All(c => c.Status == HealthStatus.Passing));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查服务 {ServiceName} 的健康状态失败", serviceName);
                return false;
            }
        }
    }
}
