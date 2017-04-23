#region License
/*
Copyright © Joan Charmant 2017.
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
using MathNet.Numerics.LinearAlgebra;

namespace Kinovea.ScreenManager
{
    public static class CircleFitter
    {
        public static Circle Fit(FilteredTrajectory traj)
        {
            if (traj.Length < 3)
                return Circle.Empty;

            // Least-squares circle fitting.
            // Ref: "Circle fitting by linear and nonlinear least squares", Coope, I.D., 
            // Journal of Optimization Theory and Applications Volume 76, Issue 2, New York: Plenum Press, February 1993.
            // Implementation based on JS implementation: 
            // http://jsxgraph.uni-bayreuth.de/wiki/index.php/Least-squares_circle_fitting

            int rows = traj.Length;
            Matrix m = new Matrix(rows, 3);
            Matrix v = new Matrix(rows, 1);

            for (int i = 0; i < rows; i++)
            {
                PointF point = traj.Coordinates(i);
                m[i, 0] = point.X;
                m[i, 1] = point.Y;
                m[i, 2] = 1.0;
                v[i, 0] = point.X * point.X + point.Y * point.Y;
            }

            try
            {
                Matrix mt = m.Clone();
                mt.Transpose();
                Matrix b = mt.Multiply(m);
                Matrix c = mt.Multiply(v);
                Matrix z = b.Solve(c);

                PointF center = new PointF((float)(z[0, 0] * 0.5), (float)(z[1, 0] * 0.5));
                double radius = Math.Sqrt(z[2, 0] + (center.X * center.X) + (center.Y * center.Y));

                return new Circle(center, (float)radius);
            }
            catch (InvalidOperationException)
            {
                return Circle.Empty;
            }
        }
    }
}
