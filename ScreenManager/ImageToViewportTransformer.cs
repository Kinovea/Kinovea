#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Drawing;
using System.Linq;

namespace Kinovea.ScreenManager
{
    public class ImageToViewportTransformer : IImageToViewportTransformer
    {
        #region Properties
        public double Scale
        {
            get { return scale; }
        }
        #endregion
        
        #region Members
        private double scale;
        private Point location;
        #endregion
    
        public ImageToViewportTransformer(double scale, Point location)
        {
            this.scale = scale;
            this.location = location;
        }

        #region Public methods
        public Point Transform(Point point)
        {
            return point.Scale(scale).Translate(location);
        }
        public Point Transform(PointF point)
        {
            return point.Scale((float)scale).Translate(location);
        }
        public List<Point> Transform(List<PointF> points)
        {
            return points.Select(p => Transform(p)).ToList();
        }
        public int Transform(int distance)
        {
            return (int)(distance * scale);
        }
        public Size Transform(Size size)
        {
            return size.Scale((float)scale);
        }
        public Size Transform(SizeF size)
        {
            return size.Scale((float)scale);
        }
        public Rectangle Transform(Rectangle rectangle)
        {
            return new Rectangle(Transform(rectangle.Location), Transform(rectangle.Size));
        }
        public Rectangle Transform(RectangleF rectangle)
        {
            return new Rectangle(Transform(rectangle.Location), Transform(rectangle.Size));
        }
        public List<Rectangle> Transform(List<Rectangle> rectangles)
        {
            return rectangles.Select(r => Transform(r)).ToList();
        }
        public QuadrilateralF Transform(QuadrilateralF quadrilateral)
        {
            Point a = Transform(quadrilateral.A);
            Point b = Transform(quadrilateral.B);
            Point c = Transform(quadrilateral.C);
            Point d = Transform(quadrilateral.D);
            return new QuadrilateralF(a, b, c, d);
        }
        #endregion
    }
}
