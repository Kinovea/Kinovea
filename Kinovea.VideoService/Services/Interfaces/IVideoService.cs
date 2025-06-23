using Kinovea.VideoService.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kinovea.VideoService.Services.Interfaces
{
    public interface IVideoService
    {
        /// <summary>
        /// 获取视频信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<ActionResult<VideoInfo>> GetVideoAsync(string id);

        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<ActionResult<AnalysisResult>> ProcessVideoAsync(VideoProcessRequest request);
    }
}
