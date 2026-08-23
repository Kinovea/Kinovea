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
        #region Properties
        public VideoFrame CurrentFrame 
        {
            get { return current; }
        }
        public bool IsEmpty
        {
            get { return current == null; }
        }

        #endregion


        #region Construction / Destruction
        public SingleFrame(){}
        public SingleFrame(VideoFrameDisposer disposer)
        {
            this.frameDisposer = disposer;
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
        private VideoFrame current = null;
        private VideoFrameDisposer frameDisposer;
        #endregion
        
        public void Add(VideoFrame frame)
        {
            Clear();
            current = frame;
        }
        public void Clear()
        {
            if (current == null)
                return;
            
            if(frameDisposer != null)
            {
                frameDisposer(current);
            }
            else
            {
                current.Image.Dispose();
            }
            
            current = null;
        }

        /// <summary>
        /// Clear the frame without disposing the bitmap.
        /// Used when the frame has been transferred to a different container.
        /// </summary>
        public void Forget()
        {
            current = null;
        }
    }
}
