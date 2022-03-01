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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public void Initialize(Size imageSize)
        {
            this.imageSize = imageSize;
        }

        public void Add(ITrackable drawing, VideoFrame videoFrame)
        {
            if(trackers.ContainsKey(drawing.Id))
               return;
            
            TrackingProfile profile = drawing.CustomTrackingProfile ?? PreferencesManager.PlayerPreferences.TrackingProfile;
            TrackerParameters parameters = new TrackerParameters(profile, imageSize);

            TrackingContext context = new TrackingContext(videoFrame.Timestamp, videoFrame.Image);
            trackers.Add(drawing.Id, new DrawingTracker(drawing, context, parameters));
        }

        public void Assign(ITrackable drawing)
        {
            if (trackers.ContainsKey(drawing.Id) && !trackers[drawing.Id].Assigned)
                trackers[drawing.Id].Assign(drawing);
        }

        public void UpdateId(Guid oldId, Guid newId)
        {
            if (oldId == newId || !trackers.ContainsKey(oldId) || trackers.ContainsKey(newId))
                return;

            trackers.Add(newId, trackers[oldId]);
            trackers.Remove(oldId);
        }

        public void AddPoint(ITrackable drawing, VideoFrame videoFrame, string key, PointF point)
        {
            if (!trackers.ContainsKey(drawing.Id))
                return;

            TrackingProfile profile = drawing.CustomTrackingProfile ?? PreferencesManager.PlayerPreferences.TrackingProfile;
            TrackerParameters parameters = new TrackerParameters(profile, imageSize);

            TrackingContext context = new TrackingContext(videoFrame.Timestamp, videoFrame.Image);

            trackers[drawing.Id].AddPoint(context, parameters, key, point);
        }

        public void RemovePoint(ITrackable drawing, string key)
        {
            if (!trackers.ContainsKey(drawing.Id))
                return;

            trackers[drawing.Id].RemovePoint(key);
        }
        
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

        public void Clear()
        {
            foreach(DrawingTracker tracker in trackers.Values)
                tracker.Dispose();
            
            trackers.Clear();
        }
        
        public void Remove(ITrackable drawing)
        {
            if(trackers.Count == 0 || !trackers.ContainsKey(drawing.Id))
                return;
            
            trackers[drawing.Id].Dispose();
            trackers.Remove(drawing.Id);
        }

        public void Track(VideoFrame videoFrame)
        {
            TrackingContext context = new TrackingContext(videoFrame.Timestamp, videoFrame.Image);
            
            foreach(DrawingTracker tracker in trackers.Values)
            {
                tracker.Track(context);
            }
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
            if (!SanityCheck(id))
                return false;

            return trackers[id].HasData;
        }
        
        public void UpdateContext(ITrackable drawing, VideoFrame videoFrame)
        {
            if(!SanityCheck(drawing.Id))
                return;
            
            TrackingContext context = new TrackingContext(videoFrame.Timestamp, videoFrame.Image);
            trackers[drawing.Id].Track(context);
        }
        
        public void ToggleTracking(ITrackable drawing)
        {
            if(!SanityCheck(drawing.Id))
                return;
           
            trackers[drawing.Id].ToggleTracking();
        }

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

        private bool SanityCheck(Guid id)
        {
            bool contains = trackers.ContainsKey(id);
            if(!contains)
            {
                log.Error("This drawing was not registered for tracking.");
                
                #if DEBUG
                throw new ArgumentException("This drawing was not registered for tracking.");
                #endif
            }
            
            return contains;
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
                    List<PointF> value = pair.Value.Select(p => metadata.CalibrationHelper.GetPoint(p)).ToList();
                    
                    string name = pair.Key;

                    // For Generic posture drawings the trackable points are always named from their index.
                    // Query the drawing itself here to get better names.
                    if (drawing is DrawingGenericPosture)
                        name = ((DrawingGenericPosture)drawing).GetTrackablePointName(pair.Key);
                    
                    mdt.Data.Add(name, value);
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

            while (r.NodeType == XmlNodeType.Element)
            {
                ReadTracker(r, scale, timeMapper);
            }

            if (!isEmpty)
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
    }
}
