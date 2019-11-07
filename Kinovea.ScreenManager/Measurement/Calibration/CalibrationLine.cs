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
using System.Drawing.Drawing2D;

using Kinovea.Services;

namespace Kinovea.ScreenManager.Deprecated
{
    /// <summary>
    /// Packages necessary info for the calibration by line.
    /// 
    /// Calibration by line uses a user-specified line that maps real world distance with pixel distance,
    /// and a coordinate system origin.
    /// </summary>
    public class CalibrationLine : ICalibrator
    {
        public SizeF Size
        {
            get
            {
                if (length == 0)
                    return new SizeF(100, 100);

                return new SizeF(length, length);
            }
        }

        public QuadrilateralF QuadImage
        {
            get
            {
                if (quadImage == QuadrilateralF.Empty)
                    return new QuadrilateralF(new Rectangle(0, 0, 100, 100));

                return quadImage;
            }
        }

        //---------------------------------------------
        // "Image" coordinate system has origin at top left and has Y-down.
        // "Calibrated plane" coordinate system has its origin at the A point of the line, is scaled and rotated and has Y-up.
        // "World" coordinate system has origin at user-defined point and is Y-up.
        //---------------------------------------------

        private float length;       // Real-world reference length.
        private QuadrilateralF quadImage = QuadrilateralF.Empty;
        private float scale = 1.0f; // Baked transform image to calibrated plane. aka: Real world units per pixel.
        private PointF a;           // Image coordinates of the line in image space (undistorted).
        private PointF b;
        private PointF origin;      // User-defined origin, in calibrated plane coordinate system.
        private Matrix m = new Matrix();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ICalibrator
        /// <summary>
        /// Takes a point in image space and returns it in world space.
        /// </summary>
        public PointF Transform(PointF p)
        {
            return CalibratedToWorld(ImageToCalibrated(p), origin);
        }

        /// <summary>
        /// Takes a point and origin in image space and returns it in world space.
        /// </summary>
        public PointF Transform(PointF p, PointF originInImage)
        {
            PointF origin = ImageToCalibrated(originInImage);
            return CalibratedToWorld(ImageToCalibrated(p), origin);
        }

        /// <summary>
        /// Takes a point in real world coordinates and gives it back in image coordinates.
        /// </summary>
        public PointF Untransform(PointF p)
        {
            return CalibratedToImage(WorldToCalibrated(p, origin));
        }

        /// <summary>
        /// Takes a point in image coordinates to act as the origin of the current coordinate system.
        /// </summary>
        public void SetOrigin(PointF p)
        {
            origin = ImageToCalibrated(p);
        }
        #endregion

        /// <summary>
        /// Initialize the mapping.
        /// length: Real world length of the reference line.
        /// a, b: Image coordinates of the reference line.
        /// </summary>
        public void Initialize(float length, PointF a, PointF b)
        {
            this.length = length;
            this.a = a;
            this.b = b;

            float pixelLength = GeometryHelper.GetDistance(a, b);
            scale = length / pixelLength;
            SetOrigin(a);

            UpdateQuadImage(a, b);
        }

        /// <summary>
        /// Updates the calibration coordinate system without changing the real-world scale of the segment, nor the user-defined origin.
        /// a, b: Image coordinates of the reference segment.
        /// </summary>
        public void Update(PointF a, PointF b)
        {
            if (length == 0)
                return;

            this.a = a;
            this.b = b;
            float pixelLength = GeometryHelper.GetDistance(a, b);
            scale = length / pixelLength;

            UpdateQuadImage(a, b);
        }

        /// <summary>
        /// Maps a point from image to calibrated plane coordinates.
        /// </summary>
        private PointF ImageToCalibrated(PointF p)
        {
            PointF p2 = new PointF(p.X - a.X, -(p.Y - a.Y));
            p2 = p2.Scale(scale, scale);
            return p2;
        }

        /// <summary>
        /// Maps a point from calibrated plane to image coordinates.
        /// </summary>
        private PointF CalibratedToImage(PointF p)
        {
            PointF p2 = p.Scale(1 / scale, 1 / scale);
            p2 = new PointF(a.X + p2.X, a.Y - p2.Y);
            return p2;
        }

        // Calibrated plane to world space.
        private PointF CalibratedToWorld(PointF p, PointF origin)
        {
            PointF p2 = new PointF(p.X - origin.X, p.Y - origin.Y);
            return p2;
        }

        // World space to calibrated plane.
        private PointF WorldToCalibrated(PointF p, PointF origin)
        {
            PointF p2 = new PointF(origin.X + p.X, origin.Y + p.Y);
            return p2;
        }

        /// <summary>
        /// Rebuild a quadrilateral to help drawing the coordinate system.
        /// </summary>
        private void UpdateQuadImage(PointF a, PointF b)
        {
            PointF c = new PointF(a.X + (b.Y - a.Y), a.Y - (b.X - a.X));
            PointF d = new PointF(c.X + (b.X - a.X), c.Y + (b.Y - a.Y));
            quadImage = new QuadrilateralF(c, d, b, a);
        }

        #region Serialization
        public void WriteXml(XmlWriter w)
        {
            //w.WriteElementString("Scale", string.Format(CultureInfo.InvariantCulture, "{0}", scale));

            w.WriteElementString("Length", XmlHelper.WriteFloat(length));

            w.WriteStartElement("Segment");
            w.WriteElementString("A", XmlHelper.WritePointF(a));
            w.WriteElementString("B", XmlHelper.WritePointF(b));
            w.WriteEndElement();

            w.WriteElementString("Origin", XmlHelper.WritePointF(origin));
        }
        public void ReadXml(XmlReader r, PointF scaling)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "Length":
                        length = float.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "Segment":
                        ParseSegment(r, scaling);
                        break;
                    case "Origin":
                        origin = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        if (float.IsNaN(origin.X) || float.IsNaN(origin.Y))
                            origin = PointF.Empty;

                        origin = origin.Scale(scaling.X, scaling.Y);
                        break;
                    case "Scale":
                        // Import and convert older format.
                        float bakedScale = float.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        length = 1.0f;
                        a = PointF.Empty;
                        b = new PointF(0, 1.0f / bakedScale);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            r.ReadEndElement();

            // Update mapping.
            float pixelLength = GeometryHelper.GetDistance(a, b);
            scale = length / pixelLength;
        }

        private void ParseSegment(XmlReader r, PointF scale)
        {
            r.ReadStartElement();

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

            r.ReadEndElement();
        }
        #endregion
    }
}
