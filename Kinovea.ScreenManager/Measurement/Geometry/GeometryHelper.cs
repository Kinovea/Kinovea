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
using System.Drawing;
using log4net;

namespace Kinovea.ScreenManager
{
    public static class GeometryHelper
    {
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
            
            float fMargin = _margin / ab.Norm();
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
        
        public static PointF GetMiddlePoint(PointF a, PointF b)
        {
            return a + (new Vector(a,b) * 0.5f);
        }

        /// <summary>
        /// Gets the distance between points A and B.
        /// </summary>
        public static float GetDistance(PointF a, PointF b)
        {
            return new Vector(a,b).Norm();
        }
        
        /// <summary>
        /// Returns the angle between vectors ab and ac in the range [-π..+π], positive CCW.
        /// </summary>
        public static float GetAngle(PointF a, PointF b, PointF c)
        {
            return GetAngle(a, b, a, c);
        }

        /// <summary>
        /// Returns the between vectors ab and cd in the range [-π..+π], positive CCW.
        /// </summary>
        public static float GetAngle(PointF a, PointF b, PointF c, PointF d)
        {
            Vector ab = new Vector(a, b);
            Vector cd = new Vector(c, d);
            float perpDot = ab.X * cd.Y - ab.Y * cd.X;
            return (float)Math.Atan2(perpDot, ab.Dot(cd));
        }

        /// <summary>
        /// Rotates point b around point a by an angle in radians).
        /// </summary>
        public static PointF Rotate(PointF a, PointF b, float radians)
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
            
            float step = (float)((2 * Math.PI) / subdivisions);
            int section = (int)(angle / step);
            if(angle % step > step / 2)
                section++;
            
            float deltaAngle = (section * step) - angle;
            
            return Rotate(pivot, point, deltaAngle);
        }
        
        /// <summary>
        /// Returns the point that is at the specified angle relatively to the [origin, leg1] segment. targetAngle is given in degrees.
        /// </summary>
        public static PointF GetPointAtAngle(PointF pivot, PointF leg1, PointF point, float targetAngle)
        {
            float distance = GetDistance(pivot, point);
            float angle = targetAngle * (float)MathHelper.DegreesToRadians;
            PointF result = GetPointAtAngleAndDistance(pivot, leg1, angle, distance);
            return result;
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
        
        /// <summary>
        /// Returns the point that is at the specified angle relatively to the [origin,leg1] segment, and at the specified distance from origin.
        /// </summary>
        public static PointF GetPointAtAngleAndDistance(PointF origin, PointF leg1, float angle, float distance)
        {
            // First get a point in the right direction, then get the point in this direction that is at the right distance.
            PointF direction = Rotate(origin, leg1, angle);
            PointF result = GetPointAtDistance(origin, direction, distance);
            return result;
        }
        
        /// <summary>
        /// Returns the point that is on a segment passing through c and parallel to [a,b].
        /// The resulting point is at the same distance from c than the candidate point.
        /// </summary>
        public static PointF GetPointOnParallel(PointF a, PointF b, PointF c, PointF point)
        {
            // Find the fourth point of the parallelogram to have the parallel segment [c,d].
            Vector ac = new Vector(a,c);
            PointF d = b.Translate(ac.X, ac.Y);
            
            float angle = GetAngle(c, a, d);
            float distance = GetDistance(c, point);
            
            PointF result = GetPointAtAngleAndDistance(c, a, angle, distance);
            return result;
        }

        /// <summary>
        /// Linear interpolation between two values.
        /// Returns: (x * (1-alpha)) + (y * alpha).
        /// </summary>
        public static float Mix(float a, float b, float alpha)
        {
            return a * (1 - alpha) + b * alpha;
        }

        /// <summary>
        /// Linear interpolation between two values.
        /// Returns: (x * (1-alpha)) + (y * alpha).
        /// </summary>
        public static PointF Mix(PointF a, PointF b, float alpha)
        {
            return new PointF(
                a.X * (1 - alpha) + b.X * alpha, 
                a.Y * (1 - alpha) + b.Y * alpha);
        }
    }
}
