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

namespace Kinovea.Video
{
    public class SingleFrame : IVideoFramesContainer
    {
        public VideoFrame CurrentFrame {
            get { return m_Current; }
        }
        
        #region Construction / Destruction
        public SingleFrame(){}
        public SingleFrame(VideoFrameDisposer _disposer)
        {
            m_Disposer = _disposer;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~SingleFrame()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Clear();
        }
        #endregion
        
        #region Members
        private VideoFrame m_Current = new VideoFrame();
        private VideoFrameDisposer m_Disposer;
        #endregion
        
        public void Add(VideoFrame _frame)
        {
            Clear();
            m_Current.Image = _frame.Image;
            m_Current.Timestamp = _frame.Timestamp;
        }
        public void Clear()
        {
            if(m_Current.Image != null)
            {
                if(m_Disposer != null)
                    m_Disposer(m_Current);
                else
                    m_Current.Image.Dispose();
            }
            
            m_Current.Image = null;
            m_Current.Timestamp = 0;
        }
    }
}
