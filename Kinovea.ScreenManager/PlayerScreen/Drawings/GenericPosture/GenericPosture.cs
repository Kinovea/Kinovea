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
        public List<GenericPosturePosition> Positions { get; private set;}
        public List<GenericPostureHandle> Handles { get; private set; }
        public List<GenericPostureComputedPoint> ComputedPoints { get; private set;}
        public List<GenericPostureAbstractHitZone> HitZones { get; private set;}
        public TrackingProfile CustomTrackingProfile { get; private set; }
        public GenericPostureCapabilities Capabilities { get; private set;}
        public Dictionary<string, bool> OptionGroups { get; private set;}
        
        public bool Trackable { get; private set;}
        public bool FromKVA { get; private set;}
        #endregion
        
        #region Members
        private List<int> trackableIndices = new List<int>();
        private List<string> defaultOptions = new List<string>();
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
            Positions = new List<GenericPosturePosition>();
            ComputedPoints = new List<GenericPostureComputedPoint>();
            HitZones = new List<GenericPostureAbstractHitZone>();
            Capabilities = GenericPostureCapabilities.None;
            CustomTrackingProfile = null;
            OptionGroups = new Dictionary<string, bool>();
            
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
                        case "Icon":
                             r.ReadOuterXml();
                            break;

                        case "Id":
                            Id = new Guid(r.ReadElementContentAsString());
                            break;
                        case "Name":
                            Name = r.ReadElementContentAsString();
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
    				    case "Positions":
    						ParsePositions(r);
    						break;
    					case "Handles":
    						ParseHandles(r);
    						break;
                        case "ComputedPoints":
    						ParseComputedPoints(r);
    						break;
                        case "HitZone":
    						ParseHitZone(r);
    						break;
                        case "TrackingProfile":
                            ParseTrackingProfile(r);
                            break;
                        case "DefaultOptions":
    						ParseDefaultOptions(r);
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
                
                ImportOptionGroups();
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
        private void ParsePositions(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Position")
                {
                    Positions.Add(new GenericPosturePosition(r));
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
        private void ParseComputedPoints(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "ComputedPoint")
                {
                    ComputedPoints.Add(new GenericPostureComputedPoint(r));
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                }
            }
            
            r.ReadEndElement();
        }
        private void ParseTrackingProfile(XmlReader r)
        {
            TrackingProfile classic = new TrackingProfile();
            double similarityThreshold = classic.SimilarityThreshold;
            double updateThreshold = classic.TemplateUpdateThreshold;
            Size searchWindow = classic.SearchWindow;
            Size blockWindow = classic.BlockWindow;
            TrackerParameterUnit searchWindowUnit = classic.SearchWindowUnit;
            TrackerParameterUnit blockWindowUnit = classic.BlockWindowUnit;
            bool resetOnMove = classic.ResetOnMove;

            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "SimilarityThreshold":
                        similarityThreshold = r.ReadElementContentAsDouble();
                        break;
                    case "UpdateTemplateThreshold":
                        updateThreshold = r.ReadElementContentAsDouble();
                        break;
                    case "SearchWindow":
                        if (r.MoveToAttribute("unit"))
                            searchWindowUnit = (TrackerParameterUnit)Enum.Parse(typeof(TrackerParameterUnit), r.ReadContentAsString());

                        r.ReadStartElement();
                        searchWindow = XmlHelper.ParseSize(r.ReadContentAsString());
                        r.ReadEndElement();
                        break;
                    case "BlockWindow":
                        if (r.MoveToAttribute("unit"))
                            blockWindowUnit = (TrackerParameterUnit)Enum.Parse(typeof(TrackerParameterUnit), r.ReadContentAsString());

                        r.ReadStartElement();
                        blockWindow = XmlHelper.ParseSize(r.ReadContentAsString());
                        r.ReadEndElement();
                        break;
                    case "ResetOnMove":
                        resetOnMove = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    default:
                        string outerXml = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                        break;
                }
            }
            
            r.ReadEndElement();

            string name = Guid.NewGuid().ToString();
            CustomTrackingProfile = new TrackingProfile(name, similarityThreshold, updateThreshold, searchWindow, blockWindow, searchWindowUnit, blockWindowUnit, resetOnMove);
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
        private void ParseDefaultOptions(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "OptionGroup")
                {
                    defaultOptions.Add(r.ReadElementContentAsString());
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
                        Points[index] = XmlHelper.ParsePointF(r.ReadElementContentAsString());
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
        private void ImportOptionGroups()
        {
            foreach(GenericPostureSegment segment in Segments)
                AddOption(segment.OptionGroup);
                
            foreach(GenericPostureHandle handle in Handles)
            {
                AddOption(handle.OptionGroup);
                
                if(handle.Constraint != null)
                    AddOption(handle.Constraint.OptionGroup);
            }
            
            foreach(GenericPostureEllipse ellipse in Ellipses)
                AddOption(ellipse.OptionGroup);
                
            foreach(GenericPostureAngle angle in Angles)
                AddOption(angle.OptionGroup);
            
            foreach(GenericPostureDistance distance in Distances)
                AddOption(distance.OptionGroup);
            
            foreach(GenericPosturePosition position in Positions)
                AddOption(position.OptionGroup);
            
            foreach(GenericPostureComputedPoint computedPoint in ComputedPoints)
                AddOption(computedPoint.OptionGroup);
            
            foreach(string defaultOption in defaultOptions)
            {
                if(OptionGroups.ContainsKey(defaultOption))
                    OptionGroups[defaultOption] = true;
            }
        }
        private void AddOption(string option)
        {
            if(!string.IsNullOrEmpty(option) && !OptionGroups.ContainsKey(option))
               OptionGroups.Add(option, false);
        }
        #endregion
   
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            Dictionary<string, PointF> trackablePoints = new Dictionary<string, PointF>();
            
            foreach(int index in trackableIndices)
                trackablePoints.Add(index.ToString(), Points[index]);
            
            return trackablePoints;
        }
        
        public void SignalAllTrackablePointsMoved(EventHandler<TrackablePointMovedEventArgs> trackablePointMoved)
        {
            foreach(int index in trackableIndices)
                trackablePointMoved(this, new TrackablePointMovedEventArgs(index.ToString(), Points[index]));
        }

        public void SignalTrackablePointMoved(int handleIndex, EventHandler<TrackablePointMovedEventArgs> trackablePointMoved)
        {
            int index = Handles[handleIndex].Reference;
            trackablePointMoved(this, new TrackablePointMovedEventArgs(index.ToString(), Points[index]));
        }

        public void SetTrackablePointValue(string name, PointF value, CalibrationHelper calibrationHelper)
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
