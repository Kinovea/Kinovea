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
        private TrackerParameters parameters;
        private Dictionary<string, TrackablePoint> trackablePoints = new Dictionary<string, TrackablePoint>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DrawingTracker(ITrackable drawing, TrackingContext context, TrackerParameters parameters)
        {
            this.drawing = drawing;
            this.drawingId = drawing.Id;
            this.parameters = parameters;
           
            foreach(KeyValuePair<string, PointF> pair in drawing.GetTrackablePoints())
                trackablePoints.Add(pair.Key, new TrackablePoint(context, parameters, pair.Value));
            
            drawing.TrackablePointMoved += drawing_TrackablePointMoved;
            assigned = true;
        }

        public void Assign(ITrackable drawing)
        {
            if (drawing.Id != drawingId)
                return;

            this.drawing = drawing;
            this.drawing.TrackablePointMoved += drawing_TrackablePointMoved;
            AfterToggleTracking();
            assigned = true;
        }

        public void AddPoint(TrackingContext context, TrackerParameters parameters, string key, PointF value)
        {
            // Some drawings like polyline have a dynamic list of trackable points.
            trackablePoints.Add(key, new TrackablePoint(context, parameters, value));
        }

        public void RemovePoint(string key)
        {
            trackablePoints.Remove(key);
        }
  
        public void Track(TrackingContext context)
        {
            // This is where we would spawn new threads for each tracking.
            Dictionary<string, bool> insertionMap = new Dictionary<string, bool>();
            bool atLeastOneInserted = false;
            foreach(KeyValuePair<string, TrackablePoint> pair in trackablePoints)
            {
                bool inserted = pair.Value.Track(context);
                drawing.SetTrackablePointValue(pair.Key, pair.Value.CurrentValue, pair.Value.TimeDifference);

                insertionMap[pair.Key] = inserted;
                if (inserted)
                    atLeastOneInserted = true;
            }

            if (atLeastOneInserted)
                FixTimelineSync(insertionMap);
        }
        
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
        
        private void drawing_TrackablePointMoved(object sender, TrackablePointMovedEventArgs e)
        {
            if(!trackablePoints.ContainsKey(e.PointName))
                throw new ArgumentException("This point is not bound.");
            
            trackablePoints[e.PointName].SetUserValue(e.Position);
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

        private void ParseTrackablePoint(XmlReader r, PointF scale, TimestampMapper timeMapper)
        {
            string key = "";
            
            bool isEmpty = r.IsEmptyElement;

            if (r.MoveToAttribute("key"))
                key = r.ReadContentAsString();

            TrackablePoint point = new TrackablePoint(r, scale, timeMapper);
            trackablePoints.Add(key, point);
        }
    }
}
