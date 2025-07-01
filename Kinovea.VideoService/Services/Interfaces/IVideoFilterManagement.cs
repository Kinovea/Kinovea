using Kinovea.ScreenManager;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频筛选器管理接口
    /// </summary>
    public interface IVideoFilterManagement
    {
        /// <summary>
        /// 激活视频筛选器：为视频应用特定的筛选器。
        /// </summary>
        /// <param name="player">播放器</param>
        /// <param name="type">筛选器类型</param>
        void ActivateVideoFilter(PlayerScreen player, VideoFilterType type);

        /// <summary>
        /// 停用视频筛选器：移除当前应用的视频筛选器。
        /// </summary>
        /// <param name="player">播放器</param>
        void DeactivateVideoFilter(PlayerScreen player);
    }
}
