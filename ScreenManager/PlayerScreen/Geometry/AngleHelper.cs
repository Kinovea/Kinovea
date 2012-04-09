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
        public double Start { get; private set; }
        public double Sweep { get; private set; }
        public Rectangle BoundingBox { get; private set; }
        public PointF TextPosition { get; private set;}
        public PointF Origin { get; private set;}
        public bool Tenth { get; private set;}
        
        private bool relative;
        private int textDistance;
        private Region hitRegion;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public AngleHelper(bool relative, int textDistance, bool tenth)
        {
            this.relative = relative;
            this.textDistance = textDistance;
            this.Tenth = tenth;
        }
        public void Update(PointF o, PointF a, PointF b, int radius)
        {
            if(o == a || o == b)
                return;

            Origin = o;
            ComputeAngles(o, a, b);
            ComputeBoundingBox(o, a, b, radius);
            ComputeTextPosition(Start, Sweep);
            ComputeHitRegion(BoundingBox, Start, Sweep);
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
            double angle = radians * GeometryHelper.RadiansToDegrees;
            
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
        private void ComputeAngles(PointF o, PointF a, PointF b)
        {
            double oa = GetAbsoluteAngle(o, a);
            double ob = GetAbsoluteAngle(o, b);
            
            Start = oa;
            if(ob > oa)
            {
                Sweep = ob - oa;
                if(relative && Sweep > 180)
                    Sweep = -(360 - Sweep);
            }
            else
            {
                Sweep = (360 - oa) + ob;
                if(relative && Sweep > 180)
                    Sweep = -(360 - Sweep);
            }
            
            Sweep %= 360;
        }
        private void ComputeBoundingBox(PointF o, PointF a, PointF b, int radius)
        {
            float r = radius;
            if(r == 0)
            {
                float oa = new Vector(o,a).Norm();
                float ob = new Vector(o,b).Norm();
                float smaller = Math.Min(oa, ob);
                r = smaller > 20 ? smaller - 10 : Math.Min(smaller, 10);
            }
            
            BoundingBox = o.Box((int)r);
        }
        private void ComputeTextPosition(double start, double sweep)
        {
            double bissect = (start + sweep/2) * GeometryHelper.DegreesToRadians;
            int adjacent = (int)(Math.Cos(bissect) * textDistance);
            int opposed = (int)(Math.Sin(bissect) * textDistance);
            
            TextPosition = new Point(adjacent, opposed);
        }
        private void ComputeHitRegion(Rectangle boundingBox, double start, double sweep)
        {
            if (BoundingBox == Rectangle.Empty)
                return;
            
            try
            {
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddPie(BoundingBox, (float)Start, (float)Sweep);
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
