using Kinovea.Video;
using Kinovea.VideoService.Models.Requests;
using Kinovea.VideoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kinovea.VideoService.Controllers
{

    /// <summary>
    /// 视频播放器控制器( 逐帧播放方案（适用于专业视频分析场景）
    /// </summary>
    [ApiController]
    [Route("api/player")]
    public class PlayerController : ControllerBase
    {
        private readonly IVideoTypeManagerService _videoTypeManager;
        private readonly IMinioService _minioService;

        private readonly IVideoSessionManager _sessionManager;
        private readonly ILogger<PlayerController> _logger;

        /// 客户端使用流程：
        //1.	创建播放会话
        //2.	获取视频基本信息
        //3.	请求具体帧数据
        //4.	实现本地帧缓存
        //5.	会话结束时清理资源
        public PlayerController(
            IVideoTypeManagerService videoTypeManager,
            IMinioService minioService,
            IVideoSessionManager sessionManager,
            ILogger<PlayerController> logger)
        {
            _videoTypeManager = videoTypeManager;
            _minioService = minioService;
            _sessionManager = sessionManager;
            _logger = logger;
        }

        [HttpPost("frame")]
        public async Task<ActionResult<byte[]>> GetVideoFrame([FromBody] VideoFrameRequest request)
        {
            string? tempFilePath = null;
            try
            {
                // 下载视频到临时文件
                tempFilePath = await _minioService.DownloadFileToTempAsync(
                    request.BucketName,
                    request.ObjectName);

                var reader = _videoTypeManager.GetVideoReader(tempFilePath);
                if (reader == null)
                    return BadRequest("无法创建视频读取器");

                // 移动到指定帧
                bool success = reader.MoveTo(request.CurrentFrame, request.TargetFrame);
                if (!success)
                    return BadRequest("无法移动到指定帧");

                // 获取当前帧的图像数据
                var frame = reader.Current;
                return Ok(frame.Image);
            }
            finally
            {
                if (tempFilePath != null)
                {
                    _minioService.CleanupTempFile(tempFilePath);
                }
            }
        }



        /// <summary>
        /// 创建播放会话
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("session")]
        public async Task<ActionResult<string>> CreateSession([FromBody] VideoSessionRequest request)
        {
            try
            {
                var sessionId = await _sessionManager.CreateSessionAsync(
                    request.BucketName,
                    request.ObjectName);
                return Ok(sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建播放会话失败");
                return StatusCode(500, "创建会话时发生错误");
            }
        }

        // 2. 获取视频信息
        [HttpGet("info/{sessionId}")]
        public async Task<ActionResult<VideoInfo>> GetVideoInfo(string sessionId)
        {
            try
            {
                var reader = await _sessionManager.GetReaderAsync(sessionId);
                if (reader == null)
                    return NotFound("会话不存在");

                return Ok(reader.Info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取视频信息失败");
                return StatusCode(500, "获取视频信息时发生错误");
            }
        }
        /// <summary>
        /// 获取指定帧
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="targetFrame"></param>
        /// <returns></returns>
        [HttpGet("frame/{sessionId}")]
        public async Task<ActionResult<VideoFrameResponse>> GetFrame(
            string sessionId,
            [FromQuery] long targetFrame)
        {
            try
            {
                var reader = await _sessionManager.GetReaderAsync(sessionId);
                if (reader == null)
                    return NotFound("会话不存在");

                // 使用时间戳来移动帧
                long currentTimestamp = reader.Current?.Timestamp ?? reader.Info.FirstTimeStamp;
                long targetTimestamp = reader.Info.FirstTimeStamp +
                    (targetFrame * reader.Info.AverageTimeStampsPerFrame);

                if (!reader.MoveTo(currentTimestamp, targetTimestamp))
                    return BadRequest("无法移动到指定帧");

                return Ok(new VideoFrameResponse
                {
                    Image = reader.Current?.Image,
                    Timestamp = reader.Current.Timestamp
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取视频帧失败");
                return StatusCode(500, "获取视频帧时发生错误");
            }
        }

        /// <summary>
        /// 批量获取帧信息（用于缓存优化）
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="startFrame"></param>
        /// <param name="endFrame"></param>
        /// <returns></returns>
        [HttpGet("frames/{sessionId}/batch")]
        public async Task<ActionResult<List<VideoFrameInfo>>> GetFramesBatch(
            string sessionId,
            [FromQuery] long startFrame,
            [FromQuery] long endFrame)
        {
            try
            {
                var reader = await _sessionManager.GetReaderAsync(sessionId);
                if (reader == null)
                    return NotFound("会话不存在");

                var frames = new List<VideoFrameInfo>();
                long baseTimestamp = reader.Info.FirstTimeStamp;
                long frameInterval = reader.Info.AverageTimeStampsPerFrame;

                for (long i = startFrame; i <= endFrame; i++)
                {
                    long currentTimestamp = reader.Current?.Timestamp ?? baseTimestamp;
                    long targetTimestamp = baseTimestamp + (i * frameInterval);

                    if (!reader.MoveTo(currentTimestamp, targetTimestamp))
                        break;

                    frames.Add(new VideoFrameInfo
                    {
                        Position = (reader.Current.Timestamp - baseTimestamp) / frameInterval,
                        Timestamp = reader.Current.Timestamp
                    });
                }

                return Ok(frames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量获取帧信息失败");
                return StatusCode(500, "获取帧信息时发生错误");
            }
        }
        /// <summary>
        /// 关闭会话
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        [HttpDelete("session/{sessionId}")]
        public async Task<ActionResult> CloseSession(string sessionId)
        {
            try
            {
                await _sessionManager.RemoveSessionAsync(sessionId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "关闭会话失败");
                return StatusCode(500, "关闭会话时发生错误");
            }
        }
    }
}
