using Kinovea.Services;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;
using Kinovea.VideoService.Models;
using System.ComponentModel;
using System.Drawing;
using Kinovea.ScreenManager;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频读取器接口
    /// </summary>
    public interface IVideoReader : IDisposable
    {
        /// <summary>
        /// 视频读取器接口
        /// </summary>
        public interface IVideoReader : IDisposable
        {

            #region 视频读取功能

            /// <summary>
            /// 打开视频文件
            /// </summary>
            /// <param name="filePath">视频文件路径</param>
            /// <returns>是否成功打开</returns>
            OpenVideoResult Open(string filePath);

            /// <summary>
            /// 关闭视频
            /// </summary>
            void CloseVideo(VideoReader reader);
            /// <summary>
            /// 提取视频摘要：获取视频的基本信息和缩略图。
            /// </summary>
            /// <param name="filePath">视频文件路径</param>
            /// <param name="thumbsToLoad">需要加载的缩略图数量</param>
            /// <param name="maxImageSize">缩略图的最大尺寸</param>
            /// <returns>视频摘要对象</returns>

            VideoSummary ExtractVideoSummary(string filePath, int thumbsToLoad, Size maxImageSize);

            #endregion

            #region 视频播放功能
            /// <summary>
            /// 按顺序播放视频的每一帧 , 移动到指定帧
            /// </summary>
            /// <param name="reader">视频</param>
            /// <param name="skip">跳过的帧数</param>
            /// <param name="decodeIfNecessary">是否解码</param>
            /// <returns>是否成功移动</returns>
            bool MoveToNextFrame(VideoReader reader, int skip, bool decodeIfNecessary);

            /// <summary>
            /// 跳转到指定帧：直接定位到视频的指定帧。
            /// </summary>
            /// <param name="reader">视频读取器</param>
            /// <param name="from">起始帧</param>
            /// <param name="target">目标帧</param>
            /// <returns>是否成功移动</returns>
            bool MoveToSpecificFrame(VideoReader reader, long from, long target);

            #endregion


            #region 视频导出功能

            /// <summary>
            /// 导出视频：将处理后的视频保存为指定格式的文件
            /// </summary>
            /// <param name="format"></param>
            /// <param name="player1"></param>
            /// <param name="player2"></param>
            /// <param name="dualPlayer"></param>
            void ExportVideo(VideoExportFormat format, PlayerScreen player1, PlayerScreen player2, DualPlayerController dualPlayer);
            #endregion

            #region 视频处理选项设置功能
            /// <summary>
            /// 设置视频处理选项：如设置图像宽高比、旋转、去马赛克、去隔行等。
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="options"></param>
            void SetVideoOptions(VideoReader reader, VideoOptions options);
            #endregion

            #region 视频筛选器管理功能
            /// <summary>
            /// 激活视频筛选器：为视频应用特定的筛选器。
            /// </summary>
            /// <param name="player"></param>
            /// <param name="type"></param>
            void ActivateVideoFilter(PlayerScreen player, VideoFilterType type);

            /// <summary>
            /// 停用视频筛选器：移除当前应用的视频筛选器。
            /// </summary>
            /// <param name="player"></param>
            void DeactivateVideoFilter(PlayerScreen player);
            #endregion

            #region 视频关键帧管理功能
            /// <summary>
            /// 添加关键帧：在视频中标记关键帧。
            /// </summary>
            /// <param name="player"></param>
            void AddKeyframe(PlayerScreen player);

            /// <summary>
            /// 跳转到上一个关键帧：快速定位到上一个关键帧。
            /// </summary>
            /// <param name="player"></param>
            void GotoPreviousKeyframe(PlayerScreen player);

            /// <summary>
            /// 跳转到下一个关键帧：快速定位到下一个关键帧。
            /// </summary>
            /// <param name="player"></param>
            void GotoNextKeyframe(PlayerScreen player);

            #endregion

            #region 视频元数据管理功能
            /// <summary>
            /// 读取视频元数据：获取视频的相关元数据信息。
            /// </summary>
            /// <param name="reader"></param>
            /// <returns></returns>
            string ReadVideoMetadata(VideoReaderFFMpeg reader);
            #endregion

            #region 视频工作区管理功能
            /// <summary>
            /// 更新视频工作区：设置或更新视频的工作区区域。
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="newZone"></param>
            /// <param name="forceReload"></param>
            /// <param name="maxMemory"></param>
            /// <param name="workerFn"></param>
            void UpdateVideoWorkingZone(VideoReader reader, VideoSection newZone, bool forceReload, int maxMemory, Action<DoWorkEventHandler> workerFn);
            #endregion
        }
    }
}