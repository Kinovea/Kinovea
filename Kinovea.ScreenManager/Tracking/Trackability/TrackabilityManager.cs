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
using System.Reflection;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Manages the drawing trackers.
    /// Each tracker is identified by the ID of the drawing it is tracking.
    /// </summary>
    public class TrackabilityManager
    {
        #region Properties
        public bool Tracking
        {
            get { return trackers.Values.Any((tracker) => tracker.IsTracking); }
        }
        public int ContentHash
        {
            get 
            {
                int hash = 0;
                foreach (DrawingTracker tracker in trackers.Values)
                {
                    if (tracker != null)
                        hash ^= tracker.ContentHash;
                }

                return hash;
            }
        }
        #endregion

        #region Members
        private Dictionary<Guid, DrawingTracker> trackers = new Dictionary<Guid, DrawingTracker>();
        private Size imageSize;
        private CameraTransformer cameraTransformer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public void Initialize(Size imageSize, CameraTransformer cameraTransformer)
        {
            this.imageSize = imageSize;
            this.cameraTransformer = cameraTransformer;
        }

        /// <summary>
        /// Add a tracker for a trackable drawing and register its points.
        /// </summary>
        public void Add(ITrackable drawing, VideoFrame videoFrame)
        {
            if(trackers.ContainsKey(drawing.Id))
               return;
            
            TrackingParameters parameters = drawing.CustomTrackingParameters ?? PreferencesManager.PlayerPreferences.TrackingParameters.Clone();
            
            TrackingContext context = new TrackingContext(videoFrame.Timestamp, videoFrame.Image);
            trackers.Add(drawing.Id, new DrawingTracker(drawing, context, parameters));
        }

        /// <summary>
        /// Finds the tracker responsible for this drawing and assign the drawing to it.
        /// When we load the KVA, the trackers and drawings are loaded independently.
        /// At this point the tracker only knows the drawing id. 
        /// This function re-inject the drawing instance into the tracker for convenience.
        /// </summary>
        public void Assign(ITrackable drawing)
        {
            if (trackers.ContainsKey(drawing.Id) && !trackers[drawing.Id].Assigned)
                trackers[drawing.Id].Assign(drawing);
        }

        /// <summary>
        /// Change the drawing id in the tracker.
        /// This happens when a drawing changes id. This might happen for some drawings
        /// like the coordinate system which is both a singleton drawing and trackable.
        /// </summary>
        public void UpdateId(Guid oldId, Guid newId)
        {
            if (oldId == newId || !trackers.ContainsKey(oldId) || trackers.ContainsKey(newId))
                return;

            trackers.Add(newId, trackers[oldId]);
            trackers.Remove(oldId);
        }

        /// <summary>
        /// Add a new point to an existing tracker.
        /// This is used for drawings that have a dynamic list of trackable points like polyline.
        /// </summary>
        public void AddPoint(ITrackable drawing, VideoFrame videoFrame, string key, PointF point)
        {
            if (!trackers.ContainsKey(drawing.Id))
                return;

            TrackingParameters parameters = drawing.CustomTrackingParameters ?? PreferencesManager.PlayerPreferences.TrackingParameters.Clone();
            TrackingContext context = new TrackingContext(videoFrame.Timestamp, videoFrame.Image);

            trackers[drawing.Id].AddPoint(context, parameters, key, point);
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

                pair.Value.Dispose();
                pruneList.Add(pair.Key);
            }

            foreach (Guid key in pruneList)
                trackers.Remove(key);
        }

        /// <summary>
        /// Delete all trackers.
        /// </summary>
        public void Clear()
        {
            foreach(DrawingTracker tracker in trackers.Values)
                tracker.Dispose();
            
            trackers.Clear();
        }
        
        /// <summary>
        /// Delete the tracker responsible for this drawing.
        /// </summary>
        public void Remove(ITrackable drawing)
        {
            if(trackers.Count == 0 || !trackers.ContainsKey(drawing.Id))
                return;
            
            trackers[drawing.Id].Dispose();
            trackers.Remove(drawing.Id);
        }

        /// <summary>
        /// Perform tracking for the current image.
        /// Track all points in all trackable drawings or use existing tracking data.
        /// Update the point coordinates in the drawing.
        /// </summary>
        public void Track(VideoFrame videoFrame)
        {
            TrackingContext context = new TrackingContext(videoFrame.Timestamp, videoFrame.Image);
            
            foreach(DrawingTracker tracker in trackers.Values)
            {
                tracker.Track(context, cameraTransformer);
            }

            context.Dispose();
        }
        
        /// <summary>
        /// Returns true if the drawing is currently actively tracking.
        /// </summary>
        public bool IsTracking(ITrackable drawing)
        {
            if (!SanityCheck(drawing.Id))
                return false;

            return trackers[drawing.Id].IsTracking;
        }

        /// <summary>
        /// Returns true if the drawing has data in its timeline.
        /// </summary>
        public bool HasData(Guid id)
        {
            if (!trackers.ContainsKey(id))
                return false;

            return trackers[id].HasData;
        }
        
        /// <summary>
        /// Set the drawing to actively tracking or not.
        /// </summary>
        public void ToggleTracking(ITrackable drawing)
        {
            if(!SanityCheck(drawing.Id))
                return;
           
            trackers[drawing.Id].ToggleTracking();
        }

        /// <summary>
        /// Returns the list of trackable points for the drawing.
        /// This is used by the kinematics forms.
        /// </summary>
        public Dictionary<string, TrackablePoint> GetTrackablePoints(ITrackable drawing)
        {
            if (!SanityCheck(drawing.Id))
                return null;

            return trackers[drawing.Id].TrackablePoints;
        }

        /// <summary>
        /// Returns the position of the point nearest to that time.
        /// This is used by linear kinematics.
        /// </summary>
        public PointF GetLocation(ITrackable drawing, string key, long time)
        {
            return GetLocation(drawing.Id, key, time);
        }

        /// <summary>
        /// Returns the position of the point nearest to that time.
        /// This is used by linear kinematics.
        /// </summary>
        public PointF GetLocation(Guid id, string key, long time)
        {
            if (!SanityCheck(id))
                return PointF.Empty;

            return trackers[id].GetLocation(key, time);
        }

        public PointF GetReferenceValue(Guid id, string key)
        {
            if (!SanityCheck(id))
                return PointF.Empty;

            return trackers[id].GetReferenceValue(key);
        }

        /// <summary>
        /// Collect the data used for spreadsheet export.
        /// Updates the passed list.
        /// </summary>
        public void CollectMeasuredData(Metadata metadata, List<MeasuredDataTimeseries> timelines)
        {
            foreach (DrawingTracker tracker in trackers.Values)
            {
                AbstractDrawing drawing = metadata.FindDrawing(tracker.ID);
                if (drawing == null)
                    continue;

                MeasuredDataTimeseries mdt = new MeasuredDataTimeseries();
                mdt.Name = drawing.Name;
                List<long> timestamps = tracker.CollectTimeVector();
                if (timestamps == null || timestamps.Count == 0)
                    continue;

                mdt.FirstTimestamp = timestamps[0];
                Dictionary<string, List<PointF>> dataRaw = tracker.CollectData();

                // Convert the values to the user coordinate system.
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

                if (drawing is DrawingAngle drawingAngle)
                {
                    //retrieve the angleOptions from drawing
                    AngleOptions angleOptions = drawingAngle.AngleOptions;

                    //retrieve all trackable points from drawing
                    Dictionary<string, TrackablePoint> trackablePoints = this.GetTrackablePoints(drawingAngle);
                    if (trackablePoints == null || trackablePoints.Count != 3) //if the number of tracked points doesn't match, skip the code
                        continue;

                    //store angle keys
                    List<string> keys = new List<string>{"o", "a", "b"};

                    //the Timeline data of each key is save to a TrackingTemplate object
                    Timeline<TrackingTemplate> timelineO = trackablePoints[keys[0]].Timeline;
                    Timeline<TrackingTemplate> timelineA = trackablePoints[keys[1]].Timeline;
                    Timeline<TrackingTemplate> timelineB = trackablePoints[keys[2]].Timeline;

                    //lists of TimedPoint samples from the raw timelines is created
                    List<TimedPoint> samplesO = new List<TimedPoint>();
                    List<TimedPoint> samplesA = new List<TimedPoint>();
                    List<TimedPoint> samplesB = new List<TimedPoint>();
                    //now populate each individual sample with the X & Y coordinates and timestamp with its corresponding entry
                    foreach (var entry in timelineO.Enumerate())
                        samplesO.Add(new TimedPoint(entry.Location.X, entry.Location.Y, entry.Time));
                    foreach (var entry in timelineA.Enumerate())
                        samplesA.Add(new TimedPoint(entry.Location.X, entry.Location.Y, entry.Time));
                    foreach (var entry in timelineB.Enumerate())
                        samplesB.Add(new TimedPoint(entry.Location.X, entry.Location.Y, entry.Time));

                    //create a new FilteredTrajectory for each point and call Initialize with samples and calibration settings
                    FilteredTrajectory trajO = new FilteredTrajectory();
                    trajO.Initialize(samplesO, metadata.CalibrationHelper);
                    FilteredTrajectory trajA = new FilteredTrajectory();
                    trajA.Initialize(samplesA, metadata.CalibrationHelper);
                    FilteredTrajectory trajB = new FilteredTrajectory();
                    trajB.Initialize(samplesB, metadata.CalibrationHelper);

                    //dictionary maps keys to their FilteredTrajectory instances
                    Dictionary<string, FilteredTrajectory> trajs = new Dictionary<string, FilteredTrajectory>()
                    {
                        {"o", trajO},
                        {"a", trajA},
                        {"b", trajB}
                    };

                    //BuildKinematics is called and an angular position of each frame is calculated
                    //this returns the exact same angle data that is returned when calling from dedicated angle functions
                    AngularKinematics angularKinematics = new AngularKinematics();
                    TimeSeriesCollection tsc = angularKinematics.BuildKinematics(trajs, angleOptions, metadata.CalibrationHelper);

                    //the mdt.AngleValues is populated with the computed angular position values
                    mdt.AngleValues = new List<float>(tsc.Length);
                    for (int i = 0; i < tsc.Length; i++)
                    {
                        mdt.AngleValues.Add((float)tsc[Kinematics.AngularPosition][i]);
                    }
                }
                timelines.Add(mdt);
            }
        }

        public void WriteXml(XmlWriter w)
        {
            foreach (DrawingTracker tracker in trackers.Values)
                WriteTracker(w, tracker.ID);
        }

        public void WriteTracker(XmlWriter w, Guid id)
        {
            if (!trackers.ContainsKey(id))
                return;

            DrawingTracker tracker = trackers[id];
            if (tracker.Empty)
                return;

            w.WriteStartElement("TrackableDrawing");
            w.WriteAttributeString("id", tracker.ID.ToString());
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
                if (trackers.ContainsKey(tracker.ID))
                {
                    trackers[tracker.ID].Dispose();
                    trackers[tracker.ID] = tracker;
                }
                else
                {
                    trackers.Add(tracker.ID, tracker);
                }
            }
            else
            {
                string unparsed = r.ReadOuterXml();
            }
        }

        /// <summary>
        /// Import a drawing tracker from outside.
        /// This is used by non-KVA importers.
        /// </summary>
        public void ImportTracker(DrawingTracker tracker)
        {
            if (trackers.ContainsKey(tracker.ID))
            {
                trackers[tracker.ID].Dispose();
                trackers[tracker.ID] = tracker;
            }
            else
            {
                trackers.Add(tracker.ID, tracker);
            }
        }


        private bool SanityCheck(Guid id)
        {
            bool contains = trackers.ContainsKey(id);
            if (!contains)
            {
                log.Error("This drawing was not registered for tracking.");

#if DEBUG
                throw new ArgumentException("This drawing was not registered for tracking.");
#endif
            }

            return contains;
        }

    }
}
