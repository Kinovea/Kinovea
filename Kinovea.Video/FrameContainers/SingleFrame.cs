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
    // A "cache" with a capacity of a single frame, used for synchronous decoding.
    public class SingleFrame : IVideoFramesContainer
    {
        public VideoFrame CurrentFrame {
            get { return current; }
        }
        
        #region Construction / Destruction
        public SingleFrame(){}
        public SingleFrame(VideoFrameDisposer disposer)
        {
            this.disposer = disposer;
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
        private VideoFrame current = new VideoFrame();
        private VideoFrameDisposer disposer;
        #endregion
        
        public void Add(VideoFrame frame)
        {
            Clear();
            current.Image = frame.Image;
            current.Timestamp = frame.Timestamp;
        }
        public void Clear()
        {
            if(current.Image != null)
            {
                if(disposer != null)
                    disposer(current);
                else
                    current.Image.Dispose();
            }
            
            current.Image = null;
            current.Timestamp = 0;
        }
    }
}
