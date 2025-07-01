using Kinovea.Services;
using Kinovea.Video;
using System.ComponentModel;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频工作区管理接口
    /// </summary>
    public interface IVideoWorkingZoneManagement
    {
        /// <summary>
        /// 更新视频工作区：设置或更新视频的工作区区域。
        /// </summary>
        /// <param name="reader">视频读取器</param>
        /// <param name="newZone">新工作区</param>
        /// <param name="forceReload">是否强制重新加载</param>
        /// <param name="maxMemory">最大内存限制</param>
        /// <param name="workerFn">工作函数</param>
        void UpdateVideoWorkingZone(VideoReader reader, VideoSection newZone, bool forceReload, int maxMemory, Action<DoWorkEventHandler> workerFn);
    }
}
