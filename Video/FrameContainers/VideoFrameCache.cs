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
using System.Threading;
using Kinovea.Base;

namespace Kinovea.Video
{
    /// <summary>
    /// A buffer to cache some frames around the current point.
    /// The cache segment lives inside the working zone boundaries.
    /// It is a contiguous set of frames, however it may wrap over the end of the working zone.
    /// </summary>
    /// <remarks>
    /// Thread safety:
    /// Locking is necessary around all access to m_Cache as it is read and written by both the UI and the decoding thread.
    /// Assumedly, there is no need to lock around access to m_Current.
    /// This is because m_Cache is only accessed for add by the decoding thread and this has no impact on m_Current reference.
    /// The only thing that alters the reference to m_Current are: MoveNext, MoveTo, PurgeOutsiders, Clear.
    /// All these are initiated by the UI thread itself, so it will not be using m_Current simultaneously.
    /// Similarly, drop count is only updated in MoveNext and MoveTo, so only from the UI thread.
    ///</remarks>
    public class VideoFrameCache : IEnumerable, IDisposable
    {
        #region Properties
        public VideoFrame Current { get { return m_Current; } }
        public VideoSection Segment { get { return m_Segment;} }
        public int Count { get { lock(m_Locker) return m_Cache.Count; } }
        public bool Empty { get { return m_Current == null; } }
        public int Drops { get { return m_Drops; } }
        public int Capacity { get { return m_Capacity; }}
        
        public bool HasNext(int _skip) 
        {
            if(m_Current == null)
            {
                return false;
            }
            else
            {
                lock(m_Locker) 
                    return m_CurrentIndex + m_Drops + _skip < m_Cache.Count - 1;
            }
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
            get { 
                lock(m_Locker) 
                {
                    return m_Cache[(m_Cache.Count / 2)].Image; 
                }
            }
        }
        #endregion
        
        #region Members
        private List<VideoFrame> m_Cache = new List<VideoFrame>();
        private VideoSection m_Segment;
        private VideoSection m_WorkingZone;
        private int m_CurrentIndex = -1;
        private VideoFrame m_Current;
        private readonly object m_Locker = new object();
        
        private int m_DefaultCapacity = 25;
        private int m_Capacity = 25;
        private int m_DefaultRemembranceCapacity = 8;
        private int m_RemembranceCapacity = 8; // Capacity and Remembrance will later be taken from Prefs.
        private bool m_PrependingBlock;
        private int m_InsertIndex;
        private int m_Drops;
        private VideoFrameDisposer m_DisposeBitmap;
        private TimeWatcher m_TimeWatcher = new TimeWatcher();
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
        public bool MoveNext(int _skip)
        {
            m_TimeWatcher.Restart();
            bool read = false;
            lock(m_Locker)
            {
                int lastIndex = m_Cache.Count - 1;
                int expectedIndex = m_CurrentIndex + m_Drops + _skip;
            
                if(expectedIndex < lastIndex)
                {
                    m_CurrentIndex = expectedIndex + 1;
                    m_Drops = 0;
                    read = true;
                }
                else
                {
                    m_Drops++;
                    log.DebugFormat("Decoding Drops: {0}.", m_Drops);
                }
            
                if(m_CurrentIndex >= 0 && m_CurrentIndex <= lastIndex)
                    m_Current = m_Cache[m_CurrentIndex];
            }
            RemembranceCheck();
            m_TimeWatcher.DumpTimes();
            return read;
        }
        public bool MoveTo(long _timestamp)
        {
            m_Drops = 0;
                
            if(!Contains(_timestamp))
                return false;
            
            if( m_Current != null && _timestamp == m_Current.Timestamp)
                return true;

            lock(m_Locker)
            {
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
            }
            
            RemembranceCheck();
            
            return true;
        }
        public void SkipDrops()
        {
            m_Drops = 0;
        }
        
        public void Add(VideoFrame _frame)
        {
            lock(m_Locker)
            {
                //log.DebugFormat("Add - Pushing frame to cache. {0}/{1} ({2}).", m_Cache.Count+1, m_Capacity, m_RemembranceCapacity);
                
                if(m_PrependingBlock)
                    m_Cache.Insert(m_InsertIndex++, _frame);
                else
                    m_Cache.Add(_frame);
                
                UpdateSegment();
                
                while (m_Cache.Count >= m_Capacity)
                {
                    // Will release its lock and block until there is a pulse.
                    // We do this after the actual Add so the decoding thread can 
                    // check for cancellation *before* pushing another frame.
                    Monitor.Wait(m_Locker);
                }
            }
        }
        
        public IEnumerator GetEnumerator()
        {
            // Provided to support foreach construct.
            // Should be removed in favor of Reader.FrameEnumerator.
            
            
            // This one is not thread safe and can't be made so easily. 
            // Check if we could use a copy of the list instead.
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
        public List<Bitmap> ToBitmapList()
        {
            lock(m_Locker) 
                return m_Cache.Select(frame => frame.Image).ToList();
        }
        public void Revert()
        {
            lock(m_Locker)
            {
                for(int i = 0; i<m_Cache.Count/2; i++)
                {
                    Bitmap tmp = m_Cache[i].Image;
                    m_Cache[i].Image = m_Cache[m_Cache.Count -1 - i].Image;
                    m_Cache[m_Cache.Count -1 - i].Image = tmp;
                }
            }
        }
        public void Clear()
        {
            lock(m_Locker)
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
                
                Monitor.Pulse(m_Locker);
            }
        }
        public void RemoveOldest()
        {
            // This may be used to unblock the decoding thread.
            
            if(m_Cache.Count < 1)
                return;
            
            lock(m_Locker)
            {
                DisposeFrame(m_Cache[0]);
                m_Cache.RemoveAt(0);
                m_CurrentIndex--;
                UpdateSegment();
                
                Monitor.Pulse(m_Locker);
            }
        }
        
        /// <summary>
        /// Remove all items that are outside the working zone.
        /// </summary>
        public void PurgeOutsiders()
        {
            lock(m_Locker)
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
                
                m_CurrentIndex = Math.Max(0, m_CurrentIndex);
                m_Current = m_Cache[m_CurrentIndex];
                
                m_Cache.RemoveAll(frame => object.ReferenceEquals(null, frame));
                UpdateSegment();
                
                Monitor.Pulse(m_Locker);
            }
        }
        public void DisableCapacityCheck()
        {
            // This may be used to load a whole video initially in the cache.
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
            lock(m_Locker)
                foreach(VideoFrame vf in m_Cache)
                    vf.Image.Save(String.Format("{0}.bmp", vf.Timestamp));
        }
        #endregion
        
        #endregion
        
        #region Private methods
        private IEnumerable<int> SortedFrames()
        {
            // /!\ Should only be called from inside a lock construct.
            
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
            // Forget oldest frame(s) if needed and unblock the decoding thread.
            if(m_CurrentIndex < m_RemembranceCapacity)
                return;

            lock(m_Locker)
            {
                int framesToForget = m_CurrentIndex - m_RemembranceCapacity + 1;
                
                for(int i=0;i<framesToForget;i++)
                    DisposeFrame(m_Cache[i]);

                m_Cache.RemoveRange(0, framesToForget);
                m_CurrentIndex -= framesToForget;
                
                UpdateSegment();
                
                Monitor.Pulse(m_Locker);
            }
        }
        #endregion
    }
}
