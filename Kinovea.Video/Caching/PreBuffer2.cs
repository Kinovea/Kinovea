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
        public CacheAddResult Add(VideoFrame frame)
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
                        log.DebugFormat("Duplicate add request for [{0}]. Cached: {1}.", frame.Timestamp, frames.Count);
                        return CacheAddResult.Duplicate;
                    }

                    if (interruptAdd)
                    {
                        return CacheAddResult.Interrupted;
                    }

                    if (frames.Count < capacity)
                    {
                        break;
                    }

                    // Keep waiting.
                    log.DebugFormat("Waiting to add [{0}]. Cached: {1}.", frame.Timestamp, frames.Count);
                    Monitor.Wait(sync);
                }

                frames.Add(frame.Timestamp, frame);
                log.DebugFormat("Added frame [{0}]. Cached: {1}.", frame.Timestamp, frames.Count);

                Monitor.PulseAll(sync);
            }

            return CacheAddResult.Added;
        }

        /// <summary>
        /// Temporarily close the cache for business.
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

        /// <summary>
        /// Reopen the cache for business.
        /// </summary>
        public void ResetInterruptAdd()
        {
            lock (sync)
            {
                interruptAdd = false;
            }
        }


        /// <summary>
        /// Prepare the cache for a new job.
        /// Look for a frame matching the job target timestamp.
        /// Acquires it if found within tolerance.
        /// 
        /// Tries to preserve whatever cached continuity the new job can reuse.
        /// It can return with a full cache if all the frames are useful for 
        /// the new job.
        /// 
        /// Interrupts add and pulses waiters.
        /// Returns whether the target was acquired.
        /// </summary>
        public CachePreparationResult PrepareForNewJob(PlayerState state)
        {
            log.DebugFormat("PrepareForNewJob: target [~{0}].", state.ReferenceTimestamp);

            CachePreparationResult result = null;
            List<VideoFrame> removed = null;
            long target = state.ReferenceTimestamp;

            lock (sync)
            {
                if (frames.Count == 0)
                {
                    result = CachePreparationResult.Empty();
                    log.Debug("PrepareForNewJob: cache is empty.");
                }
                else
                {
                    // Try to acquire the target to pin it.
                    bool targetAcquired = false;
                    int indexAcquired = -1;
                    long acquiredTimestamp = -1;
                    VideoFrame closest = FindClosest(target);
                    if (Math.Abs(closest.Timestamp - target) <= tolerance)
                    {
                        current = closest;
                        targetAcquired = true;
                        indexAcquired = frames.IndexOfKey(closest.Timestamp);
                        acquiredTimestamp = closest.Timestamp;
                        log.DebugFormat("PrepareForNewJob: target acquired [~{0}] -> [{1}]", target, acquiredTimestamp);
                    }

                    // Find the dense section after the target if any.
                    bool denseForward = false;
                    long denseEnd = -1;
                    if (targetAcquired)
                    {
                        // For now we just assume the frames are dense and count until the end.
                        // Later we will keep the previous timestamp in each frame
                        // and we will check for gaps.
                        denseForward = true;
                        denseEnd = frames.Keys[frames.Count - 1];
                        log.DebugFormat("PrepareForNewJob: dense forward from [{0}] to [{1}]", acquiredTimestamp, denseEnd);
                    }

                    // Evict non-useful frames.
                    if (targetAcquired)
                    {
                        // Evict in the past based on retention window.
                        removed = EvictBehind(indexAcquired, framesToKeepBehind);

                        // Evict in the future only if needed?
                        // If we are going into a sparse job we don't need to evict?

                        // 1. If the next job is dense, and we have denseEnd finish
                        // before the end of the cache, we can remove the frames after denseEnd.
                        // In that case the decoder will have to restart the GOP.

                        // 2. If the next job is sparse, we don't care about gaps and the 
                        // decoder can continue from wherever it is.

                    }
                    else
                    {
                        // Depends if next job is sparse or dense.
                        // Depends if the target is ahead or behind.

                        if (target > frames.Keys[frames.Count - 1])
                        {
                            // The target is ahead of the cache.
                            // Conceptually we would like to evict the frames
                            // behind the target up to the retention window.
                            // For simplicity we just keep a retention window worth of frames at the end.
                            int index = frames.Count - 1;
                            removed = EvictBehind(index, framesToKeepBehind);

                            // The decoder will decide to seek or continue decoding 
                            // depending on whether the target is nearby or far ahead.

                            // TODO: if we are going into a dense job we must validate 
                            // that the frames we keep are dense.

                        }
                        else if (target < frames.Keys[0])
                        {
                            // The target is behind the entire cache.
                            // This will for sure result in a seek.
                            removed = EvictPurge();
                        }
                        else
                        {
                            // The target is somewhere in the middle of the cache but
                            // not within tolerance of any frame.

                            // If we are going into dense job, we will have to seek.
                            removed = EvictPurge();

                            // If we are going into a sparse job, we can keep the frames around.
                            // Evict behind based on retention window.
                            // Do not evict ahead.
                            // Unless we are on the first frame ?
                        }
                    }

                    bool full = frames.Count >= capacity;
                    result = new CachePreparationResult(
                        targetAcquired, 
                        acquiredTimestamp, 
                        denseForward, 
                        denseEnd,
                        frames.Values[frames.Count - 1].Timestamp,
                        full);

                    log.DebugFormat("Job preparation complete. Acquired: {0}. Evicted {1} frames. Cached: {2}.", 
                        targetAcquired,    
                        removed == null ? 0 : removed.Count,
                        frames.Count);

                    Print();
                }


                // Interrupt the decoder thread if blocked in Add().
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

            return result;
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
                StringBuilder sb= new StringBuilder();
                sb.AppendFormat("Cache ({0}) @[{1}]: ", 
                    frames.Count,
                    current == null ? "null" : current.Timestamp.ToString());

                for (int i = 0; i < frames.Count; i++)
                {
                    VideoFrame frame = frames.Values[i];
                    sb.AppendFormat("[{0}] ", frame.Timestamp);
                }

                log.Debug(sb.ToString());
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
                int index = frames.IndexOfKey(closest.Timestamp);
                evictedFrames = EvictBehind(index, framesToKeepBehind);

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
        private List<VideoFrame> EvictBehind(int index, int framesToKeep)
        {
            if (current == null)
                return null;

            int removeCount = index - framesToKeep;
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

        private List<VideoFrame> EvictOne(int index)
        {
            if (frames.Count == 0)
                return null;

            VideoFrame frame = frames.Values[index];
            frames.RemoveAt(index);
            log.DebugFormat("Evicted frame [{0}]. Cached: {1}.", frame.Timestamp, frames.Count);
            return new List<VideoFrame>() { frame };
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

            log.DebugFormat("Evicted {0} frames. Cached: {1}.", removed.Count, frames.Count);

            return removed;
        }

        #endregion
    }
}
