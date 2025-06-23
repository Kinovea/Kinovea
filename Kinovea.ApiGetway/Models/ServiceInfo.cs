

namespace Kinovea.ApiGetway.Models
{
    public class ServiceInfo
    {
        public string Name { get; internal set; }
        public string Url { get; internal set; }
        public string Status { get; internal set; }
        public DateTime LastHeartbeat { get; internal set; }
        public Dictionary<string, string> Metadata { get; internal set; }
    }
}
