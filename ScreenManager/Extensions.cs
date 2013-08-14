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
        public static Point Translate(this Point point, int x, int y)
        {
            return new Point(point.X + x, point.Y + y);
        }
        public static Point Translate(this Point point, Point translation)
        {
            return new Point(point.X + translation.X, point.Y + translation.Y);
        }
        public static Point Scale(this Point point, double scaleX, double scaleY)
        {
            return new Point((int)(point.X * scaleX), (int)(point.Y * scaleY));
        }
        public static Point Scale(this Point point, double scale)
        {
            return new Point((int)(point.X * scale), (int)(point.Y * scale));
        }
        public static PointF ToPointF(this Point point)
        {
            return new PointF(point.X, point.Y);
        }
        public static Point Subtract(this Point p, Point p2)
        {
            return new Point(p.X - p2.X, p.Y - p2.Y);
        }
        public static Rectangle Box(this Point point, int radius)
        {
            return new Rectangle(point.X - radius, point.Y - radius, radius * 2, radius * 2);
        }
        public static Rectangle Box(this Point point, Size size)
        {
            return new Rectangle(point.X - size.Width/2, point.Y - size.Height/2, size.Width, size.Height);
        }
        
        // PointF
        // TODO: check if the PointF to Point extensions are really needed.
        public static Point Translate(this PointF point, int x, int y)
        {
            return new Point((int)point.X + x, (int)point.Y + y);
        }
        public static PointF Translate(this PointF point, float x, float y)
        {
            return new PointF(point.X + x, point.Y + y);
        }
        public static Point Translate(this PointF point, Point translation)
        {
            return new Point((int)point.X + translation.X, (int)point.Y + translation.Y);
        }
        public static PointF Scale(this PointF point, float scaleX, float scaleY)
        {
            return new PointF(point.X * scaleX, point.Y * scaleY);
        }
        public static PointF Scale(this PointF point, float scale)
        {
            return new PointF(point.X * scale, point.Y * scale);
        }
        public static Rectangle Box(this PointF point, int radius)
        {
            return new Rectangle((int)point.X - radius, (int)point.Y - radius, radius * 2, radius * 2);
        }
        
        // Color
        public static Color Invert(this Color color)
        {
            return Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
        }
        
        // Size
        public static bool FitsIn(this Size size, Size container)
        {
            return size.Width <= container.Width && size.Height <= container.Height;
        }
        public static bool CloseTo(this Size size, Size other)
        {
            if(size == other)
                return true;
            
            int widthDifference = size.Width - other.Width;
            int heightDifference = size.Height - other.Height;
            return widthDifference > -4 && widthDifference < 4 && heightDifference > -4 && heightDifference < 4;
        }
        public static Size Scale(this Size s, float scale)
        {
            return new Size((int)(s.Width * scale), (int)(s.Height * scale));
        }
        
        // SizeF
        public static Size Scale(this SizeF s, float scale)
        {
            return new Size((int)(s.Width * scale), (int)(s.Height * scale));
        }
        
        // Rectangle
        public static Rectangle Translate(this Rectangle r, Point t)
        {
            return new Rectangle(r.X + t.X, r.Y + t.Y, r.Width, r.Height);
        }
        public static Rectangle Scale(this Rectangle rect, double scaleX, double scaleY)
        {
            return new Rectangle((int)(rect.Left * scaleX), (int)(rect.Top * scaleY), (int)(rect.Width * scaleX), (int)(rect.Height * scaleY));
        }
        public static Point Center(this Rectangle rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }
        public static PointF Center(this RectangleF rect)
        {
            return new PointF(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }
    }
}