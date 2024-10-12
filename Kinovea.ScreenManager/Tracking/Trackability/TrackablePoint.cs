#region License
/*
Copyright © Joan Charmant 21/08/2012.
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
using System.Drawing;
using Kinovea.Video;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Represent a point of a larger drawing that can be tracked in time. 
    /// Hosts a timeline and current value.
    /// Reading the position of the point always returns the closest entry from the timeline,
    /// or a special non-tracked value if the timeline is empty.
    /// </summary>
    public class TrackablePoint
    {
        #region Properties

        /// <summary>
        /// Position at the closest entry we could find in the timeline, for the current video time.
        /// This should be updated at each frame whether tracking is active or not.
        /// </summary>
        public PointF CurrentValue
        {
            get { return currentValue; }
        }

        /// <summary>
        /// Reference position set by the user. It is only valid at the 
        /// reference timestamp that was active when the user set it.
        /// This supports camera motion on drawings that are not enabled for tracking.
        /// </summary>
        public PointF ReferenceValue
        {
            get { return nonTrackingValue; }
        }

        /// <summary>
        /// Distance in timestamps between the current video time and the time of the closest tracked value.
        /// This is used by drawings to change opacity based on whether the drawing has tracking data at this time or not.
        /// </summary>
        public long TimeDifference
        {
            get { return timeDifference; }
        }
        public int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= trackerParameters.ContentHash;
                hash ^= nonTrackingValue.GetHashCode();
                foreach (TrackingTemplate frame in trackTimeline.Enumerate())
                    hash ^= frame.ContentHash;

                return hash;
            }
        }
        public bool Empty
        {
            get { return trackTimeline.Count == 0; }
        }

        public Timeline<TrackingTemplate> Timeline
        {
            get { return trackTimeline; }
        }
        #endregion
        
        private bool isTracking;
        private PointF currentValue;
        private PointF nonTrackingValue;
        private long timeDifference = -1;
        private TrackingContext context;
        private TrackingParameters trackerParameters = new TrackingParameters();
        private Timeline<TrackingTemplate> trackTimeline = new Timeline<TrackingTemplate>();
        private Dictionary<long, PointF> cameraTrackCache = new Dictionary<long, PointF>();
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TrackablePoint(TrackingContext context, TrackingParameters trackerParameters, PointF value)
        {
            this.context = context;
            this.trackerParameters = trackerParameters;
            this.currentValue = value;
            this.nonTrackingValue = value;
        }

        /// <summary>
        /// Importing from outside for non-KVA importers.
        /// </summary>
        public TrackablePoint(TrackingParameters trackerParameters, PointF value, Timeline<TrackingTemplate> trackTimeline)
        {
            this.trackerParameters = trackerParameters;
            this.currentValue = value;
            this.nonTrackingValue = value;
            this.trackTimeline = trackTimeline;
        }
        
        /// <summary>
        /// Value adjusted manually by the user.
        /// </summary>
        public bool SetUserValue(PointF value)
        {
            // The context should have been set at Track() time when we landed on the video frame.
            if (context == null)
                return false;

            bool inserted = false;
            currentValue = value;
            timeDifference = 0;

            // There are several cases.
            // The important point is that as long as we have tracking data the "reading" part will always pick the nearest point from the timeline.
            // We do not store a point position for all "non-tracked" times, this would break the trajectory when in-between tracked points.
            // The non-tracking value is only for when the timeline is empty.
            // Cases:
            // 1. We are actively tracking: update the timeline.
            // 2. We are not tracking and we never tracked this object: do not update the timeline.
            // 3. We are not tracking but we are on a previously tracked point: update the timeline.
            // In this case it doesn't make sense to force the user to re-enable tracking, we know this point is tracked.
            // 4. We are not tracking and we are not on a tracked point: update the timeline.
            // This is the tricky case, but if we don't update the timeline the move is lost.
            if (isTracking || trackTimeline.HasData())
            {
                trackTimeline.Insert(context.Time, CreateTrackFrame(value, 1.0f, PositionningSource.Manual));
                inserted = true;
            }
            else
            {
                nonTrackingValue = value;
                cameraTrackCache.Clear();
            }

            return inserted;
        }
        
        /// <summary>
        /// Track the point in the current image, or use the existing data if already known.
        /// We do this even if the drawing is currently not tracking, to push the existing tracked data in the object.
        /// Important: for drawings containing multiple trackable points, either all or none of them should have a new value.
        /// If some of them successfully track and some other don't, the one that didn't must insert the closest frame value.
        /// This way we ensure the timelines are always of the same length.
        /// </summary>
        public bool Track(TrackingContext context)
        {
            bool inserted = false;
            this.context = context;
           
            if (!trackTimeline.HasData())
            {
                // Not a single entry in the timeline.
                // This drawing has never been activated for tracking so far.
                currentValue = nonTrackingValue;
                timeDifference = -1;

                if (isTracking)
                {
                    // Use the current user-set position as a first tracked point.
                    trackTimeline.Insert(context.Time, CreateTrackFrame(currentValue, 1.0f, PositionningSource.Manual));
                    timeDifference = 0;
                }
                
                return isTracking;
            }

            TrackingTemplate closestFrame = trackTimeline.ClosestFrom(context.Time);
            if (closestFrame.Template == null)
            {
                // This point has entries in the timeline but doesn't have the corresponding image pattern.
                // This happen when the timeline is imported from a KVA.
                currentValue = closestFrame.Location;
                timeDifference = Math.Abs(context.Time - closestFrame.Time);

                if (isTracking)
                {
                    // Make sure we extract the pattern and update the entry at this time.
                    // Note: the position we are using could come from a different time. 
                    // But since we are actively tracking we need to end up adding an entry at this time.
                    // But since we don't have any specific template to look for, we will just create an entry at the same location.
                    // If we are on the right time, perfect. If not, it will use the location from the closest and comit it to this time.
                    // When the user switched tracking ON for this drawing, they saw where the point was.
                    // If they moved it manually before changing frame, it will be handled in SetUserValue.
                    // If not, it means they are content with the position it has and thus this insertion is correct.
                    PositionningSource source = timeDifference == 0 ? closestFrame.PositionningSource : PositionningSource.ForcedClosest;
                    trackTimeline.Insert(context.Time, CreateTrackFrame(closestFrame.Location, 1.0f, source));
                    timeDifference = 0;
                }

                return isTracking;
            }

            if(closestFrame.Time == context.Time)
            {
                // We found an entry at the exact time requested.
                currentValue = closestFrame.Location;
                timeDifference = 0;
                return false;
            }

            if (!isTracking)
            {
                // We did not find the exact requested time in the timeline, and we are not currently tracking.
                currentValue = closestFrame.Location;
                timeDifference = Math.Abs(context.Time - closestFrame.Time);
                return false;
            }

            // We did not find the exact requested time in the timeline, but tracking is active so let's look for the pattern.
            TrackResult result = Tracker.Track(trackerParameters.SearchWindow, closestFrame, context);

            if(result.Similarity >= trackerParameters.SimilarityThreshold)
            {
                currentValue = result.Location;
                timeDifference = 0;
                
                if(result.Similarity > trackerParameters.TemplateUpdateThreshold)
                {
                    Bitmap template = BitmapHelper.Copy(closestFrame.Template);
                    TrackingTemplate newFrame = new TrackingTemplate(context.Time, result.Location, (float)result.Similarity, template, PositionningSource.TemplateMatching);
                    trackTimeline.Insert(context.Time, newFrame);
                }
                else
                {
                    trackTimeline.Insert(context.Time, CreateTrackFrame(result.Location, (float)result.Similarity, PositionningSource.TemplateMatching));  
                }

                inserted = true;
            }
            else
            {
                // Tracking failure.
                currentValue = closestFrame.Location;
                timeDifference = Math.Abs(context.Time - closestFrame.Time);
                inserted = false;
            }

            return inserted;
        }

        public PointF CameraTrack(TrackingContext context, CameraTransformer cameraTransformer, long referenceTimestamp)
        {
            if (isTracking || !cameraTransformer.Initialized)
                return currentValue;

            if (!cameraTrackCache.ContainsKey(context.Time))
            {
                // Since the point is not actively tracked currentValue is still the value at the reference timestamp.
                // (even if the user moves the point while on a different frame ?)
                PointF p = cameraTransformer.Transform(referenceTimestamp, context.Time, currentValue);
                cameraTrackCache[context.Time] = p;
            }
            
            return cameraTrackCache[context.Time];
        }


        public void ForceInsertClosestLocation()
        {
            // This function is used when a drawing containing multiple trackable points has some of the points failing 
            // the template matching and others succeeding, or when a point that wasn't in the timeline range is moved manually.
            // We must always keep the same number of entries in the timelines of all trackable points of a given drawing.
            // In this function we force the points that failed tracking to insert a dummy value in their timeline.
            TrackingTemplate closestFrame = trackTimeline.ClosestFrom(context.Time);
            if (closestFrame == null)
                return;

            currentValue = closestFrame.Location;
            timeDifference = Math.Abs(context.Time - closestFrame.Time);

            // If time difference is zero, we actually already had an entry at that time, so nothing more to do here.
            if (timeDifference > 0)
                trackTimeline.Insert(context.Time, CreateTrackFrame(currentValue, 1.0f, PositionningSource.ForcedClosest));
        }
        
        /// <summary>
        /// Import the current value of the point from the drawing into the non-tracking value.
        /// This is used in the context of camera tracking when we move the point manually.
        /// This means the reference timestamp has changed, so we should invalidate the cache.
        /// </summary>
        public void CommitNonTrackingValue(PointF value)
        {
            if (nonTrackingValue == value)
                return;

            nonTrackingValue = value;
            cameraTrackCache.Clear();
        }

        public void Reset()
        {
            ClearTimeline();
        }
       
        public bool SetTracking(bool isTracking)
        {
            if(this.isTracking == isTracking)
                return false;
            
            this.isTracking = isTracking;
            
            if(!isTracking)
            {
                currentValue = nonTrackingValue;
                timeDifference = long.MaxValue;
                return false;
            }

            if (context != null)
                return Track(context);
            else
                return false;
        }

        /// <summary>
        /// Returns the position of the point nearest to that time.
        /// This is used by linear kinematics.
        /// </summary>
        public PointF GetLocation(long time)
        {
            if (trackTimeline.Count == 0)
                return currentValue;

            TrackingTemplate closestFrame = trackTimeline.ClosestFrom(time);
            return closestFrame.Location;
        }

        /// <summary>
        /// Returns the position of the point suitable for KVA storage.
        /// </summary>
        public PointF GetNonTrackingValue()
        {
            return nonTrackingValue;
        }

        public void WriteXml(XmlWriter w)
        {
            w.WriteStartElement("TrackerParameters");
            trackerParameters.WriteXml(w);
            w.WriteEndElement();

            w.WriteElementString("NonTrackingValue", XmlHelper.WritePointF(nonTrackingValue));
            w.WriteElementString("CurrentValue", XmlHelper.WritePointF(currentValue));

            w.WriteStartElement("Timeline");
            foreach (TrackingTemplate frame in trackTimeline.Enumerate())
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
                        trackerParameters.ReadXml(r);
                        break;
                    case "NonTrackingValue":
                        nonTrackingValue = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        nonTrackingValue = nonTrackingValue.Scale(scale.X, scale.Y);
                        cameraTrackCache.Clear();
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
                        TrackingTemplate frame = new TrackingTemplate(r, scale, timeMapper);
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

        /// <summary>
        /// Creates a timeline entry (TrackFrame) from an existing location.
        /// Does not perform any tracking.
        /// Extracts the pattern from the image.
        /// </summary>
        private TrackingTemplate CreateTrackFrame(PointF location, float score, PositionningSource positionningSource)
        {
            Rectangle region = location.Box(trackerParameters.BlockWindow).ToRectangle();
            Bitmap template = context.Image.ExtractTemplate(region);
            return new TrackingTemplate(context.Time, location, score, template, positionningSource);
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
