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

namespace Kinovea.Video
{
    [SupportedExtensions(new string[] {".gif"})]
    public class GIFVideoReader : VideoReader
    {
        #region Properties
        public override bool Loaded
        {
            get { return m_Loaded; }
        }
        public override VideoInfo Info
        {
            get { return m_VideoInfo; }
        }
        public override VideoSection Selection
        {
            get { return m_Selection; }
        }
        public override bool Caching
        {
            get { return true; }
        }
        public override VideoFrame Current
        {
            get { return m_Current; }
        }
        #endregion
        
        #region Members
        private bool m_Loaded;
        private VideoInfo m_VideoInfo;
        private VideoSection m_Selection;
        private Image m_Gif;
        private int m_Count;
        private VideoFrame m_Current = new VideoFrame();
        private SortedList<long, VideoFrame> m_Cache = new SortedList<long, VideoFrame>();
        private int m_CacheIndex = -1;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public override OpenVideoResult Open(string _FilePath)
        {
            OpenVideoResult result = OpenVideoResult.UnknownError;
            
            if(m_Loaded) 
                Close();
                
            do 
            {
                m_Gif = Image.FromFile(_FilePath);
                if(m_Gif == null)
                {
                    result = OpenVideoResult.FileNotOpenned;
                    log.ErrorFormat("The file could not be openned.");
                    break;
                }
                
                m_VideoInfo.FirstTimeStamp = 0;
                m_VideoInfo.AverageTimeStampsPerSeconds = 100;
                m_VideoInfo.FilePath = _FilePath;
                
                // Duration in frames
        		FrameDimension fd = new FrameDimension(m_Gif.FrameDimensionsList[0]);
        		m_Count = m_Gif.GetFrameCount(fd);
        		    
                // Duration of first interval. (PropertyTagFrameDelay)
                // The byte array returned by the Value property contains 32bits integers for each frame interval (in 1/100th).
                PropertyItem pi = m_Gif.GetPropertyItem(0x5100);
		        int interval = BitConverter.ToInt32(pi.Value, 0);
                m_VideoInfo.DurationTimeStamps = m_Count * interval;
		        m_VideoInfo.FrameIntervalMilliseconds = interval * 10;
        		
                m_VideoInfo.FramesPerSeconds = 100D/interval;
        		m_VideoInfo.AverageTimeStampsPerFrame = interval;

                m_VideoInfo.DecodingSize = m_Gif.Size;
        		
        		DumpInfo();
        		m_Loaded = true;
                result = OpenVideoResult.Success;
            }
            while(false);

            if(m_Loaded)
                Cache();

            return result;
        }
        public override void Close()
        {
            // TODO
        }
        public override bool MoveNext()
        {
            m_CacheIndex++;
            if(m_CacheIndex > m_Cache.Count - 1)
            {
                if(Options.AutoRewind)
                {
                    m_CacheIndex = 0;
                }
                else
                {
                    m_CacheIndex = m_Cache.Count - 1;
                    return false;
                }
            }
        
            m_Current = m_Cache.Values[m_CacheIndex];
            return true;
        }
        public override bool MoveTo(long _timestamp)
        {
            return false;
        }
        
        private void Cache()
        {
            m_Cache.Clear();
            
            FrameDimension fd = new FrameDimension(m_Gif.FrameDimensionsList[0]);
            for(int i = 0; i<m_Count; i++)
            {
                m_Gif.SelectActiveFrame(fd, i);

                VideoFrame vf = new VideoFrame();
                vf.Timestamp = i * m_VideoInfo.AverageTimeStampsPerFrame;
                vf.Image = new Bitmap(m_VideoInfo.DecodingSize.Width, m_VideoInfo.DecodingSize.Height, PixelFormat.Format32bppPArgb);
		        Graphics g = Graphics.FromImage(vf.Image);
                g.DrawImage(m_Gif, 0, 0, m_VideoInfo.DecodingSize.Width, m_VideoInfo.DecodingSize.Height); 
		    
                m_Cache.Add(vf.Timestamp, vf);
            }
        }
        private void DumpInfo()
        {
            log.Debug("---------------------------------------------------");
            log.DebugFormat("[File] - Filename : {0}", Path.GetFileName(m_VideoInfo.FilePath));
            log.DebugFormat("[GIF] - First interval (ms): {0}", m_VideoInfo.FrameIntervalMilliseconds);
            log.DebugFormat("[GIF] - Duration (ts): {0}", m_VideoInfo.DurationTimeStamps);
            log.DebugFormat("[GIF] - Duration (s): {0}", (double)m_VideoInfo.DurationTimeStamps/(double)m_VideoInfo.AverageTimeStampsPerSeconds);
            log.DebugFormat("[GIF] - Computed fps: {0}", m_VideoInfo.FramesPerSeconds);
            log.DebugFormat("[GIF] - Size (pixels): {0}", m_Gif.Size);
            log.Debug("---------------------------------------------------");
        }
        
    }
}
