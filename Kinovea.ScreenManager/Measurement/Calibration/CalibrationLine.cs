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
    /// Packages necessary info for the calibration by line.
    /// 
    /// Calibration by line uses a user-specified line that maps real world distance with pixel distance,
    /// and a coordinate system origin.
    /// </summary>
    public class CalibrationLine : ICalibrator
    {
        private PointF origin;
        private float scale = 1.0f; // Baked transform.
        private float length; // Real-world reference length.
        private PointF a;       // Image coordinates of the line (undistorted space).
        private PointF b;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        #region ICalibrator
        public PointF Transform(PointF p)
        {
            PointF p2 = new PointF(p.X - origin.X, - (p.Y - origin.Y));
            p2 = p2.Scale(scale, scale);
            return p2;
        }

        public PointF Transform(PointF p, PointF origin)
        {
            PointF p2 = new PointF(p.X - origin.X, -(p.Y - origin.Y));
            p2 = p2.Scale(scale, scale);
            return p2;
        }
       
        public PointF Untransform(PointF p)
        {
            PointF p2 = p.Scale(1/scale, 1/scale);
            p2 = new PointF(p2.X + origin.X, origin.Y - p2.Y);
            return p2;
        }
        
        public void SetOrigin(PointF p)
        {
            origin = p;
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
        }

        /// <summary>
        /// Updates the calibration coordinate system without changing the real-world scale of the segment, nor the user-defined origin.
        /// a, b: Image coordinates of the reference segment.
        /// </summary>
        public void Update(PointF a, PointF b)
        {
            if (length == 0)
            {
                return;
            }

            this.a = a;
            this.b = b;
            float pixelLength = GeometryHelper.GetDistance(a, b);
            scale = length / pixelLength;
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
