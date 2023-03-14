#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Temporary interface while the two transformation systems are alive.
    /// The transformation system for the capture screen has been refactored in 0.8.21.
    /// When the new transformation system is ported to the player screen, this interface can die.
    /// </summary>
    public interface IImageToViewportTransformer
    {
        double Scale { get; }
        
        // Transform : from image coordinates to screen coordinates.
        Point Transform(Point point);
        Point Transform(PointF point);
        List<Point> Transform(IEnumerable<PointF> points);
        int Transform(int distance);
        Size Transform(Size size);
        Size Transform(SizeF size);
        Rectangle Transform(Rectangle rectangle);
        Rectangle Transform(RectangleF rectangle);
        List<Rectangle> Transform(List<Rectangle> rectangles);
        QuadrilateralF Transform(QuadrilateralF quadrilateral);
        
        // Untransform : from screen coordinates to image coordinates.
        PointF Untransform(Point point);
        SizeF Untransform(SizeF size);
        RectangleF Untransform(Rectangle rectangle);
        int Untransform(int value);
    }
}
