#region License
/*
Copyright © Joan Charmant 2012.
joan.charmant@gmail.com 
 
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

using Kinovea.Video;
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
        public bool IsTracking
        {
            get { return isTracking; }
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
        #endregion
        
        private ITrackable drawing;
        private Guid drawingId;
        private bool isTracking;
        private bool assigned;
        private TrackerParameters parameters;
        private Dictionary<string, TrackablePoint> trackablePoints = new Dictionary<string, TrackablePoint>();
        
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
  
        public void Track(TrackingContext context)
        {
            // This is where we would spawn new threads for each tracking.
            // TODO: Extract the bitmapdata once and pass it to all.
            foreach(KeyValuePair<string, TrackablePoint> pair in trackablePoints)
            {
                pair.Value.Track(context);
                
                if(isTracking)
                    drawing.SetTrackablePointValue(pair.Key, pair.Value.CurrentValue);
            }
        }
        
        public void ToggleTracking()
        {
            isTracking = !isTracking;
            AfterToggleTracking();
        }

        private void AfterToggleTracking()
        {
            foreach(KeyValuePair<string, TrackablePoint> pair in trackablePoints)
                pair.Value.SetTracking(isTracking);
            
            drawing.SetTracking(isTracking);
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

            if (r.MoveToAttribute("tracking"))
                isTracking = XmlHelper.ParseBoolean(r.ReadContentAsString());

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
