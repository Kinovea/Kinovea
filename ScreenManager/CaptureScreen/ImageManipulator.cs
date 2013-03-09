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
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Simple helper for image panning.
    /// </summary>
    public class ImageManipulator
    {
        public bool Started
        {
            get { return moving;}
        }
        
        public Point ImageLocation
        {
            get { return imageLocation;}
        }
        
        public Size ImageSize
        {
            get { return ImageSize;}
        }
    
        private Point mouseStart;
        private Point imageStart;
        private bool moving;
        private Point imageLocation;
        private Size imageSize;
        private int stickyMarginInside = 17;
        private int stickyMarginOutside = 17;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public ImageManipulator()
        {
        }
        
        public void Start(Point mousePoint, Point imagePoint)
        {
            mouseStart = mousePoint;
            imageStart = imagePoint;
            moving = true;
        }
        public void End()
        {
            moving = false;
        }
        public void Pan(Point mousePoint, bool sticky, Size imageSize, Size containerSize)
        {
            if(!moving)
                return;
                
            int dx = mousePoint.X - mouseStart.X;
            int dy = mousePoint.Y - mouseStart.Y;
            
            imageLocation = imageStart.Translate(dx, dy);
            
            if(sticky)
                imageLocation = StickToBorders(imageLocation, imageSize, containerSize);
        }
        
        private Point StickToBorders(Point imageStart, Size imageSize, Size containerSize)
        {
            Point result = imageStart;
            
            if(imageStart.X > -stickyMarginOutside && imageStart.X < stickyMarginInside)
                result.X = 0;
            
            if(imageStart.Y > -stickyMarginOutside && imageStart.Y < stickyMarginInside)
                result.Y = 0;
            
            if(imageStart.X + imageSize.Width > containerSize.Width - stickyMarginInside && 
               imageStart.X + imageSize.Width < containerSize.Width + stickyMarginOutside)
                result.X = containerSize.Width - imageSize.Width;

            if(imageStart.Y + imageSize.Height > containerSize.Height - stickyMarginInside && 
               imageStart.Y + imageSize.Height < containerSize.Height + stickyMarginOutside)
                result.Y = containerSize.Height - imageSize.Height;

            return result;
        }
        
        private Point StickToCenter(Point imageStart, Size imageSize, Size containerSize)
        {
            Point result = imageStart;
            
            if(imageStart.X + imageSize.Width/2 > containerSize.Width/2 - stickyMarginInside && 
               imageStart.X + imageSize.Width/2 < containerSize.Width/2 + stickyMarginInside)
                result.X = (containerSize.Width - imageSize.Width)/2;

            if(imageStart.Y + imageSize.Height/2 > containerSize.Height/2 - stickyMarginInside && 
               imageStart.Y + imageSize.Height/2 < containerSize.Height/2 + stickyMarginInside)
                result.Y = (containerSize.Height - imageSize.Height)/2;
                
           return result;
        }
    }
}
