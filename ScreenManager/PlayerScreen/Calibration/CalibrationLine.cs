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
    /// Packages necessary info for the calibration by line.
    /// 
    /// Calibration by line uses a user-specified line that maps real world distance with pixel distance,
    /// and a coordinate system origin.
    /// </summary>
    public class CalibrationLine : ICalibrator
    {
        public bool IsOriginSet
		{
			get { return (origin.X >= 0 && origin.Y >= 0); }
		}
        
        public PointF Origin 
        {
            get { return origin;}
        }
        private PointF origin = new PointF(-1, -1);
        private float scale = 1.0f;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        
        #region ICalibrator
        public PointF Transform(PointF p)
        {
            PointF p2 = p;
            
            if(IsOriginSet)
                p2 = new PointF(p.X - origin.X, - (p.Y - origin.Y));

            p2 = p2.Scale(scale, scale);
            return p2;
        }
       
        public PointF Untransform(PointF p)
        {
            PointF p2 = p;
            
            p2 = p.Scale(1/scale, 1/scale);
            
            if(IsOriginSet)
                p2 = new PointF(p2.X + origin.X, origin.Y - p2.Y);
            
            return p2;
        }
        #endregion
        
        public void SetOrigin(PointF p)
        {
            origin = p;
        }
        public void SetPixelToUnit(float ratio)
        {
            scale = ratio;
        }
        
        #region Serialization
        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("Origin", String.Format(CultureInfo.InvariantCulture, "{0};{1}", origin.X, origin.Y));
            w.WriteElementString("Scale", String.Format(CultureInfo.InvariantCulture, "{0}", scale));
        }
        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
			{
                switch(r.Name)
                {
                    case "Origin":
                        origin = XmlHelper.ParsePointF(r.ReadElementContentAsString());
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
