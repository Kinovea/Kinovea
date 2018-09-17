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
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A simple button like control to be drawn by its parent.
    /// Honors transparency.
    /// </summary>
    public class EmbeddedButton
    {
        public EventHandler Click;
        
        public Point Location { get; set; }
        public Cursor CursorMouseOver { get; private set;}

        private Bitmap image;
            
        public EmbeddedButton(Bitmap image, int x, int y)
        {
            this.image = image;
            this.Location = new Point(x,y);
            this.CursorMouseOver = Cursors.Hand;
        }
        public EmbeddedButton(Bitmap image, int x, int y, Cursor cursorMouseOver)
        {
            this.image = image;
            this.Location = new Point(x,y);
            this.CursorMouseOver = cursorMouseOver;
        }
        public void Draw(Graphics canvas)
        {
            canvas.DrawImageUnscaled(image, Location.X, Location.Y);
        }
        public bool HitTest(Point mouse)
        {
            Rectangle r = new Rectangle(Location, image.Size);
            r = r.Center().Box(image.Size.Width/2 + 4);
            return r.Contains(mouse);
        }
        public bool ClickTest(Point mouse)
        {
            bool handled = false;
            if(Click != null && mouse.X >= Location.X && mouse.X <= Location.X + image.Width && mouse.Y >= Location.Y && mouse.Y <= Location.Y + image.Height)
            {
                handled = true;
                Click(this, EventArgs.Empty);
            }
            
            return handled;
        }
    }
}
