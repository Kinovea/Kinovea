using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Kinovea.ScreenManager
{
    public static class PointExtensions
    {
        /// <summary>
        /// Get a bounding box around the point.
        /// </summary>
        public static Rectangle Box(this Point _point, int _radius)
        {
            return new Rectangle(_point.X - _radius, _point.Y - _radius, _radius * 2, _radius * 2);
        }
    }
}