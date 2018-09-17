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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A class representing a quadrilateral, with some helper methods.
    /// The corners can be accessed via ABCD properties, the indexer or the enumerator.
    /// When using the indexer, A=0, B=1, C=2, D=3.
    /// Points are defined clockwise, "A" being top left.
    /// Note that unlike Rectangle, this is a reference type.
    /// </summary>
    public class QuadrilateralF : IEnumerable
    {
        #region Properties
        public PointF A
        {
            get { return corners[0]; }
            set { corners[0] = value;}
        }
        public PointF B
        {
            get { return corners[1]; }
            set { corners[1] = value;}
        }
        public PointF C
        {
            get { return corners[2]; }
            set { corners[2] = value;}
        }
        public PointF D
        {
            get { return corners[3]; }
            set { corners[3] = value;}
        }
        public PointF this [int corner]
        {
            get { return corners[corner]; }
            set { corners[corner] = value; }
        }
        public bool IsConvex
        {
            get { return IsQuadConvex(); }
        }
        public bool IsRectangle
        {
            get { return (A.Y == B.Y && B.X == C.X && C.Y == D.Y && D.X == A.X); }
        }
        public static QuadrilateralF UnitSquare
        {
            get 
            { 
                return new QuadrilateralF(1, 1);
            }
        }
        public static QuadrilateralF CenteredUnitSquare
        {
            get
            {
                return new QuadrilateralF(new PointF(-1, 1), new PointF(1, 1), new PointF(1, -1), new PointF(-1, -1));
            }
        }
        #endregion
        
        #region Members
        private PointF[] corners = new PointF[4];
        private const double radToDeg = 180D / Math.PI;
        #endregion
        
        public QuadrilateralF()
        {
        }
        
        public QuadrilateralF(PointF a, PointF b, PointF c, PointF d)
        {
            A = new PointF(a.X, a.Y);
            B = new PointF(b.X, b.Y);
            C = new PointF(c.X, c.Y);
            D = new PointF(d.X, d.Y);
        }
        
        public QuadrilateralF(float width, float height)
        {
            A = new PointF(0, 0);
            B = new PointF(width, 0);
            C = new PointF(width, height);
            D = new PointF(0, height);
        }

        public QuadrilateralF(RectangleF rect)
        {
            A = rect.Location;
            B = new PointF(rect.Right, rect.Top);
            C = new PointF(rect.Right, rect.Bottom);
            D = new PointF(rect.Left, rect.Bottom);
        }

        #region Public methods
        public void Translate(float x, float y)
        {
            corners = corners.Select( p => p.Translate(x,y)).ToArray();
        }
        public void Scale(float x, float y)
        {
            corners = corners.Select(p => p.Scale(x, y)).ToArray();
        }
        public void Expand(float width, float height)
        {
            A = A.Translate(-width, -height);
            B = B.Translate(width, -height);
            C = C.Translate(width, height);
            D = D.Translate(-width, height);
        }
        public void MakeRectangle(int anchor)
        {
            // Forces the other points to align with the anchor.
            switch (anchor)
            {
                case 0:
                    B = new PointF(B.X, A.Y);
                    D = new PointF(A.X, D.Y);
                    C = new PointF(B.X, D.Y);
                    break;
                case 1:
                    A = new PointF(A.X, B.Y);
                    C = new PointF(B.X, C.Y);
                    D = new PointF(A.X, C.Y);
                    break;
                case 2:
                    D = new PointF(D.X, C.Y);
                    B = new PointF(C.X, B.Y);
                    A = new PointF(D.X, B.Y);
                    break;
                case 3:
                    C = new PointF(C.X, D.Y);
                    A = new PointF(D.X, A.Y);
                    B = new PointF(C.X, A.Y);
                    break;
            }
        }
        public void MakeSquare(int anchor)
        {
            // Forces the other points to align and makes square on the smallest side.
            // Assumes the opposite point is already aligned with the other two.
            float width = 0;
            float height = 0;
            float side = 0;
            
            switch (anchor)
            {
                case 0:
                    width = C.X - A.X;
                    height = C.Y - A.Y;
                    side = Math.Min(width, height);
                    A = new PointF(C.X - side, C.Y - side);
                    B = new PointF(C.X, C.Y - side);
                    D = new PointF(C.X - side, C.Y);
                    break;
                case 1:
                    width = B.X - D.X;
                    height = D.Y - B.Y;
                    side = Math.Min(width, height);
                    A = new PointF(D.X, D.Y - side);
                    B = new PointF(D.X + side, D.Y - side);
                    C = new PointF(D.X + side, D.Y);
                    break;
                case 2:
                    width = C.X - A.X;
                    height = C.Y - A.Y;
                    side = Math.Min(width, height);
                    B = new PointF(A.X + side, A.Y);
                    C = new PointF(A.X + side, A.Y + side);
                    D = new PointF(A.X, A.Y + side);
                    break;
                case 3:
                    width = B.X - D.X;
                    height = D.Y - B.Y;
                    side = Math.Min(width, height);
                    A = new PointF(B.X - side, B.Y);
                    C = new PointF(B.X, B.Y + side);
                    D = new PointF(B.X - side, B.Y + side);
                    break;
            }
        }
        public bool Contains(PointF point)
        {
            if (!IsQuadConvex())
                return false;
            
            bool hit = false;
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                areaPath.AddPolygon(corners.ToArray());
                using(Region r = new Region(areaPath))
                {
                    hit = r.IsVisible(point);
                }
            }
            return hit;
        }
        public QuadrilateralF Clone()
        {
            return new QuadrilateralF(A,B,C,D);
        }
        public IEnumerator GetEnumerator()
        {
            return corners.GetEnumerator();
        }
        public PointF[] ToArray()
        {
            return corners.ToArray();
        }
        public RectangleF GetBoundingBox()
        {
            float top = float.MaxValue;
            float left = float.MaxValue;
            float bottom = float.MinValue;
            float right = float.MinValue;
            
            foreach(PointF corner in corners)
            {
                top = Math.Min(top, corner.Y);
                left = Math.Min(left, corner.X);
                bottom = Math.Max(bottom, corner.Y);
                right = Math.Max(right, corner.X);
            }
            
            return new RectangleF(left, top, right - left, bottom - top);
        }

        public override int GetHashCode()
        {
            int iHash = 0;
            iHash ^= A.GetHashCode();
            iHash ^= B.GetHashCode();
            iHash ^= C.GetHashCode();
            iHash ^= D.GetHashCode();
            return iHash;
        } 
        
        #endregion

        #region Private methods
        private bool IsQuadConvex()
        {
            // Angles must all be > 180 or all < 180.
            double[] angles = new double[4];
            angles[0] = GetAngle(A, B, C);
            angles[1] = GetAngle(B, C, D);
            angles[2] = GetAngle(C, D, A);
            angles[3] = GetAngle(D, A, B);

            if ((angles[0] > 0 && angles[1] > 0 && angles[2] > 0 && angles[3] > 0) ||
                (angles[0] < 0 && angles[1] < 0 && angles[2] < 0 && angles[3] < 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private double GetAngle(PointF a, PointF b, PointF c)
        {
            // Compute the angle ABC.
            // using scalar and vector product between vectors BA and BC.

            double bax = (double)(a.X - b.X);
            double bcx = (double)(c.X - b.X);
            double scalX =  bax * bcx;

            double bay = (double)(a.Y - b.Y);
            double bcy = (double)(c.Y - b.Y);
            double scalY = bay * bcy;
            
            double scal = scalX + scalY;
            
            double normab = Math.Sqrt(bax * bax + bay * bay);
            double normbc = Math.Sqrt(bcx * bcx + bcy * bcy);
            double norm = normab * normbc;

            double angle = Math.Acos((double)(scal / norm));

            if ((bax * bcy - bay * bcx) < 0)
            {
                angle = -angle;
            }

            return angle * radToDeg;
        }
        #endregion
   }
}

