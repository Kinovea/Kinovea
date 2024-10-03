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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This class is the "drawable" abstraction of an angle, it is used for drawing the pie section of the angle on screen.
    /// It uses conventions used by .NET DrawPie:
    /// - Value is in degree and range from 0 to 360, always positive.
    /// - Positive direction is clockwise.
    /// - Y-axis direction is down.
    /// </summary>
    public class SweepAngle
    {
        /// <summary>
        /// Origin of the angle.
        /// </summary>
        public PointF Origin 
        {
            get { return origin; }
        }
        
        /// <summary>
        /// Absolute angle of the first leg.
        /// </summary>
        public float Start
        {
            get { return start; }
        }

        /// <summary>
        /// Displacement of the angle in degrees, positive is clockwise starting at X axis.
        /// </summary>
        public float Sweep
        {
            get { return sweep; }
        }

        /// <summary>
        /// The bounding box of the full circle.
        /// </summary>
        public RectangleF BoundingBox
        {
            get { return boundingBox; }
        }

        /// <summary>
        /// Pointy end of the arrow.
        /// The arrow always goes from the reference leg to the measure leg.
        /// It does not indicate the rotation direction (cw vs ccw).
        /// </summary>
        public PointF ArrowEnd
        {
            get { return arrowEnd; }
        }

        /// <summary>
        /// Start of the segment that should be used to draw the arrow.
        /// This should only be used to get a direction.
        /// </summary>
        public PointF ArrowStart
        {
           get { return arrowStart; }
        }

        private const double TAU = Math.PI * 2;
        private float start = 270;
        private float sweep = 90;
        private PointF origin = PointF.Empty;
        private RectangleF boundingBox = RectangleF.Empty;
        private PointF arrowEnd = PointF.Empty;
        private PointF arrowStart = PointF.Empty;
        private Region hitRegion;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override string ToString()
        {
            return string.Format("[Angle Start={0}, Sweep={1}]", Start, Sweep);
        }

        /// <summary>
        /// Update angle going from oa to ob.
        /// </summary>
        public void Update(PointF o, PointF a, PointF b, float radius, bool signed, bool ccw)
        {
            origin = o;

            float oa = 0;
            float ob = 0;

            if (ccw)
            {
                oa = GetAbsoluteAngle(o, b);
                ob = GetAbsoluteAngle(o, a);
            }
            else
            {
                oa = GetAbsoluteAngle(o, a);
                ob = GetAbsoluteAngle(o, b);
            }

            start = oa;
            sweep = ob > oa ? ob - oa : (360 - oa) + ob;
            bool neg = false;

            if (signed && sweep > 180)
            {
                start = ob;
                sweep = 360 - sweep;
                neg = true;
            }

            UpdateBoundingBox(o, a, b, radius, ccw, neg);
            UpdateHitRegion();
        }

        public bool Hit(PointF p)
        {
            if (hitRegion == null)
                return false;

            if (boundingBox.Size == Size.Empty)
                return false;
            else
                return hitRegion.IsVisible(p);
        }

        private float GetAbsoluteAngle(PointF o, PointF p)
        {
            float dx = p.X - o.X;
            float dy = o.Y - p.Y;

            // CCW angle in range [-π..+π].
            double angle = Math.Atan2(dy, dx);

            // CW angle in range [0..τ].
            double drawPieRadians = angle > 0 ? TAU - angle : - angle;

            return (float)MathHelper.Degrees(drawPieRadians);
        }

        private void UpdateBoundingBox(PointF o, PointF a, PointF b, float radius, bool ccw, bool neg)
        {
            // If the radius is negative we count it from the end of the smallest segment.
            if (radius <= 0)
            {
                // Anything between 0 and -10 is aliased to -10 for backward compat.
                radius = Math.Min(radius, -10);
                
                float oa = new Vector(o, a).Norm();
                float ob = new Vector(o, b).Norm();
                float smallest = Math.Min(oa, ob);
                radius = smallest + radius > 0 ? smallest + radius : Math.Min(smallest, 10);
                
                // Truncate the radius to match .NET behavior.
                radius = (float)(int)radius;
                
                Vector vob = new Vector(o, b);
                arrowEnd = o + vob.Normalized() * radius;

                // For the origin direction of the arrow, if we use the tangent it looks offset
                // for some reason. Take a point at a small arc along the circle instead.
                bool useTangent = false;
                if (useTangent)
                {
                    if ((ccw && neg) || (!ccw && !neg))
                        arrowStart = arrowEnd + new Vector(vob.Y, -vob.X).Normalized() * 100;
                    else 
                        arrowStart = arrowEnd + new Vector(-vob.Y, vob.X).Normalized() * 100;
                }
                else
                {
                    float radians = MathHelper.Radians(1);
                    if ((ccw && neg) || (!ccw && !neg))
                        radians = -radians;

                    double cx = Math.Cos(radians);
                    double cy = Math.Sin(radians);
                    Vector dirX = vob.Normalized() * radius;
                    Vector dirY = new Vector(-dirX.Y, dirX.X);
                    arrowStart = o + dirX * (float)cx + dirY * (float)cy;
                    Vector arrowDir = new Vector(arrowEnd, arrowStart);
                    arrowStart = arrowEnd + arrowDir.Normalized() * 100;
                }
            }

            boundingBox = o.Box((int)radius);
        }

        private void UpdateHitRegion()
        {
            if (boundingBox.Size == SizeF.Empty)
                return;

            try
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddPie(boundingBox.ToRectangle(), start, sweep);

                    if (hitRegion != null)
                        hitRegion.Dispose();

                    hitRegion = new Region(path);
                }
            }
            catch (Exception)
            {
                log.DebugFormat("Error while computing hit region of angle helper.");
            }
        }

    }
}
