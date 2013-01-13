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
        public void Update(PointF o, PointF a, PointF b, int radius, CalibrationHelper calibration)
        {
            if(o == a || o == b)
                return;

            Origin = o;
            Angle = ComputeAngle(o, a, b);
            
            if(calibration != null && calibration.CalibratorType == CalibratorType.Plane)
            {
                PointF calibratedO = calibration.GetPoint(o);
                PointF calibratedA = calibration.GetPoint(a);
                PointF calibratedB = calibration.GetPoint(b);
                
                CalibratedAngle = ComputeAngle(calibratedO, calibratedA, calibratedB);
            }
            else
            {
                CalibratedAngle = Angle;
            }
            
            ComputeBoundingBox(o, a, b, (float)radius);
            ComputeTextPosition(Angle);
            ComputeHitRegion(BoundingBox, Angle);
        }
        public bool Hit(Point p)
        {
           if (BoundingBox == Rectangle.Empty)
                return false;
            else
                return hitRegion.IsVisible(p);
        }
        private double GetAbsoluteAngle(PointF o, PointF p)
        {
            double radians = Math.Atan((double)(p.Y - o.Y) / (double)(p.X - o.X));
            double angle = radians * MathHelper.RadiansToDegrees;
            
            // We get a value between -90 and +90, depending on the quadrant.
            // Translate to 0 -> 360 clockwise with 0 at right.
            if(p.X >= o.X)
            {
                if(p.Y <= o.Y)
                    angle = 360 + angle;
            }
            else
            {
                angle = 180 + angle;
            }
            
            return angle % 360;
        }
        private Angle ComputeAngle(PointF o, PointF a, PointF b)
        {
            float oa = (float)GetAbsoluteAngle(o, a);
            float ob = (float)GetAbsoluteAngle(o, b);
            
            float start = 0;
            float sweep = 0;
            
            start = oa;
            if(ob > oa)
            {
                sweep = ob - oa;
                if(relative && sweep > 180)
                    sweep = -(360 - sweep);
            }
            else
            {
                sweep = (360 - oa) + ob;
                if(relative && sweep > 180)
                    sweep = -(360 - sweep);
            }
            
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
            
            BoundingBox = o.Box((int)radius);
        }
        private void ComputeTextPosition(Angle angle)
        {
            double bissect = (angle.Start + angle.Sweep/2) * MathHelper.DegreesToRadians;
            int adjacent = (int)(Math.Cos(bissect) * textDistance);
            int opposed = (int)(Math.Sin(bissect) * textDistance);
            
            TextPosition = new Point(adjacent, opposed);
        }
        private void ComputeHitRegion(Rectangle boundingBox, Angle angle)
        {
            if (BoundingBox == Rectangle.Empty)
                return;
            
            try
            {
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddPie(BoundingBox, angle.Start, angle.Sweep);
                    if(hitRegion != null)
                        hitRegion.Dispose();
                    hitRegion = new Region(gp);
                }
            }
            catch(Exception)
            {
                log.DebugFormat("Error while computing hit region of angle helper.");
            }
        }
    }
}
