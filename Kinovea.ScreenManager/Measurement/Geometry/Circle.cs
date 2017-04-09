using System;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public struct Circle
    {
        public PointF Center;
        public float Radius;

        public static readonly Circle Empty;
        
        static Circle()
        {
            Empty = new Circle(PointF.Empty, 0);
        }

        public Circle(PointF center, float radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        public override string ToString()
        {
            return string.Format("[Circle Center={0}, r={1:0.000}]", Center, Radius);
        }
    }
}
