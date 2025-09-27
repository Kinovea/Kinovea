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

using Kinovea.Video;
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Manages the drawing trackers.
    /// Drawing trackers are responsible for updating the position of trackable points in drawings.
    /// All drawings that have such points must have a drawing tracker even if they are not actively tracked.
    /// This handles object tracking, camera tracking and non-tracking scenarios.
    /// </summary>
    public class TrackabilityManager
    {
        #region Properties

        /// <summary>
        /// True if at least one drawing is currently actively tracking.
        /// </summary>
        public bool AnyTracking
        {
            get { return trackers.Values.Any((tracker) => tracker.IsCurrentlyTracking); }
        }
        public int ContentHash
        {
            get
            {
                int hash = 0;
                foreach (var tracker in trackers.Values)
                {
                    hash ^= tracker.ContentHash;
                }
                return hash;
            }
        }
        #endregion

        #region Members
        private Dictionary<Guid, DrawingTracker> trackers = new Dictionary<Guid, DrawingTracker>();
        private Dictionary<Guid, Guid> mapTrackIdToDrawingId = new Dictionary<Guid, Guid>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        /// <summary>
        /// Add a tracker for a trackable drawing and register its points.
        /// videoFrame can contain a null image if this is added from a capture screen.
        /// </summary>
        public void Add(ITrackable drawing, long timestamp)
        {
            if(trackers.ContainsKey(drawing.Id))
            {
                trackers[drawing.Id].Reassign(drawing, timestamp);
            }
            else
            {
                trackers.Add(drawing.Id, new DrawingTracker(drawing, timestamp));
            }
        }

        /// <summary>
        /// Finds the tracker responsible for this drawing and assign the drawing to it,
        /// and optionally all the tracks if object tracking was initialized.
        /// When we load from KVA, the trackers and drawings are loaded independently.
        /// At this point the tracker only knows the drawing id. 
        /// Re-inject the drawing instance into the tracker.
        /// </summary>
        public void Assign(ITrackable drawing, List<DrawingTrack> allTracks)
        {
            if (trackers.ContainsKey(drawing.Id) && !trackers[drawing.Id].Assigned)
            {
                List<Guid> bound = trackers[drawing.Id].Assign(drawing, allTracks);

                foreach (var id in bound)
                {
                    mapTrackIdToDrawingId.Add(id, drawing.Id);
                }
            }
        }

        /// <summary>
        /// Returns true if this drawing id has a tracker assigned to it.
        /// </summary>
        public bool IsAssigned(Guid id)
        {
            return trackers.ContainsKey(id);
        }

        /// <summary>
        /// Change the drawing id in the tracker without invalidating the tracking data.
        /// This happens when a drawing changes id. This might happen for some drawings
        /// like the coordinate system which is both a singleton drawing and trackable.
        /// </summary>
        public void UpdateId(Guid oldId, Guid newId)
        {
            if (oldId == newId)
                return;

            if (!trackers.ContainsKey(oldId))
                return;
                
            if (trackers.ContainsKey(newId))
            {
                trackers.Remove(oldId);
            }
            else
            {
                trackers.Add(newId, trackers[oldId]);
                trackers.Remove(oldId);
            }

        }

        /// <summary>
        /// Add a new point to an existing tracker.
        /// This is used for drawings that have a dynamic list of trackable points like polyline.
        /// </summary>
        public void AddPoint(ITrackable drawing, string key, long timestamp, PointF point)
        {
            if (!trackers.ContainsKey(drawing.Id))
                return;

            trackers[drawing.Id].AddPoint(key, timestamp, point);
        }

        public void RemovePoint(ITrackable drawing, string key)
        {
            if (!trackers.ContainsKey(drawing.Id))
                return;

            trackers[drawing.Id].RemovePoint(key);
        }
        
        /// <summary>
        /// Delete trackers that were not assigned a drawing.
        /// </summary>
        public void CleanUnassigned()
        {
            HashSet<Guid> pruneList = new HashSet<Guid>();
            foreach (KeyValuePair<Guid, DrawingTracker> pair in trackers)
            {
                if (pair.Value.Assigned)
                    continue;

                ForgetTracks(pair.Value.Id);
                pair.Value.Dispose();
                pruneList.Add(pair.Key);
            }

            foreach (Guid key in pruneList)
                trackers.Remove(key);
        }

        /// <summary>
        /// Delete all trackers.
        /// Note: this also deletes the tracker for the coordinate system
        /// so this has to be re-assigned later if needed.
        /// </summary>
        public void Clear()
        {
            foreach(DrawingTracker tracker in trackers.Values)
            {
                ForgetTracks(tracker.Id);
                tracker.Dispose();
            }
            
            trackers.Clear();
        }
        
        /// <summary>
        /// Delete the tracker responsible for this drawing.
        /// </summary>
        public void Remove(ITrackable drawing)
        {
            if(trackers.Count == 0 || !trackers.ContainsKey(drawing.Id))
                return;

            ForgetTracks(drawing.Id);
            trackers[drawing.Id].Dispose();
            trackers.Remove(drawing.Id);
        }

        /// <summary>
        /// Returns true if the drawing is currently actively tracking.
        /// </summary>
        public bool IsTracking(ITrackable drawing)
        {
            if (!SanityCheck(drawing.Id))
                return false;

            return trackers[drawing.Id].IsCurrentlyTracking;
        }

        /// <summary>
        /// Returns true if object tracking was ever turned on for the drawing.
        /// </summary>
        public bool IsObjectTrackingInitialized(Guid id)
        {
            if (!trackers.ContainsKey(id))
                return false;

            return trackers[id].IsObjectTrackingInitialized;
        }

        /// <summary>
        /// Turn on tracking for a drawing for the first time.
        /// Takes the list of track ids assigned to each trackable point.
        /// </summary>
        public void InitializeTracking(ITrackable drawing, Dictionary<string, DrawingTrack> tracks)
        {
            if (!SanityCheck(drawing.Id))
                return;

            trackers[drawing.Id].InitializeTracking(tracks);

            // Keep a map from track id to drawing, to quickly update the drawing when its underlying track changes.
            foreach (var t in tracks.Values)
            {
                if (mapTrackIdToDrawingId.ContainsKey(t.Id))
                {
                    // Not supported.
                    // Currently each track is exclusively associated to a trackable point.
                    // It might be better in the future if two drawings can "share" a track object.
                    // And that we can freely attach object points to existing tracks.
                }
                else
                {
                    mapTrackIdToDrawingId.Add(t.Id, drawing.Id);
                }
            }
        }

        /// <summary>
        /// Toggles the tracking mode for this drawing.
        /// </summary>
        public void ToggleTracking(ITrackable drawing)
        {
            if(!SanityCheck(drawing.Id))
                return;
           
            trackers[drawing.Id].ToggleTracking();
        }

        public void BeforeTrackingStep(long timestamp)
        {
            foreach (DrawingTracker tracker in trackers.Values)
            {
                tracker.BeforeTrackingStep(timestamp);
            }
        }

        /// <summary>
        /// Returns true if the track is bound to any drawing.
        /// </summary>
        public bool IsTrackBoundToAnyDrawing(Guid trackId)
        {
            return mapTrackIdToDrawingId.ContainsKey(trackId);
        }

        /// <summary>
        /// One track has finished its tracking step.
        /// TimedPoint has been set to an known position from a previous tracking 
        /// step, to a new location after tracking or to null if tracking failed.
        /// Update the corresponding trackable points in the trackable drawings.
        /// This handles "object tracking". Camera tracking is handled after this step.
        /// We must go through this for every frame whether the track is opened or not.
        /// </summary>
        public void AfterTrackTrackingStep(DrawingTrack track, TimedPoint tp)
        {
            // Threading:
            // This may run in the tracking thread of this particular track if we are 
            // actively tracking, that is, any time there is at least one track active
            // and we move step by step.
            // In this context multiple tracks referenced by a given drawing may come here at the same time.
            // When not actively tracking this is run sequentially on the main thread.
            if (!mapTrackIdToDrawingId.ContainsKey(track.Id))
                return;
            
            Guid drawingId = mapTrackIdToDrawingId[track.Id];
            if (!SanityCheck(drawingId))
                return;

            trackers[drawingId].ObjectTrackingStep(track, tp);
        }

        /// <summary>
        /// Perform camera tracking step: update all trackable points of all trackable drawings 
        /// that are not using object tracking to match the camera motion.
        /// </summary>
        public void CameraTrackingStep(CameraTransformer cameraTransformer)
        {
            if (!cameraTransformer.Initialized)
                return;
            
            foreach (DrawingTracker tracker in trackers.Values)
            {
                tracker.CameraTrackingStep(cameraTransformer);
            }
        }

        /// <summary>
        /// Returns the list of tracks backing the trackable points for the drawing.
        /// This is used by the kinematics diagrams and for batch manipulation of tracks via the drawing menus.
        /// </summary>
        public Dictionary<string, DrawingTrack> GetTrackingTracks(ITrackable drawing)
        {
            if (!SanityCheck(drawing.Id))
                return null;

            return trackers[drawing.Id].GetTracks();
        }

        /// <summary>
        /// Turn the drawing back to non-tracking mode.
        /// </summary>
        public void DeinitializeTracking(ITrackable drawing)
        {
            if (!SanityCheck(drawing.Id))
                return;

            // At this point it shouldn't have any bound tracks.
            trackers[drawing.Id].DeinitializeTracking();
        }

        /// <summary>
        /// Returns the position of the point nearest to that time.
        /// This is used for moving coordinate systems.
        /// </summary>
        public PointF GetPointAtTime(ITrackable drawing, string key, long time)
        {
            return GetPointAtTime(drawing.Id, key, time);
        }

        /// <summary>
        /// Returns the position of the point nearest to that time.
        /// This is used for moving coordinate systems.
        /// </summary>
        public PointF GetPointAtTime(Guid id, string key, long time)
        {
            if (!SanityCheck(id))
                return PointF.Empty;

            return trackers[id].GetPointAtTime(key, time);
        }

        public PointF GetReferenceValue(Guid id, string key)
        {
            if (!SanityCheck(id))
                return PointF.Empty;

            return trackers[id].GetReferenceValue(key);
        }


        #region KVA Serialization
        public void WriteXml(XmlWriter w)
        {
            foreach (DrawingTracker tracker in trackers.Values)
                WriteTracker(w, tracker.Id);
        }

        public void WriteTracker(XmlWriter w, Guid id)
        {
            if (!trackers.ContainsKey(id))
                return;

            DrawingTracker tracker = trackers[id];
            w.WriteStartElement("TrackableDrawing");
            w.WriteAttributeString("id", tracker.Id.ToString());
            w.WriteAttributeString("name", tracker.Name);
            tracker.WriteXml(w);
            w.WriteEndElement();
        }

        public void ReadXml(XmlReader r, PointF scale, TimestampMapper timeMapper)
        {
            bool isEmpty = r.IsEmptyElement;
            r.ReadStartElement();

            if (isEmpty)
                return;

            while (r.NodeType == XmlNodeType.Element)
            {
                ReadTracker(r, scale, timeMapper);
            }

            r.ReadEndElement();
        }

        public void ReadTracker(XmlReader r, PointF scale, TimestampMapper timeMapper)
        {
            if (r.Name == "TrackableDrawing")
            {
                DrawingTracker tracker = new DrawingTracker(r, scale, timeMapper);
                if (trackers.ContainsKey(tracker.Id))
                {
                    ForgetTracks(tracker.Id);
                    trackers[tracker.Id].Dispose();
                    trackers[tracker.Id] = tracker;
                }
                else
                {
                    trackers.Add(tracker.Id, tracker);
                }
            }
            else
            {
                string unparsed = r.ReadOuterXml();
            }
        }
        #endregion

        #region Spreadsheet serialization
        /// <summary>
        /// Collect the data used for spreadsheet export.
        /// Updates the passed list.
        /// </summary>
        public void CollectMeasuredData(Metadata metadata, List<MeasuredDataTimeseries> timelines)
        {
            foreach (DrawingTracker tracker in trackers.Values)
            {
                AbstractDrawing drawing = metadata.FindDrawing(tracker.Id);
                if (drawing == null)
                    continue;

                if (!IsObjectTrackingInitialized(drawing.Id))
                    continue;

                MeasuredDataTimeseries mdt = new MeasuredDataTimeseries();
                mdt.Name = drawing.Name;

                var tracks = tracker.GetTracks();
                if (tracks == null || tracks.Count == 0)
                    continue;

                // Get the list of times from the first track.
                List<TimedPoint> timedPoints = tracks.First().Value.GetTimedPoints();
                List<long> timestamps = timedPoints.Select(tp => tp.T).ToList();
                if (timestamps == null || timestamps.Count == 0)
                    continue;

                // Logic elsewhere should have enforced that the tracks are synchronized. 
                // Quick sanity check, just to check that we have the same number of entries.
                bool sane = true;
                foreach (var pair in tracks.Skip(1))
                {
                    if (pair.Value.GetTimedPoints().Count != timestamps.Count)
                    {
                        sane = false;
                        break;
                    }
                }

                if (!sane)
                {
                    log.ErrorFormat("Tracks desynchronized.");
                    continue;
                }

                // From now on assume the tracks are in sync.
                mdt.FirstTimestamp = timestamps[0];
                Dictionary<string, List<PointF>> dataRaw = tracker.CollectObjectTrackingData();

                // Convert the values to the user coordinate system.
                // FIXME: this still go through the conversion manually but now that these points 
                // are backed by trajectory objects we must already have functions to compute
                // this elsewhere.
                mdt.Times = timestamps.Select(ts => metadata.GetNumericalTime(ts, TimeType.UserOrigin)).ToList();
                mdt.Data = new Dictionary<string, List<PointF>>();
                foreach (var pair in dataRaw)
                {
                    // Each item here is the list of positions of this particular point over time.
                    // We need to convert these pixel locations based on frame time to account for moving coordinate system.
                    // All points in the object should have exactly the same number of entries, which should also match timestamps.Count.
                    List<PointF> value = new List<PointF>();
                    if (PreferencesManager.PlayerPreferences.ExportSpace == ExportSpace.WorldSpace)
                    {
                        int frame = 0;
                        foreach (var p in pair.Value)
                        {
                            long ts = timestamps[frame];
                            value.Add(metadata.CalibrationHelper.GetPointAtTime(p, ts));
                            frame++;
                        }
                    }
                    else
                    {
                        value = pair.Value;
                    }

                    string name = pair.Key;

                    // For Generic posture drawings the trackable points are always named from their index.
                    // Query the drawing itself here to get better names.
                    if (drawing is DrawingGenericPosture)
                        name = ((DrawingGenericPosture)drawing).GetTrackablePointName(pair.Key);

                    mdt.Data.Add(name, value);
                }

                // Special case for angles, add the angle value to the time series.
                if (drawing is DrawingAngle drawingAngle)
                {
                    Dictionary<string, FilteredTrajectory> trajs = new Dictionary<string, FilteredTrajectory>();
                    foreach (var pair in tracks)
                    {
                        trajs.Add(pair.Key, pair.Value.FilteredTrajectory);
                    }

                    AngularKinematics angularKinematics = new AngularKinematics();
                    TimeSeriesCollection tsc = angularKinematics.BuildKinematics(trajs, drawingAngle.AngleOptions, metadata.CalibrationHelper);
                    mdt.AngleValues = tsc[Kinematics.AngularPosition].Select(value => (float)value).ToList();
                }

                timelines.Add(mdt);
            }
        }

        /// <summary>
        /// Import a drawing tracker from outside.
        /// This is used by non-KVA importers.
        /// </summary>
        public void ImportTracker(DrawingTracker tracker, List<DrawingTrack> tracks)
        {
            if (trackers.ContainsKey(tracker.Id))
            {
                ForgetTracks(tracker.Id);
                trackers[tracker.Id].Dispose();
                trackers[tracker.Id] = tracker;
            }
            else
            {
                trackers.Add(tracker.Id, tracker);
            }

            // Add track mapping.
            foreach (var track in tracks)
            {
                mapTrackIdToDrawingId.Add(track.Id, tracker.Id);
            }
        }
        #endregion


        /// <summary>
        /// Forget all tracks bound to the passed drawing.
        /// </summary>
        private void ForgetTracks(Guid id)
        {
            mapTrackIdToDrawingId = mapTrackIdToDrawingId
                .Where(kv => kv.Value != id)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// Forget one track that is about to be deleted.
        /// </summary>
        public void ForgetTrack(Guid id)
        {
            if (!mapTrackIdToDrawingId.ContainsKey(id))
                return;

            if (trackers.ContainsKey(mapTrackIdToDrawingId[id]))
                trackers[mapTrackIdToDrawingId[id]].ForgetTrack(id);

            mapTrackIdToDrawingId.Remove(id);
        }


        public void LogTrackers()
        {
            if (trackers.Count == 0)
            {
                log.Debug("No trackers registered.");
                return;
            }
            
            foreach (KeyValuePair<Guid, DrawingTracker> pair in trackers)
            {
                log.DebugFormat("{0}", pair.Value.ToString());
            }
        }

        /// <summary>
        /// Verify that this drawing is assigned to a tracker.
        /// </summary>
        private bool SanityCheck(Guid id)
        {
            bool contains = trackers.ContainsKey(id);
            if (!contains)
            {
                log.ErrorFormat("This drawing was not registered for tracking. {0}.", id.ToString());

#if DEBUG
                throw new ArgumentException("This drawing was not registered for tracking.");
#endif
            }

            return contains;
        }

    }
}
