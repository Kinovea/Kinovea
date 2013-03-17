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
    /// A simple button like control to be drawn by its parent.
    /// Honors transparency.
    /// </summary>
    public class EmbeddedButton
    {
        public EventHandler Click;
        
        private Bitmap image;
        private Point location;
            
        public EmbeddedButton(Bitmap image, int x, int y)
        {
            this.image = image;
            location = new Point(x,y);
        }
        public void Draw(Graphics canvas)
        {
            canvas.DrawImageUnscaled(image, location.X, location.Y);
        }
        public bool ClickTest(Point mouse)
        {
            bool handled = false;
            if(Click != null && mouse.X >= location.X && mouse.X <= location.X + image.Width && mouse.Y >= location.Y && mouse.Y <= location.Y + image.Height)
            {
                handled = true;
                Click(this, EventArgs.Empty);
            }
            
            return handled;
        }
    }
}
