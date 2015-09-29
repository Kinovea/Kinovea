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
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    public class AngleHelper
    {
        public Angle Angle { get; private set;}
        public Angle CalibratedAngle { get; private set;}
        
        public Rectangle BoundingBox { get; private set; }
        public PointF TextPosition { get; private set;}
        public PointF Origin { get; private set;}
        public bool Tenth { get; private set;}
        public string Symbol { get; private set;}
        public Color Color { get; private set;}
        
        private bool relative;
        private int textDistance;
        private Region hitRegion;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public AngleHelper(bool relative, int textDistance, bool tenth, string symbol)
        {
            this.relative = relative;
            this.textDistance = textDistance;
            this.Tenth = tenth;
            this.Symbol = symbol;
        }
        public void Update(PointF o, PointF a, PointF b, int radius, Color color, CalibrationHelper calibration, IImageToViewportTransformer transformer)
        {
            if(o == a || o == b)
                return;

            Origin = o;
            Angle = ComputeAngle(o, a, b, false);
            Color = color;

            if (calibration == null)
            {
                CalibratedAngle = Angle;
            }
            else if(calibration.CalibratorType == CalibratorType.Plane)
            {
                PointF calibratedO = calibration.GetPoint(o);
                PointF calibratedA = calibration.GetPoint(a);
                PointF calibratedB = calibration.GetPoint(b);
                CalibratedAngle = ComputeAngle(calibratedO, calibratedA, calibratedB, true);
            }
            else if (calibration.CalibratorType == CalibratorType.Line)
            {
                // Note that direction of Y-axis is not the same that the one used for the uncalibrated space.
                PointF calibratedO = calibration.GetPoint(o);
                PointF calibratedA = calibration.GetPoint(a);
                PointF calibratedB = calibration.GetPoint(b);
                CalibratedAngle = ComputeAngle(calibratedO, calibratedA, calibratedB, true);
            }

            ComputeBoundingBox(o, a, b, (float)radius);
            ComputeTextPosition(Angle, transformer);
            ComputeHitRegion(BoundingBox, Angle);
        }
        public bool Hit(PointF p)
        {
            if (hitRegion == null)
                return false;

           if (BoundingBox.Size == Size.Empty)
                return false;
            else
                return hitRegion.IsVisible(p);
        }
        
        private float GetAbsoluteAngle(PointF o, PointF p, bool yUp)
        {
            // Note that angles in Kinovea are generally expressed using the convention of .NET System.Drawing:
            // Values range from 0 to 360°, always positive, clockwise direction, start at X axis.
            // This differs from Atan2 which has values ranging from 0 to π, positive for counter-clockwise, negative for clockwise.
            // The direction of Y is up for calibrated spaces, and down for drawing space.
            float dx = p.X - o.X;
            float dy = p.Y - o.Y;
            
            if (!yUp)
                dy = -dy;

            double angle = Math.Atan2(dy, dx);
            double ccwTau = angle > 0 ? Math.PI * 2 - angle : -angle;
            return (float)(ccwTau * MathHelper.RadiansToDegrees);
        }
        
        private Angle ComputeAngle(PointF o, PointF a, PointF b, bool yUp)
        {
            float oa = GetAbsoluteAngle(o, a, yUp);
            float ob = GetAbsoluteAngle(o, b, yUp);
            
            float start = oa;
            float sweep = ob > oa ? ob - oa : (360 - oa) + ob;
            
            if (relative && sweep > 180)
                sweep = -(360 - sweep);
            
            sweep %= 360;
            
            return new Angle(start, sweep);
        }
 
        private void ComputeBoundingBox(PointF o, PointF a, PointF b, float radius)
        {
            if(radius == 0)
            {
                // Special case meaning "biggest as possible" -> up to the small leg.
                float oa = new Vector(o,a).Norm();
                float ob = new Vector(o,b).Norm();
                float smallest = Math.Min(oa, ob);
                radius = smallest > 20 ? smallest - 10 : Math.Min(smallest, 10);
            }
            
            BoundingBox = o.Box((int)radius).ToRectangle();
        }

        private void ComputeTextPosition(Angle angle, IImageToViewportTransformer transformer)
        {
            int imageTextDistance = transformer.Untransform(textDistance);
            double bissect = (angle.Start + angle.Sweep/2) * MathHelper.DegreesToRadians;
            int adjacent = (int)(Math.Cos(bissect) * imageTextDistance);
            int opposed = (int)(Math.Sin(bissect) * imageTextDistance);
            
            TextPosition = new Point(adjacent, opposed);
        }

        private void ComputeHitRegion(Rectangle boundingBox, Angle angle)
        {
            if (BoundingBox.Size == Size.Empty)
                return;
            
            try
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddPie(BoundingBox, angle.Start, angle.Sweep);
                    if(hitRegion != null)
                        hitRegion.Dispose();
                    hitRegion = new Region(path);
                }
            }
            catch(Exception)
            {
                log.DebugFormat("Error while computing hit region of angle helper.");
            }
        }

        private void Tests()
        {
            PointF o = PointF.Empty;
            PointF a = new PointF(-1, -1);
            PointF b = new PointF(1, -1);
            PointF c = new PointF(1, 1);
            PointF d = new PointF(-1, +1);

            Angle tabf = ComputeAngle(o, a, b, false);
            Angle tbaf = ComputeAngle(o, b, a, false);
            Angle tabt = ComputeAngle(o, a, b, true);
            Angle tbat = ComputeAngle(o, b, a, true);

            Angle tbcf = ComputeAngle(o, b, c, false);
            Angle tcbf = ComputeAngle(o, c, b, false);
            Angle tbct = ComputeAngle(o, b, c, true);
            Angle tcbt = ComputeAngle(o, c, b, true);

            Angle tcdf = ComputeAngle(o, c, d, false);
            Angle tdcf = ComputeAngle(o, d, c, false);
            Angle tcdt = ComputeAngle(o, c, d, true);
            Angle tdct = ComputeAngle(o, d, c, true);

            Angle tdaf = ComputeAngle(o, d, a, false);
            Angle tadf = ComputeAngle(o, a, d, false);
            Angle tdat = ComputeAngle(o, d, a, true);
            Angle tadt = ComputeAngle(o, a, d, true);
        }
    }
}
