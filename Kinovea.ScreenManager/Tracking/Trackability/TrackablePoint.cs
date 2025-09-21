using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Represents a point inside a drawing that can be impacted by camera tracking.
    /// These drawings have 3 modes they can be in:
    /// - non tracking: object tracking was never turned on there is no camera motion data.
    /// - camera tracking: object tracking was never turned on and we have camera motion data.
    /// - object tracking: each point is backed by a track object.
    /// 
    /// This class deals with non-tracking and camera tracking.
    /// When the object is tracked we handle it at the DrawingTracker level by delegating 
    /// to the underlying track objects. The tracks have their own way to deal with camera tracking.
    /// 
    /// KVA serialization: because the drawings core coordinates are constantly modified 
    /// by the tracking system, the serialization must use the reference value stored here which is stable.
    /// </summary>
    public class TrackablePoint
    {
        #region Properties
        
        /// <summary>
        /// Coordinates of the point at the reference timestamp.
        /// Suitable for storage in KVA fragments.
        /// </summary>
        public PointF ReferenceValue
        {
            get { return referenceValue; }
        }

        /// <summary>
        /// Timestamp at which the reference value was created.
        /// </summary>
        public long ReferenceTimestamp
        {
            get { return referenceTimestamp; }
        }

        /// <summary>
        /// Content hash for the trackable point.
        /// This is how the point stays stable for serialization despite 
        /// its core coordinates being modified by object or camera tracking.
        /// </summary>
        public int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= referenceValue.GetHashCode();
                hash ^= referenceTimestamp.GetHashCode();
                return hash;
            }
        }
        #endregion

        # region Members 
        private long referenceTimestamp;
        private PointF referenceValue;
        private Dictionary<long, PointF> cameraTrackCache = new Dictionary<long, PointF>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public TrackablePoint(long timestamp, PointF value)
        {
            referenceTimestamp = timestamp;
            referenceValue = value;
        }

        /// <summary>
        /// Called when the user moves the trackable point or the whole drawing on a frame.
        /// This is now considered the reference value.
        /// </summary>
        public void SetReferenceValue(long timestamp, PointF value)
        {
            referenceTimestamp = timestamp;
            referenceValue = value;
            cameraTrackCache.Clear();
        }

        /// <summary>
        /// Return the coordinate of the point for the current timestamp, according to 
        /// camera tracking if it's active, or the non-tracking value otherwise.
        /// </summary>
        public PointF CameraTrack(long currentTimestamp, CameraTransformer cameraTransformer)
        {
            if (!cameraTransformer.Initialized)
            {
                return referenceValue;
            }

            if (cameraTrackCache.ContainsKey(currentTimestamp))
            {
                return cameraTrackCache[currentTimestamp];
            }

            PointF p = cameraTransformer.Transform(referenceTimestamp, currentTimestamp, referenceValue);
            cameraTrackCache[currentTimestamp] = p;
            return p;
        }
    }
}
