using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class TimedPoint
    {
        public float X;
        public float Y;
        public long T;

        public TimedPoint(float x, float y, long t)
        {
            this.X = x;
            this.Y = y;
            this.T = t;
        }

        public PointF Point
        {
            get { return new PointF(X, Y); }
        }
    }
}
