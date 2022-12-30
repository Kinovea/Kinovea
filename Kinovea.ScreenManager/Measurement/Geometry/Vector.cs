#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public struct Vector
    {
        public float X;
        public float Y;
        
        public Vector(float _x, float _y)
        {
            X=_x;
            Y=_y;
        }
        public Vector(Point a, Point b)
        {
             X = b.X - a.X;
             Y = b.Y - a.Y;
        }
        public Vector(PointF a, PointF b)
        {
             X = b.X - a.X;
             Y = b.Y - a.Y;
        }
        
        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }
        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }
        public static PointF operator +(Vector v, Point p)
        {
            return p+v;
        }
        public static PointF operator +(Point p, Vector v)
        {
            return new PointF(p.X + v.X, p.Y + v.Y);
        }
        public static PointF operator +(Vector v, PointF p)
        {
            return p+v;
        }
        public static PointF operator +(PointF p, Vector v)
        {
            return new PointF(p.X + v.X, p.Y + v.Y);
        }
        public static Vector operator *(Vector v, float f)
        {
            return new Vector(v.X * f, v.Y * f);
        }
        public static Vector operator *(float f, Vector v)
        {
            return v*f;
        }
        public static Vector operator /(Vector v, float f)
        {
            return new Vector(v.X / f, v.Y / f);
        }
        public static Vector operator /(float f, Vector v)
        {
            return v/f;
        }
        
        public float Dot(Vector v)
        {
            return X * v.X + Y * v.Y;
        }
        public float Norm()
        {
            return (float)Math.Sqrt((double)Squared());
        }
        public float Squared()
        {
            return X*X + Y*Y;
        }
        public Vector Normalized()
        {
            return this * (1.0f / Norm());
        }
        public Vector Negate()
        {
            return new Vector(-X, -Y);
        }

        public PointF ToPointF()
        {
            return new PointF(X, Y);
        }
        public override string ToString()
        {
            return string.Format("[Vector X={0}, Y={1}]", X, Y);
        }

    }
}
