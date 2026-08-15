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
        private bool interruptAdd;
        private int capacity = 32;
        private int framesToKeepBehind = 8; // Retention window behind current.
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

            log.DebugFormat("Request to add [{0}] to cache. Cached: {1}.", frame.Timestamp, frames.Count);

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
                    log.DebugFormat("Cache full. Waiting to add [{0}]. Cached: {1}.", frame.Timestamp, frames.Count);
                    Monitor.Wait(sync);
                }

                frames.Add(frame.Timestamp, frame);
                log.DebugFormat("Added frame [{0}] to cache. Cached: {1}.", frame.Timestamp, frames.Count);

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
            List<VideoFrame> evictedFrames = null;

            log.DebugFormat("Acquiring closest frame to [{0}]. Cached: {1}.", timestamp, frames.Count);

            lock (sync)
            {
                if (frames.Count == 0)
                {
                    current = null;
                    return;
                }

                VideoFrame closest = FindClosest(timestamp);
                if (ReferenceEquals(current, closest))
                    return;

                log.DebugFormat("Setting current frame to [{0}].", closest.Timestamp);
                current = closest;

                // Remove old frames from the cache.
                evictedFrames = EvictFramesBehindCurrent();

                // Unblock the decoding thread if it was waiting for space in the cache.
                if (evictedFrames != null)
                {
                    Monitor.PulseAll(sync);
                }
            }

            if (evictedFrames != null)
            {
                foreach (VideoFrame frame in evictedFrames)
                {
                    DisposeFrame(frame);
                }
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
        /// Find the closest frame to the target timestamp.
        /// </summary>
        private VideoFrame FindClosest(long timestamp)
        {
            if (frames.Count == 0)
                throw new InvalidProgramException();

            VideoFrame closest;

            int index = LowerBound(timestamp);
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
        /// Remove one or more old frames from the cache.
        /// Returns the removed frames so they can be disposed outside the lock.
        /// Caller must hold sync.
        /// </summary>
        private List<VideoFrame> EvictFramesBehindCurrent()
        {
            if (current == null)
                return null;

            int currentIndex = frames.IndexOfKey(current.Timestamp);
            int removeCount = currentIndex - framesToKeepBehind;
            if (removeCount <= 0)
                return null;

            List<VideoFrame> removed = new List<VideoFrame>();
            for (int i = 0; i < removeCount; i++)
            {
                VideoFrame frame = frames.Values[0];
                frames.RemoveAt(0);
                removed.Add(frame);
            }

            log.DebugFormat("Evicted {0} frames behind current. Cached: {1}.", removeCount, frames.Count);

            return removed;
        }

        #endregion
    }
}
