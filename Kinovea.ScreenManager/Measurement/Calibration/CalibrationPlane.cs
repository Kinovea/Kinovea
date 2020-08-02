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
using System.Drawing;
using System.Globalization;
using System.Xml;

using Kinovea.Services;
using AForge.Math;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Packages necessary info for the calibration by plane.
    /// 
    /// Calibration by plane uses a user-specified quadrilateral defining a rectangle on the ground or wall,
    /// and maps the image coordinates with the system defined by the rectangle.
    /// We keep a separate user-defined world origin, that is expressed in the real-world coordinate system defined by the rectangle.
    /// The rectangle itself is referred to as the "Calibration" coordinate system.
    /// </summary>
    public class CalibrationPlane : ICalibrator
    {
        /// <summary>
        /// Real world dimension of the reference rectangle.
        /// </summary>
        public SizeF Size
        {
            get { return size; }
            set { size = value;}
        }

        /// <summary>
        /// Projection of the rectangle defining the world plane onto image space.
        /// </summary>
        public QuadrilateralF QuadImage
        {
            get { return quadImage; }
        }

        public bool Valid
        {
            get { return valid; }
        }

        public ProjectiveMapping ProjectiveMapping
        {
            get { return mapping; }
        }

        public bool Perspective
        {
            get { return perspective; }
        }

        public CalibrationAxis CalibrationAxis
        {
            get { return calibrationAxis; }
        }
        
        private bool initialized;
        private SizeF size;         // Real-world reference rectangle size.
        private QuadrilateralF quadImage = new QuadrilateralF();
        private bool valid;
        private bool perspective;
        private ProjectiveMapping mapping = new ProjectiveMapping();
        private PointF origin;      // User-defined origin, in calibrated plane coordinate system.
        private CalibrationAxis calibrationAxis = CalibrationAxis.LineHorizontal;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        #region ICalibrator
        /// <summary>
        /// Takes a point in image space and returns it world space, assuming static origin.
        /// </summary>
        public PointF Transform(PointF p)
        {
            if (!initialized)
                return p;

            return CalibratedToWorld(mapping.Backward(p), origin);
        }

        /// <summary>
        /// Takes a point and origin in image space and returns it world space.
        /// </summary>
        public PointF Transform(PointF p, PointF originImage)
        {
            if (!initialized)
                return p;

            PointF origin = mapping.Backward(originImage);
            return CalibratedToWorld(mapping.Backward(p), origin);
        }

        /// <summary>
        /// Takes a point in real world coordinates and gives it back in image coordinates.
        /// </summary>
        public PointF Untransform(PointF p)
        {
            return Untransform(p, origin);
        }

        public PointF Untransform(PointF p, PointF origin)
        {
            if (!initialized)
                return p;

            return mapping.Forward(WorldToCalibrated(p, origin));
        }

        /// <summary>
        /// Takes a point in real world coordinates and gives it back as an homogenous vector in the projective plane.
        /// </summary>
        public Vector3 Project(PointF p)
        {
            if (!initialized)
                return new Vector3(p.X, p.Y, 1.0f);

            PointF c = WorldToCalibrated(p, origin);
            Vector3 v = new Vector3(c.X, c.Y, 1.0f);

            return mapping.Forward(v);
        }

        /// <summary>
        /// Takes a point in image coordinates to act as the origin of the current coordinate system.
        /// </summary>
        public void SetOrigin(PointF p)
        {
            if (!initialized)
                return;

            origin = mapping.Backward(p);
        }

        #endregion

        public Vector3 Project(Vector3 v)
        {
            if (!initialized)
                return new Vector3();

            return mapping.Forward(v);
        }

        /// <summary>
        /// Initialize the projective mapping from a quadrilateral.
        /// size: Real world dimension of the reference rectangle.
        /// quadImage: Image coordinates of the reference rectangle.
        /// </summary>
        public void Initialize(SizeF sizeWorld, QuadrilateralF quadImage)
        {
            //PointF originImage = initialized ? Untransform(PointF.Empty) : quadImage.D;
            PointF originImage = quadImage.D;
            
            this.size = sizeWorld;
            this.quadImage = quadImage.Clone();
            mapping.Update(new QuadrilateralF(size.Width, size.Height), quadImage);
            origin = mapping.Backward(originImage);
            this.initialized = true;

            valid = quadImage.IsConvex;
            perspective = !quadImage.IsAxisAlignedRectangle;
        }

        /// <summary>
        /// Initialize the projective mapping from a line.
        /// length: Real world length of the line.
        /// a, b: Image coordinates of the line vertices.
        /// </summary>
        public void Initialize(float lengthWorld, PointF startImage, PointF endImage, CalibrationAxis calibrationAxis)
        {
            this.calibrationAxis = calibrationAxis;
            QuadrilateralF quadImage = MakeQuad(startImage, endImage, calibrationAxis);
            SizeF sizeWorld = new SizeF(lengthWorld, lengthWorld);

            Initialize(sizeWorld, quadImage);
            perspective = false;
        }

        /// <summary>
        /// Updates the calibration coordinate system without changing the real-world scale of the rectangle or the user-defined origin.
        /// Quadrilateral variant.
        /// </summary>
        public void Update(QuadrilateralF quadImage)
        {
            if (!initialized || size.IsEmpty)
            {
                valid = false;
                return;
            }

            this.quadImage = quadImage.Clone();
            mapping.Update(new QuadrilateralF(size.Width, size.Height), quadImage);
            valid = quadImage.IsConvex;
        }

        /// <summary>
        /// Updates the calibration coordinate system without changing the real-world scale of the rectangle or the user-defined origin.
        /// Line variant.
        /// </summary>
        public void Update(PointF startImage, PointF endImage)
        {
            Update(MakeQuad(startImage, endImage, calibrationAxis));
        }

        private PointF CalibratedToWorld(PointF p, PointF origin)
        {
            return new PointF(p.X - origin.X, -p.Y + origin.Y);
        }

        private PointF WorldToCalibrated(PointF p, PointF origin)
        {
            return new PointF(origin.X + p.X, origin.Y - p.Y);
        }

        /// <summary>
        /// Build a quadrilateral from a single line.
        /// The quadrilateral will be a square with the original line at the bottom edge.
        /// </summary>
        private QuadrilateralF MakeQuad(PointF start, PointF end, CalibrationAxis calibrationAxis)
        {
            switch (calibrationAxis)
            {
                case CalibrationAxis.LineHorizontal:
                {
                    // Rebuild a quadrilateral as a square, assuming the passed line is the bottom edge, left to right.
                    // The base quadrilateral is defined as ABCD going CW from top-left, 
                    // the line is making up the DC vector which will map to the +X axis.
                    PointF d = start;
                    PointF c = end;
                    PointF a = new PointF(d.X + (c.Y - d.Y), d.Y - (c.X - d.X));
                    PointF b = new PointF(a.X + (c.X - d.X), a.Y + (c.Y - d.Y));
                    return new QuadrilateralF(a, b, c, d);
                }
                case CalibrationAxis.LineVertical:
                {
                    // Rebuild a quadrilateral as a square, assuming the passed line is the left edge, bottom to top.
                    // The base quadrilateral is defined as ABCD going CW from top-left, 
                    // the line is making up the DA vector which will map to the +Y axis.
                    PointF d = start;
                    PointF a = end;
                    PointF c = new PointF(d.X + (d.Y - a.Y), d.Y + (a.X - d.X));
                    PointF b = new PointF(a.X + (c.X - d.X), a.Y + (c.Y - d.Y));
                    return new QuadrilateralF(a, b, c, d);
                }
                case CalibrationAxis.ImageAxes:
                default:
                {
                    // Rebuild a quadrilateral as a square aligned to the normal image axes (except Y goes up).
                    // The base quadrilateral is defined as ABCD going CW from top-left, 
                    // the line length is giving the length of the DC edge of the square.
                    float length = GeometryHelper.GetDistance(start, end);
                    PointF d = start;
                    PointF c = new PointF(d.X + length, d.Y);
                    PointF a = new PointF(d.X + (c.Y - d.Y), d.Y - (c.X - d.X));
                    PointF b = new PointF(a.X + (c.X - d.X), a.Y + (c.Y - d.Y));
                    return new QuadrilateralF(a, b, c, d);
                }
            }
        }

        #region Serialization
        public void WritePlaneXml(XmlWriter w)
        {
            w.WriteElementString("Size", XmlHelper.WriteSizeF(size));
            
            w.WriteStartElement("Quadrilateral");
            WritePointF(w, "A", quadImage.A);
            WritePointF(w, "B", quadImage.B);
            WritePointF(w, "C", quadImage.C);
            WritePointF(w, "D", quadImage.D);
            w.WriteEndElement();

            WritePointF(w, "Origin", origin);
        }

        public void WriteLineXml(XmlWriter w)
        {
            w.WriteElementString("Length", XmlHelper.WriteFloat(size.Width));

            w.WriteStartElement("Segment");
            w.WriteElementString("A", XmlHelper.WritePointF(quadImage.D));
            w.WriteElementString("B", XmlHelper.WritePointF(quadImage.C));
            w.WriteEndElement();

            WritePointF(w, "Origin", origin);
            w.WriteElementString("Axis", calibrationAxis.ToString());
        }

        private void WritePointF(XmlWriter w, string name, PointF p)
        {
            w.WriteElementString(name, XmlHelper.WritePointF(p));
        }
        public void ReadPlaneXml(XmlReader r, PointF scale)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "Size":
                        size = XmlHelper.ParseSizeF(r.ReadElementContentAsString());
                        break;
                    case "Quadrilateral":
                        quadImage = ParseQuadrilateral(r, scale);
                        break;
                    case "Origin":
                        origin = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            r.ReadEndElement();
            
            mapping.Update(new QuadrilateralF(size.Width, size.Height), quadImage);
            valid = quadImage.IsConvex;
            initialized = true;
        }

        public void ReadLineXml(XmlReader r, PointF scaling)
        {
            r.ReadStartElement();
            float length = 0;
            SegmentF line = SegmentF.Empty;
            calibrationAxis = CalibrationAxis.LineHorizontal;

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Length":
                        length = float.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "Segment":
                        line = ParseSegment(r, scaling);
                        break;
                    case "Origin":
                        // Import from older format.
                        origin = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        if (float.IsNaN(origin.X) || float.IsNaN(origin.Y))
                            origin = PointF.Empty;

                        origin = origin.Scale(scaling.X, scaling.Y);
                        break;
                    case "Axis":
                        calibrationAxis = (CalibrationAxis)Enum.Parse(typeof(CalibrationAxis), r.ReadElementContentAsString());
                        break;

                    case "Scale":
                        // Import and convert from older format.
                        // Create a fake line of 100 px horizontal at the origin.
                        float bakedScale = float.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        float lengthPixels = 100;
                        PointF start = origin;
                        PointF end = origin.Translate(lengthPixels, 0);
                        line = new SegmentF(start, end);
                        length = lengthPixels * bakedScale;
                        
                        // The actual origin should be expressed in the calibrated plane coordinate system, which has its true origin at the A point of the quad.
                        origin = new PointF(0, length);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            r.ReadEndElement();

            // Update mapping.
            size = new SizeF(length, length);
            quadImage = MakeQuad(line.Start, line.End, calibrationAxis);

            mapping.Update(new QuadrilateralF(size.Width, size.Height), quadImage);
            valid = quadImage.IsConvex;
            initialized = true;
        }

        private QuadrilateralF ParseQuadrilateral(XmlReader r, PointF scale)
        {
            r.ReadStartElement();
            PointF a = PointF.Empty;
            PointF b = PointF.Empty;
            PointF c = PointF.Empty;
            PointF d = PointF.Empty;

            while (r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "A":
                        a = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    case "B":
                        b = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    case "C":
                        c = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    case "D":
                        d = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            QuadrilateralF quad = new QuadrilateralF(a, b, c, d);
            quad.Scale(scale.X, scale.Y);

            r.ReadEndElement();

            return quad;
        }

        private SegmentF ParseSegment(XmlReader r, PointF scale)
        {
            r.ReadStartElement();
            PointF a = PointF.Empty;
            PointF b = PointF.Empty;

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "A":
                        a = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    case "B":
                        b = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            //.Scale(scale.X, scale.Y);

            SegmentF segment = new SegmentF(a, b);

            r.ReadEndElement();

            return segment;
        }
        #endregion
    }
}
