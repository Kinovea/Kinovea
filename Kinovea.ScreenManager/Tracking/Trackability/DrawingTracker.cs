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
using System.Web.UI;
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The drawing tracker is responsible for updating the position of trackable points in a drawing.
    /// All trackable drawings must have a DrawingTracker, even if they are never actively tracked.
    /// This handles both object tracking, camera tracking and no tracking.
    /// The drawing itself doesn't know about its tracker or bound tracks.
    /// </summary>
    public class DrawingTracker
    {
        #region Properties

        /// <summary>
        /// The id of the drawing this tracker is responsible for.
        /// </summary>
        public Guid Id
        {
            get { return drawingId; }
        }

        /// <summary>
        /// Whether the tracker is assigned to a drawing.
        /// </summary>
        public bool Assigned
        {
            get { return assigned; }
        }

        /// <summary>
        /// Whether object tracking was ever turned on for this drawing.
        /// If true it should have a list of track ids associated with the trackable points.
        /// </summary>
        public bool IsObjectTrackingInitialized
        {
            get { return isObjectTrackingInitialized; }
        }

        /// <summary>
        /// Returns true if the drawing is currently actively tracked.
        /// </summary>
        public bool IsCurrentlyTracking
        {
            get { return isCurrentlyTracking; }
        }

        #endregion

        #region Members
        private ITrackable drawing;
        private Guid drawingId;
        private bool assigned;

        // Tracking mode
        private bool isObjectTrackingInitialized; 
        private bool isCurrentlyTracking;
        private long trackingTimestamp;

        // Mapping between trackable points and track objects, used for object tacking.
        private Dictionary<string, DrawingTrack> mapPointToTrack = new Dictionary<string, DrawingTrack>();
        private Dictionary<Guid, string> mapTrackIdToPoint = new Dictionary<Guid, string>();
        // Mapping used for non-tracking or camera tracking.
        private Dictionary<string, TrackablePoint2> trackablePoints2 = new Dictionary<string, TrackablePoint2>();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Creation and Assignation

        /// <summary>
        /// Create a tracker for a trackable drawing and register all its points.
        /// This ctor is called when we first add the drawing interactively (not when loaded via KVA).
        /// All trackable drawings must have a tracker assigned even if they are never actively tracked.
        /// </summary>
        public DrawingTracker(ITrackable drawing, long timestamp)
        {
            this.drawing = drawing;
            this.drawingId = drawing.Id;
            drawing.TrackablePointMoved += drawing_TrackablePointMoved;
            assigned = true;

            trackablePoints2.Clear();
            foreach (KeyValuePair<string, PointF> pair in drawing.GetTrackablePoints())
            {
                trackablePoints2.Add(pair.Key, new TrackablePoint2(timestamp, pair.Value));
            }
        }

        /// <summary>
        /// Import data from the outside.
        /// This is used by non-KVA importers.
        /// </summary>
        public DrawingTracker(ITrackable drawing, Dictionary<string, DrawingTrack> tracks)
            : this(drawing, drawing.ReferenceTimestamp)
        {
            InitializeTracking(tracks);
            isCurrentlyTracking = false;
        }

        public void Dispose()
        {
            // Refactoring 2025-09: since we don't use the trackable points with their 
            // own tracker inside there is no native data held here anymore.

            if (drawing != null)
                drawing.TrackablePointMoved -= drawing_TrackablePointMoved;
        }

        /// <summary>
        /// Assign the drawing this tracker is responsible for.
        /// When we load the KVA, the trackers and drawings are loaded independently.
        /// Before this point the tracker only knew the drawing id, not the drawing object itself.
        /// Returns a list of track ids that were bound to the drawing points.
        /// </summary>
        public List<Guid> Assign(ITrackable drawing, List<DrawingTrack> allTracks)
        {
            List<Guid> bound = new List<Guid>();
            if (drawing.Id != drawingId)
            {
                log.Error("Mismatched drawing Id during Assign.");
                return bound;
            }

            this.drawing = drawing;
            this.drawing.TrackablePointMoved += drawing_TrackablePointMoved;
            assigned = true;

            trackablePoints2.Clear();
            foreach (KeyValuePair<string, PointF> pair in drawing.GetTrackablePoints())
            {
                trackablePoints2.Add(pair.Key, new TrackablePoint2(drawing.ReferenceTimestamp, pair.Value));
            }

            if (mapTrackIdToPoint.Count > 0)
            {
                if (allTracks == null)
                {
                    log.DebugFormat("Tracks not defined.");
                }

                // Rebuild the map of point to track.
                mapPointToTrack.Clear();
                foreach (var pair in mapTrackIdToPoint)
                {
                    DrawingTrack track = allTracks.FirstOrDefault(t => t.Id == pair.Key);
                    if (track != null)
                    {
                        mapPointToTrack[pair.Value] = track;
                        track.PointMoving += drawingTrack_PointMoved;
                        bound.Add(track.Id);
                    }
                    else
                    {
                        log.Error("DrawingTracker: could not find track with id " + pair.Key);
                    }
                }

                if (mapPointToTrack.Count != mapTrackIdToPoint.Count)
                {
                    log.Error("DrawingTracker: some tracks were not found for the drawing.");
                    mapPointToTrack.Clear();
                    mapTrackIdToPoint.Clear();
                    bound.Clear();
                }

                isObjectTrackingInitialized = true;
            }

            return bound;
        }

        /// <summary>
        /// Forget which drawing this tracker was assigned to.
        /// Only called when re-assigning a different drawing.
        /// Reset everything.
        /// </summary>
        public void Unassign()
        {
            if (!assigned)
                return;

            assigned = false;
            this.drawing.TrackablePointMoved -= drawing_TrackablePointMoved;
            isCurrentlyTracking = false;
            foreach (var track in mapPointToTrack.Values)
            {
                track.PointMoving -= drawingTrack_PointMoved;
            }
            trackablePoints2.Clear();
        }

        /// <summary>
        /// Update the tracker with a new version of the drawing object.
        /// This is used when the drawing is modified by other processes, not by the user nor tracking.
        /// For example when merging a KVA file and the drawing already existed we swap it with 
        /// the one coming from the new KVA (ex: coordinate system).
        /// The drawing object itself changed so we re-init everything.
        /// </summary>
        public void Reassign(ITrackable drawing, long timestamp)
        {
            Unassign();

            this.drawing = drawing;
            this.drawingId = drawing.Id;
            drawing.TrackablePointMoved += drawing_TrackablePointMoved;
            assigned = true;

            trackablePoints2.Clear();
            foreach (KeyValuePair<string, PointF> pair in drawing.GetTrackablePoints())
            {
                trackablePoints2.Add(pair.Key, new TrackablePoint2(timestamp, pair.Value));
            }

            // We keep the association with the tracks if any.
            // So we don't clear the mapPointToTrack and mapTrackIdToPoint.
        }

        /// <summary>
        /// Add a new point to an existing tracker.
        /// Only used during the initial creation of a polyline object.
        /// </summary>
        public void AddPoint(string key, long timestamp, PointF value)
        {
            trackablePoints2.Add(key, new TrackablePoint2(timestamp, value));
        }

        /// <summary>
        /// Remove a point from an existing tracker.
        /// Only used during the initial creation of a polyline object.
        /// </summary>
        public void RemovePoint(string key)
        {
            trackablePoints2.Remove(key);
        }
        #endregion

        #region Start/Stop object tracking

        /// <summary>
        /// Turn on tracking for the first time.
        /// Takes the list of track ids assigned to each point.
        /// </summary>
        public void InitializeTracking(Dictionary<string, DrawingTrack> tracks)
        {
            // Keep a map of point to track to open/close them when the drawing start/stop tracking.
            mapPointToTrack = tracks;
            isObjectTrackingInitialized = true;
            isCurrentlyTracking = true;

            // Build the reverse map to quickly update points after a track step.
            mapTrackIdToPoint.Clear();
            foreach (KeyValuePair<string, DrawingTrack> pair in tracks)
                mapTrackIdToPoint[pair.Value.Id] = pair.Key;

            // Listen to point moved events from the tracks.
            foreach (var track in tracks.Values)
            {
                track.PointMoving += drawingTrack_PointMoved;
            }

            // From now on we are never going to inject the non-tracking
            // or camera tracking values into the drawing, but we still need them
            // around to store a stable value for KVA serialization.
            // Otherwise the value in the drawing is constantly changing from frame to frame.


            // TODO: for polyline we'll need a way to initialize just the new point
            // and de-initialize removed points.
        }

        /// <summary>
        /// Open/Close this drawing for object tracking.
        /// </summary>
        public void ToggleTracking()
        {
            if (!isObjectTrackingInitialized)
            {
                log.Error("Toggling object tracking on unitialized drawing tracker");
                return;
            }

            isCurrentlyTracking = !isCurrentlyTracking;

            foreach (var pair in mapPointToTrack)
            {
                if (isCurrentlyTracking)
                {
                    pair.Value.StartTracking();
                }
                else
                {
                    pair.Value.StopTracking();
                }
            }
        }
        #endregion

        #region Tracking step (on any frame)

        /// <summary>
        /// Called once per frame before object tracking and camera tracking steps.
        /// </summary>
        public void BeforeTrackingStep(long timestamp)
        {
            // We are guaranteed to come here at least once per frame, after video decode.
            // Keep track of the current time in case the user moves a point manually.
            // We'll need this to establish the reference point for camera tracking.
            // We also need this for object tracking to calculate how far we are from
            // the tracked segment.
            trackingTimestamp = timestamp;
        }

        /// <summary>
        /// Object tracking step (once per frame for each trackable point).
        /// A track referenced by this drawing has finished its tracking step.
        /// Synchronize the corresponding trackable point with the value in the track.
        /// </summary>
        public void ObjectTrackingStep(DrawingTrack track, TimedPoint tp)
        {
            //--------------------------------------------------------
            // Note: the "tracking step" is not only for active tracking,
            // it runs for every frame, and is where we synchronize the trackable points 
            // with the underlying track data.
            //
            // Threading:
            // If at least one track is open we run inside a parallel-for and
            // come here in the tracking thread of a particular track.
            // Hence multiple tracks of to the same drawing may come here at the same time.
            //--------------------------------------------------------

            if (!isObjectTrackingInitialized)
            {
                // Implementation error.
                // If this drawing is not using object tracking we shouldn't have a track associated with it.
                log.Error("DrawingTracker: object tracking is not initialized.");
                return;
            }

            if (!mapTrackIdToPoint.ContainsKey(track.Id))
            {
                // Implementation error.
                // If we have activated object tracking we should definitely know about the underlying track.
                log.Error("DrawingTracker: track is not bound to any of our points.");
                return;
            }

            string key = mapTrackIdToPoint[track.Id];

            if (!isCurrentlyTracking)
            {
                if (tp != null)
                {
                    drawing.SetTrackablePointValue(key, tp.Point, tp.T - trackingTimestamp);
                }
                else
                {
                    // We shouldn't get here.
                    // While not open for tracking the track should return whatever point is closest
                    // and it should always have at least one point from creation.
                    log.Error("DrawingTracker: No track data.");
                }
                
                return;
            }

            if (tp == null)
            {
                // We are tracking but the tracking failed for some reason.
                // Fallback to the closest point in the track.
                // One case of this is stepping backward before the underlying tracks exist.
                TimedPoint tp2 = track.GetTimedPoint(trackingTimestamp);
                if (tp2 != null)
                {
                    drawing.SetTrackablePointValue(key, tp2.Point, tp2.T - trackingTimestamp);
                }
                else
                {
                    log.Error("DrawingTracker: No track data.");
                }
                
                return;
            }
            else
            {
                // Tracking succeeded or we are on a frame where we already had tracking data.
                drawing.SetTrackablePointValue(key, tp.Point, 0);
            }
        }

        /// <summary>
        /// Camera tracking step (called once per frame, only if camera tracking is active).
        /// If the drawing is not using object tracking and we have camera motion information, 
        /// synchronize the trackable points with the current frame motion.
        /// </summary>
        public void CameraTrackingStep(CameraTransformer cameraTransformer)
        {
            if (isObjectTrackingInitialized)
            {
                // Bail out if we have turned on object tracking for this drawing.
                // The two are incompatible, camera tracking means the object stays static
                // relative to the background, so the points positions are entirely managed
                // by the camera transformer.
                return;
            }
                     
            // Update the drawing based on camera motion data.
            foreach (var pair in trackablePoints2)
            {
                PointF p = pair.Value.CameraTrack(trackingTimestamp, cameraTransformer);
                drawing.SetTrackablePointValue(pair.Key, p, -1);
            }
        }
        #endregion

        #region Manual placement
        /// <summary>
        /// Raised when the user manipulates an opened track (dragging the search area).
        /// Update the corresponding trackable point in the drawing.
        /// </summary>
        private void drawingTrack_PointMoved(object sender, EventArgs<TimedPoint> e)
        {
            DrawingTrack drawingTrack = sender as DrawingTrack;
            if (drawingTrack == null)
            {
                return;
            }

            if (!mapTrackIdToPoint.ContainsKey(drawingTrack.Id))
            {
                return;
            }

            TimedPoint tp = e.Value;
            if (tp == null)
            {
                return;
            }

            string key = mapTrackIdToPoint[drawingTrack.Id];
            drawing.SetTrackablePointValue(key, tp.Point, 0);
        }

        /// <summary>
        /// Manual adjustment of a trackable point by the user.
        /// When the user moves the entire drawing or just one point.
        /// Also when the coordinate system origin is updated by a tracked calibration object.
        /// </summary>
        private void drawing_TrackablePointMoved(object sender, TrackablePointMovedEventArgs e)
        {
            if (isObjectTrackingInitialized)
            {
                if (mapPointToTrack.ContainsKey(e.PointName))
                {
                    // For now we don't support moving the object manually if it is tracked.
                    // The user must turn tracking on and move the tracks.
                    // TODO.
                    //DrawingTrack track = mapPointToTrack[e.PointName];
                }
                else
                {
                    log.Error("DrawingTracker: the point is not bound to a track.");
                }
            }
            else
            {
                // Set the new reference position for no-tracking and camera tracking.
                // Even if we don't have camera tracking active right now, we may have it later,
                // so we must keep the reference frame up to date. The user is saying that 
                // the points are over the world elements at this frame.

                if (trackablePoints2.ContainsKey(e.PointName))
                {
                    trackablePoints2[e.PointName].SetReferenceValue(trackingTimestamp, e.Position);
                }
                else
                {
                    log.Error("DrawingTracker: the point is unknown.");
                }
            }
        }

        #endregion

        #region Extracting data
        /// <summary>
        /// Returns the coordinates of a point at the reference timestamp.
        /// Used for storage in KVA fragments.
        /// </summary>
        public PointF GetReferenceValue(string key)
        {
            if (!trackablePoints2.ContainsKey(key))
                return PointF.Empty;

            return trackablePoints2[key].ReferenceValue;
        }

        /// <summary>
        /// Returns the tracks used in object tracking.
        /// Used for kinematics diagrams.
        /// Returns null if the drawing is not doing object tracking.
        /// </summary>
        public Dictionary<string, DrawingTrack> GetTracks()
        {
            if (!isObjectTrackingInitialized)
                return null;
            
            return mapPointToTrack;
        }

        /// <summary>
        /// Returns the position of the point nearest to that time.
        /// Used for moving coordinate systems.
        /// If the drawing is not tracked returns the reference position.
        /// </summary>
        public PointF GetPointAtTime(string key, long time)
        {
            if (isObjectTrackingInitialized)
            {
                if (mapPointToTrack.ContainsKey(key))
                {
                    return mapPointToTrack[key].GetTimedPoint(time).Point;
                }
                else
                {
                    return PointF.Empty;
                }
            }
            else
            {
                if (trackablePoints2.ContainsKey(key))
                {
                    return trackablePoints2[key].ReferenceValue;
                }
                else
                {
                    return PointF.Empty;
                }
            }
        }

        /// <summary>
        /// Returns a map of point names to lists of raw 2D pixel coordinates.
        /// Used for spreadsheet export.
        /// </summary>
        public Dictionary<string, List<PointF>> CollectObjectTrackingData()
        {
            Dictionary<string, List<PointF>> data = new Dictionary<string, List<PointF>>();
            foreach (var pair in mapPointToTrack)
            {
                string key = pair.Key;
                List<PointF> values = pair.Value.GetTimedPoints().Select(tp => tp.Point).ToList();
                data.Add(key, values);
            }

            return data;
        }
        #endregion

        #region Serialization
        public void WriteXml(XmlWriter w)
        {
            foreach (KeyValuePair<string, TrackablePoint2> pair in trackablePoints2)
            {
                w.WriteStartElement("TrackablePoint");
                w.WriteAttributeString("key", pair.Key);

                if (isObjectTrackingInitialized && mapPointToTrack.ContainsKey(pair.Key))
                {
                    w.WriteElementString("TrackId", mapPointToTrack[pair.Key].Id.ToString());
                }

                w.WriteElementString("ReferenceTimestamp", pair.Value.ReferenceTimestamp.ToString());
                w.WriteElementString("ReferenceValue", XmlHelper.WritePointF(pair.Value.ReferenceValue));
                w.WriteEndElement();
            }
        }

        public DrawingTracker(XmlReader r, PointF scale, TimestampMapper timeMapper)
        {
            bool isEmpty = r.IsEmptyElement;

            if (r.MoveToAttribute("id"))
                drawingId = new Guid(r.ReadContentAsString());

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

            // At this point we only have the drawing id, not the drawing object itself.
            // We will later get a call via "Assign()" with the drawing object.
            // Similarly if we are object-tracking, we only have the track ids and we
            // will get the actual tracks during Assign().
            if (mapTrackIdToPoint.Count > 0 && mapTrackIdToPoint.Count != trackablePoints2.Count)
            {
                log.Error("Deserialized drawing tracker has inconsistent trackable points vs track ids.");
            }
        }

        private void ParseTrackablePoint(XmlReader r, PointF scale, TimestampMapper timeMapper)
        {
            string key = "";
            long referenceTimestamp = 0;
            PointF referenceValue = PointF.Empty;

            if (r.MoveToAttribute("key"))
                key = r.ReadContentAsString();

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "TrackId":
                        Guid trackId = XmlHelper.ParseGuid(r.ReadElementContentAsString());
                        mapTrackIdToPoint[trackId] = key;
                        break;
                    case "ReferenceTimestamp":
                        referenceTimestamp = r.ReadElementContentAsLong();
                        break;
                    case "ReferenceValue":
                        referenceValue = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        break;
                }
            }

            r.ReadEndElement();

            TrackablePoint2 point = new TrackablePoint2(referenceTimestamp, referenceValue);
            trackablePoints2.Add(key, point);
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}), hasTrackingData:{2}, tracking:{3}", drawing.Name, drawing.Id, IsObjectTrackingInitialized, IsCurrentlyTracking);
        }
        #endregion
    }
}
