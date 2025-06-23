using Microsoft.AspNetCore.Mvc;

namespace Kinovea.ApiGetway.Controllers
{
    /// <summary>
    /// 控制器用于处理视频流请求
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VideoStreamController : ControllerBase
    {
        private readonly ILogger<VideoStreamController> _logger;

        public VideoStreamController(ILogger<VideoStreamController> logger)
        {
            _logger = logger;
        }

        [HttpGet("stream/{id}")]
        public async Task<IActionResult> GetVideoStream(string id)
        {
            try
            {
                // 实现视频流处理逻辑
                var stream = await GetVideoStreamFromService(id);
                return File(stream, "video/mp4");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取视频流失败");
                return StatusCode(500);
            }
        }
    }
}
