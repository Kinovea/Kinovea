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
    [SupportedExtensions(".gif")]
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
        #endregion
        
        #region Members
        private bool m_Loaded;
        private VideoInfo m_VideoInfo;
        private int m_Count;
        private Image m_Gif;
        private FrameDimension m_FrameDimension;
        		
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public GIFVideoReader()
        {
            Cache = new VideoFrameCache();
        }
        #endregion
        
        #region Public Methods
        public override OpenVideoResult Open(string _filePath)
        {
            if(m_Loaded)
                Close();
                
            m_VideoInfo.FirstTimeStamp = 0;
            m_VideoInfo.AverageTimeStampsPerSeconds = 100;
            m_VideoInfo.FilePath = _filePath;
            
            OpenVideoResult res = LoadFile(_filePath, true);
            m_Gif.Dispose();
            DumpInfo();
            return res;
        }
        public override void Close()
        {
            Cache.Clear();
        }
        public override bool MoveNext(bool _synchrounous)
        {
            return Cache.MoveNext();
        }
        public override bool MoveTo(long _timestamp)
        {
            return Cache.MoveTo(_timestamp);
        }
        public override VideoSummary ExtractSummary(string _filePath, int _thumbs, int _width)
        {
            // NOT TESTED - NOT USED for now.
            VideoSummary summary = null;
           
            OpenVideoResult res = LoadFile(_filePath, true);
            
            if(res == OpenVideoResult.Success)
            {
                string kvaFile = string.Format("{0}\\{1}.kva", Path.GetDirectoryName(m_VideoInfo.FilePath), Path.GetFileNameWithoutExtension(m_VideoInfo.FilePath));
                bool hasKva = File.Exists(kvaFile);
                bool isImage = m_Count == 1;
                int durationMillisecs = (int)((double)m_Count * m_VideoInfo.FrameIntervalMilliseconds);
                
                List<Bitmap> thumbs = new List<Bitmap>();
                if(_thumbs > 0)
                {
                    int step = (int)Math.Ceiling(m_Count / (double)_thumbs);
                    for(int i = 0; i<m_Count; i+=step)
                        thumbs.Add(GetFrameAt(i));
                }
                
                summary = new VideoSummary(isImage, hasKva, m_VideoInfo.OriginalSize, durationMillisecs, thumbs);
            }
            
            if(m_Loaded)
                Close();
            
            m_Gif.Dispose();
            return summary;
        }
        public override string ReadMetadata()
        {
            return "";
        }
        public override bool CanCacheWorkingZone(VideoSection _newZone, int _maxSeconds, int _maxMemory)
        {
            return true;
        }
        public override void ReadMany(BackgroundWorker _bgWorker, VideoSection _section, bool _prepend)
        {
             // TODO: put the code of Load here.
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
                
                // Duration in frames
        		m_FrameDimension = new FrameDimension(m_Gif.FrameDimensionsList[0]);
        		m_Count = m_Gif.GetFrameCount(m_FrameDimension);
        		    
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

                m_VideoInfo.DecodingSize = m_Gif.Size;
                m_VideoInfo.OriginalSize = m_Gif.Size;
        		
                if(_cache)
                    LoadCache();
                
        		m_Loaded = true;
                result = OpenVideoResult.Success;
            }
            while(false);

            return result;
        }
        private void LoadCache()
        {
            // Use cache mechanics.
            // Set the sentinels first, then add.
            
            Cache.Clear();
            Cache.FullZone = true;
            for(int i = 0; i<m_Count; i++)
            {
                VideoFrame vf = new VideoFrame();
                vf.Timestamp = i * m_VideoInfo.AverageTimeStampsPerFrame;
                vf.Image = GetFrameAt(i);
                Cache.Add(vf);
            }
            
            Cache.SetWorkingZoneSentinels(Cache.Segment);
        }
        private Bitmap GetFrameAt(int _target)
        {
            m_Gif.SelectActiveFrame(m_FrameDimension, _target);
            Bitmap bmp = new Bitmap(m_VideoInfo.DecodingSize.Width, m_VideoInfo.DecodingSize.Height, PixelFormat.Format32bppPArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(m_Gif, 0, 0, m_VideoInfo.DecodingSize.Width, m_VideoInfo.DecodingSize.Height);
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
            log.DebugFormat("[GIF] - Size (pixels): {0}", m_VideoInfo.DecodingSize);
            log.Debug("---------------------------------------------------");
        }
        #endregion
    }
}
