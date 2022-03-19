#region License
/*
Copyright © Joan Charmant 2011.
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
using SystemBitmap = System.Drawing.Bitmap;

namespace Kinovea.Video.Bitmap
{
    /// <summary>
    /// A video reader that is capable of providing a frame for any arbitrary timestamp.
    /// This reader is actually a shell around a generator object that will provide the actual behavior.
    /// 
    /// Avoid using this to wrap regular video sources.
    /// Rule of thumb to decide between a generator or a full video reader:
    /// A generator must be able to create a frame for an arbitrary timestamp.
    /// If the underlying source is limited in time, it should be exposed through a VideoReader.
    /// Usage for generators is: random images, images with the timestamp painted on for tests, single image file.
    /// </summary>
    [SupportedExtensions(".jpg;.jpeg;.png;.bmp")]
    public class VideoReaderBitmap : VideoReader
    {
        #region Properties
        public override VideoFrame Current { 
            get { return current; }
        }
        public override VideoCapabilities Flags { 
            get { return VideoCapabilities.CanDecodeOnDemand | VideoCapabilities.CanChangeWorkingZone | VideoCapabilities.CanChangeImageRotation;}
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
        private bool initialized;
        private VideoFrame current = new VideoFrame();
        private VideoSection workingZone;
        private VideoInfo videoInfo = new VideoInfo();
        #endregion
        
        #region Public methods
        public override OpenVideoResult Open(string filePath)
        {
            OpenVideoResult res = InstanciateGenerator(filePath);
            if(res != OpenVideoResult.Success)
                return res;
            
            SetupVideoInfo(filePath);
            workingZone = new VideoSection(0, videoInfo.DurationTimeStamps);
            
            return res;
        }
        public override void Close()
        {
            generator.Close();
        }
        public override VideoSummary ExtractSummary(string filePath, int thumbs, Size maxSize)
        {
            OpenVideoResult res = Open(filePath);
            VideoSummary summary = new VideoSummary(filePath);

            if (res != OpenVideoResult.Success || generator == null)
                return summary;
            
            SystemBitmap bmp = generator.Generate(0);
            Size size = bmp.Size;
            summary.ImageSize = size;

            // TODO: compute the correct ratio stretched size. Currently this uses the maxSize.width
            // which is not the actual final width of the thumbnail if the ratio is more vertical than 4:3.
            int height = (int)(size.Height / ((float)size.Width / maxSize.Width));
            
            SystemBitmap thumb = new SystemBitmap(maxSize.Width, height);
            Graphics g = Graphics.FromImage(thumb);
            g.DrawImage(bmp, 0, 0, maxSize.Width, height);
            g.Dispose();
            Close();

            summary.Thumbs.Add(thumb);

            summary.IsImage = true;
            summary.DurationMilliseconds = 0;

            return summary;
        }
        public override void PostLoad(){}
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
        public override void BeforeFrameEnumeration(){}
        public override void AfterFrameEnumeration(){}

        public override bool ChangeImageRotation(ImageRotation rotation)
        {
            generator.SetRotation(rotation);
            UpdateSizeInfo();
            return false;
        }
        #endregion

        #region Private methods
        private OpenVideoResult InstanciateGenerator(string filePath)
        {
            OpenVideoResult res = OpenVideoResult.NotSupported;
            string extension = Path.GetExtension(filePath).ToLower();
            switch(extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                {
                    generator = new FrameGeneratorImageFile();
                    break;
                }
                default:
                    throw new NotImplementedException();
            }
            
            if(generator != null)
            {
                res = generator.Open(filePath);
                initialized = res == OpenVideoResult.Success;
            }
            return res;
        }
        private void SetupVideoInfo(string filePath)
        {
            videoInfo.FilePath = filePath;

            // 10 seconds @ 25fps.
            videoInfo.FirstTimeStamp = 0;
            videoInfo.AverageTimeStampsPerFrame = 1;
            videoInfo.FramesPerSeconds = 25;
            videoInfo.DurationTimeStamps = 251;
            videoInfo.FrameIntervalMilliseconds = 1000 / videoInfo.FramesPerSeconds;
            videoInfo.AverageTimeStampsPerSeconds = videoInfo.FramesPerSeconds * videoInfo.AverageTimeStampsPerFrame;

            UpdateSizeInfo();
        }

        private void UpdateSizeInfo()
        {
            videoInfo.OriginalSize = generator.OriginalSize;
            videoInfo.AspectRatioSize = generator.ReferenceSize;
            videoInfo.ReferenceSize = generator.ReferenceSize;
        }

        private bool UpdateCurrent(long timestamp)
        {
            // We can generate at any timestamp, but we still need to report when the
            // end of the working zone is reached. Otherwise frame enumerators like
            // in video save would just go on for ever.
            if(generator == null || !workingZone.Contains(timestamp))
                return false;
            
            if(current != null && current.Image != null)
                generator.DisposePrevious(current.Image);

            SystemBitmap bmp = generator.Generate(timestamp);
            current.Image = bmp;
            current.Timestamp = timestamp;
            return true;
        }
        #endregion
    }
}
