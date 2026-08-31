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

        public bool Empty
        {
            get
            {
                lock (sync)
                {
                    return frames.Count == 0;
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

        /// <summary>
        /// Get or set the far ahead threshold for relating timestamps to the cache.
        /// </summary>
        public double FarAheadThreshold
        {
            get { return farAheadThreshold; }
            set { farAheadThreshold = value; }
        }

        #endregion

        #region Members
        private readonly object sync = new object();
        private readonly SortedList<long, VideoFrame> frames = new SortedList<long, VideoFrame>();
        private VideoFrame current;
        private bool interruptAdd;
        private int capacity = 32;
        private int framesToKeepBehind = 8; // Retention window behind current.
        private double tolerance = 0.0;
        private double farAheadThreshold = 0.0;
        private VideoFrameDisposer frameDisposer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction & destruction
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
                Shutdown();
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Set the closest frame to the target as "current".
        /// Evicts old frames outside of retention window.
        /// </summary>
        public void AcquireClosest(long timestamp)
        {
            //---------------------------------
            // Runs on the UI thread.
            //---------------------------------
            List<VideoFrame> removed = null;

            //log.DebugFormat("Acquiring closest frame to [{0}]. Cached: {1}.", timestamp, frames.Count);

            lock (sync)
            {
                if (frames.Count == 0)
                {
                    current = null;
                    return;
                }

                VideoFrame closest = FindClosest(frames, timestamp);
                if (ReferenceEquals(current, closest))
                    return;

                //log.DebugFormat("Setting current frame to [{0}].", closest.Timestamp);
                current = closest;

                // Remove old frames from the cache if outside the retention window.
                int index = frames.IndexOfKey(closest.Timestamp);
                removed = EvictBehind(index, framesToKeepBehind);

                // Unblock the decoding thread if it was waiting for space in the cache.
                if (removed != null)
                {
                    Monitor.PulseAll(sync);
                }
            }

            if (removed != null)
            {
                foreach (VideoFrame frame in removed)
                {
                    DisposeFrame(frame);
                }
            }
        }

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
                    if (interruptAdd)
                    {
                        return CacheAddResult.Interrupted;
                    }

                    // Test if full before testing for duplicate.
                    // This avoids decoding the same frames multiple times when 
                    // moving backwards at full capacity.
                    if (frames.Count >= capacity)
                    {
                        log.DebugFormat("Waiting to add [{0}]. Cached: {1}.", frame.Timestamp, frames.Count);
                        Monitor.Wait(sync);
                        continue;
                    }

                    // Test duplicate.
                    if (frames.ContainsKey(frame.Timestamp))
                    {
                        log.DebugFormat("Duplicate add request for [{0}]. Cached: {1}.", frame.Timestamp, frames.Count);
                        return CacheAddResult.Duplicate;
                    }

                    break;
                }

                frames.Add(frame.Timestamp, frame);
                log.DebugFormat("Added frame [{0}]. Cached: {1}.", 
                    frame.Timestamp, frames.Count);

                Monitor.PulseAll(sync);
            }

            return CacheAddResult.Added;
        }


        /// <summary>
        /// Special add that does not block and evicts the oldest frame if full.
        /// May still return Duplicate if the frame is already cached.
        /// Never returns Interrupted.
        /// </summary>
        public CacheAddResult ForcedAdd(VideoFrame frame)
        {
            lock (sync)
            {
                if (frames.ContainsKey(frame.Timestamp))
                {
                    log.DebugFormat("Duplicate add request for [{0}]. Cached: {1}.", frame.Timestamp, frames.Count);
                    return CacheAddResult.Duplicate;
                }

                if (frames.Count >= capacity)
                {
                    EvictOne(0);
                }

                frames.Add(frame.Timestamp, frame);
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
                log.Debug("Closing cache for business xxxxxxxx");
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
                log.Debug("Opening cache for business oooooooo");
                interruptAdd = false;
            }
        }


        /// <summary>
        /// Try to acquire the target of the player state.
        /// Evicts old frames outside of retention window of the closest match.
        /// </summary>
        public TryAcquireResult TryAcquire(PlayerState state)
        {
            TryAcquireResult result = TryAcquireResult.Empty();
            List<VideoFrame> removed = null;
            long target = state.ReferenceTimestamp;

            lock (sync)
            {
                if (frames.Count == 0)
                {
                    return new TryAcquireResult(false, -1);
                }

                bool targetAcquired = false;
                long acquiredTimestamp = -1;
                VideoFrame closest = FindClosest(frames, target);
                int index = frames.IndexOfKey(closest.Timestamp);
                if (Math.Abs(closest.Timestamp - target) <= tolerance)
                {
                    current = closest;
                    targetAcquired = true;
                    acquiredTimestamp = closest.Timestamp;
                    log.DebugFormat("TryAcquire: target acquired [~{0}] -> [{1}]", target, acquiredTimestamp);
                }

                // Do the normal eviction of old frames outside the retention window.
                // (Even if the target wasn't acquired).
                removed = EvictBehind(index, framesToKeepBehind);
                
                result = new TryAcquireResult(targetAcquired, acquiredTimestamp);
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
        /// Find where the target timestamp is with regards 
        /// to the cache boundaries, with fuzzy matching.
        /// </summary>
        public CacheTimestampRelation RelateTimestamp(long target)
        {
            lock (sync)
            {
                return RelateTimestamp(frames, target);
            }
        }

        /// <summary>
        /// Remove and dispose all frames including the one pointed to by "current".
        /// Pulse any waiters.
        /// </summary>
        public void Shutdown()
        {
            List<VideoFrame> removed = new List<VideoFrame>();

            lock (sync)
            {
                removed = frames.Values.ToList();
                frames.Clear();
                current = null;

                // Close for business.
                log.Debug("Shutdown - Closing cache for business xxxxxxxx");
                interruptAdd = true;

                // Unblock the decoder if waiting in Add().
                Monitor.PulseAll(sync);
            }

            foreach (var frame in removed)
            {
                DisposeFrame(frame);
            }

            log.Debug("Cache cleared.");
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove all except current.
        /// </summary>
        public void Purge()
        {
            List<VideoFrame> removed = new List<VideoFrame>();

            lock (sync)
            {
                for (int i = frames.Count - 1; i >= 0; i--)
                {
                    VideoFrame frame = frames.Values[i];
                    if (ReferenceEquals(frame, current))
                        continue;

                    frames.RemoveAt(i);
                    removed.Add(frame);
                }

                log.DebugFormat("Cache purge. Removed {0} frames. Cached: {1}.", removed.Count, frames.Count);
            }

            foreach (VideoFrame frame in removed)
            {
                DisposeFrame(frame);
            }
        }

        public void Print()
        {
            // Print the entire cache.
            lock (sync)
            {
                log.DebugFormat("Cache. Current at index [{0}] = [{1}]. Total: {2}.",
                    current == null ? "-" : frames.IndexOfValue(current).ToString(),
                    current == null ? "-" : current.Timestamp.ToString(),
                    frames.Count);

                StringBuilder sb= new StringBuilder();
                for (int i = 0; i < frames.Count; i++)
                {
                    VideoFrame frame = frames.Values[i];
                    if (ReferenceEquals(frame, current))
                    {
                        sb.AppendFormat("[> {0} <] ", frame.Timestamp);
                    }
                    else
                    {
                        sb.AppendFormat("[{0}] ", frame.Timestamp);
                    }
                }

                log.Debug(sb.ToString());
            }
        }
        
        #endregion

        #region Shared
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

        private CacheTimestampRelation RelateTimestamp(SortedList<long, VideoFrame> frames, long target)
        {
            if (frames.Count == 0)
            {
                return CacheTimestampRelation.Empty;
            }

            VideoFrame closest = FindClosest(frames, target);

            if (Math.Abs(closest.Timestamp - target) <= tolerance)
            {
                return CacheTimestampRelation.InBoundsMatch;
            }

            long first = frames.Values[0].Timestamp;
            long last = frames.Values[frames.Count - 1].Timestamp;

            if (target < first)
            {
                return CacheTimestampRelation.Behind;
            }

            if (target > last)
            {
                if (target - last > farAheadThreshold)
                {
                    return CacheTimestampRelation.FarAhead;
                }

                return CacheTimestampRelation.Ahead;
            }

            return CacheTimestampRelation.InBoundsNoMatch;
        }

        #endregion

        #region Eviction

        /// <summary>
        /// Remove old frames from the cache with a retention window.
        /// Protects current.
        /// Caller must hold sync.
        /// Returns the removed frames.
        /// </summary>
        private List<VideoFrame> EvictBehind(int pivotIndex, int framesToKeep)
        {
            if (frames.Count < 2)
                return null;

            // Retains pivot and anything after it.
            // Of items before pivot, retain at most framesToKeep immediately before it.
            // protect current even if it's earlier than retention window.

            int retainFrom = Math.Max(0, pivotIndex - framesToKeep);
            List<VideoFrame> removed = new List<VideoFrame>();

            for (int i = retainFrom - 1; i >= 0; i--)
            {
                VideoFrame frame = frames.Values[i];
                if (ReferenceEquals(frame, current))
                    continue;

                frames.RemoveAt(i);
                removed.Add(frame);
            }

            //log.DebugFormat("EvictBehind evicted {0} frames. Cached: {1}.", removed.Count, frames.Count);
            return removed;
        }

        /// <summary>
        /// Evict one frame at index unless it's the only frame in the cache
        /// or the one pointed by current.
        /// Returns the removed frame.
        /// Caller must hold sync.
        /// </summary>
        private List<VideoFrame> EvictOne(int index)
        {
            if (current == null || frames.Count < 2)
                return null;

            if (current == frames.Values[index])
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
            if (current == null || frames.Count < 2)
                return null;

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
