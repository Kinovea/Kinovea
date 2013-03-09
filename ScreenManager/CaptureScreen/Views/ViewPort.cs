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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Viewport is the core viewer for the image and drawing tools.
    /// This class should be only concerned with display, the actual intelligence being in ViewportController.
    /// Interactions only concerned with moving the image around, zoom window, etc. are handled here.
    /// </summary>
    public partial class Viewport : Control
    {
        private ViewportController controller;
        private Size imageSize;
        private Rectangle displayRectangle; // Position and size of the region of the viewport where we draw the image.
        private Rectangle unstretchedDisplayRectangle;
        private ImageManipulator manipulator = new ImageManipulator(); // This will ultimately be the responsibility of the hand tool.
        private bool filling;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public Viewport(ViewportController controller)
        {
            this.controller = controller;
            this.BackColor = Color.FromArgb(255, 44, 44, 44);
            this.DoubleBuffered = true;
        }
        
        public void InitializeDisplayRectangle(Rectangle saved, Size imageSize)
        {
            this.imageSize = imageSize;
            filling = false;
            InitializeDisplayRectangle(saved);
        }
        
        #region Drawing
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            
            DrawImage(e.Graphics);
        }
        
        private void DrawImage(Graphics canvas)
        {
            if(controller.Bitmap == null)
                return;

            try
            {
                if(displayRectangle.Size == controller.Bitmap.Size)
                {
                    canvas.DrawImageUnscaled(controller.Bitmap, displayRectangle.Location);
                }
                else
                {
                    canvas.DrawImage(controller.Bitmap, displayRectangle);
                }
            }
            catch(Exception e)
            {
                log.ErrorFormat(e.Message);
            }
        }
        #endregion
        
        #region Interaction
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            if(e.Button != MouseButtons.Left)
                return;
            
            if(displayRectangle.Contains(e.Location))
                manipulator.Start(e.Location, displayRectangle.Location);
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if(e.Button != MouseButtons.Left)
                return;
            
            //if(displayRectangle.Contains(e.Location))
            if(manipulator.Started)
            {
                filling = false;
                bool sticky = true; // depends on SHIFT.
                manipulator.Pan(e.Location, sticky, displayRectangle.Size, this.Size);
                displayRectangle.Location = manipulator.ImageLocation;
            }
        }
        
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            manipulator.End();
            controller.UpdateDisplayRectangle(displayRectangle);
        }
        
        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            
            if(displayRectangle.Contains(this.PointToClient(Control.MousePosition)))
            {
                filling = !filling;
                if(filling)
                    Fill();
                else
                    InitializeDisplayRectangle(unstretchedDisplayRectangle);
            }
        }
        
        private void InitializeDisplayRectangle(Rectangle saved)
        {
            if(saved.Size != Size.Empty)
            {
                displayRectangle = saved;
            }
            else
            {
                int left = (this.Size.Width - imageSize.Width)/2;
                int top = (this.Size.Height - imageSize.Height)/2;
                displayRectangle = new Rectangle(left, top, imageSize.Width, imageSize.Height);
            }
        }
        
        private void Fill()
        {
            unstretchedDisplayRectangle = displayRectangle;
            displayRectangle = UIHelper.RatioStretch(displayRectangle.Size, this.Size);
            controller.UpdateDisplayRectangle(displayRectangle);
        }
        #endregion
        
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            if(filling)
                Fill();
        }
        
    }
}
