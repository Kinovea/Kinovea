using Kinovea.VideoService.Models;
using Kinovea.VideoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kinovea.VideoService.Services.Implementations
{
    public class VideoService : IVideoService
    {
        public async Task<ActionResult<VideoInfo>> GetVideoAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<ActionResult<AnalysisResult>> ProcessVideoAsync(VideoProcessRequest request)
        {
            return new ActionResult<AnalysisResult>(new AnalysisResult
            {
                Success = false,
                Message = "This service is not implemented yet."
            });
        }
    }
}
