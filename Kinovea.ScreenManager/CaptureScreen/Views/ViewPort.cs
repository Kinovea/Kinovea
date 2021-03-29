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
        #region Members
        private ViewportController controller;
        private Size referenceSize;             // Original image size after optional rotation.
        private Rectangle displayRectangle;     // Position and size of the region of the viewport where we draw the image.
        private ImageManipulator manipulator = new ImageManipulator();

        private ZoomHelper zoomHelper = new ZoomHelper();
        private List<EmbeddedButton> resizers = new List<EmbeddedButton>();
        private static Bitmap resizerBitmap = Properties.Resources.resizer;
        private static int resizerOffset = resizerBitmap.Width / 2;
        private int resizerIndex = -1;
        private MessageToaster toaster;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public Viewport(ViewportController controller)
        {
            this.controller = controller;
            this.BackColor = Color.FromArgb(255, 44, 44, 44);
            this.DoubleBuffered = true;
            this.MouseWheel += Viewport_MouseWheel;
            
            resizers.Add(new EmbeddedButton(resizerBitmap, 0, 0, Cursors.SizeNWSE));
            resizers.Add(new EmbeddedButton(resizerBitmap, 0, 0, Cursors.SizeNESW));
            resizers.Add(new EmbeddedButton(resizerBitmap, 0, 0, Cursors.SizeNWSE));
            resizers.Add(new EmbeddedButton(resizerBitmap, 0, 0, Cursors.SizeNESW));
            
            toaster = new MessageToaster(this);
        }
        
        public void InitializeDisplayRectangle(Rectangle saved, Size referenceSize)
        {
            this.referenceSize = referenceSize;
            InitializeDisplayRectangle(saved);
            if (displayRectangle != saved)
                controller.UpdateDisplayRectangle(displayRectangle);
            
            ForceZoomValue();
        }
        
        public void IncreaseZoom()
        {
            zoomHelper.Increase();
            AfterZoomChanged(displayRectangle.Center());
        }

        public void DecreaseZoom()
        {
            zoomHelper.Decrease();
            AfterZoomChanged(displayRectangle.Center());
        }

        public void ResetZoom()
        {
            zoomHelper.Increase();
            AfterZoomChanged(displayRectangle.Center());
        }
        
        public void SetContextMenu(ContextMenuStrip menuStrip)
        {
            this.ContextMenuStrip = menuStrip;
        }

        public void ToastMessage(string message, int duration)
        {
            toaster.SetDuration(duration);
            toaster.Show(message);
        }
        
        #region Drawing
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            if(controller.Bitmap == null)
                return;
                
            ConfigureCanvas(e.Graphics);

            DrawImage(e.Graphics);
            DrawKVA(e.Graphics);
            DrawResizers(e.Graphics);
            toaster.Draw(e.Graphics);
        }
        private void ConfigureCanvas(Graphics canvas)
        {
            canvas.InterpolationMode = InterpolationMode.NearestNeighbor;
            canvas.PixelOffsetMode = PixelOffsetMode.Half;
            canvas.CompositingQuality = CompositingQuality.HighSpeed;
            canvas.SmoothingMode = SmoothingMode.None;
            
            //canvas.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            //canvas.InterpolationMode = InterpolationMode.Bilinear;
        }
        private void DrawImage(Graphics canvas)
        {
            try
            {
                if (displayRectangle.Size == controller.Bitmap.Size)
                    canvas.DrawImageUnscaled(controller.Bitmap, displayRectangle.Location);
                else
                    canvas.DrawImage(controller.Bitmap, displayRectangle);
            }
            catch (Exception e)
            {
                log.ErrorFormat(e.Message);
            }
        }
        private void DrawKVA(Graphics canvas)
        {
            if(referenceSize.Width != 0)
            {
                float zoom = (float)displayRectangle.Size.Width / referenceSize.Width;
                controller.DrawKVA(canvas, displayRectangle.Location, zoom);
            }
        }
        private void DrawResizers(Graphics canvas)
        {
            foreach(EmbeddedButton resizer in resizers)
                resizer.Draw(canvas);
        }
        #endregion
        
        #region Interaction
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
                OnMouseLeftDown(e);
            else if (e.Button == MouseButtons.Right)
                OnMouseRightDown(e);
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.Focus();
            
            if(controller.Bitmap == null)
                return;

            if(e.Button == MouseButtons.Left)
                OnMouseLeftMove(e);
            else if(e.Button == MouseButtons.None)
                OnMouseNoneMove(e);
        }
        
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            if(controller.Bitmap == null)
                return;

            controller.OnMouseUp(e.Location, ModifierKeys, displayRectangle.Location, zoomHelper.Value);
            manipulator.End();

            // TODO: do not call this if we are not resizing.
            ForceZoomValue();
            controller.UpdateDisplayRectangle(displayRectangle);
            Cursor = controller.GetCursor(zoomHelper.Value);
        }
        
        protected override void OnDoubleClick(EventArgs e)
        {
            if(controller.Bitmap == null)
                return;
                
            base.OnDoubleClick(e);
            Point mouse = this.PointToClient(Control.MousePosition);
            if(displayRectangle.Contains(mouse))
            {
                manipulator.Expand(referenceSize, displayRectangle, this.Size);
                displayRectangle = manipulator.DisplayRectangle;
                AfterDisplayRectangleChanged();
                ForceZoomValue();
                controller.UpdateDisplayRectangle(displayRectangle);
            }
        }
        
        private void Viewport_MouseWheel(object sender, MouseEventArgs e)
        {
            if(controller.Bitmap == null)
                return;
                
            if ((ModifierKeys & Keys.Control) == Keys.Control)
                Zoom(e);
        }
        
        private int HitTestResizers(Point mouse)
        {
            int hit = -1;
            for(int i = 0; i < resizers.Count; i++)
            {
                if(resizers[i].HitTest(mouse))
                {
                    hit = i+1;
                    break;
                }
            }
            return hit;
        }
        
        private int HitTestImage(Point mouse)
        {
            if(displayRectangle.Contains(mouse))
                return 0;
            
            return -1;
        }
        
        private void Zoom(MouseEventArgs e)
        {
            int steps = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            if(steps > 0)
                zoomHelper.Increase();
            else
                zoomHelper.Decrease();

            AfterZoomChanged(e.Location);
        }

        /// <summary>
        /// Explicit zoom change (mouse scroll or reset).
        /// </summary>
        private void AfterZoomChanged(Point location)
        {
            RecomputeDisplayRectangle(referenceSize, displayRectangle, location);
            ToastZoom();
            controller.InvalidateCursor();
        }
        
        private void ToastZoom()
        {
            string message = "";
            if (zoomHelper.Value <= 1.0f)
                message = string.Format("{0:0}%", Math.Round(zoomHelper.Value * 100));
            else if (zoomHelper.Value < 10.0f)
                message = string.Format("{0:0.0}x", Math.Round(zoomHelper.Value, 1));
            else
                message = string.Format("{0:0}x", Math.Round(zoomHelper.Value));

            message = string.Format("Zoom:{0}", message);

            ToastMessage(message, 750);
        }
        
        /// <summary>
        /// Implicit zoom change (resizers).
        /// </summary>
        private void ForceZoomValue()
        {
            // The display rectangle has changed size outside the context of zoom (e.g: dragging corners).
            // Recompute the current zoom value to keep it in sync.
            float oldZoom = zoomHelper.Value;
            float newZoom = (float)displayRectangle.Size.Width / referenceSize.Width;
            
            zoomHelper.Value = newZoom;
            
            float maxDifference = 0.1f;
            if (Math.Abs(newZoom - oldZoom) > maxDifference)
                ToastZoom();

            controller.InvalidateCursor();
        }
        
        private void InitializeDisplayRectangle(Rectangle saved)
        {
            if (saved.Size == Size.Empty)
            {
                if (referenceSize.FitsIn(this.Size))
                {
                    int left = (this.Size.Width - referenceSize.Width) / 2;
                    int top = (this.Size.Height - referenceSize.Height) / 2;
                    displayRectangle = new Rectangle(left, top, referenceSize.Width, referenceSize.Height);
                }
                else
                {
                    displayRectangle = UIHelper.RatioStretch(referenceSize, this.Size);
                }
            }
            else
            {
                displayRectangle = saved;
            }

            AfterDisplayRectangleChanged();
        }
        
        private void RecomputeDisplayRectangle(Size imageSize, Rectangle displayRectangle, Point mouse)
        {
            // Resize display rectangle while keeping it centerer on the mouse.
            PointF normalizedMouse = new PointF((mouse.X - displayRectangle.X) / (float)displayRectangle.Width, (mouse.Y - displayRectangle.Y) / (float)displayRectangle.Height);
            Size size = imageSize.Scale(zoomHelper.Value);
            int left = (int)Math.Round((mouse.X - (size.Width * normalizedMouse.X)));
            int top = (int)Math.Round((mouse.Y - (size.Height * normalizedMouse.Y)));
            Point location = new Point(left, top);
            this.displayRectangle = new Rectangle(location, size);
            
            AfterDisplayRectangleChanged();
        }
        
        private void AfterDisplayRectangleChanged()
        {
            // Force aspect ratio to match original size.
            double aspectRatio = (double)referenceSize.Width / referenceSize.Height;
            displayRectangle.Width = (int)Math.Round((double)displayRectangle.Height * aspectRatio);

            resizers[0].Location = new Point(displayRectangle.Left - resizerOffset, displayRectangle.Top - resizerOffset);
            resizers[1].Location = new Point(displayRectangle.Right - resizerOffset, displayRectangle.Top - resizerOffset);
            resizers[2].Location = new Point(displayRectangle.Right - resizerOffset, displayRectangle.Bottom - resizerOffset);
            resizers[3].Location = new Point(displayRectangle.Left - resizerOffset, displayRectangle.Bottom - resizerOffset);
        }
        
        /// <summary>
        /// Update the cursor to the current tool or resizer.
        /// </summary>
        private void UpdateCursor(Point mouse)
        {
            // This is called for every mouse move so it must be as quick as possible and not create unecessary resources.
            int onResizerIndex = -1;
            for(int i = 0; i < resizers.Count; i++)
            {
                if (!resizers[i].HitTest(mouse))
                    continue;
                
                onResizerIndex = i;
                break;
            }

            if (onResizerIndex >= 0)
            {
                if (onResizerIndex != resizerIndex)
                {
                    resizerIndex = onResizerIndex;
                    Cursor = resizers[resizerIndex].CursorMouseOver;
                }
                
                return;
            }

            // Not on a resizer (hand tool or normal tool).
            resizerIndex = -1;
            Cursor = controller.GetCursor(zoomHelper.Value);
        }
        #endregion
        
        private void OnMouseLeftDown(MouseEventArgs e)
        {
            if(controller.Bitmap == null)
                return;
                
            if(manipulator.Started)
                return;
            
            int hit = HitTestResizers(e.Location);
            if(hit > 0)
            {
                // Cursor should be resizer cursor.
                manipulator.Start(e.Location, hit, displayRectangle);
                return;
            }
            
            bool handled = controller.OnMouseLeftDown(e.Location, displayRectangle.Location, zoomHelper.Value);
            if(handled)
            {
                Cursor = controller.GetCursor(zoomHelper.Value);
                return;
            }
            
            hit = HitTestImage(e.Location);
            if(hit == 0)
            {
                manipulator.Start(e.Location, hit, displayRectangle);
                Cursor = manipulator.GetCursorClosedHand();
            }
        }
        
        private void OnMouseRightDown(MouseEventArgs e)
        {
            if(controller.Bitmap == null)
                return;
                
            controller.OnMouseRightDown(e.Location, displayRectangle.Location, zoomHelper.Value);
        }
        
        private void OnMouseLeftMove(MouseEventArgs e)
        {
            if(manipulator.Started)
            {
                bool sticky = true;
                manipulator.Move(e.Location, sticky, this.Size, referenceSize);
                displayRectangle = manipulator.DisplayRectangle;
                AfterDisplayRectangleChanged();
                return;
            }
            
            controller.OnMouseLeftMove(e.Location, ModifierKeys, displayRectangle.Location, zoomHelper.Value);
        }
        
        private void OnMouseNoneMove(MouseEventArgs e)
        {
            UpdateCursor(e.Location);
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }
        
    }
}
