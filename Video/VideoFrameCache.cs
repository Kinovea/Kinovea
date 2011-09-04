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
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Kinovea.Video
{
    /// <summary>
    /// A buffer to cache some frames around the current point.
    /// The cache segment lives inside the working zone boundaries.
    /// It is a contiguous set of frames, however it may wrap over the end of the working zone.
    /// </summary>
    public class VideoFrameCache : IEnumerable, IDisposable
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
        public bool HasNext {
            get { 
                if(m_Current == null)
                    return false;
                else
                    return m_CurrentIndex < m_Cache.Count - 1;
            }
        }
        public bool Empty {
            get { return m_Current == null; }
        }
        public VideoSection WorkingZone { 
            get { return m_WorkingZone; }
            set { m_WorkingZone = value;}
        }
        
        /// <summary>
        /// Returns an arbitrary image suitable for demo-ing the effect of a filter.
        /// Currently returns the image at the middle of the buffer.
        /// </summary>
        public Bitmap Representative {
            get { return m_Cache[(m_Cache.Count / 2) - 1].Image; }
        }
        #endregion
        
        #region Members
        private List<VideoFrame> m_Cache = new List<VideoFrame>();
        private VideoSection m_Segment;
        private VideoSection m_WorkingZone;
        private int m_CurrentIndex = -1;
        private VideoFrame m_Current;
        
        private int m_DefaultCapacity = 25;
        private int m_DefaultRemembranceCapacity = 20;
        private int m_Capacity = 25;
        private int m_RemembranceCapacity = 20; // Capacity and Remembrance will be taken from Prefs.
        private bool m_PrependingBlock;
        private int m_InsertIndex;
        private int m_Drops;
        private VideoFrameDisposer m_DisposeBitmap;
        private Stopwatch m_Stopwatch = new Stopwatch();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Construction & Disposal
        public VideoFrameCache(){}
        public VideoFrameCache(VideoFrameDisposer _disposeDelegate)
        {
            m_DisposeBitmap = _disposeDelegate;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
		}
        ~VideoFrameCache()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Clear();
        }
        #endregion
        
        #region Public methods
        public bool MoveNext()
        {
            bool read = false;
            if(m_CurrentIndex + m_Drops >= m_Cache.Count - 1)
            {
                // The frame is not available, call it a drop and move to newest available.
                m_Drops = m_Cache.Count - m_CurrentIndex - m_Drops;
                m_CurrentIndex = m_Cache.Count - 1;
            }
            else
            {
                m_CurrentIndex = m_CurrentIndex + m_Drops + 1;
                read = true;
            }
            
            if(m_CurrentIndex >= 0 && m_CurrentIndex <= m_Cache.Count - 1)
                m_Current = m_Cache[m_CurrentIndex];
            
            RemembranceCheck();
            
            return read;
        }
        public bool MoveTo(long _timestamp)
        {
            if(!Contains(_timestamp))
                return false;
            
            if( m_Current!=null && _timestamp == m_Current.Timestamp)
                return true;

            foreach(int i in SortedFrames())
            {
                if(m_Cache[i].Timestamp >= _timestamp)
                {
                    m_CurrentIndex = i;
                    break;
                }
            }
            
            if(m_CurrentIndex >= 0 && m_CurrentIndex <= m_Cache.Count - 1)
                m_Current = m_Cache[m_CurrentIndex];
            
            RemembranceCheck();
            
            return true;
        }
        
        public IEnumerator GetEnumerator()
        {
            return m_Cache.GetEnumerator();
        }
        public bool Contains(long _timestamp)
        {
            if(m_Segment.Wrapped)
            {
                bool postWrap = _timestamp >= m_WorkingZone.Start && _timestamp <= m_Segment.End;
                bool preWrap = _timestamp >= m_Segment.Start && _timestamp <= m_WorkingZone.End;
                return postWrap || preWrap;
            }
            else
            {
                return m_Segment.Contains(_timestamp);
            }
        }
        public void Add(VideoFrame _frame)
        {
            if(m_Cache.Count >= m_Capacity)
            {
                // TODO: Block thread.
            }
            else
            {
                if(m_PrependingBlock)
                    m_Cache.Insert(m_InsertIndex++, _frame);
                else
                    m_Cache.Add(_frame);
                
                UpdateSegment();
            }
        }
        public List<Bitmap> ToBitmapList()
        {
            return m_Cache.Select(frame => frame.Image).ToList();
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
        public void Clear()
        {
            m_Current = null;
            
            foreach(VideoFrame vf in m_Cache)
                DisposeFrame(vf);
            
            m_Cache.Clear();
            m_CurrentIndex = -1;
            m_Drops = 0;
            m_Segment = VideoSection.Empty;
            m_Capacity = m_DefaultCapacity;
            m_RemembranceCapacity = m_DefaultRemembranceCapacity;
        }
        
        /// <summary>
        /// Remove all items that are outside the working zone.
        /// </summary>
        public void PurgeOutsiders()
        {
            int removedAtLeft = 0;
            foreach(int i in SortedFrames())
            {
                if(m_WorkingZone.Contains(m_Cache[i].Timestamp))
                    continue;
                
                if(m_Cache[i].Timestamp < m_WorkingZone.Start)
                    removedAtLeft++;
                    
                DisposeFrame(m_Cache[i]);
                m_Cache[i] = null;
                
                if(i==m_CurrentIndex)
                    m_CurrentIndex = -1;
            }
            
            if(m_CurrentIndex >= removedAtLeft)
                m_CurrentIndex-=removedAtLeft;
            
            m_Cache.RemoveAll(frame => object.ReferenceEquals(null, frame));
            UpdateSegment();
        }
        public void DisableCapacityCheck()
        {
            m_Capacity = int.MaxValue;
            m_RemembranceCapacity = int.MaxValue;
        }
        public void SetPrependBlock(bool _prepend)
        {
            m_PrependingBlock = _prepend;
            m_InsertIndex = 0;
        }
        public bool IsRolloverJump(long _timestamp)
        {
            // A rollover (back to begining after end of working zone),
            // is an out of segment jump that will still be contiguous after the jump.
            // In this special case the cache need not be cleared.
            return _timestamp == m_WorkingZone.Start && Contains(m_WorkingZone.End);
        }
        
        #region Debug
        public void DumpToDisk()
        {
            foreach(VideoFrame vf in m_Cache)
                vf.Image.Save(String.Format("{0}.bmp", vf.Timestamp));
        }
        #endregion
        
        #endregion
        
        #region Private methods
        private IEnumerable<int> SortedFrames()
        {
            // Returns an iterator on the cache in the order of timestamps.
            // Can be used to loop over the frames without bothering about wrapping.
            // todo: keep track of the wrap index outside here.
            int wrapIndex = GetWrapIndex();
            for(int i = 0; i<m_Cache.Count; i++)
            {
                int next = i + wrapIndex;
                if(next > m_Cache.Count - 1)
                    next -= m_Cache.Count;
                yield return next;
            }
        }
        private void DisposeFrame(VideoFrame _frame)
        {
            if(m_DisposeBitmap != null)
                m_DisposeBitmap(_frame);
            else
                _frame.Image.Dispose();
        }
        private void UpdateSegment()
        {
            m_Segment = new VideoSection(m_Cache[0].Timestamp, m_Cache[m_Cache.Count - 1].Timestamp);
            //log.DebugFormat("Segment:{0}, WZ:{1}", m_Segment, m_WorkingZone);
        }
        private int GetWrapIndex()
        {
            // Return the index of the frame right after the wrap break.
            if(m_Cache.Count < 2 || !m_Segment.Wrapped)
                return 0;
            
            int i = 1;
            while(m_Cache[i].Timestamp > m_Cache[i-1].Timestamp)
                i++;
            
            return i;
        }
        private void RemembranceCheck()
        {
            // Forget oldest frame(s) if needed. This should unblock the decoding thread.
            if(m_CurrentIndex >= m_RemembranceCapacity)
            {
                int framesToForget = m_CurrentIndex - m_RemembranceCapacity + 1; 
                
                for(int i=0;i<framesToForget;i++)
                    DisposeFrame(m_Cache[i]);

                m_Cache.RemoveRange(0, framesToForget);
                m_CurrentIndex -= framesToForget;
                m_Segment = new VideoSection(m_Cache[0].Timestamp, m_Segment.End);
            }
        }
        #endregion
    }
}
