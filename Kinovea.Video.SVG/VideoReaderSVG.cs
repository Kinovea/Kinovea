#region License
/*
Copyright © Joan Charmant 2013.
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

namespace Kinovea.Video.SVG
{
    /// <summary>
    /// A video reader to convert an SVG file into a video.
    /// This reader is actually a shell around a generator object that will provide the actual behavior.
    /// </summary>
    [SupportedExtensions(".svg;")]
    public class VideoReaderSVG : VideoReader
    {
        #region Properties
        public override VideoFrame Current
        {
            get { return current; }
        }
        public override VideoCapabilities Flags
        {
            get { return VideoCapabilities.CanDecodeOnDemand | VideoCapabilities.CanChangeWorkingZone | 
                VideoCapabilities.CanChangeDecodingSize | VideoCapabilities.CanScaleIndefinitely; }
        }
        public override bool CanDrawUnscaled
        {
            get { return true; }
        }
        public override VideoInfo Info
        {
            get { return videoInfo; }
        }
        public override bool Loaded
        {
            get { return initialized; }
        }
        public override VideoSection WorkingZone
        {
            get { return workingZone; }
        }
        public override VideoDecodingMode DecodingMode
        {
            get { return initialized ? VideoDecodingMode.OnDemand : VideoDecodingMode.NotInitialized; }
        }
        #endregion

        #region Members
        private IFrameGenerator generator;
        private bool initialized;
        private VideoFrame current = new VideoFrame();
        private VideoSection workingZone;
        private VideoInfo videoInfo = new VideoInfo();
        private Size decodingSize = new Size(640, 480);
        #endregion

        #region Public methods
        public override OpenVideoResult Open(string filePath)
        {
            OpenVideoResult res = InstanciateGenerator(filePath);
            if (res != OpenVideoResult.Success)
                return res;

            SetupVideoInfo(filePath);
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

            if (res != OpenVideoResult.Success || generator == null)
                return summary;

            Bitmap bmp = generator.Generate(0, maxSize);
            Bitmap thumb = new Bitmap(bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(thumb);
            g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
            g.Dispose();
            Close();

            summary.Thumbs.Add(thumb);
            summary.IsImage = true;
            summary.ImageSize = Size.Empty;
            summary.DurationMilliseconds = 0;

            return summary;
        }
        public override void PostLoad() { }
        public override bool MoveNext(int skip, bool decodeIfNecessary)
        {
            return UpdateCurrent(Current.Timestamp + videoInfo.AverageTimeStampsPerFrame);
        }
        public override bool MoveTo(long timestamp)
        {
            return UpdateCurrent(timestamp);
        }
        
        public override void UpdateWorkingZone(VideoSection newZone, bool forceReload, int maxMemory, Action<DoWorkEventHandler> workerFn)
        {
            workingZone = newZone;
        }

        public override void BeforeFrameEnumeration() { }
        public override void AfterFrameEnumeration() { }

        public override bool ChangeDecodingSize(Size size)
        {
            decodingSize = size;
            return true;
        }
        #endregion

        #region Private methods
        private OpenVideoResult InstanciateGenerator(string filePath)
        {
            OpenVideoResult res = OpenVideoResult.NotSupported;
            string extension = Path.GetExtension(filePath).ToLower();
            if (extension != ".svg")
                throw new NotImplementedException();

            generator = new FrameGeneratorSVG();
            res = generator.Open(filePath);
            initialized = res == OpenVideoResult.Success;
            return res;
        }

        private void SetupVideoInfo(string filePath)
        {
            videoInfo.AverageTimeStampsPerFrame = 1;
            videoInfo.FilePath = filePath;
            videoInfo.FirstTimeStamp = 0;

            // Testing: 10 seconds @ 25fps @ 640x480.
            videoInfo.DurationTimeStamps = 251;
            videoInfo.FramesPerSeconds = 25;
            videoInfo.FrameIntervalMilliseconds = 1000 / videoInfo.FramesPerSeconds;
            videoInfo.AverageTimeStampsPerSeconds = videoInfo.FramesPerSeconds * videoInfo.AverageTimeStampsPerFrame;

            videoInfo.OriginalSize = generator.OriginalSize != Size.Empty ? generator.OriginalSize : new Size(640, 480);
            videoInfo.AspectRatioSize = videoInfo.OriginalSize;
            videoInfo.ReferenceSize = videoInfo.OriginalSize;
            
            decodingSize = videoInfo.ReferenceSize;
        }

        private bool UpdateCurrent(long timestamp)
        {
            // We can generate at any timestamp, but we still need to report when the
            // end of the working zone is reached. Otherwise frame enumerators like
            // in video save would just go on for ever.
            if (generator == null || !workingZone.Contains(timestamp))
                return false;

            if (current != null && current.Image != null)
                generator.DisposePrevious(current.Image);

            Bitmap bmp = generator.Generate(timestamp, decodingSize);
            current.Image = bmp;
            current.Timestamp = timestamp;
            return true;
        }
        #endregion
    }
}

