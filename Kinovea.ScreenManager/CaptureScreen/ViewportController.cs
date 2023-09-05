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
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Note: this class goal is to be shared between Capture and Playback screens.
    /// The viewport is the piece of UI that contains the image and the drawings, and manages the main user interaction with them.
    /// (The drawings should be able to go outside the image).
    /// </summary>
    public class ViewportController : IDrawingHostView
    {
        #region Events
        public event EventHandler DisplayRectangleUpdated;
        public event EventHandler Poked;
        #endregion

        #region Properties
        public Viewport View
        {
            get { return view; }
        }

        public Bitmap Bitmap
        {
            get { return bitmap; }
            set { bitmap = value; }
        }

        public long Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }

        public Rectangle DisplayRectangle
        {
            get { return displayRectangle; }
        }

        public MetadataRenderer MetadataRenderer
        {
            get { return metadataRenderer; }
            set { metadataRenderer = value; }
        }

        public MetadataManipulator MetadataManipulator
        {
            get { return metadataManipulator; }
            set
            {
                if (metadataManipulator != null)
                    metadataManipulator.LabelAdded -= LabelAdded;

                metadataManipulator = value;
                metadataManipulator.LabelAdded += LabelAdded;
            }
        }

        public bool IsUsingHandTool
        {
            get { return metadataManipulator == null ? true : metadataManipulator.IsUsingHandTool; }
        }
        #endregion

        #region Members
        private Viewport view;
        private Bitmap bitmap;
        private long timestamp;
        private Rectangle displayRectangle;
        private MetadataRenderer metadataRenderer;
        private MetadataManipulator metadataManipulator;

        private ContextMenuStrip popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuConfigureDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuConfigureOpacity = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteDrawing = new ToolStripMenuItem();
        #endregion

        public ViewportController()
        {
            view = new Viewport(this);
            InitializeContextMenu();
        }

        public void ReloadUICulture()
        {
            ReloadMenuCulture();
        }

        public void Refresh()
        {
            view.Invalidate();
        }

        /// <summary>
        /// Make sure the viewport will not try to draw the bitmap.
        /// Use this when the bitmap is about to be disposed from elsewhere.
        /// </summary>
        public void ForgetBitmap()
        {
            if (bitmap == null)
                return;

            bitmap.Dispose();
            bitmap = null;
        }

        public void InitializeDisplayRectangle(Rectangle displayRectangle, Size referenceSize)
        {
            view.InitializeDisplayRectangle(displayRectangle, referenceSize);
        }

        public void UpdateDisplayRectangle(Rectangle rectangle)
        {
            displayRectangle = rectangle;
            if (DisplayRectangleUpdated != null)
                DisplayRectangleUpdated(this, EventArgs.Empty);
        }

        public void ToastMessage(string message, int duration)
        {
            view.ToastMessage(message, duration);
        }

        public void StartingRecording()
        {
            ToastMessage(ScreenManagerLang.Toast_StartRecord, 1000);
        }

        public void UpdateRecordingIndicator(RecordingStatus status, float progress)
        {
            view.UpdateRecordingIndicator(status, progress);
        }
        public void StoppingRecording()
        {
            ToastMessage(ScreenManagerLang.Toast_StopRecord, 750);
        }

        public void DrawKVA(Graphics canvas, Point location, float zoom)
        {
            if (metadataRenderer == null)
                return;

            metadataRenderer.Render(canvas, location, zoom, timestamp);
        }

        public bool OnMouseDown(MouseEventArgs e, Point imageLocation, float imageZoom)
        {
            if (metadataManipulator == null)
                return false;

            Poke();
            return metadataManipulator.StartMove(e, imageLocation, imageZoom);
        }

        public bool OnMouseMove(MouseEventArgs e, Keys modifiers, Point imageLocation, float imageZoom)
        {
            if (metadataManipulator == null)
                return false;

            return metadataManipulator.ContinueMove(e, modifiers, imageLocation, imageZoom);
        }

        public void OnMouseUp(MouseEventArgs e, Keys modifiers, Point imageLocation, float imageZoom)
        {
            if (metadataManipulator == null)
                return;

            metadataManipulator.StopMove(e, bitmap, modifiers, imageLocation, imageZoom);
            Refresh();
        }

        public void OnMouseRightDown(Point mouse, Point imageLocation, float imageZoom)
        {
            if (metadataManipulator == null)
                return;

            Poke();
            metadataManipulator.HitTest(mouse, imageLocation, imageZoom);
            PrepareContextMenu();
            view.SetContextMenu(popMenu);
        }

        public Cursor GetCursor(float imageZoom)
        {
            if (metadataManipulator == null)
                return Cursors.Default;

            return metadataManipulator.GetCursor(imageZoom);
        }

        public void InvalidateCursor()
        {
            if (metadataManipulator == null)
                return;

            metadataManipulator.InvalidateCursor();
        }

        #region IDrawingHostView
        public void DoInvalidate()
        {
            Refresh();
        }
        public void InvalidateFromMenu()
        {
            DoInvalidate();
        }
        public void InitializeEndFromMenu(bool cancelLastPoint)
        {
            if (metadataManipulator == null)
                return;

            metadataManipulator.InitializeEndFromMenu(cancelLastPoint);
        }

        public long CurrentTimestamp
        {
            get { return 0; }
        }

        public void UpdateFramesMarkers()
        {
            // No implementation needed.
        }
        #endregion

        #region Private methods
        private void InitializeContextMenu()
        {
            mnuConfigureDrawing.Click += mnuConfigureDrawing_Click;
            mnuConfigureDrawing.Image = Properties.Drawings.configure;
            mnuConfigureOpacity.Click += mnuConfigureOpacity_Click;
            mnuConfigureOpacity.Image = Properties.Drawings.persistence;
            mnuDeleteDrawing.Click += mnuDeleteDrawing_Click;
            mnuDeleteDrawing.Image = Properties.Drawings.delete;
            ReloadMenuCulture();
        }

        private void ReloadMenuCulture()
        {
            mnuConfigureDrawing.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
            mnuConfigureOpacity.Text = ScreenManagerLang.Generic_Opacity;
            mnuDeleteDrawing.Text = ScreenManagerLang.mnuDeleteDrawing;
        }

        private void Poke()
        {
            if (Poked != null)
                Poked(this, EventArgs.Empty);
        }
        private void PrepareContextMenu()
        {
            popMenu.Items.Clear();

            AbstractDrawing drawing = metadataManipulator.HitDrawing;
            if (drawing == null)
            {
                // TODO: general context menu at screen level. (close, snapshot, settings.)
                return;
            }
            else
            {
                PrepareContextMenuDrawing(drawing);
            }
        }

        private void PrepareContextMenuDrawing(AbstractDrawing drawing)
        {
            if((drawing.Caps & DrawingCapabilities.ConfigureColor) == DrawingCapabilities.ConfigureColor ||
               (drawing.Caps & DrawingCapabilities.ConfigureColorSize) == DrawingCapabilities.ConfigureColorSize)
            {
                mnuConfigureDrawing.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
                popMenu.Items.Add(mnuConfigureDrawing);
            }
            
            if((drawing.Caps & DrawingCapabilities.Opacity) == DrawingCapabilities.Opacity)
                popMenu.Items.Add(mnuConfigureOpacity);
            
            popMenu.Items.Add(new ToolStripSeparator());

            bool hasExtraMenus = AddDrawingCustomMenus(drawing, popMenu.Items);

            if (metadataManipulator.HitKeyframe != null)
            {
                if (hasExtraMenus)
                    popMenu.Items.Add(new ToolStripSeparator());

                popMenu.Items.Add(mnuDeleteDrawing);
            }
        }
        private bool AddDrawingCustomMenus(AbstractDrawing drawing, ToolStripItemCollection menuItems)
        {
            bool hasExtraMenu = (drawing.ContextMenu != null && drawing.ContextMenu.Count > 0);
            if (!hasExtraMenu)
                return false;

            foreach (ToolStripItem tsmi in drawing.ContextMenu)
            {
                ToolStripMenuItem menuItem = tsmi as ToolStripMenuItem;
                
                // Inject a dependency on this screen into the drawing.
                tsmi.Tag = this;

                // Also inject for all the sub menus.
                if (menuItem != null && menuItem.DropDownItems.Count > 0)
                {
                    foreach (ToolStripItem subMenu in menuItem.DropDownItems)
                        subMenu.Tag = this;
                }

                if (tsmi.MergeIndex >= 0)
                    menuItems.Insert(tsmi.MergeIndex, tsmi);
                else
                    menuItems.Add(tsmi);
            }

            return true;
        }
        private void LabelAdded(object sender, DrawingEventArgs e)
        {
            //AbstractDrawing drawing, int keyframeIndex)
            
            
            DrawingText label = e.Drawing as DrawingText;
            if(label == null)
                return;
                
            label.ContainerScreen = view;
            view.Controls.Add(label.EditBox);
            label.EditBox.BringToFront();
            label.EditBox.Focus();
        }
        
        #region Menu handlers
        private void mnuConfigureDrawing_Click(object sender, EventArgs e)
        {
            IDecorable drawing = metadataManipulator.HitDrawing as IDecorable;
            if(drawing == null || drawing.DrawingStyle == null || drawing.DrawingStyle.Elements.Count == 0)
                return;

            metadataManipulator.ConfigureDrawing(metadataManipulator.HitDrawing, Refresh);
            
            Refresh();
        }
        private void mnuConfigureOpacity_Click(object sender, EventArgs e)
        {
            /*AbstractDrawing drawing = metadataManipulator.HitDrawing;
            
            formConfigureOpacity fco = new formConfigureOpacity(drawing, pbSurfaceScreen);
            FormsHelper.Locate(fco);
            fco.ShowDialog();
            fco.Dispose();
            Refresh();*/
        }
        private void mnuDeleteDrawing_Click(object sender, EventArgs e)
        {
            metadataManipulator.DeleteHitDrawing();
            Refresh();
        }
        #endregion
        
        
        #endregion
    }
}
