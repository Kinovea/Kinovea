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
using System.Drawing;
using Kinovea.Services;

namespace Kinovea.Video
{
    /// <summary>
    /// A cache of the whole working zone.
    /// All methods run in the UI thread.
    /// Acquires are synchronous and instantaneous, no eviction.
    /// </summary>
    public class Cache : IVideoFramesContainer, IWorkingZoneFramesContainer
    {
        #region Properties
        public VideoFrame CurrentFrame 
        {
            get 
            { 
                return current; 
            }
        }
        public VideoSection WorkingZone 
        {
            get 
            {
                return workingZone;
            }
        }
        public bool Empty
        {
            get 
            { 
                return frames.Count == 0; 
            }
        }
        /// <summary>
        /// Get or set the tolerance for matching request-space timestamps built
        /// from pixel location and clock, to media-space timestamps.
        /// This should be set to half average timestamps per frame.
        /// </summary>
        public double Tolerance
        {
            get { return tolerance; }
            set { tolerance = value; }
        }
        #endregion

        #region Members
        private SortedList<long, VideoFrame> frames = new SortedList<long, VideoFrame>();
        private VideoSection workingZone = VideoSection.MakeEmpty();
        private VideoFrame current;
        private double tolerance = 0.0;
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

        public void AcquireClosest(long timestamp)
        {
            if (frames.Count == 0)
            {
                current = null;
                return;
            }

            current = FindClosest(frames, timestamp);
        }

        /// <summary>
        /// Move forward by a number of frames.
        /// </summary>
        public bool MoveBy(int steps)
        {
            if(frames.Count < 1 || steps <= 0 || current == null)
                return false;
            
            int targetIndex = frames.IndexOfValue(current) + steps;
            if(targetIndex > frames.Count - 1)
                return false;
            
            current = frames.Values[targetIndex];
            return true;
        }
        
        /// <summary>
        /// Add a frame to the cache.
        /// This always succeeds, the caller handles the memory budget.
        /// </summary>
        public CacheAddResult Add(VideoFrame frame)
        {
            if (frames.ContainsKey(frame.Timestamp))
            {
                return CacheAddResult.Duplicate;
            }

            frames.Add(frame.Timestamp, frame);
            UpdateWorkingZone();
            return CacheAddResult.Added;
        }

        public void Clear()
        {
            foreach(VideoFrame frame in frames.Values)
            {
                DisposeFrame(frame);
            }
                
            frames.Clear();
            current = null;
            workingZone = VideoSection.MakeEmpty();
            
            log.Debug("Cache cleared.");
        }
        
        /// <summary>
        /// Evict all frames outside the new working zone.
        /// </summary>
        public void ReduceWorkingZone(VideoSection zone)
        {
            for (int i = frames.Count - 1; i >= 0; i--)
            {
                VideoFrame frame = frames.Values[i];

                if (!zone.Contains(frame.Timestamp))
                {
                    DisposeFrame(frame);
                    frames.RemoveAt(i);
                }
            }

            UpdateWorkingZone();

            current = frames.Count > 0 ? frames.Values[0] : null;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update the internal working zone with the actual timestamps of the first and last frame.
        /// </summary>
        private void UpdateWorkingZone()
        {
            if(frames.Count == 0)
            {
                workingZone = VideoSection.MakeEmpty();
            }

            long start = frames.Values[0].Timestamp;
            long end = frames.Values[frames.Count - 1].Timestamp;


            workingZone = new VideoSection(start, end);
        }
        #endregion

        #region IWorkingZoneFramesContainer implementation
        public IReadOnlyList<VideoFrame> Frames 
        {
            get 
            { 
                return (IReadOnlyList<VideoFrame>)frames.Values;
            }
        }
        
        public Bitmap Representative 
        {
            get 
            { 
                if (frames.Count == 0)
                    return null;

                return frames.Values[(frames.Count / 2)].Image;
            }
        }

        #endregion

        #region Shared

        // The following functions are shared between Cache and PreBuffer and 
        // should be factorized in a base class.

        private void DisposeFrame(VideoFrame frame)
        {
            if (frameDisposer != null)
            {
                frameDisposer(frame);
            }
            else
            {
                frame.Image.Dispose();
            }
        }


        /// <summary>
        /// Find the closest frame to the target timestamp.
        /// </summary>
        private VideoFrame FindClosest(SortedList<long, VideoFrame> frames, long timestamp)
        {
            if (frames.Count == 0)
                return null;

            VideoFrame closest;

            int index = LowerBound(frames, timestamp);
            if (index == 0)
            {
                closest = frames.Values[0];
            }
            else if (index == frames.Count)
            {
                closest = frames.Values[frames.Count - 1];
            }
            else
            {
                VideoFrame before = frames.Values[index - 1];
                VideoFrame after = frames.Values[index];
                long deltaBefore = timestamp - before.Timestamp;
                long deltaAfter = after.Timestamp - timestamp;
                closest = deltaBefore <= deltaAfter ? before : after;
            }

            return closest;
        }

        /// <summary>
        /// Find the first frame with a timestamp greater than or equal to the target timestamp.
        /// </summary>
        private int LowerBound(SortedList<long, VideoFrame> frames, long timestamp)
        {
            // Assumes the caller already holds the lock.
            int low = 0;
            int high = frames.Count;

            while (low < high)
            {
                int mid = low + ((high - low) / 2);

                if (frames.Keys[mid] < timestamp)
                    low = mid + 1;
                else
                    high = mid;
            }

            return low;
        }
    
        #endregion    
    }
}
