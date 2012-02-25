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
using System.IO;

namespace Kinovea.Video
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
    public class VideoReaderGenerator : VideoReader
    {
        #region Properties
        public override VideoFrame Current { 
            get { return m_Current; }
        }
	    public override VideoCapabilities Flags { 
            get { return VideoCapabilities.CanDecodeOnDemand | VideoCapabilities.CanChangeWorkingZone;}
        }
        public override VideoInfo Info { 
            get { return m_VideoInfo;} 
        }
	    public override bool Loaded { 
            get{ return m_Initialized; } 
        }
		public override VideoSection WorkingZone { 
            get { return m_WorkingZone; }
        }
        public override VideoDecodingMode DecodingMode { 
            get { return m_Initialized ? VideoDecodingMode.OnDemand : VideoDecodingMode.NotInitialized; }
		}
        #endregion
        
        #region Members
        private IFrameGenerator m_Generator;
        private bool m_Initialized;
        private VideoFrame m_Current = new VideoFrame();
        private VideoSection m_WorkingZone;
        private VideoInfo m_VideoInfo = new VideoInfo();
        #endregion
        
        #region Public methods
        public override OpenVideoResult Open(string _filePath)
        {
            InstanciateGenerator(_filePath);
            if(!m_Initialized)
                return OpenVideoResult.NotSupported;
            
            SetupVideoInfo(_filePath);
            m_WorkingZone = new VideoSection(0, m_VideoInfo.DurationTimeStamps - m_VideoInfo.AverageTimeStampsPerFrame);
            
            return OpenVideoResult.Success;
        }
        public override void Close()
        {
            m_Generator.Close();
        }
        public override VideoSummary ExtractSummary(string _filePath, int _thumbs, int _width)
        {
            Open(_filePath);
            
            if(m_Generator == null)
                return VideoSummary.GetInvalid(_filePath);
            
            Bitmap bmp = m_Generator.Generate(0);
            Size size = bmp.Size;
            
            int height = (int)(size.Height / ((float)size.Width / _width));
            
            Bitmap thumb = new Bitmap(_width, height);
            Graphics g = Graphics.FromImage(thumb);
            g.DrawImage(bmp, 0, 0, _width, height);
            g.Dispose();
            Close();
            
            bool hasKva = VideoSummary.HasCompanionKva(_filePath);

            return new VideoSummary(_filePath, true, hasKva, size, 0, new List<Bitmap>{ thumb });
        }
        public override void PostLoad(){}
        public override bool MoveNext(int _skip, bool _decodeIfNecessary)
        {
            return UpdateCurrent(Current.Timestamp + m_VideoInfo.AverageTimeStampsPerFrame);
        }
        public override bool MoveTo(long _timestamp)
        {
            return UpdateCurrent(_timestamp);
        }
        public override void UpdateWorkingZone(VideoSection _newZone, bool _forceReload, int _maxSeconds, int _maxMemory, Action<DoWorkEventHandler> _workerFn)
		{
            m_WorkingZone = _newZone;
		}
        public override void BeforeFrameEnumeration(){}
        public override void AfterFrameEnumeration(){}
        #endregion
        
        #region Private methods
        private void InstanciateGenerator(string _filePath)
        {
            string extension = Path.GetExtension(_filePath).ToLower();
            switch(extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                {
                    m_Generator = new FrameGeneratorImageFile();
                    break;
                }
                default:
                    throw new NotImplementedException();
            }
            
            if(m_Generator != null)
            {
                m_Generator.Initialize(_filePath);
                m_Initialized = true;
            }
        }
        private void SetupVideoInfo(string _filePath)
        {
            m_VideoInfo.AverageTimeStampsPerFrame = 1;
            m_VideoInfo.FilePath = _filePath;
            m_VideoInfo.FirstTimeStamp = 0;
            
            // Testing: 10 seconds @ 25fps.
            m_VideoInfo.DurationTimeStamps = 251;
            m_VideoInfo.FramesPerSeconds = 25;
            m_VideoInfo.FrameIntervalMilliseconds = 1000 / m_VideoInfo.FramesPerSeconds;
            m_VideoInfo.AverageTimeStampsPerSeconds = m_VideoInfo.FramesPerSeconds * m_VideoInfo.AverageTimeStampsPerFrame;
            
            // Testing 640x480.
            if(m_Generator.Size != Size.Empty)
                m_VideoInfo.OriginalSize = m_Generator.Size;
            else
                m_VideoInfo.OriginalSize = new Size(640, 480);
            m_VideoInfo.AspectRatioSize = m_VideoInfo.OriginalSize;
            
        }
        private bool UpdateCurrent(long _timestamp)
        {
            // We can generate at any timestamp, but we still need to report when the
            // end of the working zone is reached. Otherwise frame enumerators like
            // in video save would just go on for ever.
            if(m_Generator == null || !m_WorkingZone.Contains(_timestamp))
                return false;
            
            if(m_Current != null && m_Current.Image != null)
                m_Generator.DisposePrevious(m_Current.Image);

            Bitmap bmp = m_Generator.Generate(_timestamp);
            m_Current.Image = bmp;
            m_Current.Timestamp = _timestamp;
            return true;
        }
        #endregion
    }
}
