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
    /// Packages necessary info for the calibration by line.
    /// 
    /// Calibration by line uses a user-specified line that maps real world distance with pixel distance,
    /// and a coordinate system origin.
    /// </summary>
    public class CalibrationLine : ICalibrator
    {
        private PointF origin;
        private float scale = 1.0f;
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
        
        
        public void Initialize(float ratio)
        {
            scale = ratio;
        }
        
        #region Serialization
        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("Origin", XmlHelper.WritePointF(origin));
            w.WriteElementString("Scale", string.Format(CultureInfo.InvariantCulture, "{0}", scale));
        }
        public void ReadXml(XmlReader r, PointF scaling)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "Origin":
                        origin = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        origin = origin.Scale(scaling.X, scaling.Y);
                        break;
                    case "Scale":
                        scale = float.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
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
