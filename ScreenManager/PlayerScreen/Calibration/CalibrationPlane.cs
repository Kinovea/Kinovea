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
        private ProjectiveMapping mapping;      // ProjectiveMapping owned and modified by a DrawingPlane.
        
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
        
        public void Initialize(SizeF size, ProjectiveMapping mapping)
        {
            this.size = size;
            this.mapping = mapping;
            this.initialized = true;
        }
    }
}
