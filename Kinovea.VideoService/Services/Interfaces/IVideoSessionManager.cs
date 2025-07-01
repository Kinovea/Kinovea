using Kinovea.Video;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频会话管理
    /// </summary>
    public interface IVideoSessionManager
    {
        Task<string> CreateSessionAsync(string bucketName, string objectName);
        Task<VideoReader> GetReaderAsync(string sessionId);
        Task RemoveSessionAsync(string sessionId);
        Task<bool> IsSessionActiveAsync(string sessionId);
    }
}
