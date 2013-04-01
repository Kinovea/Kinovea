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
        // Point
        public static Point Translate(this Point _point, int _x, int _y)
        {
            return new Point(_point.X + _x, _point.Y + _y);
        }
        public static Point Scale(this Point _point, double _scaleX, double _scaleY)
        {
            return new Point((int)(_point.X * _scaleX), (int)(_point.Y * _scaleY));
        }
        public static PointF ToPointF(this Point point)
        {
            return new PointF(point.X, point.Y);
        }
        public static Point Subtract(this Point p, Point p2)
        {
            return new Point(p.X - p2.X, p.Y - p2.Y);
        }
        public static Rectangle Box(this Point _point, int _radius)
        {
            return new Rectangle(_point.X - _radius, _point.Y - _radius, _radius * 2, _radius * 2);
        }
        public static Rectangle Box(this Point _point, Size _size)
        {
            return new Rectangle(_point.X - _size.Width/2, _point.Y - _size.Height/2, _size.Width, _size.Height);
        }
        
        // PointF
        public static Point Translate(this PointF _point, int _x, int _y)
        {
            return new Point((int)_point.X + _x, (int)_point.Y + _y);
        }
        public static PointF Translate(this PointF _point, float _x, float _y)
        {
            return new PointF(_point.X + _x, _point.Y + _y);
        }
        public static PointF Scale(this PointF _point, float _scaleX, float _scaleY)
        {
            return new PointF(_point.X * _scaleX, _point.Y * _scaleY);
        }
        public static Rectangle Box(this PointF _point, int _radius)
        {
            return new Rectangle((int)_point.X - _radius, (int)_point.Y - _radius, _radius * 2, _radius * 2);
        }
        
        // Color
        public static Color Invert(this Color _color)
        {
            return Color.FromArgb(_color.A, 255 - _color.R, 255 - _color.G, 255 - _color.B);
        }
        
        // Size
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
        public static Size Scale(this Size s, float scale)
        {
            return new Size((int)(s.Width * scale), (int)(s.Height * scale));
        }
        
        // Rectangle
        public static Rectangle Translate(this Rectangle r, Point t)
        {
            return new Rectangle(r.X + t.X, r.Y + t.Y, r.Width, r.Height);
        }
        public static Rectangle Scale(this Rectangle _rect, double _scaleX, double _scaleY)
        {
            return new Rectangle((int)(_rect.Left * _scaleX), (int)(_rect.Top * _scaleY), (int)(_rect.Width * _scaleX), (int)(_rect.Height * _scaleY));
        }
        public static Point Center(this Rectangle _rect)
        {
            return new Point(_rect.X + _rect.Width / 2, _rect.Y + _rect.Height / 2);
        }
        public static PointF Center(this RectangleF rect)
        {
            return new PointF(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }
    }
}