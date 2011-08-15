using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Kinovea.ScreenManager
{
    public static class Extensions
    {
        /// <summary>
        /// Get a bounding box around a point.
        /// </summary>
        public static Rectangle Box(this Point _point, int _radius)
        {
            return new Rectangle(_point.X - _radius, _point.Y - _radius, _radius * 2, _radius * 2);
        }
        
        /// <summary>
        /// Get a bounding box around a point.
        /// </summary>
        public static Rectangle Box(this Point _point, Size _size)
        {
            return new Rectangle(_point.X - _size.Width/2, _point.Y - _size.Height/2, _size.Width, _size.Height);
        }
        
        /// <summary>
        /// Translate a point by x pixels horizontally, y pixels vertically.
        /// </summary>
        public static Point Translate(this Point _point, int _x, int _y)
        {
            return new Point(_point.X + _x, _point.Y + _y);
        }
        
        /// <summary>
        /// Get the complementary color.
        /// </summary>
        public static Color Invert(this Color _color)
        {
            return Color.FromArgb(_color.A, 255 - _color.R, 255 - _color.G, 255 - _color.B);
        }
    }
}