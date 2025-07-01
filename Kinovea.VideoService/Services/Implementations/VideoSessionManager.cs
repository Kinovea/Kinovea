using Kinovea.Services;
using Kinovea.Video;
using Kinovea.VideoService.Models;
using Kinovea.VideoService.Services.Interfaces;
using System.Collections.Concurrent;

namespace Kinovea.VideoService.Services.Implementations
{
    /// <summary>
    /// 实现会话管理方法...
    /// </summary>
    public class VideoSessionManager : IVideoSessionManager
    {
        private readonly ConcurrentDictionary<string, VideoSession> _sessions = new();
        private readonly IMinioService _minioService;
        private readonly IVideoTypeManagerService _videoTypeManager;
        private readonly ILogger<VideoSessionManager> _logger;

        public Task<string> CreateSessionAsync(string bucketName, string objectName)
        {
            throw new NotImplementedException();
        }

        public Task<VideoReader> GetReaderAsync(string sessionId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsSessionActiveAsync(string sessionId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveSessionAsync(string sessionId)
        {
            throw new NotImplementedException();
        }
    }
}
