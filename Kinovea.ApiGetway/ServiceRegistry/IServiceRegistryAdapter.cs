using Kinovea.ApiGetway.Models;

namespace Kinovea.ApiGetway.ServiceRegistry
{
    /// <summary>
    /// 服务注册适配器接口
    /// </summary>
    public interface IServiceRegistryAdapter
    {
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="name">服务名称</param>
        /// <param name="url">服务URL</param>
        /// <param name="metadata">服务元数据</param>
        /// <returns>注册成功返回true，否则返回false</returns>
        Task<bool> RegisterServiceAsync(string name, string url, Dictionary<string, string> metadata);

        /// <summary>
        /// 注销服务
        /// </summary>
        /// <param name="serviceId">服务ID</param>
        /// <returns>注销成功返回true，否则返回false</returns>
        Task<bool> DeregisterServiceAsync(string serviceId);

        /// <summary>
        /// 获取所有服务
        /// </summary>
        /// <returns>服务列表</returns>
        Task<IEnumerable<ServiceInfo>> GetServicesAsync();

        /// <summary>
        /// 获取服务健康状态
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>服务健康状态</returns>
        Task<bool> IsServiceHealthyAsync(string serviceName);
    }
}