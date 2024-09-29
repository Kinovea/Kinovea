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
using System.Drawing.Imaging;

namespace Kinovea.ScreenManager
{
    public static class Tracker
    {  
        /// <summary>
        /// Tracks a reference template in the given image. Returns similarity score and position of best candidate.
        /// </summary>
        public static TrackResult Track(Size searchWindow, TrackFrame reference, TrackingContext context)
        {
            if(context.CVImage == null || reference.Template == null)
                throw new ArgumentException("image");

            Rectangle searchZone = reference.Location.Box(searchWindow).ToRectangle();
            Rectangle imageBounds = new Rectangle(0, 0, context.CVImage.Width, context.CVImage.Height);
            searchZone.Intersect(imageBounds);
            
            if(searchZone == Rectangle.Empty)
                return new TrackResult(0, Point.Empty);
            
            Bitmap template = reference.Template;
            Rectangle templateBounds = new Rectangle(0, 0, template.Width, template.Height);
            if(searchZone.Width < template.Width || searchZone.Height < template.Height)
                return new TrackResult(0, Point.Empty);

            var cvImageROI = context.CVImage[searchZone.Y, searchZone.Y + searchZone.Height, searchZone.X, searchZone.X + searchZone.Width];
            var cvTemplate = OpenCvSharp.Extensions.BitmapConverter.ToMat(template);

            int similarityMapWidth = searchZone.Width - template.Width + 1;
            int similarityMapHeight = searchZone.Height - template.Height + 1;
            var similarityMap = new OpenCvSharp.Mat(new OpenCvSharp.Size(similarityMapWidth, similarityMapHeight), OpenCvSharp.MatType.CV_32FC1);
            OpenCvSharp.Cv2.MatchTemplate(cvImageROI, cvTemplate, similarityMap, OpenCvSharp.TemplateMatchModes.CCoeffNormed);

            cvImageROI.Dispose();
            cvTemplate.Dispose();

            double min;
            double max;
            OpenCvSharp.Point minLoc;
            OpenCvSharp.Point maxLoc;
            OpenCvSharp.Cv2.MinMaxLoc(similarityMap, out min, out max, out minLoc, out maxLoc);

            Point location = new Point(searchZone.Left + maxLoc.X + template.Width / 2, searchZone.Top + maxLoc.Y + template.Height / 2);
            
            return new TrackResult(max, location);
        }
    }
}
