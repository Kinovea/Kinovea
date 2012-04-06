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
        public const double RadiansToDegrees = 180 / Math.PI;
        public const double DegreesToRadians = Math.PI / 180;
        
        /// <summary>
        /// Get the point on segment [AB] that is the closest from point C.
        /// </summary>
        public static PointF GetClosestPoint(PointF a, PointF b, PointF c, PointLinePosition allowedPosition, int _margin)
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
            
            Vector v = ab * t;
            return a + v;
        }
        public static PointF GetPointAtDistance(PointF a, PointF b, float distance)
        {
            Vector ab = new Vector(a,b);
            float d = distance / ab.Norm();
            Vector v = ab * d;
            return a + v;
        }
        public static float GetDistance(PointF a, PointF b)
        {
            return new Vector(a,b).Norm();
        }
        
        /// <summary>
        /// Return the signed angle (in radians) between vectors ab and ac.
        /// </summary>
        public static float GetAngle(PointF a, PointF b, PointF c)
        {
            Vector ab = new Vector(a,b);
            Vector ac = new Vector(a,c);
            float  perpDot = ab.X*ac.Y - ab.Y*ac.X;
            return  (float)Math.Atan2(perpDot, ab.Dot(ac));
        }
        
        /// <summary>
        /// Rotates point b around point a by given rotation factor (in radians).
        /// </summary>
        public static PointF Pivot(PointF a, PointF b, float radians)
        {
            Vector v = new Vector(a,b);
            float dx = (float)(v.X * Math.Cos(radians) - v.Y * Math.Sin(radians));
            float dy = (float)(v.X * Math.Sin(radians) + v.Y * Math.Cos(radians));
            return new PointF(a.X + dx, a.Y + dy);
        }
    }
}
