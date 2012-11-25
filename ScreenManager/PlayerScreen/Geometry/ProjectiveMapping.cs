#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Maps a quadrilateral in image space to a unit square in user defined plane.
    /// Used to process points through the perspective-grid-defined plane and get their 2D coordinates on this plane.
    /// Code mostly extracted from AForge.NET QuadTransformationCalcs.
    /// Based on Paul Heckbert "Projective Mappings for Image Warping".
    /// </summary>
    public class ProjectiveMapping
    {
        private const double TOLERANCE = 1e-13;
        private double[,] mapMatrix;
        private double[,] unmapMatrix;
        
        public void Init(Quadrilateral plane, Quadrilateral image)
        {
           double[,] squareToInput = MapSquareToQuad(plane);
           double[,] squareToOutput = MapSquareToQuad(image);

           if (squareToOutput == null )
               return;

           mapMatrix = MultiplyMatrix(squareToOutput, AdjugateMatrix(squareToInput));
           unmapMatrix = AdjugateMatrix(mapMatrix);
           //unmapMatrix = MultiplyMatrix(AdjugateMatrix(squareToOutput), squareToInput);
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
        /// Maps a point from image coordinates to plane coordinates.
        /// </summary>
        public PointF Backward(PointF p)
        {
           double factor = unmapMatrix[2, 0] * p.X + unmapMatrix[2, 1] * p.Y + unmapMatrix[2, 2];
           double x = (unmapMatrix[0, 0] * p.X + unmapMatrix[0, 1] * p.Y + unmapMatrix[0, 2] ) / factor;
           double y = (unmapMatrix[1, 0] * p.X + unmapMatrix[1, 1] * p.Y + unmapMatrix[1, 2] ) / factor;
           
           return new PointF((float)x, (float)y);
        }
        
        // Get the transform matrix from unit square to quad.
        private static double[,] MapSquareToQuad(Quadrilateral quad)
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
        
        
        
        
    }
}
