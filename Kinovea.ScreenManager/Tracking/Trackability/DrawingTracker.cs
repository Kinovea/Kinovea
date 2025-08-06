#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Linq;
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The drawing tracker is responsible for storing the timeline of values for each trackable point in the drawing.
    /// It knows about the drawing it tracks (via ITrackable) and updates it by pushing values directly to it.
    /// The drawing doesn't know about its tracker, it just raises events when the user changes the point manually.
    /// </summary>
    public class DrawingTracker
    {
        #region Properties

        /// <summary>
        /// Returns true if the drawing is currently actively tracked.
        /// A non tracked drawing can still have entries in its timeline. 
        /// The position of the point is always the closest entry from the timeline, or a special non-tracked value if the timeline is empty.
        /// </summary>
        public bool IsTracking
        {
            get { return isTracking; }
        }

        /// <summary>
        /// Returns true if there are entries in the timelines.
        /// </summary>
        public bool HasData
        {
            get { return trackablePoints.Count > 0 && trackablePoints.First().Value.Timeline.HasData(); }
        }

        public Guid ID
        {
            get { return drawingId; }
        }

        public bool Assigned
        {
            get { return assigned; }
        }
        public int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= isTracking.GetHashCode();
                foreach (TrackablePoint point in trackablePoints.Values)
                    hash ^= point.ContentHash;

                return hash;
            }
        }
        public bool Empty
        {
            get 
            {
                foreach (TrackablePoint point in trackablePoints.Values)
                    if (!point.Empty)
                        return false;

                return true;
            }
        }

        public Dictionary<string, TrackablePoint> TrackablePoints
        {
            get
            {
                return trackablePoints;
            }
        }
        #endregion

        private ITrackable drawing;
        private Guid drawingId;
        private bool isTracking;
        private bool assigned;
        private long trackingTimestamp;
        private bool isCameraTracking;
        private TrackingParameters parameters;
        private Dictionary<string, TrackablePoint> trackablePoints = new Dictionary<string, TrackablePoint>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Create a tracker for a trackable drawing and register all its points.
        /// </summary>
        public DrawingTracker(ITrackable drawing, TrackingContext context, TrackingParameters parameters)
        {
            this.drawing = drawing;
            this.drawingId = drawing.Id;
            this.parameters = parameters;
           
            foreach(KeyValuePair<string, PointF> pair in drawing.GetTrackablePoints())
                trackablePoints.Add(pair.Key, new TrackablePoint(context, parameters, pair.Value));
            
            drawing.TrackablePointMoved += drawing_TrackablePointMoved;
            assigned = true;
        }

        /// <summary>
        /// Import data from the outside.
        /// This is used by non-KVA importers.
        /// </summary>
        public DrawingTracker(ITrackable drawing, Dictionary<string, TrackablePoint> trackablePoints)
        {
            this.drawing = drawing;
            this.drawingId = drawing.Id;
            this.parameters = PreferencesManager.PlayerPreferences.TrackingParameters.Clone();
            this.trackablePoints = trackablePoints;
            drawing.TrackablePointMoved += drawing_TrackablePointMoved;
            assigned = true;
        }

        /// <summary>
        /// Register the drawing this tracker is responsible for.
        /// </summary>
        public void Assign(ITrackable drawing)
        {
            if (drawing.Id != drawingId)
                return;

            this.drawing = drawing;
            this.drawing.TrackablePointMoved += drawing_TrackablePointMoved;
            AfterToggleTracking();
            assigned = true;
        }

        /// <summary>
        /// Add a new point to an existing tracker.
        /// This is used for drawings that have a dynamic list of trackable points like polyline.
        /// </summary>
        public void AddPoint(TrackingContext context, TrackingParameters parameters, string key, PointF value)
        {
            // Some drawings like polyline have a dynamic list of trackable points.
            trackablePoints.Add(key, new TrackablePoint(context, parameters, value));
        }

        /// <summary>
        /// Remove a point from an existing tracker.
        /// </summary>
        public void RemovePoint(string key)
        {
            trackablePoints.Remove(key);
        }

        /// <summary>
        /// Update the tracker with a new version of the drawing object.
        /// This is used when the drawing is modified by other processes, not by the user nor tracking.
        /// For example when merging a KVA file and the drawing already existed we swap it with 
        /// the one coming from the new KVA.
        /// The drawing object itself changed so we re-init everything.
        /// </summary>
        public void Reinitialize(ITrackable drawing, TrackingContext context, TrackingParameters parameters)
        {
            this.drawing.TrackablePointMoved -= drawing_TrackablePointMoved;

            this.drawing = drawing;
            this.drawingId = drawing.Id;
            this.parameters = parameters;

            trackablePoints.Clear();
            foreach (KeyValuePair<string, PointF> pair in drawing.GetTrackablePoints())
                trackablePoints.Add(pair.Key, new TrackablePoint(context, parameters, pair.Value));

            drawing.TrackablePointMoved += drawing_TrackablePointMoved;
            assigned = true;
        }

        /// <summary>
        /// Track each trackable points in the current image, or use existing tracking data.
        /// Update the point coordinate in the drawing.
        /// </summary>
        public void Track(TrackingContext context, CameraTransformer cameraTransformer)
        {
            // Backup the timestamp in case we move a point manually later.
            trackingTimestamp = context.Time;
            isCameraTracking = cameraTransformer.Initialized;

            // This is where we would spawn new threads for each tracking.
            Dictionary<string, bool> insertionMap = new Dictionary<string, bool>();
            bool atLeastOneInserted = false;
            foreach(KeyValuePair<string, TrackablePoint> pair in trackablePoints)
            {
                bool inserted = pair.Value.Track(context);
                PointF p = pair.Value.CameraTrack(context, cameraTransformer, drawing.ReferenceTimestamp);
                drawing.SetTrackablePointValue(pair.Key, p, pair.Value.TimeDifference);

                insertionMap[pair.Key] = inserted;
                if (inserted)
                    atLeastOneInserted = true;
            }

            if (atLeastOneInserted)
                FixTimelineSync(insertionMap);
        }
        
        /// <summary>
        /// Set this drawing to actively tracking or not.
        /// </summary>
        public void ToggleTracking()
        {
            isTracking = !isTracking;
            AfterToggleTracking();
        }

        /// <summary>
        /// Returns the position of the point nearest to that time.
        /// This is used by linear kinematics.
        /// </summary>
        public PointF GetLocation(string key, long time)
        {
            if (!trackablePoints.ContainsKey(key))
                return PointF.Empty;

            return trackablePoints[key].GetLocation(time);
        }

        /// <summary>
        /// Returns the position of the point suitable for storage.
        /// This is the location at the reference time.
        /// </summary>
        public PointF GetReferenceValue(string key)
        {
            if (!trackablePoints.ContainsKey(key))
                return PointF.Empty;

            return trackablePoints[key].ReferenceValue;
        }

        public void Reset()
        {
            foreach (TrackablePoint trackablePoint in trackablePoints.Values)
                trackablePoint.Reset();
        }

        public void Dispose()
        {
            foreach (TrackablePoint trackablePoint in trackablePoints.Values)
                trackablePoint.Reset();
                        
            if (drawing != null)
                drawing.TrackablePointMoved -= drawing_TrackablePointMoved;
        }

        /// <summary>
        /// Returns a single array of all the timestamps.
        /// </summary>
        public List<long> CollectTimeVector()
        {
            // Internally we keep different time vectors for each trackable point but they are always in sync.
            if (trackablePoints == null || trackablePoints.Count == 0)
                return null;
            
            KeyValuePair<string, TrackablePoint> pair = trackablePoints.First();
            Timeline<TrackingTemplate> timeline = pair.Value.Timeline;
            if (!timeline.HasData() || timeline.Times == null)
                return null;

            return new List<long>(timeline.Times);
        }

        /// <summary>
        /// Returns a dictionary of the trackable points mapping names to list of 2D pixel coordinates.
        /// </summary>
        public Dictionary<string, List<PointF>> CollectData()
        {
            Dictionary<string, List<PointF>> data = new Dictionary<string, List<PointF>>();
            foreach (KeyValuePair<string, TrackablePoint> pair in trackablePoints)
            {
                string key = pair.Key;
                List<PointF> values = pair.Value.Timeline.Enumerate().Select(frame => frame.Location).ToList();
                data.Add(key, values);
            }

            return data;
        }

        public void WriteXml(XmlWriter w)
        {
            foreach (KeyValuePair<string, TrackablePoint> pair in trackablePoints)
            {
                w.WriteStartElement("TrackablePoint");
                w.WriteAttributeString("key", pair.Key.ToString());
                pair.Value.WriteXml(w);
                w.WriteEndElement();
            }
        }

        public DrawingTracker(XmlReader r, PointF scale, TimestampMapper timeMapper)
        {
            bool isEmpty = r.IsEmptyElement;

            if (r.MoveToAttribute("id"))
                drawingId = new Guid(r.ReadContentAsString());

            isTracking = false;

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "TrackablePoint":
                        ParseTrackablePoint(r, scale, timeMapper);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        break;
                }
            }

            if (!isEmpty)
                r.ReadEndElement();
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}), hasData:{2}, tracking:{3}", drawing.Name, drawing.Id, HasData, IsTracking);
        }

        #region Private

        private void AfterToggleTracking()
        {
            Dictionary<string, bool> insertionMap = new Dictionary<string, bool>();
            bool atLeastOneInserted = false;

            foreach (KeyValuePair<string, TrackablePoint> pair in trackablePoints)
            {
                bool inserted = pair.Value.SetTracking(isTracking);
                insertionMap[pair.Key] = inserted;
                if (inserted)
                    atLeastOneInserted = true;
            }

            if (atLeastOneInserted)
                FixTimelineSync(insertionMap);
        }

        /// <summary>
        /// For drawings containing multiple trackable points, make sure that if any one of them 
        /// successfully tracked by template matching, we have a corresponding data point in the timeline 
        /// of the other trackable points. 
        /// Contract: caller must gather that at least one point has tracked, and only call in here if so.
        /// </summary>
        private void FixTimelineSync(Dictionary<string, bool> insertionMap)
        {
            foreach (KeyValuePair<string, TrackablePoint> pair in trackablePoints)
            {
                if (insertionMap[pair.Key])
                    continue;

                // Force insert using closest existing value.
                pair.Value.ForceInsertClosestLocation();
                drawing.SetTrackablePointValue(pair.Key, pair.Value.CurrentValue, pair.Value.TimeDifference);
            }
        }

        private void drawing_TrackablePointMoved(object sender, TrackablePointMovedEventArgs e)
        {
            if (!trackablePoints.ContainsKey(e.PointName))
                throw new ArgumentException("This point is not bound.");

            bool inserted = trackablePoints[e.PointName].SetUserValue(e.Position);

            // This is called when we manually move a point even though the object is not in tracking mode.
            // In this case we may have added a new entry in the timeline. 
            // We must ensure the other points have a data point at that time too. 
            // This is also called programmatically when the origin of the coordinate system is updated for a moving system.
            if (inserted)
            {
                foreach (KeyValuePair<string, TrackablePoint> pair in trackablePoints)
                {
                    if (pair.Key == e.PointName)
                        continue;

                    // Force insert using closest existing value.
                    pair.Value.ForceInsertClosestLocation();
                    drawing.SetTrackablePointValue(pair.Key, pair.Value.CurrentValue, pair.Value.TimeDifference);
                }
            }

            // Update the object reference timestamp, for camera tracking.
            // From now on when we paint this object it should be relative to this frame.
            if (drawing.ReferenceTimestamp != trackingTimestamp)
            {
                drawing.ReferenceTimestamp = trackingTimestamp;

                // This means all the points must be commited to their current location on this frame.
                var sourcePoints = drawing.GetTrackablePoints();
                foreach (KeyValuePair<string, PointF> sp in sourcePoints)
                {
                    if (trackablePoints.ContainsKey(sp.Key))
                    {
                        trackablePoints[sp.Key].CommitNonTrackingValue(sp.Value);
                    }
                }
            }
        }

        private void ParseTrackablePoint(XmlReader r, PointF scale, TimestampMapper timeMapper)
        {
            string key = "";

            bool isEmpty = r.IsEmptyElement;

            if (r.MoveToAttribute("key"))
                key = r.ReadContentAsString();

            TrackablePoint point = new TrackablePoint(r, scale, timeMapper);
            trackablePoints.Add(key, point);
        }


        #endregion
    }
}
