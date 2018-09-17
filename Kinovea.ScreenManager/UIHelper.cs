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
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public static class UIHelper
    {
        /// <summary>
        /// Returns the active rectangle that the image should be drawn onto in order to completely fill the container.
        /// </summary>
        public static Rectangle RatioStretch(Size imageSize, Size containerSize)
        {
            float ratioWidth = (float)imageSize.Width / containerSize.Width;
            float ratioHeight = (float)imageSize.Height / containerSize.Height;
            float ratio = Math.Max(ratioWidth, ratioHeight);
            Size size = new Size((int)(imageSize.Width / ratio), (int)(imageSize.Height / ratio));
            
            int left = (containerSize.Width - size.Width)/2;
            int top = (containerSize.Height - size.Height)/2;
            Point location = new Point(left, top);
            
            return new Rectangle(location, size);
        }
    }
}
