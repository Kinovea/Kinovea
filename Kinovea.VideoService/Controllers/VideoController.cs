using Kinovea.VideoService.Models;
using Kinovea.VideoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// 构造函数
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
    }
}
