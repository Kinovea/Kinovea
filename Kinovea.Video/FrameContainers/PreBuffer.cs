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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using Kinovea.Services;

namespace Kinovea.Video
{
    /// <summary>
    /// A buffer to anticipate some frames from the future, and remember some from the past.
    /// The prebuffered section is entirely contained inside the working zone boundaries.
    /// It is a contiguous set of frames, except that it may wrap over the end of the working zone.
    /// </summary>
    /// <remarks>
    /// Naming:
    /// - Segment: the section of prebuffered frames, contained inside the working zone.
    /// - OldFramesCapacity: the number of frames kept that are older than the current point.
    ///
    /// Thread safety:
    /// Locking is necessary around all access to m_Frames as it is read and written by both the UI and the decoding thread.
    /// Assumedly, there is no need to lock around access to m_Current.
    /// This is because m_Frames is only accessed for add by the decoding thread and this has no impact on m_Current reference.
    /// The only thing that alters the reference to m_Current are: MoveNext, MoveTo, PurgeOutsiders, Clear.
    /// All these are initiated by the UI thread itself, so it will not be using m_Current simultaneously.
    /// Similarly, drop count is only updated in MoveNext and MoveTo, so only from the UI thread.
    ///</remarks>
    public class PreBuffer : IDisposable, IVideoFramesContainer
    {
        #region Properties
        public VideoFrame CurrentFrame { 
            get { return m_Current; } 
        }
        public VideoSection Segment 
        { 
            get 
            { 
                lock(m_Locker) 
                    return m_Segment;
            } 
        }
        public int Drops { 
            get { return m_Drops; }
        }
        #endregion
        
        #region Members
        private List<VideoFrame> m_Frames = new List<VideoFrame>();
        private VideoSection m_Segment = VideoSection.Empty;
        private VideoSection m_WorkingZone = VideoSection.Empty;
        private int m_CurrentIndex = -1;
        private VideoFrame m_Current;
        private readonly object m_Locker = new object();
        
        private int m_DefaultTotalCapacity = 25;
        private int m_TotalCapacity = 25;
        private int m_DefaultOldFramesCapacity = 8;
        private int m_OldFramesCapacity = 8; // Will later be taken from Prefs, possibly in MB instead of frames.
        private int m_Drops;
        private VideoFrameDisposer m_DisposeBitmap;
        private TimeWatcher m_TimeWatcher = new TimeWatcher();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Construction & Disposal
        public PreBuffer(){}
        public PreBuffer(VideoFrameDisposer _disposeDelegate)
        {
            m_DisposeBitmap = _disposeDelegate;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~PreBuffer()
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
        public bool MoveBy(int _frames)
        {
            //m_TimeWatcher.Restart();
            bool read = false;
            lock(m_Locker)
            {
                int lastIndex = m_Frames.Count - 1;
                int expectedCurrentIndex = m_CurrentIndex + m_Drops + _frames - 1;
            
                if(expectedCurrentIndex < lastIndex)
                {
                    m_CurrentIndex = expectedCurrentIndex + 1;
                    m_Drops = 0;
                    read = true;
                }
                else
                {
                    m_Drops = expectedCurrentIndex - lastIndex + 1;
                    //log.DebugFormat("Decoding Drops: {0}.", m_Drops);
                }
                
                if(m_CurrentIndex >= 0 && m_CurrentIndex <= lastIndex)
                    m_Current = m_Frames[m_CurrentIndex];
            }
            
            // ForgetOldFrames will take another lock, should it be inside the first ?
            // It will possibily do a Pulse.
            ForgetOldFrames();
            //m_TimeWatcher.DumpTimes();
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
                    if(m_Frames[i].Timestamp >= _timestamp)
                    {
                        m_CurrentIndex = i;
                        break;
                    }
                }
            
                if(m_CurrentIndex >= 0 && m_CurrentIndex <= m_Frames.Count - 1)
                    m_Current = m_Frames[m_CurrentIndex];
            }
            
            ForgetOldFrames();
            
