using Kinovea.Video;
using Kinovea.Video.FFMpeg;
using Kinovea.VideoService.Services.Interfaces;
using System;
using System.Drawing;

namespace Kinovea.VideoService.Services.Implementations
{
    /// <summary>
    /// 基本视频读取器只需实现文件操作和播放功能
    /// </summary>
    public class BasicVideoReader : IVideoFileOperations, IVideoPlayback, IDisposable
    {
        private VideoReaderFFMpeg _reader;

        /// <summary>
        /// 打开视频文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public OpenVideoResult Open(string filePath)
        {
            DisposeReader();
            _reader = new VideoReaderFFMpeg();
            return _reader.Open(filePath);
        }

        /// <summary>
        /// 提取视频摘要：获取视频的基本信息和缩略图。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="thumbsToLoad"></param>
        /// <param name="maxImageSize"></param>
        /// <returns></returns>
        public VideoSummary ExtractVideoSummary(string filePath, int thumbsToLoad, Size maxImageSize)
        {
            using var reader = new VideoReaderFFMpeg();
            return reader.ExtractSummary(filePath, thumbsToLoad, maxImageSize);
        }

        public bool MoveToNextFrame(VideoReader reader, int skip, bool decodeIfNecessary)
        {
            return reader?.MoveNext(skip, decodeIfNecessary) ?? false;
        }

        public bool MoveToSpecificFrame(VideoReader reader, long from, long target)
        {
            return reader?.MoveTo(from, target) ?? false;
        }

        public void CloseVideo(VideoReader reader)
        {
            reader?.Close();
        }

        public void Dispose()
        {
            DisposeReader();
        }

        private void DisposeReader()
        {
            if (_reader != null)
            {
                _reader.Close();
                (_reader as IDisposable)?.Dispose();
                _reader = null;
            }
        }
    }
}
