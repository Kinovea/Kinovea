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
    public class Quadrilateral : IEnumerable
    {
        #region Properties
        public Point A
        {
            get { return m_Corners[0]; }
            set { m_Corners[0] = value;}
        }
        public Point B
        {
            get { return m_Corners[1]; }
            set { m_Corners[1] = value;}
        }
        public Point C
        {
            get { return m_Corners[2]; }
            set { m_Corners[2] = value;}
        }
        public Point D
        {
            get { return m_Corners[3]; }
            set { m_Corners[3] = value;}
        }
        public Point this [int corner]
        {
            get { return m_Corners[corner]; }
            set { m_Corners[corner] = value; }
        }
        public bool IsConvex
        {
            get { return IsQuadConvex(); }
        }
        public bool IsRectangle
        {
            get { return (A.Y == B.Y && B.X == C.X && C.Y == D.Y && D.X == A.X); }
        }
        public static Quadrilateral UnitRectangle
        {
            get 
            { 
                Quadrilateral q =  new Quadrilateral() {
                    A = new Point(0, 0), 
                    B = new Point(1, 0), 
                    C = new Point(1, 1), 
                    D = new Point(0, 1) 
                }; 
                return q;
            }
        }
        #endregion
        
        #region Members
        private Point[] m_Corners = new Point[4];
        private const double radToDeg = 180D / Math.PI;
        #endregion
        
        #region Public methods
        public void Translate(int x, int y)
        {
            m_Corners = m_Corners.Select( p => p.Translate(x,y)).ToArray();
        }
        public void Expand(int _width, int _height)
        {
            A = A.Translate(-_width, -_height);
            B = B.Translate(_width, -_height);
            C = C.Translate(_width, _height);
            D = D.Translate(-_width, _height);
        }
        public void MakeRectangle(int _anchor)
        {
            // Forces the other points to align with the anchor.
            // Assumes the opposite point is already aligned with the other two.
            switch (_anchor)
            {
                case 0:
                    B = new Point(B.X, A.Y);
                    D = new Point(A.X, D.Y);
                    break;
                case 1:
                    A = new Point(A.X, B.Y);
                    C = new Point(B.X, C.Y);
                    break;
                case 2:
                    D = new Point(D.X, C.Y);
                    B = new Point(C.X, B.Y);
                    break;
                case 3:
                    C = new Point(C.X, D.Y);
                    A = new Point(D.X, A.Y);
                    break;
            }
        }
        public void MakeSquare(int _anchor)
        {
            // Forces the other points to align and makes square on the smallest side.
            // Assumes the opposite point is already aligned with the other two.
            int width = 0;
            int height = 0;
            int side = 0;
            
            switch (_anchor)
            {
                case 0:
                    width = C.X - A.X;
                    height = C.Y - A.Y;
                    side = Math.Min(width, height);
                    A = new Point(C.X - side, C.Y - side);
                    B = new Point(C.X, C.Y - side);
                    D = new Point(C.X - side, C.Y);
                    break;
                case 1:
                    width = B.X - D.X;
                    height = D.Y - B.Y;
                    side = Math.Min(width, height);
                    A = new Point(D.X, D.Y - side);
                    B = new Point(D.X + side, D.Y - side);
                    C = new Point(D.X + side, D.Y);
                    break;
                case 2:
                    width = C.X - A.X;
                    height = C.Y - A.Y;
                    side = Math.Min(width, height);
                    B = new Point(A.X + side, A.Y);
                    C = new Point(A.X + side, A.Y + side);
                    D = new Point(A.X, A.Y + side);
                    break;
                case 3:
                    width = B.X - D.X;
                    height = D.Y - B.Y;
                    side = Math.Min(width, height);
                    A = new Point(B.X - side, B.Y);
                    C = new Point(B.X, B.Y + side);
                    D = new Point(B.X - side, B.Y + side);
                    break;
            }
        }
        public bool Contains(Point _point)
        {
            if (!IsQuadConvex())
                return false;
                
            GraphicsPath areaPath = new GraphicsPath();
            areaPath.AddLine(A, B);
            areaPath.AddLine(B, C);
            areaPath.AddLine(C, D);
            areaPath.CloseAllFigures();
            Region areaRegion = new Region(areaPath);
            
            return areaRegion.IsVisible(_point);
        }
        public Quadrilateral Clone()
        {
            return new Quadrilateral() {A=A, B=B, C=C, D=D};
        }
        public IEnumerator GetEnumerator()
        {
            return m_Corners.GetEnumerator();
        }
        public Point[] ToArray()
        {
            return m_Corners.ToArray();
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
        private double GetAngle(Point _a, Point _b, Point _c)
        {
            // Compute the angle ABC.
            // using scalar and vector product between vectors BA and BC.

            double bax = (double)(_a.X - _b.X);
            double bcx = (double)(_c.X - _b.X);
            double scalX =  bax * bcx;

            double bay = (double)(_a.Y - _b.Y);
            double bcy = (double)(_c.Y - _b.Y);
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
