using Kinovea.VideoService.Models;
using Kinovea.VideoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Kinovea.VideoService.Controllers
{
    /// <summary>
    /// 视频控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService _videoService;
        private readonly IVideoReaderFactory _videoReaderFactory;
        private readonly ILogger<VideoController> _logger;

        public VideoController(
            IVideoService videoService,
            IVideoReaderFactory videoReaderFactory,
            ILogger<VideoController> logger)
        {
            _videoService = videoService;
            _videoReaderFactory = videoReaderFactory;
            _logger = logger;
        }

        /// <summary>
        /// 获取视频信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<VideoInfo>> GetVideo(string id)
        {
            var video = await _videoService.GetVideoAsync(id);
            return Ok(video);
        }

        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("process")]
        public async Task<ActionResult<AnalysisResult>> ProcessVideo([FromBody] VideoProcessRequest request)
        {
            var result = await _videoService.ProcessVideoAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// 获取视频元数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet("metadata")]
        public async Task<ActionResult<VideoMetadata>> GetMetadata([FromQuery] string path)
        {
            try
            {
                using var reader = _videoReaderFactory.CreateReader(path);
                await reader.OpenAsync(path);
                var metadata = await reader.GetMetadataAsync();
                return Ok(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取视频元数据失败");
                return StatusCode(500, "获取视频元数据失败");
            }
        }

        /// <summary>
        /// 获取视频帧
        /// </summary>
        /// <param name="path"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        [HttpGet("frame")]
        public async Task<ActionResult> GetFrame([FromQuery] string path, [FromQuery] long position)
        {
            try
            {
                using var reader = _videoReaderFactory.CreateReader(path);
                await reader.OpenAsync(path);
                var frame = await reader.GetFrameAsync(TimeSpan.FromMilliseconds(position));
                return File(frame.Data, frame.Format);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取视频帧失败");
                return StatusCode(500, "获取视频帧失败");
            }
        }
    }
}
