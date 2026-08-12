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
            get { return currentFrame; }
        }
        public VideoSection WorkingZone {
            get { return workingZone;}
        }
        #endregion
        
        #region Members
        private List<VideoFrame> frames = new List<VideoFrame>();
        private int currentIndex = -1;
        private VideoFrame currentFrame;
        private VideoSection workingZone = VideoSection.MakeEmpty();
        private bool isPrepending;
        private int insertIndex;
        private VideoFrameDisposer frameDisposer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Construction and destruction
        public Cache(){}
        public Cache(VideoFrameDisposer frameDisposer)
        {
            this.frameDisposer = frameDisposer;
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
        public bool MoveBy(int frameCount)
        {
            if(frames.Count < 1 || frameCount < 0)
                return false;
            
            int lastIndex = frames.Count - 1;
            int targetIndex = currentIndex + frameCount;
            
            if(targetIndex > lastIndex)
                return false;
            
            currentIndex = targetIndex;
            UpdateCurrentFrame();
            return true;
        }
        public bool MoveTo(long target)
        {
            if(!Contains(target))
                return false;
            
            if( currentFrame != null && target == currentFrame.Timestamp)
                return true;

            currentIndex = frames.FindIndex(f => f.Timestamp >= target);
            UpdateCurrentFrame();
            return true;
        }
        public void Add(VideoFrame frame)
        {
            if(isPrepending)
            {
                frames.Insert(insertIndex, frame);
                insertIndex++;
            }
            else
            {
                frames.Add(frame);
            }
                
            UpdateWorkingZone();
        }
        public void Clear()
        {
            currentFrame = null;
            currentIndex = -1;
            
            foreach(VideoFrame frame in frames)
            {
                DisposeFrame(frame);
            }
                
            frames.Clear();
            UpdateWorkingZone();

            log.Debug("Cache cleared.");
        }
        /// <summary>
        /// Remove all items that are outside the working zone.
        /// </summary>
        public void ReduceWorkingZone(VideoSection newZone)
        {
            workingZone = newZone;
            
            int removedAtLeft = 0;
            for(int i = 0; i<frames.Count;i++)
            {
                if (workingZone.Contains(frames[i].Timestamp))
                    continue;
                    
                if (frames[i].Timestamp < workingZone.Start)
                    removedAtLeft++;
                        
                DisposeFrame(frames[i]);
                frames[i] = null;
                
                if (i==currentIndex)
                    currentIndex = -1;
            }
            
            if (currentIndex >= removedAtLeft)
                currentIndex-=removedAtLeft;
            
            frames.RemoveAll(frame => object.ReferenceEquals(null, frame));
            
            currentIndex = Math.Max(0, currentIndex);
            currentFrame = frames[currentIndex];
            
            UpdateWorkingZone();
        }
        
        /// <summary>
        /// Specify insertion mode for future Add operations.
        /// This can be used to add many images in front of the existing range of cached frames.
        /// </summary>
        public void SetPrepending(bool isPrepending)
        {
            this.isPrepending = isPrepending;
            
            // The insertion index is initialized to 0 and will be updated on each Add.
            insertIndex = 0;
        }
        #endregion

        #region Private Methods
        private bool Contains(long _timestamp)
        {
            return workingZone.Contains(_timestamp);
        }
        private void DisposeFrame(VideoFrame frame)
        {
            if(frameDisposer != null)
            {
                frameDisposer(frame);
            }
            else
            {
                frame.Image.Dispose();
            }
        }
        private void UpdateCurrentFrame()
        {
            if(currentIndex >= 0 && currentIndex < frames.Count)
            {
                currentFrame = frames[currentIndex];
            }
            else
            {
                currentIndex = -1;
                #if DEBUG
                throw new IndexOutOfRangeException();
                #endif
            }
        }

        /// <summary>
        /// Update the internal working zone with the actual timestamps of the first and last frame.
        /// </summary>
        private void UpdateWorkingZone()
        {
            if(frames.Count > 0)
            {
                workingZone = new VideoSection(frames[0].Timestamp, frames[frames.Count - 1].Timestamp);
            }
            else
            {
                workingZone = VideoSection.MakeEmpty();
            }
        }
        #endregion
        
        #region IWorkingZoneFramesContainer implementation
        public ReadOnlyCollection<VideoFrame> Frames {
            get { return frames.AsReadOnly(); }
        }
        public Bitmap Representative {
            get { return frames[(frames.Count / 2)].Image; }
        }
        public void Revert()
        {
            int lastIndex = frames.Count-1;
            int halfIndex = frames.Count/2;
            for(int i = 0; i<halfIndex; i++)
            {
                int opposedIndex = lastIndex - i;
                Bitmap tmp = frames[i].Image;
                frames[i].Image = frames[opposedIndex].Image;
                frames[opposedIndex].Image = tmp;
            }
        }
        #endregion
    }
}
