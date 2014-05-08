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
using System.Drawing;
using System.Globalization;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Packages necessary info for the calibration by plane.
    /// 
    /// Calibration by plane uses a user-specified quadrilateral defining a rectangle on the ground or wall,
    /// and maps the image coordinates with the system defined by the rectangle.
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

        public QuadrilateralF QuadImage
        {
            get { return quadImage; }
        }
        
        private bool initialized;
        private SizeF size;
        private QuadrilateralF quadImage = new QuadrilateralF();
        private ProjectiveMapping mapping = new ProjectiveMapping();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        // Origin of world expressed in calibration coordinates.
        private PointF origin;
        
        #region ICalibrator
        /// <summary>
        /// Takes a point in image coordinates and gives it back in real world coordinates.
        /// </summary>
        public PointF Transform(PointF p)
        {
            if(!initialized)
                return p;
            
            return CalibratedToWorld(mapping.Backward(p));
        }
        
        /// <summary>
        /// Takes a point in real world coordinates and gives it back in image coordinates.
        /// </summary>
        public PointF Untransform(PointF p)
        {
            if(!initialized)
                return p;

            return mapping.Forward(WorldToCalibrated(p));
        }

        /// <summary>
        /// Takes a point in image coordinates to act as the origin of the current coordinate system.
        /// </summary>
        public void SetOrigin(PointF p)
        {
            origin = mapping.Backward(p);
        }
        
        #endregion

        private PointF CalibratedToWorld(PointF p)
        {
            return new PointF(- origin.X + p.X, origin.Y - p.Y);
        }

        private PointF WorldToCalibrated(PointF p)
        {
            return new PointF(origin.X + p.X, origin.Y - p.Y);
        }
        
        /// <summary>
        /// Initialize the projective mapping.
        /// </summary>
        /// <param name="size">Real world dimension of the reference rectangle.</param>
        /// <param name="quadImage">Image coordinates of the reference rectangle.</param>
        public void Initialize(SizeF size, QuadrilateralF quadImage)
        {
            PointF originImage = initialized ? Untransform(PointF.Empty) : quadImage.D;
            
            this.size = size;
            this.quadImage = quadImage.Clone();
            mapping.Update(new QuadrilateralF(size.Width, size.Height), quadImage);
            SetOrigin(originImage);
            this.initialized = true;
        }

        public void Update(QuadrilateralF quadImage)
        {
            this.quadImage = quadImage.Clone();
            mapping.Update(new QuadrilateralF(size.Width, size.Height), quadImage);
        }
        
        #region Serialization
        public void WriteXml(XmlWriter w)
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
        private void WritePointF(XmlWriter w, string name, PointF p)
        {
            w.WriteElementString(name, XmlHelper.WritePointF(p));
        }
        public void ReadXml(XmlReader r, PointF scale)
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
                        ParseQuadrilateral(r, scale);
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
            initialized = true;
        }
        private void ParseQuadrilateral(XmlReader r, PointF scale)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "A":
                        quadImage.A = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    case "B":
                        quadImage.B = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    case "C":
                        quadImage.C = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    case "D":
                        quadImage.D = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            quadImage.Scale(scale.X, scale.Y);

            r.ReadEndElement();
        }
        #endregion
    }
}
