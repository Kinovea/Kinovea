using Kinovea.ScreenManager;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频导出接口
    /// </summary>
    public interface IVideoExport
    {
        /// <summary>
        /// 导出视频：将处理后的视频保存为指定格式的文件
        /// </summary>
        /// <param name="format">导出格式</param>
        /// <param name="player1">第一个播放器</param>
        /// <param name="player2">第二个播放器</param>
        /// <param name="dualPlayer">双播放器控制器</param>
        void ExportVideo(VideoExportFormat format, PlayerScreen player1, PlayerScreen player2, DualPlayerController dualPlayer);
    }

}
