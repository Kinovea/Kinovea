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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Converts coordinates and distances between image space and world space. 
    /// This implements both the calibration by plane and by line, using a homography (quad to quad mapping).
    /// 
    /// The user specifies a quadrilateral in the image corresponding to a rectangle of known size in world space.
    /// This correspondance is done in the UI via Perspective grid > Calibrate. 
    /// It is the main link between image space and world space. 
    /// The homography contained in the "ProjectiveMapper" maps coordinates in the rectified image space 
    /// to coordinates in the grid in the world.
    ///
    /// Full transform stack: 
    /// - Viewport space > Image space > Rectified image space >>> Grid space > World space > Offset space.
    ///                                                         ↑
    ///                                                     homography
    /// 
    /// Details:
    /// - Viewport space: coordinates on the screen including stretch, zoom and pan. Origin at top-left and Y-axis down.
    /// - Image space: coordinates based on the original video image size. Top-left, Y down.
    /// - Rectified image space: coordinates passed through radial distortion correction. Top-left, Y down.
    /// - Grid space: World coordinates based on the grid used for calibration. Top-left, Y down.
    /// - World space: based on the "Coordinate system" object. By default it is aligned with the grid bottom-left. Y-axis up.
    /// - Offset space: an offset is applied to the values. This is like moving the coordinate system origin but the axes are visually kept in place.
    ///
    /// Rectified image space
    /// - Anything image point or quad coming into this class should already be in rectified image space (undistorted).
    /// 
    /// Grid to World
    /// - We keep the offset of the coordinate system object origin. This is expressed in world units but in Grid space.
    /// 
    /// World to Offset:
    /// - Offset is useful if the origin would be outside of the image.
    /// 
    /// Mapping update: 
    /// - Any change in the corners of the grid object used for calibration redefines the mapping.
    /// 
    /// Tracking:
    /// - Both the grid used for calibration and the coordinate system object can be tracked independently. 
    /// - Since changes in the grid redefines the whole mapping, if both are tracked we give precedence to the grid.
    /// 
    /// </summary>
    public class CalibratorPlane
    {
        #region Properties
        /// <summary>
        /// Real world dimensions of the reference rectangle.
        /// The reference rectangle is represented by the Grid object used for calibration.
        /// </summary>
        public SizeF Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// World space quadrilateral (rectangle).
        /// Returns a copy.
        /// </summary>
        public QuadrilateralF QuadWorld
        {
            get { return quadWorld.Clone(); }
        }

        /// <summary>
        /// Rectified image space quadrilateral.
        /// Returns a copy.
        /// </summary>
        public QuadrilateralF QuadImage
        {
            get { return quadImage.Clone(); }
        }

        /// <summary>
        /// The core projective mapping used to transfom points 
        /// from rectified image space to world space and back.
        /// This does not take into account the custom origin and value offset.
        /// This should not be modified directly, use Initialize() instead.
        /// </summary>
        public ProjectiveMapper Mapper
        {
            get { return mapping; }
        }

        /// <summary>
        /// Offset in world units applied to values on top of the transform stack.
        /// </summary>
        public PointF Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        /// <summary>
        /// Whether the calibration is valid.
        /// A calibration can be invalid if the grid object used is not convex.
        /// </summary>
        public bool Valid
        {
            get { return valid; }
        }

        /// <summary>
        /// When using Calibration by line this defines whether the line represents the X, Y or neither axes.
        /// </summary>
        public CalibrationAxis CalibrationAxis
        {
            get { return calibrationAxis; }
        }
        #endregion

        #region Members
        private SizeF size;         // Real world dimension of the reference rectangle.
        private PointF origin;      // Origin of the coordinate system object with regards to the grid.
                                    // (= offset to the top-left of the grid, in world units).
        private PointF offset;      // Offset applied to values.
        private CalibrationAxis calibrationAxis = CalibrationAxis.LineHorizontal;

        private QuadrilateralF quadWorld = new QuadrilateralF();
        private QuadrilateralF quadImage = new QuadrilateralF();
        private ProjectiveMapper mapping = new ProjectiveMapper();
        
        private bool initialized;
        private bool valid;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        /// <summary>
        /// Takes a point in image space and returns it world space with offset. Assumes static origin.
        /// If the coordinate system object is tracked, you should first obtain the current origin 
        /// for the requested time separately and then call the overload taking a custom origin.
        /// The point coordinates should be in rectified image space.
        /// </summary>
        public PointF Transform(PointF p)
        {
            if (!initialized)
                return p;

            return GridToWorldWithOffset(mapping.Backward(p), origin);
        }

        /// <summary>
        /// Takes a point and origin in image space and returns it world space with offset.
        /// The point and origin coordinates should be in rectified image space.
        /// </summary>
        public PointF Transform(PointF p, PointF originInImage)
        {
            if (!initialized)
                return p;

            PointF origin = mapping.Backward(originInImage);
            return GridToWorldWithOffset(mapping.Backward(p), origin);
        }

        /// <summary>
        /// Takes a point in world coordinates (based on coordinate system object origin but without the extra offset),
        /// and returns it in rectified image space. Assumes static origin.
        /// If the coordinate system object is tracked, you should obtain the current origin 
        /// for the requested time separately and then call the overload taking a custom origin.
        /// The returned point is in rectified image space.
        /// </summary>
        public PointF Untransform(PointF p)
        {
            return Untransform(p, origin);
        }

        /// <summary>
        /// Takes a point in world coordinates (based on coordinate system object origin but without the extra offset),
        /// and returns it in rectified image space.
        /// </summary>
        public PointF Untransform(PointF p, PointF origin)
        {
            if (!initialized)
                return p;

            return mapping.Forward(WorldToGrid(p, origin));
        }
        
        /// <summary>
        /// Takes a point in real world coordinates and gives it back as an homogenous vector in the projective plane.
        /// Does not take the value offset into account.
        /// </summary>
        public Vector3 Project(PointF p)
        {
            if (!initialized)
                return new Vector3(p.X, p.Y, 1.0f);

            PointF c = WorldToGrid(p, origin);
            Vector3 v = new Vector3(c.X, c.Y, 1.0f);

            return mapping.Forward(v);
        }

        /// <summary>
        /// Takes a point in rectified image space to act as the origin of the current coordinate system.
        /// </summary>
        public void SetOrigin(PointF p)
        {
            if (!initialized)
                return;

            origin = mapping.Backward(p);
        }

        /// <summary>
        /// Realign the coordinate system object with the bottom left corner of the grid.
        /// </summary>
        public void ResetOrigin()
        {
            origin = mapping.Backward(quadImage.D);
        }

        public Vector3 Project(Vector3 v)
        {
            if (!initialized)
                return new Vector3();

            return mapping.Forward(v);
        }

        /// <summary>
        /// Initialize the projective mapping from a quadrilateral.
        /// size: Real world dimension of the reference rectangle.
        /// quadImage: Rectified image space coordinates of the rectangle.
        /// </summary>
        public void Initialize(SizeF sizeWorld, QuadrilateralF quadImage)
        {
            PointF originImage = quadImage.D;
            
            this.size = sizeWorld;
            this.quadWorld = new QuadrilateralF(size.Width, size.Height);
            this.quadImage = quadImage.Clone();
            mapping.Update(quadWorld, quadImage);
            origin = mapping.Backward(originImage);
            offset = PointF.Empty;
            valid = quadImage.IsConvex;
            this.initialized = true;
        }

        /// <summary>
        /// Initialize the projective mapping from a line.
        /// length: Real world length of the line.
        /// a, b: Rectified image space coordinates of the line vertices.
        /// </summary>
        public void Initialize(float lengthWorld, PointF startImage, PointF endImage, CalibrationAxis calibrationAxis)
        {
            this.calibrationAxis = calibrationAxis;
            QuadrilateralF quadImage = MakeQuad(startImage, endImage, calibrationAxis);
            SizeF sizeWorld = new SizeF(lengthWorld, lengthWorld);

            Initialize(sizeWorld, quadImage);
        }

        /// <summary>
        /// Updates the calibration coordinate system without changing the real-world scale of the rectangle
        /// nor the user-defined origin.
        /// Quadrilateral variant.
        /// The quadrilateral should be in rectified image space (undistorted).
        /// </summary>
        public void Update(QuadrilateralF quadImage)
        {
            if (!initialized || size.IsEmpty)
            {
                valid = false;
                return;
            }

            this.quadImage = quadImage.Clone();
            mapping.Update(quadWorld, quadImage);
            valid = quadImage.IsConvex;
        }

        /// <summary>
        /// Updates the calibration coordinate system without changing the real-world scale of the rectangle 
        /// nor the user-defined origin.
        /// Line variant.
        /// The points should be in rectified image space (undistorted).
        /// </summary>
        public void Update(PointF startImage, PointF endImage)
        {
            Update(MakeQuad(startImage, endImage, calibrationAxis));
        }

        /// <summary>
        /// Create a new CalibrationPlane initialized with the same transform as this one.
        /// </summary>
        public CalibratorPlane Clone()
        {
            CalibratorPlane clone = new CalibratorPlane();
            clone.Initialize(size, quadImage);
            clone.origin = origin;
            clone.calibrationAxis = calibrationAxis;
            return clone;
        }

        /// <summary>
        /// Takes a point from calibration-grid space and returns it in world space with offset.
        /// Calibration-grid space is in world units but with origin at the bottom-left corner of the grid and Y-down.
        /// The coordinate system object origin can be moved independently.
        /// The extra custom offset is added to values.
        /// </summary>
        private PointF GridToWorldWithOffset(PointF p, PointF origin)
        {
            return new PointF(p.X - origin.X + offset.X, - p.Y + origin.Y + offset.Y);
        }

        /// <summary>
        /// Takes a point from calibration-grid space and returns it in world space with offset.
        /// Calibration-grid space is in world units but with origin at the bottom-left corner of the grid and Y-down.
        /// The coordinate system object origin can be moved independently.
        /// The extra custom offset is added to values.
        /// </summary>
        public PointF GridToWorldWithOffset(PointF p)
        {
            return GridToWorldWithOffset(p, origin);
        }

        /// <summary>
        /// Takes a point in world space without offset and returns it in the system of 
        /// the calibration-grid (origin anchored at the bottom-left corner and Y-down).
        /// </summary>
        private PointF WorldToGrid(PointF p, PointF origin)
        {
            return new PointF(origin.X + p.X, origin.Y - p.Y);
        }

        /// <summary>
        /// Build a quadrilateral from a single line.
        /// The quadrilateral will be a square with the original line at the bottom edge.
        /// </summary>
        public static QuadrilateralF MakeQuad(PointF start, PointF end, CalibrationAxis calibrationAxis)
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
            WritePointF(w, "Offset", offset);
        }

        public void WriteLineXml(XmlWriter w)
        {
            w.WriteElementString("Length", XmlHelper.WriteFloat(size.Width));

            w.WriteStartElement("Segment");
            w.WriteElementString("A", XmlHelper.WritePointF(quadImage.D));
            w.WriteElementString("B", XmlHelper.WritePointF(quadImage.C));
            w.WriteEndElement();

            WritePointF(w, "Origin", origin);
            WritePointF(w, "Offset", offset);
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
                    case "Offset":
                        offset = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            r.ReadEndElement();

            // Semi-initialize (do not reset origin and offset).
            quadWorld = new QuadrilateralF(size.Width, size.Height);
            mapping.Update(quadWorld, quadImage);
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
                    case "Offset":
                        offset = XmlHelper.ParsePointF(r.ReadElementContentAsString());
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

            // Semi-initialize (do not reset origin and offset).
            size = new SizeF(length, length);
            quadWorld = new QuadrilateralF(size.Width, size.Height);
            quadImage = MakeQuad(line.Start, line.End, calibrationAxis);
            mapping.Update(quadWorld, quadImage);
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
