#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Xml;

using Kinovea.Services;
using System.Xml.Serialization;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// SpotLight. (MultiDrawingItem of SpotlightManager)
    /// Describe and draw a single spotlight.
    /// </summary>
    [XmlType ("Spotlight")]
    public class Spotlight : AbstractMultiDrawingItem, IKvaSerializable, ITrackable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved; 
        #endregion

        #region Properties
        public string Name
        {
            get { return "Spotlight"; }
        }
        public override int ContentHash
        {
            get { return position.GetHashCode() ^ radius.GetHashCode() ^ points["o"].GetHashCode(); }
        }
        #endregion

        #region Members
        private long position;
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private long trackingTimestamps = -1;
        private int radius;
        private Rectangle rescaledRect;
        private static readonly int minimalRadius = 10;
        private static readonly int borderWidth = 2;
        private static readonly DashStyle dashStyle = DashStyle.Dash;
        private InfosFading infosFading;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public Spotlight(long position, long averageTimeStampsPerFrame, PointF center)
        {
            this.position = position;
            points["o"] = center;
            radius = minimalRadius;
            infosFading = new InfosFading(position, averageTimeStampsPerFrame);
            infosFading.UseDefault = false;
            infosFading.FadingFrames = 25;
        }
        public Spotlight(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata metadata)
            : this(0, 0, Point.Empty)
        {
             ReadXml(xmlReader, scale, timestampMapper);
             
             infosFading = new InfosFading(position, metadata.AverageTimeStampsPerFrame);
             infosFading.UseDefault = false;
             infosFading.FadingFrames = 25;
        }
        #endregion
        
        #region Public methods
        public double AddSpot(long timestamp, GraphicsPath path, IImageToViewportTransformer transformer)
        {
            // Add the shape of this spotlight to the global mask for the frame.
            // The dim rectangle is added separately in Spotlights class.
            double opacityFactor = infosFading.GetOpacityTrackable(trackingTimestamps, timestamp);
            if(opacityFactor <= 0)
                return 0;
            
            //RescaleCoordinates(_fStretchFactor, _DirectZoomTopLeft);
            Point center = transformer.Transform(points["o"]);
            int r = transformer.Transform(radius);
            rescaledRect = center.Box(r);
            path.AddEllipse(rescaledRect);
            
            // Return the opacity factor at this spot so the spotlights manager is able to compute the global dim value.
            return opacityFactor;
        }
        public void Draw(Graphics canvas, long timestamp)
        {
            // This just draws the border.
            double opacityFactor = infosFading.GetOpacityTrackable(trackingTimestamps, timestamp);
            if (opacityFactor <= 0)
                return;
        
            Color colorPenBorder = Color.FromArgb((int)((double)255 * opacityFactor), Color.White);
            using(Pen penBorder = new Pen(colorPenBorder, borderWidth))
            {
                penBorder.DashStyle = dashStyle;
                canvas.DrawEllipse(penBorder, rescaledRect);
            }
        }
        public int HitTest(PointF point, long timestamp, IImageToViewportTransformer transformer)
        {
            // Hit Result: -1: miss, 0: on object, 1 on handle.
            int result = -1;
            double opacity = infosFading.GetOpacityTrackable(trackingTimestamps, timestamp);
            if(opacity > 0)
            {
                if(IsPointOnHandler(point, transformer))
                    result = 1;
                else if (IsPointInObject(point, transformer))
                    result = 0;
            }
            return result;
        }
        public void MouseMove(float dx, float dy)
        {
            points["o"] = points["o"].Translate(dx, dy);
            SignalTrackablePointMoved();
        }
        public void MoveHandleTo(PointF point)
        {
            // Point coordinates are descaled.
            // User is dragging the outline of the circle, figure out the new radius at this point.
            float shiftX = Math.Abs(point.X - points["o"].X);
            float shiftY = Math.Abs(point.Y - points["o"].Y);
            radius = Math.Max((int)Math.Sqrt((shiftX*shiftX) + (shiftY*shiftY)), minimalRadius);
        }
        #endregion
        
        #region ITrackable implementation and support.
        public Color Color
        {
            get { return Color.Black; }
        }
        public TrackingProfile CustomTrackingProfile
        {
            get { return null; }
        }
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            return points;
        }
        public void SetTrackablePointValue(string name, PointF value, long trackingTimestamps)
        {
            if(!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");
            
            points[name] = value;
            this.trackingTimestamps = trackingTimestamps;
        }
        private void SignalTrackablePointMoved()
        {
            if(TrackablePointMoved == null)
                return;
            
            TrackablePointMoved(this, new TrackablePointMovedEventArgs("o", points["o"]));
        }
        #endregion
        
        #region Private methods
        private bool IsPointInObject(PointF point, IImageToViewportTransformer transformer)
        {
            using(GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(points["o"].X - radius, points["o"].Y - radius, radius*2, radius*2);
                return HitTester.HitTest(path, point, 0, true, transformer);
            }
        }
        private bool IsPointOnHandler(PointF point, IImageToViewportTransformer transformer)
        {
            using(GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(points["o"].X - radius, points["o"].Y - radius, radius*2, radius*2, 0, 360);
                return HitTester.HitTest(path, point, 2, false, transformer);
            }
        }
        #endregion
        
        #region KVA Serialization
        public void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timeStampMapper)
        {
            if (timeStampMapper == null)
                timeStampMapper = TimeHelper.IdentityTimestampMapper;
            
            if (xmlReader.MoveToAttribute("id"))
                identifier = new Guid(xmlReader.ReadContentAsString());

            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "Time":
                        position = timeStampMapper(xmlReader.ReadElementContentAsLong());
                        break;
                    case "Center":
                        PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        points["o"] = p.Scale(scale.X, scale.Y);
                        break;
                    case "Radius":
                        radius = xmlReader.ReadElementContentAsInt();
                        float minScale = Math.Min(scale.X, scale.Y);
                        radius = (int)(radius * minScale);
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            xmlReader.ReadEndElement();
            SignalTrackablePointMoved();
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if ((filter & SerializationFilter.Core) == SerializationFilter.Core)
            {
                w.WriteElementString("Time", position.ToString());
                w.WriteElementString("Center", XmlHelper.WritePointF(points["o"]));
                w.WriteElementString("Radius", radius.ToString());
            }
        }
        #endregion
    }
}

