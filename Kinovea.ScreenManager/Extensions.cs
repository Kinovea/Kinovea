#region License
/*
Copyright © Joan Charmant 2011.
jcharmant@gmail.com 
 
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
        public static Point Scale(this Point point, double scaleX, double scaleY)
        {
            return new Point((int)(point.X * scaleX), (int)(point.Y * scaleY));
        }
        public static Point Scale(this Point point, double scale)
        {
            return new Point((int)(point.X * scale), (int)(point.Y * scale));
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
        public static Point ToPoint(this PointF point)
        {
            return new Point((int)point.X, (int)point.Y);
        }
        public static PointF Translate(this PointF point, float x, float y)
        {
            return new PointF(point.X + x, point.Y + y);
        }
        public static PointF Scale(this PointF point, float scaleX, float scaleY)
        {
            return new PointF(point.X * scaleX, point.Y * scaleY);
        }
        public static PointF Scale(this PointF point, float scale)
        {
            return new PointF(point.X * scale, point.Y * scale);
        }
        public static RectangleF Box(this PointF point, Size size)
        {
            return new RectangleF(point.X - size.Width / 2, point.Y - size.Height / 2, size.Width, size.Height);
        }
        public static RectangleF Box(this PointF point, int radius)
        {
            return new RectangleF(point.X - radius, point.Y - radius, radius * 2, radius * 2);
        }

        /// <summary>
        /// Returns true if this point is less than one unit away from the passed one.
        /// Used to test if two floating point pixel coordinates are conceptually in the same location.
        /// </summary>
        public static bool NearlyCoincideWith(this PointF p1, PointF p2)
        {
            return Math.Abs(p1.X - p2.X) < 0.5f && Math.Abs(p1.Y - p2.Y) < 0.5f;
        }
        
        /// <summary>
        /// Returns true if the size is smaller or equal to the container size on both dimensions.
        /// </summary>
        public static bool FitsIn(this Size size, Size container)
        {
            return size.Width <= container.Width && size.Height <= container.Height;
        }

        /// <summary>
        /// Returns true if the size is almost the same up to a threshold.
        /// </summary>
        public static bool CloseTo(this Size size, Size other, int threshold)
        {
            return Math.Abs(size.Width - other.Width) <= threshold && Math.Abs(size.Height - other.Height) <= threshold;
        }
        public static Size Scale(this Size s, float scale)
        {
            return new Size((int)(s.Width * scale), (int)(s.Height * scale));
        }
        public static Point Center(this Size size)
        {
            return new Point(size.Width / 2, size.Height / 2);
        }


        // SizeF
        public static Size ToSize(this SizeF s)
        {
            return new Size((int)s.Width, (int)s.Height);
        }
        public static SizeF Scale(this SizeF s, float scale)
        {
            return new SizeF(s.Width * scale, s.Height * scale);
        }
        
        // Rectangle
        public static Rectangle Translate(this Rectangle r, int dx, int dy)
        {
            return new Rectangle(r.X + dx, r.Y + dy, r.Width, r.Height);
        }
        public static Rectangle Scale(this Rectangle rect, double scaleX, double scaleY)
        {
            return new Rectangle((int)(rect.Left * scaleX), (int)(rect.Top * scaleY), (int)(rect.Width * scaleX), (int)(rect.Height * scaleY));
        }
        public static Point Center(this Rectangle rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }
        
        // RectangleF
        public static PointF Center(this RectangleF rect)
        {
            return new PointF(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }
        public static RectangleF Translate(this RectangleF r, float dx, float dy)
        {
            return new RectangleF(r.X + dx, r.Y + dy, r.Width, r.Height);
        }
        public static RectangleF CenteredScale(this RectangleF rect, float scale)
        {
            // FIXME: same as .Inflate ?
            // Returns a rectangle centered on the same point but scaled in both dimensions by given factor.
            return new RectangleF(rect.X - (rect.Width * (scale - 1)) / 2, rect.Y - (rect.Height * (scale - 1)) / 2, rect.Width * scale, rect.Height * scale);
        }
        public static Rectangle ToRectangle(this RectangleF rect)
        {
            return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }
        public static RectangleF Deflate(this RectangleF rect, float scale)
        {
            PointF center = rect.Center();
            return new RectangleF(center.X - ((rect.Width / 2) / scale), center.Y - ((rect.Height / 2) / scale), rect.Width / scale, rect.Height / scale);
        }


        // List<T>
        public static List<double> Subtract(this List<double> a, List<double> b)
        {
            if (a.Count != b.Count)
                throw new ArgumentException("Lists must have the same number of elements");
            
            List<double> result = new List<double>();
            for (int i = 0; i < a.Count; i++)
                result.Add(a[i] - b[i]);

            return result;
        }

        public static double[] Subtract(this double[] a, double[] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Lists must have the same number of elements");

            double[] result = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
                result[i] = a[i] - b[i];

            return result;
        }

        /// <summary>
        /// Pick a member of the list that is either exactly the candidate or the next value above the candidate.
        /// </summary>
        public static int PickAmong(this List<int> a, int candidate)
        {
            if (a.IndexOf(candidate) >= 0)
                return candidate;

            // Take the first official option after the input value.
            foreach (int value in a)
            {
                if (candidate > value)
                    continue;

                return value;
            }

            return a[a.Count - 1];
        }
        
    }
}