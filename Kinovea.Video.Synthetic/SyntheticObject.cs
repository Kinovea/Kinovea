using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.Video.Synthetic
{
    /// <summary>
    /// An object to be drawn on the video.
    /// All kinematics are expressed in pixels and seconds.
    /// </summary>
    public class SyntheticObject
    {
        public int Radius { get; set; }
        public PointF Position { get; set; }
        public double VX { get; set; }
        public double VY { get; set; }
        public double AX { get; set; }
        public double AY { get; set; }

        public SyntheticObject(int radius, PointF position, double vx, double vy, double ax, double ay)
        {
           this.Radius = radius;
           this.Position = position;
           this.VX = vx;
           this.VY = vy;
           this.AX = ax;
           this.AY = ay;
        }
    }
}
