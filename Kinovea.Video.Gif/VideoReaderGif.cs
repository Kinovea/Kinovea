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

namespace Kinovea.Video.GIF
{
    // A video reader for animated GIFs.
    [SupportedExtensions(".gif")]
    public class VideoReaderGIF : VideoReaderAlwaysCaching
    {
        #region Properties
        public override VideoCapabilities Flags {
            get { return VideoCapabilities.CanCache; }
        }
        public override bool Loaded {
            get { return loaded; }
        }
        public override VideoInfo Info {
            get { return videoInfo; }
        }
        public override VideoDecodingMode DecodingMode { 
            get { return loaded ? VideoDecodingMode.Caching : VideoDecodingMode.NotInitialized; }
        }
        #endregion
        
        #region Members
        private bool loaded;
        private VideoInfo videoInfo;
        private int count;
        private Image gif;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Public Methods
        public override OpenVideoResult Open(string filePath)
        {
            if(loaded)
                Close();
                
            videoInfo.FirstTimeStamp = 0;
            videoInfo.AverageTimeStampsPerSeconds = 100;
            videoInfo.FilePath = filePath;
            
            // This reader can only function in frozen cache mode,
            // so we systematically load the cache during opening.
            OpenVideoResult res = LoadFile(filePath, true);
            gif.Dispose();
            DumpInfo();
            return res;
        }
        public override VideoSummary ExtractSummary(string filePath, int thumbsToGet, Size maxSize)
        {
            VideoSummary summary = new VideoSummary(filePath);
           
            OpenVideoResult res = LoadFile(filePath, false);
            FrameDimension dimension = new FrameDimension(gif.FrameDimensionsList[0]);
            
            if(res == OpenVideoResult.Success)
            {
                summary.IsImage = count == 1;
                summary.DurationMilliseconds = (int)((double)count * videoInfo.FrameIntervalMilliseconds);
                summary.Framerate = videoInfo.FramesPerSeconds;

                if (thumbsToGet > 0)
                {
                    int step = (int)Math.Ceiling(count / (double)thumbsToGet);
                    for (int i = 0; i < count; i += step)
                        summary.Thumbs.Add(GetFrameAt(dimension, i));
                }
                
                summary.ImageSize = videoInfo.OriginalSize;
            }
            
            if(loaded)
                Close();
            
            gif.Dispose();
            return summary;
        }
        #endregion
        
        #region Private Methods
        private OpenVideoResult LoadFile(string filePath, bool cache)
        {
            OpenVideoResult result = OpenVideoResult.UnknownError;
            
            do
            {
                if(gif != null)
                    gif.Dispose();
                
                gif = Image.FromFile(filePath);
                if(gif == null)
                {
                    result = OpenVideoResult.FileNotOpenned;
                    log.ErrorFormat("The file could not be openned.");
                    break;
                }
                
                // Get duration in frames. 
                // .NET bitmaps actually have several lists of multiple images inside a single Bitmap.
                // For example, to hold the sequence of frames, the layers, or the different resolutions for icons.
                // FrameDimension is used to access the list of frames.
                FrameDimension dimension = new FrameDimension(gif.FrameDimensionsList[0]);
                count = gif.GetFrameCount(dimension);
                    
                // Duration of first interval. (PropertyTagFrameDelay)
                // The byte array returned by the Value property contains 32bits integers for each frame interval (in 1/100th).
                PropertyItem pi = gif.GetPropertyItem(0x5100);
                int interval = BitConverter.ToInt32(pi.Value, 0);
                if(interval <= 0)
                    interval = 5;
                videoInfo.DurationTimeStamps = count * interval;
                videoInfo.FrameIntervalMilliseconds = interval * 10;
                
                videoInfo.FramesPerSeconds = 100D/interval;
                videoInfo.AverageTimeStampsPerFrame = interval;

                videoInfo.AspectRatioSize = gif.Size;
                videoInfo.OriginalSize = gif.Size;
                
                if(cache)
                    LoadCache(dimension);
                
                loaded = true;
                result = OpenVideoResult.Success;
            }
            while(false);

            return result;
        }
        private void LoadCache(FrameDimension dimension)
        {
            Cache.Clear();
            for(int i = 0; i<count; i++)
            {
                VideoFrame vf = new VideoFrame();
                vf.Timestamp = i * videoInfo.AverageTimeStampsPerFrame;
                vf.Image = GetFrameAt(dimension, i);
                Cache.Add(vf);
            }
        }
        private Bitmap GetFrameAt(FrameDimension dimension, int target)
        {
            gif.SelectActiveFrame(dimension, target);
            Bitmap bmp = new Bitmap(videoInfo.AspectRatioSize.Width, videoInfo.AspectRatioSize.Height, PixelFormat.Format32bppPArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(gif, 0, 0, videoInfo.AspectRatioSize.Width, videoInfo.AspectRatioSize.Height);
            return bmp;
        }
        private void DumpInfo()
        {
            log.Debug("---------------------------------------------------");
            log.DebugFormat("[File] - Filename : {0}", Path.GetFileName(videoInfo.FilePath));
            log.DebugFormat("[GIF] - First interval (ms): {0}", videoInfo.FrameIntervalMilliseconds);
            log.DebugFormat("[GIF] - Duration (frames): {0}", count);
            log.DebugFormat("[GIF] - Duration (ts): {0}", videoInfo.DurationTimeStamps);
            log.DebugFormat("[GIF] - Duration (s): {0}", (double)videoInfo.DurationTimeStamps/(double)videoInfo.AverageTimeStampsPerSeconds);
            log.DebugFormat("[GIF] - Computed fps: {0}", videoInfo.FramesPerSeconds);
            log.DebugFormat("[GIF] - Size (pixels): {0}", videoInfo.AspectRatioSize);
            log.Debug("---------------------------------------------------");
        }
        #endregion
    }
}
