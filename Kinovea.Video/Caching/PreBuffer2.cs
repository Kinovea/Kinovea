using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kinovea.Video
{
    /// <summary>
    /// Cache for asynchronous reading/decoding.
    /// This cache may be sparse or dense depending on the decoding policy.
    /// </summary>
    public class PreBuffer2 : IDisposable, IVideoFramesContainer
    {
        #region Properties
        public VideoFrame CurrentFrame
        {
            get 
            { 
                return current; 
            }
        }

        public int Count
        {
            get
            {
                lock (sync)
                {
                    return frames.Count;
                }
            }
        }

        public int Capacity
        {
            get { return capacity; }
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
        private readonly object sync = new object();
        private readonly SortedList<long, VideoFrame> frames = new SortedList<long, VideoFrame>();
        private VideoFrame current;
        private bool interruptAdd;
        private int capacity = 32;
        private int framesToKeepBehind = 8; // Retention window behind current.
        private VideoFrameDisposer frameDisposer;
        private double tolerance = 0.0;
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
            // This runs on the decoding thread, with one exception,
            // we decode and store the start of the selection synchronously on the UI thread.
            //---------------------------------

            //log.DebugFormat("Request to add [{0}] to cache. Cached: {1}.", frame.Timestamp, frames.Count);

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
                    log.DebugFormat("Prebuffer waiting to add [{0}]. Cached: {1}.", frame.Timestamp, frames.Count);
                    Monitor.Wait(sync);
                }

                frames.Add(frame.Timestamp, frame);
                log.DebugFormat("Added frame [{0}] to cache. Cached: {1}.", frame.Timestamp, frames.Count);

                Monitor.PulseAll(sync);
            }
        }

        /// <summary>
        /// Unblock any thread waiting in Add().
        /// </summary>
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

        public void WakeWaiters()
        {
            lock (sync)
            {
                Monitor.PulseAll(sync);
            }
        }

        


        /// <summary>
        /// Prepare the cache for a new job.
        /// Look for an acceptable frame near the new job target timestamp.
        /// Acquires it if found within tolerance.
        /// Interrupts add and pulse waiters.
        /// Evicts non-useful frames from the cache.
        /// Returns whether the target was acquired.
        /// </summary>
        public bool PrepareForNewJob(PlayerState state)
        {
            List<VideoFrame> removed = null;
            bool acquired = false;

            lock (sync)
            {
                if (frames.Count == 0)
                {
                    log.Debug("Cache is empty. Nothing to evict.");
                }
                else
                {
                    long target = -1;
                    if (state.Mode == PlayerStateMode.Playback)
                    {
                        target = state.StartPlaybackTimestamp;
                    }
                    else
                    {
                        target = state.ReferenceTimestamp;
                    }

                    VideoFrame closest = FindClosest(target);

                    // If within tolerance, acquire it.
                    if (Math.Abs(closest.Timestamp - target) <= tolerance)
                    {
                        current = closest;
                        acquired = true;
                        // TODO: check sparse vs dense.
                    }
                    else if (target > current.Timestamp)
                    {
                        // Evict anything behind, no retention.
                        // TODO: if the current cache is sparse but the job requires
                        // dense cache, we will have to evict ahead as well.
                        removed = EvictBehind(0);
                    }
                    else
                    {
                        // We will have to seek back anyway so no point keeping anything.
                        removed = EvictPurge();
                    }

                    log.DebugFormat("Job preparation complete. Acquired: {0}. Evicted {1} frames. Cached: {2}. First: [{3}], Curr: [{4}], Last: [{5}]", 
                        acquired,    
                        removed == null ? 0 : removed.Count,
                        frames.Count,
                        frames.Values[0].Timestamp,
                        current.Timestamp,
                        frames.Values[frames.Count-1].Timestamp);
                }


                // Interrupt blocked Add().
                interruptAdd = true;
                Monitor.PulseAll(sync);
            }

            if (removed != null)
            {
                foreach (var frame in removed)
                {
                    DisposeFrame(frame);
                }
            }

            return acquired;
        }

        /// <summary>
        /// Check if the cache contains a frame with the passed timestamp.
        /// The timestamp is considered in request-space and will be 
        /// matched to media-space timestamps with a tolerance.
        /// Does NOT acquire the frame if found.
        /// </summary>
        public bool Contains(long target)
        {
            lock (sync)
            {
                VideoFrame closest = FindClosest(target);
                return Math.Abs(closest.Timestamp - target) <= tolerance;
            }
        }

        /// <summary>
        /// Remove and dispose all frames including the one pointed to by "current".
        /// Pulse any waiters.
        /// </summary>
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
        
        public void Print()
        {
            // Print the entire cache.
            lock (sync)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < frames.Count; i++)
                {
                    VideoFrame frame = frames.Values[i];
                    stringBuilder.AppendFormat("[{0}] ", frame.Timestamp);
                }

                log.DebugFormat("Cache ({0}): {1}", frames.Count, stringBuilder.ToString());
            }
        }
        
        #endregion

        #region Acquisition methods, move current to a different frame.

        /// <summary>
        /// Finds the closest frame to the target timestamp and set it as "current".
        /// Evicts old frames outside of retention window.
        /// Called during playback.
        /// </summary>
        public void AcquireClosest(long timestamp)
        {
            //---------------------------------
            // Runs on the UI thread.
            //---------------------------------
            List<VideoFrame> evictedFrames = null;

            //log.DebugFormat("Acquiring closest frame to [{0}]. Cached: {1}.", timestamp, frames.Count);

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

                //log.DebugFormat("Setting current frame to [{0}].", closest.Timestamp);
                current = closest;

                // Remove old frames from the cache if outside the retention window.
                evictedFrames = EvictBehind(framesToKeepBehind);

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

        /// <summary>
        /// Finds the closest frame to the target and set it as "current" only if 
        /// it is no further than `tolerance` timestamps.
        /// Returns true if the frame was acquired.
        /// </summary>
        public bool TryAcquireClosest(long timestamp, double tolerance)
        {
            //---------------------------------
            // Runs on the UI thread.
            //---------------------------------

            // TODO:
            // eviction strategy may be different than during playback.

            lock (sync)
            {
                if (frames.Count == 0)
                {
                    current = null;
                    return false;
                }

                VideoFrame closest = FindClosest(timestamp);

                if (Math.Abs(closest.Timestamp - timestamp) <= tolerance)
                {
                    current = closest;
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        /// <summary>
        /// Finds the frame immediately next to the passed timestamp.
        /// </summary>
        public bool TryAcquireNext(long timestamp)
        {
            //---------------------------------
            // Runs on the UI thread.
            //---------------------------------

            return false;
        }

        /// <summary>
        /// Finds the frame immediately previous to the passed timestamp.
        /// </summary>
        public bool TryAcquirePrevious(long timestamp)
        {
            //---------------------------------
            // Runs on the UI thread.
            //---------------------------------

            return false;
        }

        #endregion


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
        /// Remove old frames from the cache with a retention window.
        /// Returns the removed frames so they can be disposed outside the lock.
        /// Caller must hold sync.
        /// </summary>
        private List<VideoFrame> EvictBehind(int framesToKeep)
        {
            if (current == null)
                return null;

            int currentIndex = frames.IndexOfKey(current.Timestamp);
            int removeCount = currentIndex - framesToKeep;
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

        /// <summary>
        /// Remove future frames from the cache.
        /// </summary>
        /// <param name="framesToKeep"></param>
        /// <returns></returns>
        private List<VideoFrame> EvictAhead(int framesToKeep)
        {
            if (current == null)
                return null;

            int currentIndex = frames.IndexOfKey(current.Timestamp);
            int removeCount = frames.Count - (currentIndex + 1 + framesToKeep);
            if (removeCount <= 0)
                return null;

            List<VideoFrame> removed = new List<VideoFrame>();
            for (int i = 0; i < removeCount; i++)
            {
                VideoFrame frame = frames.Values[frames.Count - 1];
                frames.RemoveAt(frames.Count - 1);
                removed.Add(frame);
            }

            log.DebugFormat("Evicted {0} frames ahead of current. Cached: {1}.", removeCount, frames.Count);
            return removed;
        }

        /// <summary>
        /// Remove all frames except the one pointed to by "current".
        /// Returns the removed frames so they can be disposed outside the lock.
        /// Caller must hold sync.
        /// </summary>
        private List<VideoFrame> EvictPurge()
        {
            List<VideoFrame> removed = new List<VideoFrame>();
            for (int i = frames.Count - 1; i >= 0; i--)
            {
                VideoFrame frame = frames.Values[i];
                if (!ReferenceEquals(frame, current))
                {
                    frames.RemoveAt(i);
                    removed.Add(frame);
                }
            }

            return removed;
        }

        #endregion
    }
}
