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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;
using System.Globalization;

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
        public string DisplayName { get; private set;}
        public Bitmap Icon { get; private set;}
        
        /// <summary>
        /// The low level list of points that make up the drawing.
        /// These may or may not be drawn or manipulatable.
        /// In the new system this is built from the Points.
        /// </summary>
        public List<PointF> PointList { get; private set; }

        /// <summary>
        /// List of sub-drawings similar to the marker tool.
        /// Points are drawn but not necessarily movable.
        /// To be movable there needs to be a handle referencing it.
        /// </summary>
        public List<GenericPosturePoint> Points { get; private set; }

        /// <summary>
        /// List of sub-drawings similar to the line tool.
        /// </summary>
        public List<GenericPostureSegment> Segments { get; private set;}

        /// <summary>
        /// List of sub-drawings similar to the poly line tool.
        /// </summary>
        public List<GenericPosturePolyline> Polylines { get; private set; }

        /// <summary>
        /// List of sub-drawings similar to the circle tool.
        /// </summary>
        public List<GenericPostureCircle> Circles { get; private set;}

        /// <summary>
        /// List of sub-drawings similar to the angle tool.
        /// </summary>
        public List<GenericPostureAngle> Angles { get; private set;}

        /// <summary>
        /// List of handles to manipulate various parts of the drawing 
        /// </summary>
        public List<GenericPostureHandle> Handles { get; private set; }
        
        /// <summary>
        /// List of polygons that define hit areas for moving the whole drawing at once
        /// as opposed to moving individual points or lines.
        /// </summary>
        public List<GenericPostureAbstractHitZone> HitZones { get; private set;}

        /// <summary>
        /// List of defined distances declared by the posture.
        /// This will show up as a mini label attached to the underlying segment.
        /// </summary>
        public List<GenericPostureDistance> Distances { get; private set;}
        
        /// <summary>
        /// List of defined positions.
        /// </summary>
        public List<GenericPosturePosition> Positions { get; private set;}
        
        /// <summary>
        /// List of points computed by weighted average of other points.
        /// </summary>
        public List<GenericPostureComputedPoint> ComputedPoints { get; private set;}

        /// <summary>
        /// Generic capabilities supported by the drawing like flipping.
        /// </summary>
        public GenericPostureCapabilities Capabilities { get; private set;}
        
        /// <summary>
        /// List of options declared by the drawing.
        /// Options enable/disable visibility and constraints on certain parts of the drawing.
        /// </summary>
        public Dictionary<string, GenericPostureOption> Options { get; private set; }

        public bool HasNonHiddenOptions { get; private set; }

        public TrackingProfile CustomTrackingProfile { get; private set; }
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
            DisplayName = "";
            Icon = null;
            this.FromKVA = fromKVA;
            
            PointList = new List<PointF>();

            Points = new List<GenericPosturePoint>();
            Segments = new List<GenericPostureSegment>();
            Circles = new List<GenericPostureCircle>();
            Angles = new List<GenericPostureAngle>();
            Polylines = new List<GenericPosturePolyline>();

            Handles = new List<GenericPostureHandle>();
            HitZones = new List<GenericPostureAbstractHitZone>();

            Distances = new List<GenericPostureDistance>();
            Positions = new List<GenericPosturePosition>();
            ComputedPoints = new List<GenericPostureComputedPoint>();

            Capabilities = GenericPostureCapabilities.None;
            Options = new Dictionary<string, GenericPostureOption>();

            CustomTrackingProfile = new TrackingProfile();
            
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
            	string formatVersion = r.ReadElementContentAsString("FormatVersion", "");
                CheckFormatVersion(formatVersion);

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
                        case "DisplayName":
                            DisplayName = r.ReadElementContentAsString();
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
                string formatVersion = r.ReadElementContentAsString("FormatVersion", "");
                CheckFormatVersion(formatVersion);

                while (r.NodeType == XmlNodeType.Element)
    			{
                    switch(r.Name)
    				{
                        // Header section
                        case "Id":
                            Id = new Guid(r.ReadElementContentAsString());
                            break;
                        case "Name":
                            Name = r.ReadElementContentAsString();
                            break;
                        case "DisplayName":
                            DisplayName = r.ReadElementContentAsString();
                            break;
                        case "Icon":
                             r.ReadOuterXml();
                            break;

                        // Data section

                        // Deprecated (1.0)
                        case "PointCount":
                            // Ignored.
                            r.ReadElementContentAsInt();
    						break;
                        case "InitialConfiguration":
                            ParseInitialConfiguration(r);
                            break;

                        case "Points":
                            ParsePoints(r);
                            break;
                        case "Segments":
    						ParseSegments(r);
    						break;
    					case "Ellipses":
                        case "Circles":
    						ParseCircles(r);
    						break;
    					case "Angles":
    						ParseAngles(r);
    						break;
                        case "Polylines":
                            ParsePolylines(r);
                            break;
                        // Interaction section
    					case "Handles":
    						ParseHandles(r);
    						break;
                        case "HitZone":
                            ParseHitZone(r);
                            break;

                        // Variables section
                        case "Distances":
    						ParseDistances(r);
    						break;
    				    case "Positions":
    						ParsePositions(r);
    						break;
                        case "ComputedPoints":
    						ParseComputedPoints(r);
    						break;

                        // Menus
                        case "Options":
                            ParseOptions(r);
                            break;
    		            case "Capabilities":
    						ParseCapabilities(r);
    						break;
                            
                        // Extra
                        case "TrackingProfile":
                            CustomTrackingProfile.ReadXml(r);
                            break;

                        default:
    						string unparsed = r.ReadOuterXml();
    						log.DebugFormat("Unparsed content in XML: {0}", unparsed);
    						break;
                    }
                }
                
                r.ReadEndElement();
                
                ConsolidateOptions();
            }
            catch(Exception e)
            {
                log.ErrorFormat("An error occurred during the parsing of a custom tool.");
                log.ErrorFormat(e.ToString());
            }
        }
        private void CheckFormatVersion(string version)
        {
            double format;
            bool read = double.TryParse(version, NumberStyles.Any, CultureInfo.InvariantCulture, out format);
            if (!read)
            {
                log.ErrorFormat("The format of the posture tool couldn't be read. {0}", version);
                return;
            }

            // We don't restrict on format for now.
            // But 1.0 will be deprecated in a future version.
        }

        #region Header
        private void ParseIcon(XmlReader r)
        {
            string base64 = r.ReadElementContentAsString();
            byte[] bytes = Convert.FromBase64String(base64);
            Icon = (Bitmap)Image.FromStream(new MemoryStream(bytes));
        }

        #endregion

        #region Data
        
        #region Deprecated
        private void ParseInitialConfiguration(XmlReader r)
        {
            r.ReadStartElement();
            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Point")
                {
                    PointF value = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                    Points.Add(new GenericPosturePoint(value));
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content: {0}", outerXml);
                }
            }

            r.ReadEndElement();

            foreach (var point in Points)
                PointList.Add(point.Value);
        }
        #endregion

        private void ParsePoints(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Point")
                {
                    Points.Add(new GenericPosturePoint(r));
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                }
            }

            r.ReadEndElement();

            foreach (var point in Points)
                PointList.Add(point.Value);
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
        private void ParseCircles(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Ellipse" || r.Name == "Circle")
                {
                    Circles.Add(new GenericPostureCircle(r));
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
        private void ParsePolylines(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Polyline")
                {
                    Polylines.Add(new GenericPosturePolyline(r));
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                }
            }

            r.ReadEndElement();
        }
        #endregion

        #region Interaction
        private void ParseHandles(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Handle")
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

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Polygon")
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
        #endregion

        #region Variables
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
        private void ParseComputedPoints(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "ComputedPoint")
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
        #endregion

        #region Menus
        private void ParseOptions(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Option")
                {
                    GenericPostureOption option = new GenericPostureOption(r);
                    if (!Options.ContainsKey(option.Key))
                        Options.Add(option.Key, option);
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

        /// <summary>
        /// Look for extra options that are declared directly at the object level.
        /// This is mostly a remnant of the older format where the options were only declared like this.
        /// The new format has an Options node with a proper list.
        /// </summary>
        private void ConsolidateOptions()
        {
            foreach(GenericPostureSegment segment in Segments)
                AddOption(segment.OptionGroup);
                
            foreach(GenericPostureHandle handle in Handles)
            {
                AddOption(handle.OptionGroup);
                
                if(handle.Constraint != null)
                    AddOption(handle.Constraint.OptionGroup);
            }
            
            foreach(GenericPostureCircle circle in Circles)
                AddOption(circle.OptionGroup);
                
            foreach(GenericPostureAngle angle in Angles)
                AddOption(angle.OptionGroup);
            
            foreach(GenericPostureDistance distance in Distances)
                AddOption(distance.OptionGroup);
            
            foreach(GenericPosturePosition position in Positions)
                AddOption(position.OptionGroup);
            
            foreach(GenericPostureComputedPoint computedPoint in ComputedPoints)
                AddOption(computedPoint.OptionGroup);

            HasNonHiddenOptions = Options.Values.Any(o => !o.Hidden);
        }

        /// <summary>
        /// Check if an option listed at the object level is already known and add it otherwise.
        /// </summary>
        private void AddOption(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            string[] keys = value.Split(new char[] { '|' });
            foreach (string key in keys)
            {
                bool known = Options.Values.Any(o => o.Label == key || o.Key == key);
                if (!known)
                {
                    GenericPostureOption option = new GenericPostureOption(key, key, false, false);
                    Options.Add(key, option);
                }
            }
        }
        #endregion

        #endregion

        #region Tracking support.
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            Dictionary<string, PointF> trackablePoints = new Dictionary<string, PointF>();
            
            foreach(int index in trackableIndices)
                trackablePoints.Add(index.ToString(), PointList[index]);
            
            return trackablePoints;
        }
        public void SignalAllTrackablePointsMoved(EventHandler<TrackablePointMovedEventArgs> trackablePointMoved)
        {
            foreach(int index in trackableIndices)
                trackablePointMoved(this, new TrackablePointMovedEventArgs(index.ToString(), PointList[index]));
        }
        public void SignalTrackablePointMoved(int handleIndex, EventHandler<TrackablePointMovedEventArgs> trackablePointMoved)
        {
            int index = Handles[handleIndex].Reference;
            trackablePointMoved(this, new TrackablePointMovedEventArgs(index.ToString(), PointList[index]));
        }
        public void SetTrackablePointValue(string name, PointF value, CalibrationHelper calibrationHelper, EventHandler<TrackablePointMovedEventArgs> trackablePointMoved)
        {
            // Value coming from tracking.
            int pointIndex = int.Parse(name);
            if(pointIndex >= PointList.Count)
                throw new ArgumentException("This point is not bound.");
            
            int handleIndex = Handles.FindIndex((h) => h.Reference == pointIndex);

            // Honor the constraint system.
            // Tracking can move the endpoint of a horizontal slide arbitrarily and we force it back.
            GenericPostureConstraintEngine.MoveHandle(this, calibrationHelper, handleIndex, value, Keys.None);

            // Store the final position into the tracking timeline for proper kinematics.
            if (PointList[pointIndex] != value && trackablePointMoved != null)
                SignalTrackablePointMoved(handleIndex, trackablePointMoved);
        }
        #endregion

        /// <summary>
        /// Assign a new value to a point by the name of the handle.
        /// This is used internally when setting up the posture from a file.
        /// For the normal user-interaction see GenericPostureConstraintEngine > MovePointHandle().
        /// </summary>
        public void AssignValue(string pointName, PointF value)
        {
            // Find the point by the name of the handle.
            for (int i = 0; i < Handles.Count; i++)
            {
                if (Handles[i].Name == pointName)
                {
                    PointList[Handles[i].Reference] = value;
                    return;
                }
            }
        }

        public void FlipHorizontal()
        {
            RectangleF boundingBox = GetBoundingBox();
            PointF center = boundingBox.Center();
            
            for(int i = 0; i<PointList.Count; i++)
            {
                float x = center.X + (center.X - PointList[i].X);
                PointList[i] = new PointF(x, PointList[i].Y);
            }
        }
        public void FlipVertical()
        {
            RectangleF boundingBox = GetBoundingBox();
            PointF center = boundingBox.Center();
            
            for(int i = 0; i<PointList.Count; i++)
            {
                float y = center.Y + (center.Y - PointList[i].Y);
                PointList[i] = new PointF(PointList[i].X, y);
            }
        }
        
        private RectangleF GetBoundingBox()
        {
            float left = float.MaxValue;
            float right = float.MinValue;
            float top = float.MaxValue;
            float bottom = float.MinValue;
            
            foreach(PointF p in PointList)
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
