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

        public VideoSessionManager(
            IMinioService minioService,
            IVideoTypeManagerService videoTypeManager,
            ILogger<VideoSessionManager> logger)
        {
            _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
            _videoTypeManager = videoTypeManager ?? throw new ArgumentNullException(nameof(videoTypeManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CreateSessionAsync(string bucketName, string objectName)
        {
            try
            {
                _logger.LogInformation("创建会话: Bucket={BucketName}, Object={ObjectName}", bucketName, objectName);

                // 检查文件是否存在
                if (!await _minioService.ObjectExistsAsync(bucketName, objectName))
                {
                    throw new FileNotFoundException($"文件不存在: {bucketName}/{objectName}");
                }

                // 生成唯一会话ID
                string sessionId = Guid.NewGuid().ToString();

                // 下载文件到临时目录
                string tempFilePath = await _minioService.DownloadFileToTempAsync(bucketName, objectName);

                // 检查文件格式是否支持
                string extension = Path.GetExtension(tempFilePath);
                if (!_videoTypeManager.IsFormatSupported(extension))
                {
                    // 清理临时文件
                    _minioService.CleanupTempFile(tempFilePath);
                    throw new NotSupportedException($"不支持的视频格式: {extension}");
                }

                // 获取视频读取器
                VideoReader reader = _videoTypeManager.GetVideoReader(tempFilePath);
                if (reader == null)
                {
                    // 清理临时文件
                    _minioService.CleanupTempFile(tempFilePath);
                    throw new InvalidOperationException($"无法获取视频读取器: {tempFilePath}");
                }

                // 打开视频文件
                var openResult = reader.Open(tempFilePath);
                if (openResult != OpenVideoResult.Success)
                {
                    // 清理临时文件
                    _minioService.CleanupTempFile(tempFilePath);
                    throw new InvalidOperationException($"打开视频文件失败: {openResult}");
                }

                // 创建会话对象
                var session = new VideoSession
                {
                    Reader = reader,
                    TempFilePath = tempFilePath,
                    LastAccessed = DateTime.UtcNow
                };

                // 存储会话
                _sessions[sessionId] = session;

                _logger.LogInformation("会话创建成功: SessionId={SessionId}, TempFilePath={TempFilePath}",
                    sessionId, tempFilePath);

                return sessionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建会话失败: Bucket={BucketName}, Object={ObjectName}", bucketName, objectName);
                throw;
            }
        }

        public async Task<VideoReader> GetReaderAsync(string sessionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    throw new ArgumentException("会话ID不能为空", nameof(sessionId));
                }

                if (!_sessions.TryGetValue(sessionId, out VideoSession? session))
                {
                    throw new InvalidOperationException($"会话不存在: {sessionId}");
                }

                // 更新最后访问时间
                session.LastAccessed = DateTime.UtcNow;

                _logger.LogDebug("获取视频读取器: SessionId={SessionId}", sessionId);

                return await Task.FromResult(session.Reader);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取视频读取器失败: SessionId={SessionId}", sessionId);
                throw;
            }
        }

        public async Task<bool> IsSessionActiveAsync(string sessionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return false;
                }

                bool exists = _sessions.ContainsKey(sessionId);

                _logger.LogDebug("检查会话状态: SessionId={SessionId}, Active={Active}", sessionId, exists);

                return await Task.FromResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查会话状态失败: SessionId={SessionId}", sessionId);
                return false;
            }
        }

        public async Task RemoveSessionAsync(string sessionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    throw new ArgumentException("会话ID不能为空", nameof(sessionId));
                }

                if (_sessions.TryRemove(sessionId, out VideoSession? session))
                {
                    _logger.LogInformation("移除会话: SessionId={SessionId}", sessionId);

                    // 释放视频读取器资源
                    try
                    {
                        session.Reader?.Close();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "关闭视频读取器时发生错误: SessionId={SessionId}", sessionId);
                    }

                    // 清理临时文件
                    if (!string.IsNullOrWhiteSpace(session.TempFilePath))
                    {
                        try
                        {
                            _minioService.CleanupTempFile(session.TempFilePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "清理临时文件时发生错误: TempFilePath={TempFilePath}", session.TempFilePath);
                        }
                    }

                    _logger.LogInformation("会话移除成功: SessionId={SessionId}", sessionId);
                }
                else
                {
                    _logger.LogWarning("尝试移除不存在的会话: SessionId={SessionId}", sessionId);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除会话失败: SessionId={SessionId}", sessionId);
                throw;
            }
        }

        /// <summary>
        /// 清理过期会话（可选的后台任务方法）
        /// </summary>
        /// <param name="expiredMinutes">过期分钟数，默认30分钟</param>
        public async Task CleanupExpiredSessionsAsync(int expiredMinutes = 30)
        {
            try
            {
                var expiredTime = DateTime.UtcNow.AddMinutes(-expiredMinutes);
                var expiredSessions = _sessions
                    .Where(kvp => kvp.Value.LastAccessed < expiredTime)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (string sessionId in expiredSessions)
                {
                    await RemoveSessionAsync(sessionId);
                    _logger.LogInformation("清理过期会话: SessionId={SessionId}", sessionId);
                }

                if (expiredSessions.Count > 0)
                {
                    _logger.LogInformation("清理过期会话完成，共清理 {Count} 个会话", expiredSessions.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理过期会话时发生错误");
            }
        }

        /// <summary>
        /// 获取当前活跃会话数量
        /// </summary>
        /// <returns>活跃会话数量</returns>
        public int GetActiveSessionCount()
        {
            return _sessions.Count;
        }
    }
}