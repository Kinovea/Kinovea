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
    /// <summary>
    /// Maps a quadrilateral in image space to a quadrilateral in user defined plane.
    /// Used to process points through the perspective-grid-defined plane and get their 2D coordinates on this plane.
    /// Code mostly extracted from AForge.NET QuadTransformationCalcs.
    /// Based on Paul Heckbert "Projective Mappings for Image Warping".
    /// </summary>
    public class ProjectiveMapping
    {
        /// <summary>
        /// x' = Hx. Finds projected coordinates (image coordinates) from plane coordinate.
        /// </summary>
        public Matrix3x3 Matrix
        {
            get { return matrix; }
        }

        /// <summary>
        /// Finds plane coordinates from projected coordinates.
        /// </summary>
        public Matrix3x3 Adjugate
        {
            get { return adjugate; }
        }

        public Matrix3x3 Inverse
        {
            get { return inverse; }
        }
        

        private const double TOLERANCE = 1e-13;
        private double[,] mapMatrix;
        private double[,] unmapMatrix;

        private Matrix3x3 matrix;
        private Matrix3x3 adjugate;
        private Matrix3x3 inverse;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Update(QuadrilateralF plane, QuadrilateralF image)
        {
            double[,] squareToInput = MapSquareToQuad(plane);
            double[,] squareToOutput = MapSquareToQuad(image);

            if (squareToOutput == null )
                return;

            mapMatrix = MultiplyMatrix(squareToOutput, AdjugateMatrix(squareToInput));
            unmapMatrix = AdjugateMatrix(mapMatrix);

            Vector3 row0 = new Vector3((float)mapMatrix[0, 0], (float)mapMatrix[0, 1], (float)mapMatrix[0, 2]);
            Vector3 row1 = new Vector3((float)mapMatrix[1, 0], (float)mapMatrix[1, 1], (float)mapMatrix[1, 2]);
            Vector3 row2 = new Vector3((float)mapMatrix[2, 0], (float)mapMatrix[2, 1], (float)mapMatrix[2, 2]);
            matrix = Matrix3x3.CreateFromRows(row0, row1, row2);
            
            adjugate = matrix.Adjugate();

            try
            {
                inverse = matrix.Inverse();
            }
            catch
            {
                // Singular matrix.
                inverse = Matrix3x3.Identity;
            }
        }
        
        /// <summary>
        /// Maps a point from plane coordinates to image coordinates.
        /// </summary>
        public PointF Forward(PointF p)
        {
           double factor = mapMatrix[2, 0] * p.X + mapMatrix[2, 1] * p.Y + mapMatrix[2, 2];
           double x = (mapMatrix[0, 0] * p.X + mapMatrix[0, 1] * p.Y + mapMatrix[0, 2] ) / factor;
           double y = (mapMatrix[1, 0] * p.X + mapMatrix[1, 1] * p.Y + mapMatrix[1, 2] ) / factor;
           
           return new PointF((float)x, (float)y);
        }

        /// <summary>
        /// Maps a quadrilateral from plane coordinates to image coordinates.
        /// </summary>
        public QuadrilateralF Forward(QuadrilateralF q)
        {
            PointF a = Forward(q.A);
            PointF b = Forward(q.B);
            PointF c = Forward(q.C);
            PointF d = Forward(q.D);
            return new QuadrilateralF(a, b, c, d);
        }
        
        /// <summary>
        /// Maps a point from image coordinates to plane coordinates.
        /// </summary>
        public PointF Backward(PointF p)
        {
           double factor = unmapMatrix[2, 0] * p.X + unmapMatrix[2, 1] * p.Y + unmapMatrix[2, 2];
           double x = (unmapMatrix[0, 0] * p.X + unmapMatrix[0, 1] * p.Y + unmapMatrix[0, 2] ) / factor;
           double y = (unmapMatrix[1, 0] * p.X + unmapMatrix[1, 1] * p.Y + unmapMatrix[1, 2] ) / factor;
           
           return new PointF((float)x, (float)y);
        }
        
        /// <summary>
        /// Maps a quadrilateral from image coordinates to plane coordinates.
        /// </summary>
        public QuadrilateralF Backward(QuadrilateralF q)
        {
            PointF a = Backward(q.A);
            PointF b = Backward(q.B);
            PointF c = Backward(q.C);
            PointF d = Backward(q.D);
            return new QuadrilateralF(a, b, c, d);
        }

        #region Homogenous coordinates

        /// <summary>
        /// Maps a vector from plane coordinates to image coordinates.
        /// </summary>
        public Vector3 Forward(Vector3 p)
        {
            return Matrix3x3.Multiply(matrix, p);
        }

        /// <summary>
        /// Maps a vector from image coordinates to plane coordinates.
        /// </summary>
        public Vector3 Backward(Vector3 p)
        {
            return Matrix3x3.Multiply(adjugate, p);
        }
        
        #endregion


        public Ellipse Ellipse()
        {
            // Maps unit circle in plane (-1;+1) to ellipse in image.
            // Used to draw angles in perspective.
            
            // Based on the following resources:
            // http://chrisjones.id.au/Ellipses/ellipse.html
            // http://mathworld.wolfram.com/Ellipse.html
            
            double j = unmapMatrix[0,0];
            double k = unmapMatrix[0,1];
            double l = unmapMatrix[0,2];
            double m = unmapMatrix[1,0];
            double n = unmapMatrix[1,1];
            double o = unmapMatrix[1,2];
            double p = unmapMatrix[2,0];
            double q = unmapMatrix[2,1];
            double r = unmapMatrix[2,2];
            
            // Ellipse : ax² + 2bxy + cy² + 2dx + 2fy + g = 0
            
            double a = (j*j) + (m*m) - (p*p);
            double b = (j*k) + (m*n) - (p*q);
            double c = (k*k) + (n*n) - (q*q);
            double d = (j*l) + (m*o) - (p*r);
            double f = (k*l) + (n*o) - (q*r);
            double g = (l*l) + (o*o) - (r*r);
            
            double factor = b*b - a*c;
            
            double x0 = (c*d - b*f) / factor;
            double y0 = (a*f - b*d) / factor;
            PointF center = new PointF((float)x0, (float)y0);
            
            double num = 2 * (a*f*f + c*d*d + g*b*b - 2*b*d*f - a*c*g);
            double factor2 = Math.Sqrt((a-c)*(a-c) + 4*b*b);
            double semiMajorAxis = Math.Sqrt(num / (factor * (factor2 - (a+c))));
            double semiMinorAxis = Math.Sqrt(num / (factor * (-factor2 - (a+c))));
            
            double rotation = 0;
            if(b==0)
            {
                if(a<c)
                    rotation = 0;
                else
                    rotation = Math.PI / 2;
            }
            else
            {
                if(a<c)
                    rotation = 0.5 * Arccotan((a-c) / (2*b));
                else
                    rotation = Math.PI / 2 + 0.5 * Arccotan((a-c) / (2*b));
                
                // Hack to fix above formulas.
                if(rotation > Math.PI / 4 && rotation < Math.PI / 2)
                    rotation += (Math.PI/2);
                else if(rotation > Math.PI*0.75 && rotation < Math.PI)
                    rotation -= (Math.PI / 2);
            }
            
            return new Ellipse(center, (float)semiMajorAxis, (float)semiMinorAxis, (float)rotation);
        }
        
        // Get the transform matrix from unit square to quad.
        private static double[,] MapSquareToQuad(QuadrilateralF quad)
        {
            double[,] sq = new double[3, 3];
            double px, py;

            px = quad[0].X - quad[1].X + quad[2].X - quad[3].X;
            py = quad[0].Y - quad[1].Y + quad[2].Y - quad[3].Y;

            if ( ( px < TOLERANCE ) && ( px > -TOLERANCE ) &&
                 ( py < TOLERANCE ) && ( py > -TOLERANCE ) )
            {
                // Input quadrilateral is a parallelogram, the mapping is affine.
                sq[0, 0] = quad[1].X - quad[0].X;
                sq[0, 1] = quad[2].X - quad[1].X;
                sq[0, 2] = quad[0].X;

                sq[1, 0] = quad[1].Y - quad[0].Y;
                sq[1, 1] = quad[2].Y - quad[1].Y;
                sq[1, 2] = quad[0].Y;

                sq[2, 0] = 0.0;
                sq[2, 1] = 0.0;
                sq[2, 2] = 1.0;
            }
            else
            {
                // Projective mapping.
                double dx1, dx2, dy1, dy2, del;

                dx1 = quad[1].X - quad[2].X;
                dx2 = quad[3].X - quad[2].X;
                dy1 = quad[1].Y - quad[2].Y;
                dy2 = quad[3].Y - quad[2].Y;

                del = Det2( dx1, dx2, dy1, dy2 );

                if ( del == 0.0 )
                    return null;

                sq[2, 0] = Det2( px, dx2, py, dy2 ) / del;
                sq[2, 1] = Det2( dx1, px, dy1, py ) / del;
                sq[2, 2] = 1.0;

                sq[0, 0] = quad[1].X - quad[0].X + sq[2, 0] * quad[1].X;
                sq[0, 1] = quad[3].X - quad[0].X + sq[2, 1] * quad[3].X;
                sq[0, 2] = quad[0].X;

                sq[1, 0] = quad[1].Y - quad[0].Y + sq[2, 0] * quad[1].Y;
                sq[1, 1] = quad[3].Y - quad[0].Y + sq[2, 1] * quad[3].Y;
                sq[1, 2] = quad[0].Y;
            }
            
            return sq;
        }
        
        // Caclculates determinant of a 2x2 matrix
        private static double Det2(double a, double b, double c, double d )
        {
            return (a * d - b * c );
        }

        // Multiply two 3x3 matrices
        private static double[,] MultiplyMatrix(double[,] a, double[,] b )
        {
            double[,] c = new double[3, 3];

            c[0, 0] = a[0, 0] * b[0, 0] + a[0, 1] * b[1, 0] + a[0, 2] * b[2, 0];
            c[0, 1] = a[0, 0] * b[0, 1] + a[0, 1] * b[1, 1] + a[0, 2] * b[2, 1];
            c[0, 2] = a[0, 0] * b[0, 2] + a[0, 1] * b[1, 2] + a[0, 2] * b[2, 2];
            c[1, 0] = a[1, 0] * b[0, 0] + a[1, 1] * b[1, 0] + a[1, 2] * b[2, 0];
            c[1, 1] = a[1, 0] * b[0, 1] + a[1, 1] * b[1, 1] + a[1, 2] * b[2, 1];
            c[1, 2] = a[1, 0] * b[0, 2] + a[1, 1] * b[1, 2] + a[1, 2] * b[2, 2];
            c[2, 0] = a[2, 0] * b[0, 0] + a[2, 1] * b[1, 0] + a[2, 2] * b[2, 0];
            c[2, 1] = a[2, 0] * b[0, 1] + a[2, 1] * b[1, 1] + a[2, 2] * b[2, 1];
            c[2, 2] = a[2, 0] * b[0, 2] + a[2, 1] * b[1, 2] + a[2, 2] * b[2, 2];

            return c;
        }
        
        // Calculates adjugate 3x3 matrix
        private static double[,] AdjugateMatrix(double[,] a )
        {
            double[,] b = new double[3, 3];
            b[0, 0] = Det2( a[1, 1], a[1, 2], a[2, 1], a[2, 2] );
            b[1, 0] = Det2( a[1, 2], a[1, 0], a[2, 2], a[2, 0] );
            b[2, 0] = Det2( a[1, 0], a[1, 1], a[2, 0], a[2, 1] );
            b[0, 1] = Det2( a[2, 1], a[2, 2], a[0, 1], a[0, 2] );
            b[1, 1] = Det2( a[2, 2], a[2, 0], a[0, 2], a[0, 0] );
            b[2, 1] = Det2( a[2, 0], a[2, 1], a[0, 0], a[0, 1] );
            b[0, 2] = Det2( a[0, 1], a[0, 2], a[1, 1], a[1, 2] );
            b[1, 2] = Det2( a[0, 2], a[0, 0], a[1, 2], a[1, 0] );
            b[2, 2] = Det2( a[0, 0], a[0, 1], a[1, 0], a[1, 1] );

            return b;
        }

        private static double Arccotan(double x)
        {
            return 2 * Math.Atan(1) - Math.Atan(x);
        }
    }
}
