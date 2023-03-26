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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Kinovea.Services;

namespace Kinovea.Video
{
    /// <summary>
    /// A frame container for the caching of the whole working zone.
    /// All methods run in the UI thread.
    /// Play head moves are synchronous and instantaneous.
    /// </summary>
    public class Cache : IVideoFramesContainer, IWorkingZoneFramesContainer
    {
        #region Properties
        public VideoFrame CurrentFrame {
            get { return m_Current; }
        }
        public VideoSection WorkingZone {
            get { return m_WorkingZone;}
        }
        #endregion
        
        #region Members
        private List<VideoFrame> m_Frames = new List<VideoFrame>();
        private int m_CurrentIndex = -1;
        private VideoFrame m_Current;
        private VideoSection m_WorkingZone = VideoSection.Empty;
        private bool m_PrependingBlock;
        private int m_InsertIndex;
        private VideoFrameDisposer m_Disposer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Construction and destruction
        public Cache(){}
        public Cache(VideoFrameDisposer _disposer)
        {
            m_Disposer = _disposer;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~Cache()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Clear();
        }
        #endregion
        
        #region Public Methods
        public bool MoveBy(int _frames)
        {
            if(m_Frames.Count < 1 || _frames < 0)
                return false;
            
            int lastIndex = m_Frames.Count - 1;
            int targetIndex = m_CurrentIndex + _frames;
            
            if(targetIndex > lastIndex)
                return false;
            
            m_CurrentIndex = targetIndex;
            UpdateCurrentFrame();
            return true;
        }
        public bool MoveTo(long target)
        {
            if(!Contains(target))
                return false;
            
            if( m_Current != null && target == m_Current.Timestamp)
                return true;

            m_CurrentIndex = m_Frames.FindIndex(f => f.Timestamp >= target);
            UpdateCurrentFrame();
            return true;
        }
        public void Add(VideoFrame _frame)
        {
            if(m_PrependingBlock)
                m_Frames.Insert(m_InsertIndex++, _frame);
            else
                m_Frames.Add(_frame);
                
            UpdateWorkingZone();
        }
        public void Clear()
        {
            m_Current = null;
            m_CurrentIndex = -1;
            
            foreach(VideoFrame frame in m_Frames)
                DisposeFrame(frame);
                
            m_Frames.Clear();
            m_WorkingZone = VideoSection.Empty;
        }
        /// <summary>
        /// Remove all items that are outside the working zone.
        /// </summary>
        public void ReduceWorkingZone(VideoSection _newZone)
        {
            m_WorkingZone = _newZone;
            
            int removedAtLeft = 0;
            for(int i = 0; i<m_Frames.Count;i++)
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
            
            m_Frames.RemoveAll(frame => object.ReferenceEquals(null, frame));
            
            m_CurrentIndex = Math.Max(0, m_CurrentIndex);
            m_Current = m_Frames[m_CurrentIndex];
            
            UpdateWorkingZone();
        }
        
        /// <summary>
        /// Enable or disable insertion mode for Add operations.
        /// This can be used to add many images in front of the existing range of cached frames.
        /// (Only the first Add is actually a prepend, subsequent Add are inserted before the old first frame)
        /// </summary>
        public void SetPrependBlock(bool _enablePrepend)
        {
            m_PrependingBlock = _enablePrepend;
            
            // The insertion index is initialized to 0 and will be updated on each Add.
            m_InsertIndex = 0;
        }
        #endregion

        #region Private Methods
        private bool Contains(long _timestamp)
        {
            return m_WorkingZone.Contains(_timestamp);
        }
        private void DisposeFrame(VideoFrame _frame)
        {
            if(m_Disposer != null)
                m_Disposer(_frame);
            else
                _frame.Image.Dispose();
        }
        private void UpdateCurrentFrame()
        {
            if(m_CurrentIndex >= 0 && m_CurrentIndex < m_Frames.Count)
            {
                m_Current = m_Frames[m_CurrentIndex];
            }
            else
            {
                m_CurrentIndex = -1;
                #if DEBUG
                throw new IndexOutOfRangeException();
                #endif
            }
        }
        private void UpdateWorkingZone()
        {
            if(m_Frames.Count > 0)
                m_WorkingZone = new VideoSection(m_Frames[0].Timestamp, m_Frames[m_Frames.Count - 1].Timestamp);
            else
                m_WorkingZone = VideoSection.Empty;
        }
        #endregion
        
        #region IWorkingZoneFramesContainer implementation
        public ReadOnlyCollection<VideoFrame> Frames {
            get { return m_Frames.AsReadOnly(); }
        }
        public Bitmap Representative {
            get { return m_Frames[(m_Frames.Count / 2)].Image; }
        }
        public void Revert()
        {
            int lastIndex = m_Frames.Count-1;
            int halfIndex = m_Frames.Count/2;
            for(int i = 0; i<halfIndex; i++)
            {
                int opposedIndex = lastIndex - i;
                Bitmap tmp = m_Frames[i].Image;
                m_Frames[i].Image = m_Frames[opposedIndex].Image;
                m_Frames[opposedIndex].Image = tmp;
            }
        }
        #endregion
    }
}
