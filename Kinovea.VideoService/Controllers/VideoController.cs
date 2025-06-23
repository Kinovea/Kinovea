using Kinovea.VideoService.Models;
using Kinovea.VideoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kinovea.VideoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService _videoService;

        [HttpGet("{id}")]
        public async Task<ActionResult<VideoInfo>> GetVideo(string id)
        {
            var video = await _videoService.GetVideoAsync(id);
            return Ok(video);
        }

        [HttpPost("process")]
        public async Task<ActionResult<AnalysisResult>> ProcessVideo([FromBody] VideoProcessRequest request)
        {
            var result = await _videoService.ProcessVideoAsync(request);
            return Ok(result);
        }
    }
}
