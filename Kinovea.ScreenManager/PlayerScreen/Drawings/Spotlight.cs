#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// SpotLight. (MultiDrawingItem of SpotlightManager)
    /// Describe and draw a single spotlight.
    /// </summary>
    public class Spotlight : IKvaSerializable, ITrackable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved; 
        #endregion
        
        #region Members
        private long position;
        
        private Guid id = Guid.NewGuid();
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private bool tracking;
        
        private int radius;
        private Rectangle rescaledRect;
        
        private static readonly int minimalRadius = 10;
        private static readonly int borderWidth = 2;
        private static readonly DashStyle dashStyle = DashStyle.Dash;
        private InfosFading infosFading;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public Spotlight(long _iPosition, long _iAverageTimeStampsPerFrame, Point _center)
        {
            position = _iPosition;
            points["o"] = _center;
            radius = minimalRadius;
            infosFading = new InfosFading(_iPosition, _iAverageTimeStampsPerFrame);
            infosFading.UseDefault = false;
            infosFading.FadingFrames = 25;
        }
        public Spotlight(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback, long _iAverageTimeStampsPerFrame)
            : this(0, 0, Point.Empty)
        {
             ReadXml(_xmlReader, _scale, _remapTimestampCallback);
             
             infosFading = new InfosFading(position, _iAverageTimeStampsPerFrame);
             infosFading.UseDefault = false;
             infosFading.FadingFrames = 25;
        }
        #endregion
        
        #region Public methods
        public double AddSpot(long timestamp, GraphicsPath path, IImageToViewportTransformer transformer)
        {
            // Add the shape of this spotlight to the global mask for the frame.
            // The dim rectangle is added separately in Spotlights class.
            double opacityFactor = infosFading.GetOpacityFactor(timestamp);
            
            if(tracking)
                opacityFactor = 1.0;
            
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
            double opacityFactor = infosFading.GetOpacityFactor(timestamp);
            
            if(tracking)
                opacityFactor = 1.0;
            
            if(opacityFactor <= 0)
                return;
        
            Color colorPenBorder = Color.FromArgb((int)((double)255 * opacityFactor), Color.White);
            using(Pen penBorder = new Pen(colorPenBorder, borderWidth))
            {
                penBorder.DashStyle = dashStyle;
                canvas.DrawEllipse(penBorder, rescaledRect);
            }
        }
        public int HitTest(Point point, long timeStamp, IImageToViewportTransformer transformer)
        {
            // Hit Result: -1: miss, 0: on object, 1 on handle.
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(timeStamp);
            if(tracking || opacity > 0)
            {
                if(IsPointOnHandler(point))
                    result = 1;
                else if (IsPointInObject(point))
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
        public Guid ID
        {
            get { return id; }
        }
        public TrackingProfile CustomTrackingProfile
        {
            get { return null; }
        }
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            return points;
        }
        public void SetTracking(bool tracking)
        {
            this.tracking = tracking;
        }
        public void SetTrackablePointValue(string name, PointF value)
        {
            if(!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");
            
            points[name] = value;
        }
        private void SignalTrackablePointMoved()
        {
            if(TrackablePointMoved == null)
                return;
            
            TrackablePointMoved(this, new TrackablePointMovedEventArgs("o", points["o"]));
        }
        #endregion
        
        #region Private methods
        private bool IsPointInObject(Point point)
        {
            bool hit = false;
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                areaPath.AddEllipse(points["o"].X - radius, points["o"].Y - radius, radius*2, radius*2);
                using(Region areaRegion = new Region(areaPath))
                {
                    hit = areaRegion.IsVisible(point);
                }
            }
            return hit;
        }
        private bool IsPointOnHandler(Point point)
        {
            bool hit = false;
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                areaPath.AddArc(points["o"].X - radius, points["o"].Y - radius, radius*2, radius*2, 0, 360);
                using(Pen areaPen = new Pen(Color.Black, 10))
                {
                    areaPath.Widen(areaPen);
                }
                using(Region r = new Region(areaPath))
                {
                    hit = r.IsVisible(point);
                }
            }
            return hit;
        }
        #endregion
        
        #region KVA Serialization
        private void ReadXml(XmlReader xmlReader, PointF scale, TimeStampMapper timeStampMapper)
        {
            if(timeStampMapper == null)
            {
                xmlReader.ReadOuterXml();
                return;                
            }
            
            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "Time":
                        position = timeStampMapper(xmlReader.ReadElementContentAsLong(), false);
                        break;
                    case "Center":
                        Point p = XmlHelper.ParsePoint(xmlReader.ReadElementContentAsString());
                        points["o"] = p.Scale(scale.X, scale.Y);
                        break;
                    case "Radius":
                        radius = xmlReader.ReadElementContentAsInt();
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            xmlReader.ReadEndElement();
        }
        public void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteElementString("Time", position.ToString());
            xmlWriter.WriteElementString("Center", string.Format(CultureInfo.InvariantCulture, "{0};{1}", points["o"].X, points["o"].Y));
            xmlWriter.WriteElementString("Radius", radius.ToString());
        }
        #endregion
    }
}

