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
            Vector ac = new Vector(a,c);
            Vector ab = new Vector(a,b);
            float ab2 = ab.Squared();
            float dot = ac.Dot(ab);
            float t =  dot / ab2;
            
            float fMargin = (float)_margin / ab.Norm();
            switch(allowedPosition)
            {
                case PointLinePosition.BeforeSegment:
                    t = Math.Min(-fMargin, t);
                    break;
                case PointLinePosition.BeforeAndOnSegment:
                    t = Math.Min(1.0f - fMargin, t);
                    break;
                case PointLinePosition.OnSegment:
                    t = Math.Min(Math.Max(fMargin, t), 1.0f - fMargin);
                    break;
                case PointLinePosition.AfterSegment:
                    t = Math.Max(1.0f + fMargin, t);
                    break;
                case PointLinePosition.AfterAndOnSegment:
                    t = Math.Max(fMargin, t);
                    break;
            }
            
            Vector closest = ab * t;
            return (new Vector(a) + closest).ToPoint();
        }
    }
}
