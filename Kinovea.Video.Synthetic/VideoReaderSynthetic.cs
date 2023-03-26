#region License
/*
Copyright © Joan Charmant 2014.
jcharmant@gmail.com 
 
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
using System.IO;
using Kinovea.Services;

namespace Kinovea.Video.Synthetic
{
    /// <summary>
    /// KSV files are simple descriptions of a video. 
    /// The format is mostly used for testing purposes to quickly generate videos of various size/framerate/duration.
    /// </summary>
    [SupportedExtensions(".ksv")]
    public class VideoReaderSynthetic : VideoReader
    {
        #region Properties
        public override VideoFrame Current 
        { 
            get { return current; }
        }
        public override VideoCapabilities Flags { 
            get { return VideoCapabilities.CanDecodeOnDemand | VideoCapabilities.CanChangeWorkingZone;}
        }
        public override VideoInfo Info { 
            get { return videoInfo;} 
        }
        public override bool Loaded { 
            get{ return initialized; } 
        }
        public override VideoSection WorkingZone { 
            get { return workingZone; }
        }
        public override VideoDecodingMode DecodingMode { 
            get { return initialized ? VideoDecodingMode.OnDemand : VideoDecodingMode.NotInitialized; }
        }
        #endregion
        
        #region Members
        private IFrameGenerator generator;
        private string filePath;
        private SyntheticVideo video;
        private bool initialized;
        private VideoFrame current = new VideoFrame();
        private VideoSection workingZone;
        private VideoInfo videoInfo = new VideoInfo();
        private bool firstDecoded;
        #endregion
        
        #region Public methods
        public override OpenVideoResult Open(string filePath)
        {
            this.filePath = filePath;
            this.video = SyntheticVideoSerializer.Deserialize(filePath);

            if (video == null)
                return OpenVideoResult.NotSupported;

            OpenVideoResult res = InstanciateGenerator(video);
            if(res != OpenVideoResult.Success)
                return res;
            
            SetupVideoInfo();
            workingZone = new VideoSection(0, videoInfo.DurationTimeStamps - videoInfo.AverageTimeStampsPerFrame);
            
            return res;
        }
        public override void Close()
        {
            generator.Close();
        }
        public override VideoSummary ExtractSummary(string filePath, int thumbs, Size maxSize)
        {
            VideoSummary summary = new VideoSummary(filePath);
            OpenVideoResult res = Open(filePath);
            
            if(res != OpenVideoResult.Success || generator == null)
                return summary;
            
            Bitmap bmp = generator.Generate(0, maxSize);
            Size size = bmp.Size;
            summary.ImageSize = size;

            int height = (int)(size.Height / ((float)size.Width / maxSize.Width));
            Bitmap thumb = new Bitmap(maxSize.Width, height);
            Graphics g = Graphics.FromImage(thumb);
            g.DrawImage(bmp, 0, 0, maxSize.Width, height);
            g.Dispose();
            Close();

            summary.Thumbs.Add(thumb);
            summary.DurationMilliseconds = (int)((videoInfo.DurationTimeStamps - videoInfo.AverageTimeStampsPerFrame) * videoInfo.FrameIntervalMilliseconds);
            summary.Framerate = videoInfo.FramesPerSeconds;
            summary.IsImage = false;
            
            return summary;
        }
        public override void PostLoad()
        {
        }
        public override bool MoveNext(int skip, bool decodeIfNecessary)
        {
            if (!firstDecoded)
            {
                firstDecoded = true;
                return UpdateCurrent(0);
            }
            else
            {
                long offset = (skip + 1) * videoInfo.AverageTimeStampsPerFrame;
                return UpdateCurrent(current.Timestamp + offset);
            }
        }
        public override bool MoveTo(long from, long target)
        {
            return UpdateCurrent(target);
        }
        public override void UpdateWorkingZone(VideoSection newZone, bool forceReload, int maxMemory, Action<DoWorkEventHandler> workerFn)
        {
            workingZone = newZone;
        }
        public override void BeforeFrameEnumeration()
        {
        }
        public override void AfterFrameEnumeration()
        {
        }
        #endregion
        
        #region Private methods
        private OpenVideoResult InstanciateGenerator(SyntheticVideo video)
        {
            OpenVideoResult res = OpenVideoResult.NotSupported;
            generator = new FrameGeneratorSyntheticVideo(video);
            res = generator.Open(null);
            initialized = res == OpenVideoResult.Success;
            return res;
        }
        private void SetupVideoInfo()
        {
            videoInfo.AverageTimeStampsPerFrame = 1;
            videoInfo.FilePath = filePath;
            videoInfo.FirstTimeStamp = 0;
            
            videoInfo.DurationTimeStamps = video.DurationFrames;
            videoInfo.FramesPerSeconds = video.FramePerSecond;
            videoInfo.FrameIntervalMilliseconds = 1000 / videoInfo.FramesPerSeconds;
            videoInfo.AverageTimeStampsPerSeconds = videoInfo.FramesPerSeconds * videoInfo.AverageTimeStampsPerFrame;
            
            videoInfo.OriginalSize = video.ImageSize;
            videoInfo.AspectRatioSize = videoInfo.OriginalSize;
            videoInfo.ReferenceSize = videoInfo.OriginalSize;
        }
        private bool UpdateCurrent(long timestamp)
        {
            if(generator == null || !workingZone.Contains(timestamp))
                return false;
            
            if(current != null && current.Image != null)
                generator.DisposePrevious(current.Image);

            Bitmap bmp = generator.Generate(timestamp);
            current.Image = bmp;
            current.Timestamp = timestamp;
            return true;
        }
        #endregion
    }
}
