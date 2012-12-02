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
using System.Globalization;
using System.Linq;
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
        
        private bool initialized;
        private SizeF size;
        private QuadrilateralF quadImage = new QuadrilateralF();
        private ProjectiveMapping mapping = new ProjectiveMapping();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        
        #region ICalibrator
        public PointF Transform(PointF p)
        {
            if(!initialized)
                return p;
            
            return mapping.Backward(p);
        }
        
        public PointF Untransform(PointF p)
        {
            if(!initialized)
                return p;
            
            return mapping.Forward(p);
        }
        #endregion
        
        public void Initialize(SizeF size, QuadrilateralF quadImage)
        {
            this.size = size;
            this.quadImage = quadImage.Clone();
            mapping.Update(new QuadrilateralF(size.Width, size.Height), quadImage);
            this.initialized = true;
        }
        
        #region Serialization
        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("Size", String.Format(CultureInfo.InvariantCulture, "{0};{1}", size.Width, size.Height));
            
            w.WriteStartElement("Quadrilateral");
            WritePointF(w, "A", quadImage.A);
            WritePointF(w, "B", quadImage.B);
            WritePointF(w, "C", quadImage.C);
            WritePointF(w, "D", quadImage.D);
            w.WriteEndElement();
            
        }
        private void WritePointF(XmlWriter w, string name, PointF p)
        {
            w.WriteElementString(name, String.Format(CultureInfo.InvariantCulture, "{0};{1}", p.X, p.Y));
        }
        public void ReadXml(XmlReader r)
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
                        ParseQuadrilateral(r);
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
        private void ParseQuadrilateral(XmlReader r)
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
            
            r.ReadEndElement();
        }
        #endregion
    }
}
