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
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace Kinovea.ScreenManager
{
    public static class Tracker
    {  
        /// <summary>
        /// Tracks a reference template in the given image. Returns similarity score and position of best candidate.
        /// </summary>
        public static TrackResult Track(TrackerParameters trackerParameters, TrackFrame reference, Bitmap image)
        {
            if(image == null)
                throw new ArgumentException("image");
            
            Rectangle searchZone = reference.Location.Box(trackerParameters.SearchWindowSize);
            Rectangle imageBounds = new Rectangle(0, 0, image.Width, image.Height);
            searchZone.Intersect(imageBounds);
            
            if(searchZone == Rectangle.Empty)
                return new TrackResult(0, Point.Empty);
            
            Bitmap template = reference.Template;
            Rectangle templateBounds = new Rectangle(0, 0, template.Width, template.Height);
            
            if(searchZone.Width < template.Width || searchZone.Height < template.Height)
                return new TrackResult(0, Point.Empty);
            
            BitmapData imageData = image.LockBits(imageBounds, ImageLockMode.ReadOnly, image.PixelFormat );
            BitmapData templateData = template.LockBits(templateBounds, ImageLockMode.ReadOnly, template.PixelFormat );
			
            Image<Bgra, Byte> cvImage = new Image<Bgra, Byte>(imageData.Width, imageData.Height, imageData.Stride, imageData.Scan0);
			Image<Bgra, Byte> cvTemplate = new Image<Bgra, Byte>(templateData.Width, templateData.Height, templateData.Stride, templateData.Scan0);
			
            cvImage.ROI = searchZone;
            
            int similarityMapWidth = searchZone.Width - template.Width + 1;
			int similarityMapHeight = searchZone.Height - template.Height + 1;
			Image<Gray, Single> similarityMap = new Image<Gray, Single>(similarityMapWidth, similarityMapHeight);
			
			CvInvoke.cvMatchTemplate(cvImage.Ptr, cvTemplate.Ptr, similarityMap.Ptr, TM_TYPE.CV_TM_CCOEFF_NORMED);
				
			image.UnlockBits(imageData);
		    template.UnlockBits(templateData);
				
		    Point minLoc = new Point(0,0);
            Point maxLoc = new Point(0,0);
            double minSimilarity = 0;
            double maxSimilarity = 0;
		    
            CvInvoke.cvMinMaxLoc(similarityMap.Ptr, ref minSimilarity, ref maxSimilarity, ref minLoc, ref maxLoc, IntPtr.Zero);
				
		    Point location = new Point(searchZone.Left + maxLoc.X + template.Width / 2, searchZone.Top + maxLoc.Y + template.Height / 2);
		    
		    return new TrackResult(maxSimilarity, location);
		}
    }
}
