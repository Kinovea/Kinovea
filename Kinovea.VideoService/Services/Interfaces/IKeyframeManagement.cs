using Kinovea.ScreenManager;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频关键帧管理接口
    /// </summary>
    public interface IKeyframeManagement
    {
        /// <summary>
        /// 添加关键帧：在视频中标记关键帧。
        /// </summary>
        /// <param name="player">播放器</param>
        void AddKeyframe(PlayerScreen player);

        /// <summary>
        /// 跳转到上一个关键帧：快速定位到上一个关键帧。
        /// </summary>
        /// <param name="player">播放器</param>
        void GotoPreviousKeyframe(PlayerScreen player);

        /// <summary>
        /// 跳转到下一个关键帧：快速定位到下一个关键帧。
        /// </summary>
        /// <param name="player">播放器</param>
        void GotoNextKeyframe(PlayerScreen player);
    }
}
