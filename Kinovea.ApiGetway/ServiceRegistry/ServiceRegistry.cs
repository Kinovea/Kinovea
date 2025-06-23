using Kinovea.ApiGetway.Models;
using System.Collections.Concurrent;

namespace Kinovea.ApiGetway.ServiceRegistry
{
    public class ServiceRegistry
    {
        private readonly ConcurrentDictionary<string, ServiceInfo> _services = new();

        public void RegisterService(string name, string url, Dictionary<string, string> metadata)
        {
            _services.AddOrUpdate(name, new ServiceInfo
            {
                Name = name,
                Url = url,
                Status = "Running",
                LastHeartbeat = DateTime.UtcNow,
                Metadata = metadata
            }, (_, existing) =>
            {
                existing.LastHeartbeat = DateTime.UtcNow;
                return existing;
            });
        }

        public IEnumerable<ServiceInfo> GetServices()
        {
            return _services.Values;
        }
    }
}
