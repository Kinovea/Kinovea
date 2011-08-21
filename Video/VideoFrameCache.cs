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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Kinovea.Video
{
    /// <summary>
    /// A buffer to cache some frames around the current point.
    /// The cache segment lives inside the working zone boundaries.
    /// </summary>
    public class VideoFrameCache : IEnumerable
    {
        #region Properties
        public VideoFrame Current {
            get { return m_Current; }
        }
        public VideoSection Segment {
            get { return m_Segment;}
        }
        public int Count {
            get { return m_Cache.Count; }
        }
        
        /// <summary>
        /// Returns an arbitrary image suitable for demoing the effect of a filter.
        /// Currently returns the image at the middle of the buffer.
        /// </summary>
        public Bitmap Representative {
            get { return m_Cache[(m_Cache.Count / 2) - 1].Image; }
        }
       
        /// <summary>
        /// True when the cache work with the whole working zone at once.
        /// In this mode remembrance capacity is considered unlimited so we never dequeue images.
        /// </summary>
        public bool FullZone { get; set; }
        #endregion
        
        #region Members
        private List<VideoFrame> m_Cache = new List<VideoFrame>();
        private int m_Capacity = 4;
        private int m_RemembranceCapacity = 2;
        private VideoSection m_Segment;
        private VideoSection m_WorkingZone;
        private int m_CurrentIndex = -1;
        private int m_Drops;
        private VideoFrame m_Current;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public IEnumerator GetEnumerator()
        {
            return m_Cache.GetEnumerator();
        }
        public void Clear()
        {
            m_Cache.Clear();
        }
        public void Add(VideoFrame _frame)
        {
            if(!FullZone && m_Cache.Count >= m_Capacity)
            {
                // Buffer is full.
            }
            else
            {
                m_Cache.Add(_frame);
                
                if(m_Cache.Count == 1)
                {
                    m_Current = _frame;
                    m_Segment = new VideoSection(_frame.Timestamp, _frame.Timestamp);
                }
                else
                {
                    m_Segment = new VideoSection(m_Segment.Start, _frame.Timestamp);
                }
                
                if(FullZone)
                {
                    m_RemembranceCapacity = m_Cache.Count;
                    m_Capacity = m_Cache.Count;
                }
            }
        }
        public List<Bitmap> ToBitmapList()
        {
            List<Bitmap> list = new List<Bitmap>();
            foreach(VideoFrame vf in m_Cache)
                list.Add(vf.Image);
			return list;
        }
        public void Revert()
        {
            for(int i = 0; i<m_Cache.Count/2; i++)
            {
                Bitmap tmp = m_Cache[i].Image;
                m_Cache[i].Image = m_Cache[m_Cache.Count -1 - i].Image;
                m_Cache[m_Cache.Count -1 - i].Image = tmp;
            }
        }
        public void SetWorkingZoneSentinels(VideoSection _wz)
        {
            if(_wz.Start >= _wz.End)
                return;
            
            if(_wz.CompareTo(m_WorkingZone) < 0)
            {
                // Contraction.
                // TODO: remove the frames outside the new boundaries.
                // must be done for both the dynamic and static mode.
            }
            
            m_WorkingZone = _wz;
            
            if(!m_WorkingZone.Contains(m_Current.Timestamp))
            {
                m_CurrentIndex = 0;
                m_Current = m_Cache[0];
            }
        }
        
        public bool MoveNext()
        {
            bool read = false;
            if(m_Current == null || m_Current.Timestamp != m_WorkingZone.End)
            {
                if(m_CurrentIndex + m_Drops >= m_Cache.Count - 1)
                {
                    // The frame is not available, call it a drop and move to newest available.
                    m_Drops = m_Cache.Count - m_CurrentIndex - m_Drops;
                    m_CurrentIndex = m_Cache.Count - 1;
                }
                else
                {
                    m_CurrentIndex++;
                    m_Current = m_Cache[m_CurrentIndex];
                    read = true;
                }
                
                m_Current = m_Cache[m_CurrentIndex];
                
                if(!FullZone && m_CurrentIndex >= m_RemembranceCapacity)
                {
                    // Forget oldest frame.
                    // This should unblock the decoding thread so we can refill future.
                }
            }
            return read;
        }
        public bool MoveTo(long _timestamp)
        {
            if(_timestamp < m_Segment.Start || _timestamp > m_Segment.End)
                return false;
            
            if(_timestamp == m_Current.Timestamp)
                return true;

            for(int i=0;i<m_Cache.Count;i++)
            {
                if(m_Cache[i].Timestamp >= _timestamp)
                {
                    m_CurrentIndex = i;
                    m_Current = m_Cache[i];
                    break;
                }
            }
            return true;
        }
    }
}
