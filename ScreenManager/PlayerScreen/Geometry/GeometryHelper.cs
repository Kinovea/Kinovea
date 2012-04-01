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
using log4net;

namespace Kinovea.ScreenManager
{
    public static class GeometryHelper
    {
        /// <summary>
        /// Get the point on segment [AB] that is the closest from point C.
        /// </summary>
        public static Point GetClosestPoint(Point a, Point b, Point c, PointLinePosition allowedPosition, int _margin)
        {
            //ILog log = LogManager.GetLogger("GeometryHelper.GetClosestPoint");
            
            if (a.X == b.X)
                return new Point(a.X, c.Y);
            
            if(a.Y == b.Y)
                return new Point(c.X, a.Y);
            
            Vector ac = new Vector(a,c);
            Vector ab = new Vector(a,b);
            float ab2 = ab.Squared();
            float dot = ac.Dot(ab);
            float t =  dot / ab2;
            
            float margin = (float)_margin / ab.Norm();
            // TODO: clamp based on allowed position.
            if(allowedPosition == PointLinePosition.OnSegment)
                t = Math.Min(Math.Max(margin, t), 1.0f - margin);
            else
                t = Math.Max(1.0f + margin, t);

            Vector closest = ab * t;
            return (new Vector(a) + closest).ToPoint();
        }
    }
}
