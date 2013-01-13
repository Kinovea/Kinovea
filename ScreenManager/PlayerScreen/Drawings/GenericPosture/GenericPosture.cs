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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Support class for custom drawings.
    /// The class takes the drawing shape and behavior from an XML file.
    /// </summary>
    public class GenericPosture
    {
        #region Properties
        public Guid Id { get; private set;}
        public string Name { get; private set;}
        public Bitmap Icon { get; private set;}
        
        public List<PointF> Points { get; private set; }
        public List<GenericPostureSegment> Segments { get; private set;}
        public List<GenericPostureEllipse> Ellipses { get; private set;}
        public List<GenericPostureAngle> Angles { get; private set;}
        public List<GenericPostureDistance> Distances { get; private set;}
        public List<GenericPostureHandle> Handles { get; private set; }
        public List<GenericPostureAbstractHitZone> HitZones { get; private set;}
        public GenericPostureCapabilities Capabilities { get; private set;}
        public bool Trackable { get; private set;}
        public bool FromKVA { get; private set;}
        #endregion
        
        #region Members
        private List<int> trackableIndices = new List<int>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public GenericPosture(string descriptionFile, bool info, bool fromKVA)
        {
            Id = Guid.Empty;
            Name = "";
            Icon = null;
            this.FromKVA = fromKVA;
            
            Points = new List<PointF>();
            Segments = new List<GenericPostureSegment>();
            Ellipses = new List<GenericPostureEllipse>();
            Handles = new List<GenericPostureHandle>();
            Angles = new List<GenericPostureAngle>();
            Distances = new List<GenericPostureDistance>();
            HitZones = new List<GenericPostureAbstractHitZone>();
            Capabilities = GenericPostureCapabilities.None;
            
            if(string.IsNullOrEmpty(descriptionFile))
                return;
            
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            XmlReader reader = XmlReader.Create(descriptionFile, settings);
            if(info)
            {
                ReadInfoXml(reader);
            }
            else
            {
                ReadXml(reader);
                trackableIndices = Handles.Where(h => h.Trackable).Select(h => h.Reference).ToList();
                Trackable = trackableIndices.Count > 0;
            }
            
            reader.Close();
        }
        #endregion
        
        #region Serialization - Reading
        private void ReadInfoXml(XmlReader r)
        {
            try
            {
                r.MoveToContent();
                
                if(!(r.Name == "KinoveaPostureTool"))
            	    return;
                
            	r.ReadStartElement();
            	r.ReadElementContentAsString("FormatVersion", "");
            	
            	while(r.NodeType == XmlNodeType.Element)
    			{
                    switch(r.Name)
    				{
                        case "Id":
                            Id = new Guid(r.ReadElementContentAsString());
                            break;
                        case "Name":
                            Name = r.ReadElementContentAsString();
                            break;
                        case "Icon":
                            ParseIcon(r);
                            break;
                        default:
    						r.ReadOuterXml();
    						break;
                    }
                }
                
                r.ReadEndElement();
            }
            catch(Exception e)
            {
                log.ErrorFormat("An error occurred during the parsing of a custom tool.");
                log.ErrorFormat(e.ToString());
            }
        }
        private void ReadXml(XmlReader r)
        {
            try
            {
                r.MoveToContent();
                
                if(!(r.Name == "KinoveaPostureTool"))
            	    return;
                
            	r.ReadStartElement();
            	r.ReadElementContentAsString("FormatVersion", "");
            	
            	while(r.NodeType == XmlNodeType.Element)
    			{
                    switch(r.Name)
    				{
                        case "Name":
                        case "Icon":
                             r.ReadOuterXml();
                            break;
                        case "Id":
                            Id = new Guid(r.ReadElementContentAsString());
                            break;
                        case "PointCount":
                            ParsePointCount(r);
    						break;
                        case "Segments":
    						ParseSegments(r);
    						break;
    					case "Ellipses":
    						ParseEllipses(r);
    						break;
    					case "Angles":
    						ParseAngles(r);
    						break;
    				    case "Distances":
    						ParseDistances(r);
    						break;
    					case "Handles":
    						ParseHandles(r);
    						break;
                        case "HitZone":
    						ParseHitZone(r);
    						break;
    		            case "Capabilities":
    						ParseCapabilities(r);
    						break;
    					case "InitialConfiguration":
    						ParseInitialConfiguration(r);
    						break;
                        default:
    						string unparsed = r.ReadOuterXml();
    						log.DebugFormat("Unparsed content in XML: {0}", unparsed);
    						break;
                    }
                }
                
                r.ReadEndElement();
            }
            catch(Exception e)
            {
                log.ErrorFormat("An error occurred during the parsing of a custom tool.");
                log.ErrorFormat(e.ToString());
            }
        }
        private void ParseIcon(XmlReader r)
        {
            string base64 = r.ReadElementContentAsString();
            byte[] bytes = Convert.FromBase64String(base64);
            Icon = (Bitmap)Image.FromStream(new MemoryStream(bytes));
        }
        private void ParsePointCount(XmlReader r)
        {
            int pointCount = r.ReadElementContentAsInt();
            for(int i=0;i<pointCount;i++)
                Points.Add(Point.Empty);
        }
        private void ParseSegments(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Segment")
                {
                    Segments.Add(new GenericPostureSegment(r));
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                }
            }
            
            r.ReadEndElement();
        }
        private void ParseEllipses(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Ellipse")
                {
                    Ellipses.Add(new GenericPostureEllipse(r));
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                }
            }
            
            r.ReadEndElement();
        }
        private void ParseAngles(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Angle")
                {
                    Angles.Add(new GenericPostureAngle(r));
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                }
            }
            
            r.ReadEndElement();
        }
        private void ParseDistances(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Distance")
                {
                    Distances.Add(new GenericPostureDistance(r));
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                }
            }
            
            r.ReadEndElement();
        }
        private void ParseHandles(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Handle")
                {
                    Handles.Add(new GenericPostureHandle(r));
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                }
            }
            
            r.ReadEndElement();
        }
        private void ParseHitZone(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Polygon")
                {
                    HitZones.Add(new GenericPostureHitZonePolygon(r));
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                }
            }
            
            r.ReadEndElement();
        }
        private void ParseCapabilities(XmlReader r)
        {
            // Note: must be an empty tag.
            if(r.MoveToAttribute("flipHorizontal"))
            {
                bool cap = r.ReadContentAsBoolean();
                if(cap)
                    Capabilities |= GenericPostureCapabilities.FlipHorizontal;
            }
            
            if(r.MoveToAttribute("flipVertical"))
            {
                bool cap = r.ReadContentAsBoolean();
                if(cap)
                    Capabilities |= GenericPostureCapabilities.FlipVertical;
            }
            
            r.ReadStartElement();
        }
        private void ParseInitialConfiguration(XmlReader r)
        {
            r.ReadStartElement();
            int index = 0;
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Point")
                {
                    if(index < Points.Count)
                    {
                        Points[index] = XmlHelper.ParsePoint(r.ReadElementContentAsString());
                        index++;
                    }
                    else
                    {
                        string outerXml = r.ReadOuterXml();
                        log.DebugFormat("Unparsed point in initial configuration: {0}", outerXml);
                    }
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content: {0}", outerXml);
                }
            }
            
            r.ReadEndElement();
        }
        #endregion
   
        public Dictionary<string, Point> GetTrackablePoints()
        {
            Dictionary<string, Point> trackablePoints = new Dictionary<string, Point>();
            
            // TODO: generalize the use of PointF.
            foreach(int index in trackableIndices)
            {
                Point p = new Point((int)Points[index].X, (int)Points[index].Y);
                trackablePoints.Add(index.ToString(), p);
            }
            
            return trackablePoints;
        }
        
        public void SignalAllTrackablePointsMoved(EventHandler<TrackablePointMovedEventArgs> trackablePointMoved)
        {
            // TODO: generalize the use of PointF.
            foreach(int index in trackableIndices)
            {
                Point p = new Point((int)Points[index].X, (int)Points[index].Y);
                trackablePointMoved(this, new TrackablePointMovedEventArgs(index.ToString(), p));
            }
        }
              
        public void SetTrackablePointValue(string name, Point value, CalibrationHelper calibrationHelper)
        {
            // Value coming from tracking.
            int pointIndex = int.Parse(name);
            if(pointIndex >= Points.Count)
                throw new ArgumentException("This point is not bound.");
            
            int handleIndex = Handles.FindIndex((h) => h.Reference == pointIndex);
            GenericPostureConstraintEngine.MoveHandle(this, calibrationHelper, handleIndex, value, Keys.None);
        }
    
        public void FlipHorizontal()
        {
            RectangleF boundingBox = GetBoundingBox();
            PointF center = boundingBox.Center();
            
            for(int i = 0; i<Points.Count; i++)
            {
                float x = center.X + (center.X - Points[i].X);
                Points[i] = new PointF(x, Points[i].Y);
            }
        }

        public void FlipVertical()
        {
            RectangleF boundingBox = GetBoundingBox();
            PointF center = boundingBox.Center();
            
            for(int i = 0; i<Points.Count; i++)
            {
                float y = center.Y + (center.Y - Points[i].Y);
                Points[i] = new PointF(Points[i].X, y);
            }
        }
        
        private RectangleF GetBoundingBox()
        {
            float left = float.MaxValue;
            float right = float.MinValue;
            float top = float.MaxValue;
            float bottom = float.MinValue;
            
            foreach(PointF p in Points)
            {
                left = Math.Min(p.X, left);
                right = Math.Max(p.X, right);
                top = Math.Min(p.Y, top);
                bottom = Math.Max(p.Y, bottom);
            }
            
            return new RectangleF(left, top, right - left, bottom - top);
        }
    }
}
