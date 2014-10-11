#region License
/*
Copyright © Joan Charmant 21/08/2012.
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
using System.Drawing;
using Kinovea.Video;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Represent a point that can be tracked in time. Hosts a timeline and current value.
    /// Tracking uses the closest known data point.
    /// If the point is not currently tracked, a separate value is kept outside the timeline.
    /// </summary>
    public class TrackablePoint
    {
        #region Properties
        public PointF CurrentValue
        {
            get { return currentValue; }
        }
        public int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= trackerParameters.ContentHash;
                hash ^= nonTrackingValue.GetHashCode();
                foreach (TrackFrame frame in trackTimeline.Enumerate())
                    hash ^= frame.ContentHash;

                return hash;
            }
        }
        public bool Empty
        {
            get { return trackTimeline.Count == 0; }
        }
        #endregion
        
        private bool isTracking;
        private PointF currentValue;
        private TrackingContext context;
        private TrackerParameters trackerParameters;
        private Timeline<TrackFrame> trackTimeline = new Timeline<TrackFrame>();
        private PointF nonTrackingValue;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TrackablePoint(TrackingContext context, TrackerParameters trackerParameters, PointF value)
        {
            this.context = context;
            this.trackerParameters = trackerParameters;
            this.currentValue = value;
            this.nonTrackingValue = value;
        }
        
        /// <summary>
        /// Value adjusted by user.
        /// </summary>
        public void SetUserValue(PointF value)
        {
            // The context should have been set at Track() time when we landed on the video frame.
            currentValue = value;

            if (!isTracking)
            {
                nonTrackingValue = value;
                return;
            }
            
            if (trackerParameters.ResetOnMove)
                ClearTimeline();
            
            trackTimeline.Insert(context.Time, CreateTrackFrame(value, PositionningSource.Manual));
        }
        
        /// <summary>
        /// Track the point in the current image, or use the existing data if already known.
        /// </summary>
        /// <param name="context"></param>
        public void Track(TrackingContext context)
        {
            this.context = context;
           
            if(!isTracking)
                return;
            
            TrackFrame closestFrame = trackTimeline.ClosestFrom(context.Time);

            if (closestFrame == null)
            {
                currentValue = nonTrackingValue;
                trackTimeline.Insert(context.Time, CreateTrackFrame(currentValue, PositionningSource.Manual));
                return;
            }

            if (closestFrame.Template == null)
            {
                // We may not have the template if the timeline was imported from KVA.
                trackTimeline.Insert(context.Time, CreateTrackFrame(closestFrame.Location, closestFrame.PositionningSource));
                currentValue = closestFrame.Location;
                return;
            }

            if(closestFrame.Time == context.Time)
            {
                currentValue = closestFrame.Location;
                return;
            }
            
            TrackResult result = Tracker.Track(trackerParameters.SearchWindow, closestFrame, context.Image);

            if(result.Similarity >= trackerParameters.SimilarityThreshold)
            {
                currentValue = result.Location;
                
                if(result.Similarity > trackerParameters.TemplateUpdateThreshold)
                {
                    Bitmap template = closestFrame.Template.CloneDeep();
                    TrackFrame newFrame = new TrackFrame(context.Time, result.Location, template, PositionningSource.TemplateMatching);
                    trackTimeline.Insert(context.Time, newFrame);
                }
                else
                {
                    trackTimeline.Insert(context.Time, CreateTrackFrame(result.Location, PositionningSource.TemplateMatching));  
                }
                
            }
            else
            {
               currentValue = closestFrame.Location;
            }
        }
        
        public void Reset()
        {
            ClearTimeline();
        }
       
        public void SetTracking(bool isTracking)
        {
            if(this.isTracking == isTracking)
                return;
            
            this.isTracking = isTracking;
            
            if(!isTracking)
                currentValue = nonTrackingValue;
            else if (context != null)
                Track(context);
        }

        public void WriteXml(XmlWriter w)
        {
            w.WriteStartElement("TrackerParameters");
            trackerParameters.WriteXml(w);
            w.WriteEndElement();

            w.WriteElementString("NonTrackingValue", XmlHelper.WritePointF(nonTrackingValue));
            w.WriteElementString("CurrentValue", XmlHelper.WritePointF(currentValue));

            w.WriteStartElement("Timeline");
            foreach (TrackFrame frame in trackTimeline.Enumerate())
            {
                w.WriteStartElement("Frame");
                w.WriteAttributeString("time", frame.Time.ToString());
                w.WriteAttributeString("location", XmlHelper.WritePointF(frame.Location));
                w.WriteAttributeString("source", frame.PositionningSource.ToString());
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        public TrackablePoint(XmlReader r, PointF scale, TimestampMapper timeMapper)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "TrackerParameters":
                        trackerParameters = TrackerParameters.ReadXml(r, scale);
                        break;
                    case "NonTrackingValue":
                        nonTrackingValue = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        nonTrackingValue = nonTrackingValue.Scale(scale.X, scale.Y);
                        break;
                    case "CurrentValue":
                        currentValue = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        currentValue = currentValue.Scale(scale.X, scale.Y);
                        break;
                    case "Timeline":
                        ParseTimeline(r, scale, timeMapper);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        break;
                }
            }

            r.ReadEndElement();
        }

        private void ParseTimeline(XmlReader r, PointF scale, TimestampMapper timeMapper)
        {
            trackTimeline.Clear();
            
            bool isEmpty = r.IsEmptyElement;

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Frame":
                        TrackFrame frame = new TrackFrame(r, scale, timeMapper);
                        trackTimeline.Insert(frame.Time, frame);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        break;
                }
            }

            if (!isEmpty)
                r.ReadEndElement();
        }

        private TrackFrame CreateTrackFrame(PointF location, PositionningSource positionningSource)
        {
            Rectangle region = location.Box(trackerParameters.BlockWindow).ToRectangle();
            Bitmap template = context.Image.ExtractTemplate(region);
            return new TrackFrame(context.Time, location, template, positionningSource);
        }

        private void ClearTimeline()
        {
            trackTimeline.Clear((frame) => 
            {
                if (frame.Template != null)
                    frame.Template.Dispose();
            });
        }
        
    }
}
