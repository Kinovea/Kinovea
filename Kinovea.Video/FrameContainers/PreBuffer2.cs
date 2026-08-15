using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kinovea.Video
{
    /// <summary>
    /// Cache for asynchronous reading/decoding.
    /// </summary>
    public class PreBuffer2 : IDisposable, IVideoFramesContainer
    {
        #region Properties
        public VideoFrame CurrentFrame
        {
            get { return current; }
        }
        #endregion

        #region Members
        private readonly object sync = new object();
        private readonly SortedList<long, VideoFrame> frames = new SortedList<long, VideoFrame>();
        private VideoFrame current;
        private int capacity = 32;
        private bool interruptAdd;
        private VideoFrameDisposer frameDisposer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction & Disposal
        public PreBuffer2() { }
        public PreBuffer2(VideoFrameDisposer frameDisposer)
        {
            this.frameDisposer = frameDisposer;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~PreBuffer2()
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

        /// <summary>
        /// Add one frame to the cache.
        /// May block until space becomes available or cancellation.
        /// If the frame is already cached or we are cancelled, the frame is disposed.
        /// </summary>
        public void Add(VideoFrame frame)
        {
            //---------------------------------
            // This runs on the decoding thread.
            //---------------------------------

            log.DebugFormat("Request to add [{0}] to cache. Cached: {1}", frame.Timestamp, frames.Count);

            lock (sync)
            {

                // Block until the cache has some room to spare or we are cancelled.
                while (true)
                {
                    if (frames.ContainsKey(frame.Timestamp))
                    {
                        frameDisposer(frame);
                        return;
                    }

                    if (interruptAdd)
                    {
                        frameDisposer(frame);
                        return;
                    }

                    if (frames.Count < capacity)
                    {
                        break;
                    }

                    // Keep waiting.
                    log.DebugFormat("Cache full. Waiting to add [{0}]. Cached: {1}", frame.Timestamp, frames.Count);
                    Monitor.Wait(sync);
                }

                frames.Add(frame.Timestamp, frame);
                log.DebugFormat("Added frame [{0}] to cache. Cached: {1}", frame.Timestamp, frames.Count);

                Monitor.PulseAll(sync);
            }
        }


        public void InterruptAdd()
        {
            lock (sync)
            {
                log.Debug("Interrupting Add().");
                interruptAdd = true;
                Monitor.PulseAll(sync);
            }
        }

        public void ResetInterruptAdd()
        {
            lock (sync)
            {
                interruptAdd = false;
            }
        }

        /// <summary>
        /// Find the closest frame to the target timestamp and set it as "current".
        /// </summary>
        public void AcquireClosest(long timestamp)
        {
            //---------------------------------
            // Runs on the UI thread.
            //---------------------------------
            VideoFrame frameToDispose = null;

            log.DebugFormat("Acquiring closest frame to [{0}]. Cached: {1}", timestamp, frames.Count);

            lock (sync)
            {
                if (frames.Count == 0)
                {
                    current = null;
                    return;
                }

                // Get frame at timestamp >= to the target timestamp.
                int index = LowerBound(timestamp);
                VideoFrame closest;
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

                if (ReferenceEquals(current, closest))
                    return;

                log.DebugFormat("Setting current frame to [{0}].", closest.Timestamp);
                current = closest;

                // If the cache is full this is an opportunity to evict one less useful frame
                // and wake the decoding thread.
                if (frames.Count >= capacity)
                {
                    frameToDispose = EvictFurthestFromCurrent();
                }

                if (frameToDispose != null)
                {
                    Monitor.PulseAll(sync);
                }
            }

            if (frameToDispose != null)
            {
                DisposeFrame(frameToDispose);
            }
        }

        public void WakeWaiters()
        {
            lock (sync)
            {
                Monitor.PulseAll(sync);
            }
        }
        #endregion


        public void Clear()
        {
            VideoFrame[] framesToDispose;

            log.Debug("Clearing cache.");

            lock (sync)
            {
                framesToDispose = frames.Values.ToArray();
                frames.Clear();
                current = null;

                Monitor.PulseAll(sync);
            }
            
            foreach (var frame in framesToDispose)
            {
                DisposeFrame(frame);
            }
        }

        #region Private methods
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
        /// Find the first frame with a timestamp greater than or equal to the target timestamp.
        /// </summary>
        private int LowerBound(long timestamp)
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
        
        /// <summary>
        /// Remove one frame from the cache (but not current).
        /// Returns the removed frame so it can be disposed outside the lock.
        /// Caller must hold sync.
        /// </summary>
        private VideoFrame EvictFurthestFromCurrent()
        {
            // Bail out if empty.
            if (frames.Count <= 1 || current == null)
                return null;

            // Since we keep them sorted the furthest is always either the first or last.
            int firstIndex = 0;
            int lastIndex = frames.Count - 1;
            VideoFrame first = frames.Values[firstIndex];
            VideoFrame last = frames.Values[lastIndex];
            long deltaFirst = Math.Abs(current.Timestamp - first.Timestamp);
            long deltaLast = Math.Abs(last.Timestamp - current.Timestamp);
            int removeIndex = deltaFirst >= deltaLast ? firstIndex : lastIndex;
            
            log.DebugFormat("Cache: removing frame [{0}] at index {1}.", frames.Values[removeIndex].Timestamp, removeIndex);

            VideoFrame removed = frames.Values[removeIndex];
            frames.RemoveAt(removeIndex);
            return removed;
        }
        
        #endregion
    }
}
