using Kinovea.Video;
using Kinovea.VideoService.Models.Requests;
using Kinovea.VideoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace Kinovea.VideoService.Controllers
{
    /// <summary>
    /// 视频控制器
    /// </summary>
    [ApiController]
    [Route("api/video")]
    public class VideoController : ControllerBase
    {
        private readonly IVideoFileOperations _videoFileOperations;
        private readonly IVideoPlayback _videoPlayback;
        private readonly IMinioService _minioService;
        private readonly ILogger<VideoController> _logger;
        private readonly IVideoTypeManagerService _videoTypeManager;

        public VideoController(
            IVideoTypeManagerService videoTypeManager,
            IVideoFileOperations videoFileOperations,
            IVideoPlayback videoPlayback,
            IMinioService minioService,
            ILogger<VideoController> logger)
        {
            _videoTypeManager = videoTypeManager;
            _videoFileOperations = videoFileOperations;
            _videoPlayback = videoPlayback;
            _minioService = minioService;
            _logger = logger;
        }

        /// <summary>
        /// 打开视频文件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("open")]
        public ActionResult<OpenVideoResult> OpenVideo([FromBody] OpenVideoRequest request)
        {
            try
            {
                var result = _videoFileOperations.Open(request.FilePath);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "打开视频文件时出错");
                return StatusCode(500, "处理请求时发生错误");
            }
        }

        /// <summary>
        /// 打开MinIO中的视频文件
        /// </summary>
        /// <param name="request">MinIO视频请求</param>
        /// <returns>视频打开结果</returns>
        [HttpPost("minio/open")]
        public async Task<ActionResult<OpenVideoResult>> OpenMinioVideoAsync([FromBody] MinioVideoRequest request)
        {
            string tempFilePath = null;

            try
            {

                // 检查参数
                if (string.IsNullOrEmpty(request.BucketName) || string.IsNullOrEmpty(request.ObjectName))
                {
                    return BadRequest("存储桶名称和对象名称不能为空");
                }

                // 检查文件扩展名是否支持
                string extension = Path.GetExtension(request.ObjectName);
                if (!_videoTypeManager.IsFormatSupported(extension))
                {
                    return BadRequest($"不支持的文件格式: {extension}");
                }
                // 检查文件是否存在于MinIO中
                var exists = await _minioService.ObjectExistsAsync(request.BucketName, request.ObjectName);
                if (!exists)
                {
                    return NotFound($"MinIO中未找到文件：{request.BucketName}/{request.ObjectName}");
                }

                // 从MinIO下载文件到临时目录
                tempFilePath = await _minioService.DownloadFileToTempAsync(request.BucketName, request.ObjectName);

                VideoReader reader = _videoTypeManager.GetVideoReader(tempFilePath);
                if (reader == null)
                {
                    return BadRequest("无法创建视频读取器");
                }
                // 打开视频文件
                var result = reader.Open(tempFilePath);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"打开MinIO视频文件时出错：{request.BucketName}/{request.ObjectName}");
                return StatusCode(500, "处理请求时发生错误");
            }
        }

        /// <summary>
        /// 获取视频摘要
        /// </summary>
        [HttpGet("summary")]
        public ActionResult<VideoSummary> GetVideoSummary([FromQuery] string filePath, [FromQuery] int thumbsCount = 3, [FromQuery] int maxWidth = 320, [FromQuery] int maxHeight = 240)
        {
            try
            {
                var maxSize = new Size(maxWidth, maxHeight);
                var summary = _videoFileOperations.ExtractVideoSummary(filePath, thumbsCount, maxSize);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取视频摘要时出错");
                return StatusCode(500, "处理请求时发生错误");
            }
        }

        /// <summary>
        /// 从MinIO获取视频摘要
        /// </summary>
        [HttpPost("minio/summary")]
        public async Task<ActionResult<VideoSummary>> GetMinioVideoSummaryAsync([FromBody] MinioVideoRequest request, [FromQuery] int thumbsCount = 3, [FromQuery] int maxWidth = 320, [FromQuery] int maxHeight = 240)
        {
            string tempFilePath = null;

            try
            {
                // 检查参数
                if (string.IsNullOrEmpty(request.BucketName) || string.IsNullOrEmpty(request.ObjectName))
                {
                    return BadRequest("存储桶名称和对象名称不能为空");
                }

                // 检查文件是否存在于MinIO中
                var exists = await _minioService.ObjectExistsAsync(request.BucketName, request.ObjectName);
                if (!exists)
                {
                    return NotFound($"MinIO中未找到文件：{request.BucketName}/{request.ObjectName}");
                }

                // 从MinIO下载文件到临时目录
                tempFilePath = await _minioService.DownloadFileToTempAsync(request.BucketName, request.ObjectName);

                // 获取视频摘要
                var maxSize = new Size(maxWidth, maxHeight);
                var summary = _videoFileOperations.ExtractVideoSummary(tempFilePath, thumbsCount, maxSize);

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取MinIO视频摘要时出错：{request.BucketName}/{request.ObjectName}");
                return StatusCode(500, "处理请求时发生错误");
            }
            finally
            {
                // 清理临时文件
                if (tempFilePath != null)
                {
                    _minioService.CleanupTempFile(tempFilePath);
                }
            }
        }

        /// <summary>
        /// 控制视频播放：按顺序播放视频的每一帧，移动到指定帧
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("nextframe")]
        public ActionResult<bool> MoveToNextFrame([FromBody] MoveFrameRequest request)
        {
            try
            {
                //bool success = _videoPlayback.MoveToNextFrame(request.Reader, request.SkipFrames, request.DecodeIfNecessary);
                //return Ok(success);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移动到下一帧时出错");
                return StatusCode(500, "处理请求时发生错误");
            }
        }
    }
}
