#region License
/*
Copyright © Joan Charmant 2011.
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
        
        public static Rectangle Box(this PointF _point, int _radius)
        {
            return new Rectangle((int)_point.X - _radius, (int)_point.Y - _radius, _radius * 2, _radius * 2);
        }
        
        /// <summary>
        /// Get the center of a rectangle.
        /// </summary>
        public static Point Center(this Rectangle _rect)
        {
            return new Point(_rect.X + _rect.Width / 2, _rect.Y + _rect.Height / 2);
        }
        
        /// <summary>
        /// Translate a point by x pixels horizontally, y pixels vertically.
        /// </summary>
        public static Point Translate(this Point _point, int _x, int _y)
        {
            return new Point(_point.X + _x, _point.Y + _y);
        }
        public static Point Translate(this PointF _point, int _x, int _y)
        {
            return new Point((int)_point.X + _x, (int)_point.Y + _y);
        }
        public static Point Scale(this Point _point, double _scaleX, double _scaleY)
        {
            return new Point((int)(_point.X * _scaleX), (int)(_point.Y * _scaleY));
        }
        public static PointF Scale(this PointF _point, float _scaleX, float _scaleY)
        {
            return new PointF(_point.X * _scaleX, _point.Y * _scaleY);
        }
        
        /// <summary>
        /// Get the complementary color.
        /// </summary>
        public static Color Invert(this Color _color)
        {
            return Color.FromArgb(_color.A, 255 - _color.R, 255 - _color.G, 255 - _color.B);
        }
        
        public static bool FitsIn(this Size _size, Size _container)
        {
            return _size.Width <= _container.Width && _size.Height <= _container.Height;
        }
        public static bool CloseTo(this Size _size, Size _other)
        {
            if(_size == _other)
                return true;
            
            int widthDifference = _size.Width - _other.Width;
            int heightDifference = _size.Height - _other.Height;
            return widthDifference > -4 && widthDifference < 4 && heightDifference > -4 && heightDifference < 4;
        }
        public static Rectangle Scale(this Rectangle _rect, double _scaleX, double _scaleY)
        {
            return new Rectangle((int)(_rect.Left * _scaleX), (int)(_rect.Top * _scaleY), (int)(_rect.Width * _scaleX), (int)(_rect.Height * _scaleY));
        }
    }
}