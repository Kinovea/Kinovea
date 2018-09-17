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
using System.Collections.Generic;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Convert points from image space to the coordinate system defined by calibration, and back.
    /// </summary>
    public interface ICalibrator
    {
        /// <summary>
        /// Takes a point in image coordinates and gives it back in real world coordinates.
        /// </summary>
        PointF Transform(PointF p);
        
        
        /// <summary>
        /// Takes a point in real world coordinates and gives it back in image coordinates.
        /// </summary>
        PointF Untransform(PointF p);

        /// <summary>
        /// Takes a point in image coordinates to act as the origin.
        /// </summary>
        void SetOrigin(PointF p);
    }
}
