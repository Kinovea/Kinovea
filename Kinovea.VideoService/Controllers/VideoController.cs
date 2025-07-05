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

        /// <summary>
        /// 上传视频文件到MinIO
        /// </summary>
        /// <param name="request">视频上传请求</param>
        /// <returns>上传结果</returns>
        [HttpPost("minio/upload")]
        public async Task<ActionResult<string>> UploadVideoToMinioAsync([FromForm] MinioVideoUploadRequest request)
        {
            try
            {
                // 验证文件
                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest("上传的文件不能为空");
                }

                // 验证参数
                if (string.IsNullOrWhiteSpace(request.BucketName))
                {
                    return BadRequest("存储桶名称不能为空");
                }

                if (string.IsNullOrWhiteSpace(request.ObjectName))
                {
                    return BadRequest("对象名称不能为空");
                }

                // 去掉限制文件大小，应为专业的视频分析场景可能需要上传大文件
                // 验证文件大小（例如：限制为 500MB）
                //const long maxFileSize = 500 * 1024 * 1024; // 500MB
                //if (request.File.Length > maxFileSize)
                //{
                //    return BadRequest($"文件大小超过限制 ({maxFileSize / (1024 * 1024)}MB)");
                //}

                // 验证文件扩展名
                string extension = Path.GetExtension(request.File.FileName);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = Path.GetExtension(request.ObjectName);
                }

                if (!_videoTypeManager.IsFormatSupported(extension))
                {
                    return BadRequest($"不支持的视频格式: {extension}");
                }

                // 保存的视频文件名规则为：用户名+时间戳+文件名+扩展名，用于避免文件名冲突，以及便于追踪
                // 用户名暂时使用默认值 "anonymous"
                string userName = "anonymous"; // 可以根据实际情况获取用户名
                string timeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string fileName = $"{userName}_{timeStamp}_{request.ObjectName}{extension}";


                // 记录上传开始
                _logger.LogInformation("开始上传视频文件: {FileName} -> {BucketName}/{ObjectName}, 大小: {FileSize} bytes",
                    request.File.FileName, request.BucketName, request.ObjectName, request.File.Length);

                // 使用文件流直接上传到MinIO
                using var fileStream = request.File.OpenReadStream();
                string uploadResult = await _minioService.UploadFileAsync(
                    request.BucketName,
                    fileName, 
                    fileStream);

                // 记录上传成功
                _logger.LogInformation("视频文件上传成功: {BucketName}/{ObjectName}, 上传结果: {UploadResult}",
                    request.BucketName, fileName, uploadResult);

                // 返回上传结果和文件信息
                var result = new
                {
                    Message = "视频上传成功",
                    request.BucketName,
                    request.ObjectName,
                    OriginalFileName = request.File.FileName,
                    FileSize = request.File.Length,
                    request.File.ContentType,
                    UploadTime = DateTime.UtcNow,
                    UploadResult = uploadResult
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传视频到MinIO时出错: {BucketName}/{ObjectName}",
                    request.BucketName, request.ObjectName);
                return StatusCode(500, $"上传视频文件时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取支持的视频格式列表
        /// </summary>
        /// <returns>支持的视频格式</returns>
        [HttpGet("supported-formats")]
        public ActionResult<object> GetSupportedFormats()
        {
            try
            {
                // 常见的视频格式扩展名
                var commonVideoExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv", ".webm", ".m4v", ".3gp" };
                
                var supportedFormats = commonVideoExtensions
                    .Where(ext => _videoTypeManager.IsFormatSupported(ext))
                    .ToList();

                return Ok(new
                {
                    SupportedFormats = supportedFormats,
                    TotalCount = supportedFormats.Count,
                    Message = "支持的视频格式列表"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取支持的视频格式时出错");
                return StatusCode(500, "获取支持格式时发生错误");
            }
        }
    }
}
