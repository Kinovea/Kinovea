#region License
/*
Copyright © Joan Charmant 2011.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Kinovea.Video.Gif
{
    // A video reader for animated GIFs.
    [SupportedExtensions(".gif")]
    public class VideoReaderGif : VideoReaderAlwaysCaching
    {
        #region Properties
        public override VideoCapabilities Flags {
            get { return VideoCapabilities.CanCache; }
        }
        public override bool Loaded {
            get { return m_Loaded; }
        }
        public override VideoInfo Info {
            get { return m_VideoInfo; }
        }
        public override VideoDecodingMode DecodingMode { 
            get { return m_Loaded ? VideoDecodingMode.Caching : VideoDecodingMode.NotInitialized; }
        }
        #endregion
        
        #region Members
        private bool m_Loaded;
        private VideoInfo m_VideoInfo;
        private int m_Count;
        private Image m_Gif;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Public Methods
        public override OpenVideoResult Open(string _filePath)
        {
            if(m_Loaded)
                Close();
                
            m_VideoInfo.FirstTimeStamp = 0;
            m_VideoInfo.AverageTimeStampsPerSeconds = 100;
            m_VideoInfo.FilePath = _filePath;
            
            // This reader can only function in frozen cache mode,
            // so we systematically load the cache during opening.
            OpenVideoResult res = LoadFile(_filePath, true);
            m_Gif.Dispose();
            DumpInfo();
            return res;
        }
        public override VideoSummary ExtractSummary(string _filePath, int _thumbs, int _width)
        {
           VideoSummary summary = null;
           
            OpenVideoResult res = LoadFile(_filePath, false);
            FrameDimension dimension = new FrameDimension(m_Gif.FrameDimensionsList[0]);
            
            if(res == OpenVideoResult.Success)
            {
                bool hasKva = VideoSummary.HasCompanionKva(_filePath);
                bool isImage = m_Count == 1;
                int durationMillisecs = (int)((double)m_Count * m_VideoInfo.FrameIntervalMilliseconds);
                
                List<Bitmap> thumbs = new List<Bitmap>();
                if(_thumbs > 0)
                {
                    int step = (int)Math.Ceiling(m_Count / (double)_thumbs);
                    for(int i = 0; i<m_Count; i+=step)
                        thumbs.Add(GetFrameAt(dimension, i));
                }
                
                summary = new VideoSummary(_filePath, isImage, hasKva, m_VideoInfo.OriginalSize, durationMillisecs, thumbs);
            }
            else
            {
                summary = VideoSummary.GetInvalid(_filePath);
            }
            
            if(m_Loaded)
                Close();
            
            m_Gif.Dispose();
            return summary;
        }
        #endregion
        
        #region Private Methods
        private OpenVideoResult LoadFile(string _filePath, bool _cache)
        {
            OpenVideoResult result = OpenVideoResult.UnknownError;
            
            do
            {
                if(m_Gif != null)
                    m_Gif.Dispose();
                
                m_Gif = Image.FromFile(_filePath);
                if(m_Gif == null)
                {
                    result = OpenVideoResult.FileNotOpenned;
                    log.ErrorFormat("The file could not be openned.");
                    break;
                }
                
                // Get duration in frames. 
                // .NET bitmaps actually have several lists of multiple images inside a single Bitmap.
                // For example, to hold the sequence of frames, the layers, or the different resolutions for icons.
                // FrameDimension is used to access the list of frames.
                FrameDimension dimension = new FrameDimension(m_Gif.FrameDimensionsList[0]);
                m_Count = m_Gif.GetFrameCount(dimension);
                    
                // Duration of first interval. (PropertyTagFrameDelay)
                // The byte array returned by the Value property contains 32bits integers for each frame interval (in 1/100th).
                PropertyItem pi = m_Gif.GetPropertyItem(0x5100);
                int interval = BitConverter.ToInt32(pi.Value, 0);
                if(interval <= 0)
                    interval = 5;
                m_VideoInfo.DurationTimeStamps = m_Count * interval;
                m_VideoInfo.FrameIntervalMilliseconds = interval * 10;
                
                m_VideoInfo.FramesPerSeconds = 100D/interval;
                m_VideoInfo.AverageTimeStampsPerFrame = interval;

                m_VideoInfo.AspectRatioSize = m_Gif.Size;
                m_VideoInfo.OriginalSize = m_Gif.Size;
                
                if(_cache)
                    LoadCache(dimension);
                
                m_Loaded = true;
                result = OpenVideoResult.Success;
            }
            while(false);

            return result;
        }
        private void LoadCache(FrameDimension _dimension)
        {
            Cache.Clear();
            for(int i = 0; i<m_Count; i++)
            {
                VideoFrame vf = new VideoFrame();
                vf.Timestamp = i * m_VideoInfo.AverageTimeStampsPerFrame;
                vf.Image = GetFrameAt(_dimension, i);
                Cache.Add(vf);
            }
        }
        private Bitmap GetFrameAt(FrameDimension _dimension, int _target)
        {
            m_Gif.SelectActiveFrame(_dimension, _target);
            Bitmap bmp = new Bitmap(m_VideoInfo.AspectRatioSize.Width, m_VideoInfo.AspectRatioSize.Height, PixelFormat.Format32bppPArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(m_Gif, 0, 0, m_VideoInfo.AspectRatioSize.Width, m_VideoInfo.AspectRatioSize.Height);
            return bmp;
        }
        private void DumpInfo()
        {
            log.Debug("---------------------------------------------------");
            log.DebugFormat("[File] - Filename : {0}", Path.GetFileName(m_VideoInfo.FilePath));
            log.DebugFormat("[GIF] - First interval (ms): {0}", m_VideoInfo.FrameIntervalMilliseconds);
            log.DebugFormat("[GIF] - Duration (frames): {0}", m_Count);
            log.DebugFormat("[GIF] - Duration (ts): {0}", m_VideoInfo.DurationTimeStamps);
            log.DebugFormat("[GIF] - Duration (s): {0}", (double)m_VideoInfo.DurationTimeStamps/(double)m_VideoInfo.AverageTimeStampsPerSeconds);
            log.DebugFormat("[GIF] - Computed fps: {0}", m_VideoInfo.FramesPerSeconds);
            log.DebugFormat("[GIF] - Size (pixels): {0}", m_VideoInfo.AspectRatioSize);
            log.Debug("---------------------------------------------------");
        }
        #endregion
    }
}
