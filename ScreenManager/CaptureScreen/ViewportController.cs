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
using System.Windows.Forms;

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
        
        public MetadataRenderer MetadataRenderer
        {
            get { return metadataRenderer; }
            set { metadataRenderer = value; }
        }

        public MetadataManipulator MetadataManipulator
        {
            get { return metadataManipulator; }
            set { metadataManipulator = value; }
        }
        
        public bool IsUsingHandTool
        {
            get { return metadataManipulator == null ? true : metadataManipulator.IsUsingHandTool; }
        }
        
        private Viewport view;
        private Bitmap bitmap;
        private Rectangle displayRectangle;
        private MetadataRenderer metadataRenderer;
        private MetadataManipulator metadataManipulator;
        
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
            if(metadataRenderer == null)
                return;
            
            metadataRenderer.Render(canvas, location, zoom);
        }
        
        public bool OnMouseLeftDown(Point mouse, Point imageLocation, float imageZoom)
        {
            if(metadataManipulator == null)
                return false;
                
            return metadataManipulator.OnMouseLeftDown(mouse, imageLocation, imageZoom);
        }
        
        public bool OnMouseLeftMove(Point mouse, Keys modifiers, Point imageLocation, float imageZoom)
        {
            if(metadataManipulator == null)
                return false;
                
            return metadataManipulator.OnMouseLeftMove(mouse, modifiers, imageLocation, imageZoom);
        }
        
        public void OnMouseUp()
        {
            if(metadataManipulator == null)
                return;
            
            metadataManipulator.OnMouseUp();
        }
        
        public Cursor GetCursor(float imageZoom)
        {
            if(metadataManipulator == null)
                return Cursors.Default;
            
            return metadataManipulator.GetCursor(imageZoom);
        }
    }
}