            return true;
        }
        public void ResetDrops()
        {
            m_Drops = 0;
        }
        public bool HasNext(int _skip)
        {
            lock(m_Locker)
                return m_CurrentIndex + m_Drops + _skip + 1 < m_Frames.Count;
        }
        public void Add(VideoFrame _frame)
        {
            lock(m_Locker)
            {
                //log.DebugFormat("Add - Pushing frame [{0}] to prebuffer. ({1}/{2}).", _frame.Timestamp, m_Frames.Count+1, m_TotalCapacity);
                m_Frames.Add(_frame);
                UpdateSegment();
                while (m_Frames.Count >= m_TotalCapacity)
                {
                    // Will release its lock and freeze until there is a pulse.
                    // We do this after the actual Add so the decoding thread, when woken up,
                    // can check for cancellation *before* pushing another frame.
                    Monitor.Wait(m_Locker);
                }
            }
        }
        public bool Contains(long _timestamp)
        {
            lock(m_Locker)
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
        }
        public void Clear()
        {
            lock(m_Locker)
            {
                m_Current = null;
                
                foreach(VideoFrame vf in m_Frames)
                    DisposeFrame(vf);
                
                m_Frames.Clear();
                m_CurrentIndex = -1;
                m_Drops = 0;
                m_Segment = VideoSection.Empty;
                m_TotalCapacity = m_DefaultTotalCapacity;
                m_OldFramesCapacity = m_DefaultOldFramesCapacity;
                
                Monitor.Pulse(m_Locker);
            }
        }
        public void UnblockAndMakeRoom()
        {
            lock(m_Locker)
            {
                // This is used to temporarily deactivate the prebuffering thread without 
                // completely clearing it. The decoding thread is potentially waiting on a full buffer,
                // so we must discard at least one frame to make it run again and check for cancellation.
                // However, the next Add is assumed to run on the UI thread, so it must not block.
                // So we actually need to have two empty slots: one to push the read,
                // and one to make that push non-blocking.
                log.Debug("Unblocking prebuffering thread and making room for a non blocking addition.");
                
                while(m_Frames.Count > m_TotalCapacity - 2)
                {
                    DisposeFrame(m_Frames[0]);
                    m_Frames.RemoveAt(0);
                    m_CurrentIndex--;
                }
                UpdateSegment();
                
                Monitor.Pulse(m_Locker);
            }
        }
        
        public void UpdateWorkingZone(VideoSection _newZone)
        {
            if(m_Frames.Count > 0)
                Clear();
            
            m_WorkingZone = _newZone;
        }
        /// <summary>
        /// Remove all items that are outside the working zone.
        /// </summary>
        public void PurgeOutsiders()
        {
            lock(m_Locker)
            {
                log.Debug("Purging Outsiders in PreBuffer.");
                int removedAtLeft = 0;
                foreach(int i in SortedFrames())
                {
                    if(m_WorkingZone.Contains(m_Frames[i].Timestamp))
                        continue;
                    
                    if(m_Frames[i].Timestamp < m_WorkingZone.Start)
                        removedAtLeft++;
                        
                    DisposeFrame(m_Frames[i]);
                    m_Frames[i] = null;
                    
                    if(i==m_CurrentIndex)
                        m_CurrentIndex = -1;
                }
                
                if(m_CurrentIndex >= removedAtLeft)
                    m_CurrentIndex-=removedAtLeft;
                
                m_CurrentIndex = Math.Max(0, m_CurrentIndex);
                m_Current = m_Frames[m_CurrentIndex];
                
                m_Frames.RemoveAll(frame => object.ReferenceEquals(null, frame));
                UpdateSegment();
                
                Monitor.Pulse(m_Locker);
            }
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
                foreach(VideoFrame vf in m_Frames)
                    vf.Image.Save(String.Format("{0}.bmp", vf.Timestamp));
        }
        #endregion
        
        #endregion
        
        #region Private methods
        private IEnumerable<int> SortedFrames()
        {
            // /!\ Should only be called from inside a lock construct.
            
            // Returns an iterator on the indices of frames in the buffer in the order of timestamps.
            // For example if the current buffer is [7;8;9;0;1] it will return [0;1;7;8;9].
            // Can be used to loop over the frames without bothering about wrapping.
            
            // todo: keep track of the wrap index outside here.
            int wrapIndex = GetWrapIndex();
            
            for(int i = 0; i<m_Frames.Count; i++)
            {
                int next = i + wrapIndex;
                if(next > m_Frames.Count - 1)
                    next -= m_Frames.Count;
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
            // Get real data from the stored frames.
            // Always inside a lock.
            if(m_Frames.Count < 1)
                m_Segment = VideoSection.Empty;
            else
                m_Segment = new VideoSection(m_Frames[0].Timestamp, m_Frames[m_Frames.Count - 1].Timestamp);

            //log.DebugFormat("Updated segment: {0}", m_Segment);
        }
        private int GetWrapIndex()
        {
            // Return the index of the frame right after the wrap break.
            // Always inside a lock.
            if(m_Frames.Count < 2 || !m_Segment.Wrapped)
                return 0;
            
            int i = 1;
            while(m_Frames[i].Timestamp > m_Frames[i-1].Timestamp)
                i++;
            
            return i;
        }
        private void ForgetOldFrames()
        {
            if(m_CurrentIndex < m_OldFramesCapacity)
                return;

            lock(m_Locker)
            {
                int framesToForget = m_CurrentIndex - m_OldFramesCapacity + 1;
                //log.DebugFormat("Forgetting {0} frames.", framesToForget);
                
                for(int i=0;i<framesToForget;i++)
                    DisposeFrame(m_Frames[i]);

                m_Frames.RemoveRange(0, framesToForget);
                m_CurrentIndex -= framesToForget;


                UpdateSegment();
                
                Monitor.Pulse(m_Locker);
            }
        }
        #endregion
    }
}
