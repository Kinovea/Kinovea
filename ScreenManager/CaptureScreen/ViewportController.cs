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
    /// Note: this class goal is to be shared between Capture and Playback screens.
    /// The viewport is the piece of UI that contains the image and the drawings, and manages the main user interaction with them.
    /// (The drawings should be able to go outside the image).
    /// </summary>
    public class ViewportController
    {
        public event EventHandler DisplayRectangleUpdated;
        
        public Viewport View
        {
            get { return view;}
        }
        
        public Bitmap Bitmap
        {
            get { return bitmap;}
            set { bitmap = value;}
        }
        
        public Rectangle DisplayRectangle
        {
            get { return displayRectangle;}
        }
        
        public Metadata Metadata
        {
            get { return metadata;}
            set { metadata = value;}
        }
        
        private Viewport view;
        private Bitmap bitmap;
        private Rectangle displayRectangle;
        private Metadata metadata;
        
        public ViewportController()
        {
            view = new Viewport(this);
        }
        
        public void Refresh()
        {
            view.Invalidate();
        }
        
        public void InitializeDisplayRectangle(Rectangle displayRectangle, Size size)
        {
            view.InitializeDisplayRectangle(displayRectangle, size);
        }
        
        public void UpdateDisplayRectangle(Rectangle rectangle)
        {
            displayRectangle = rectangle;
            if(DisplayRectangleUpdated != null)
                DisplayRectangleUpdated(this, EventArgs.Empty);
        }
        
        public void DrawKVA(Graphics canvas, Point location, float zoom)
        {
            KVARenderer.Render(metadata, 0, canvas, location, zoom);
        }
    }
}
