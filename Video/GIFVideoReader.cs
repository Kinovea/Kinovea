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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Kinovea.Video.Gif
{
    [SupportedExtensions(new string[] {".gif"})]
    public class GIFVideoReader : VideoReader
    {
        #region Properties
        public override VideoReaderFlags Flags {
            get { return VideoReaderFlags.AlwaysCaching; }
        }
        public override bool Loaded {
            get { return m_Loaded; }
        }
        public override VideoInfo Info {
            get { return m_VideoInfo; }
        }
        public override VideoSection WorkingZone {
            get { return Cache.Segment; }
            set {}
        }
        public override bool Caching {
            get { return true; }
        }
        /*public override VideoFrameCache Cache {
            get { return m_FrameCache; }
        }*/
        #endregion
        
        #region Members
        private bool m_Loaded;
        private VideoInfo m_VideoInfo;
        private int m_Count;
        //private VideoFrameCache m_FrameCache = new VideoFrameCache();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public GIFVideoReader()
        {
            Cache = new VideoFrameCache();
        }
        #endregion
        
        #region Public Methods
        public override OpenVideoResult Open(string _FilePath)
        {
            OpenVideoResult result = OpenVideoResult.UnknownError;
            
            if(m_Loaded) 
                Close();
                
            do 
            {
                Image gif = Image.FromFile(_FilePath);
                if(gif == null)
                {
                    result = OpenVideoResult.FileNotOpenned;
                    log.ErrorFormat("The file could not be openned.");
                    break;
                }
                
                m_VideoInfo.FirstTimeStamp = 0;
                m_VideoInfo.AverageTimeStampsPerSeconds = 100;
                m_VideoInfo.FilePath = _FilePath;
                
                // Duration in frames
        		FrameDimension fd = new FrameDimension(gif.FrameDimensionsList[0]);
        		m_Count = gif.GetFrameCount(fd);
        		    
                // Duration of first interval. (PropertyTagFrameDelay)
                // The byte array returned by the Value property contains 32bits integers for each frame interval (in 1/100th).
                PropertyItem pi = gif.GetPropertyItem(0x5100);
		        int interval = BitConverter.ToInt32(pi.Value, 0);
		        if(interval <= 0)
		            interval = 5;
                m_VideoInfo.DurationTimeStamps = m_Count * interval;
		        m_VideoInfo.FrameIntervalMilliseconds = interval * 10;
        		
                m_VideoInfo.FramesPerSeconds = 100D/interval;
        		m_VideoInfo.AverageTimeStampsPerFrame = interval;

                m_VideoInfo.DecodingSize = gif.Size;
                m_VideoInfo.OriginalSize = gif.Size;
        		
                // Immediately feed the cache.
                LoadCache(gif);
                gif.Dispose();
                
        		DumpInfo();
        		m_Loaded = true;
                result = OpenVideoResult.Success;
            }
            while(false);

            return result;
        }
        public override void Close()
        {
            // Nothing to do.
        }
        public override bool MoveNext()
        {
            return Cache.MoveNext();
        }
        public override bool MoveTo(long _timestamp)
        {
            return Cache.MoveTo(_timestamp);
        }
        /*public override bool Cache(long _start, long _end, int _maxSeconds, int _maxMemory)
        {
            // Nothing more to do as this reader is cache only.
            return true;
        }*/
        #endregion
        
        #region Private Methods
        private void LoadCache(Image _gif)
        {
            Cache.Clear();
            Cache.FullZone = true;
            FrameDimension fd = new FrameDimension(_gif.FrameDimensionsList[0]);
            for(int i = 0; i<m_Count; i++)
            {
                _gif.SelectActiveFrame(fd, i);

                VideoFrame vf = new VideoFrame();
                vf.Timestamp = i * m_VideoInfo.AverageTimeStampsPerFrame;
                vf.Image = new Bitmap(m_VideoInfo.DecodingSize.Width, m_VideoInfo.DecodingSize.Height, PixelFormat.Format32bppPArgb);
		        Graphics g = Graphics.FromImage(vf.Image);
                g.DrawImage(_gif, 0, 0, m_VideoInfo.DecodingSize.Width, m_VideoInfo.DecodingSize.Height); 
		    
                Cache.Add(vf);
            }
            
            Cache.SetWorkingZoneSentinels(Cache.Segment);
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
            log.DebugFormat("[GIF] - Size (pixels): {0}", m_VideoInfo.DecodingSize);
            log.Debug("---------------------------------------------------");
        }
        #endregion
    }
}
