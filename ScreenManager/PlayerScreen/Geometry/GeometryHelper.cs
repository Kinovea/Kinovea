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
        public const float RadiansToDegrees = (float)(180 / Math.PI);
        public const float DegreesToRadians = (float)(Math.PI / 180);
        
        /// <summary>
        /// Gets the point on segment [AB] that is the closest from point C.
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
        
        /// <summary>
        /// Gets the point on the (AB) line that is a the specified distance (in pixels) starting from A.
        /// </summary>
        public static PointF GetPointAtDistance(PointF a, PointF b, float distance)
        {
            Vector ab = new Vector(a,b);
            float d = distance / ab.Norm();
            Vector v = ab * d;
            return a + v;
        }
        
        /// <summary>
        /// Gets the distance between points A and B.
        /// </summary>
        public static float GetDistance(PointF a, PointF b)
        {
            return new Vector(a,b).Norm();
        }
        
        /// <summary>
        /// Gets the distance between points A and B.
        /// </summary>
        public static float GetDistance(Point a, Point b)
        {
            return new Vector(a,b).Norm();
        }
        
        /// <summary>
        /// Returns the signed angle (in radians) between vectors ab and ac.
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
        
        /// <summary>
        /// Gets the position of the point by restricting rotation steps allowed relatively to [pivot,leg1] segment.
        /// </summary>
        public static PointF GetPointAtClosestRotationStep(PointF pivot, PointF leg1, PointF point, int subdivisions)
        {
            // Computes the delta angle between the current point and the closest step.
            // Pivot the current angle by this delta angle.
            
            float angle = GetAngle(pivot, leg1, point);
            if(angle < 0)
                angle += (float)(2*Math.PI);
            
            float degrees = angle * RadiansToDegrees;
            
            float step = 360 / subdivisions;
            int section = (int)(degrees / step);
            if(degrees % step > step / 2)
                section++;
            
            float deltaAngle = (section * step) - degrees;
            
            return Pivot(pivot, point, deltaAngle * DegreesToRadians);
        }
        
        
        /// <summary>
        /// Gets the position of the point by restricting rotation steps allowed. 
        /// In this version, steps are relative to the trig circle.
        /// </summary>
        public static PointF GetPointAtClosestRotationStepCardinal(PointF pivot, PointF point, int subdivisions)
        {
            PointF zero = new PointF(pivot.X + 100, pivot.Y);
            return GetPointAtClosestRotationStep(pivot, zero, point, subdivisions);
        }
        /*
        /// <summary>
        /// Gets the position of the point by restricting rotation steps allowed. 
        /// In this version, steps are relative to the trig circle.
        /// For integer coordinates points.
        /// </summary>
        public static Point GetPointAtClosestRotationStepCardinal(Point pivot, Point point, int subdivisions)
        {
            PointF result = GetPointAtClosestRotationStepCardinal(new PointF(pivot.X, pivot.Y), new PointF(point.X, point.Y), subdivisions);
            return result.ToPoint();
        }*/
        
        /// <summary>
        /// Returns the point that is at the specified angle relatively to the [origin,leg1] segment, and at the specified distance from origin.
        /// </summary>
        public static PointF GetPointAtAngleAndDistance(PointF origin, PointF leg1, float angle, float distance)
        {
            // First get a point in the right direction, then get the point in this direction that is at the right distance.
            PointF direction = Pivot(origin, leg1, angle);
            PointF result = GetPointAtDistance(origin, direction, distance);
            return result;
        }
    }
}
