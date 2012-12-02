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
using System.Linq;

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
        
        private PointF origin = new PointF(-1, -1);
        private float scale = 1.0f;
        
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
    }
}
