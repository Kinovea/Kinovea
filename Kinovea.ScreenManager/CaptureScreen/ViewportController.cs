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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Note: this class goal is to be shared between Capture and Playback screens.
    /// The viewport is the piece of UI that contains the image and the drawings, and manages the main user interaction with them.
    /// (The drawings should be able to go outside the image).
    /// </summary>
    public class ViewportController : IDisposable, IDrawingHostView
    {
        #region Events
        public event EventHandler ConfigureAsked;
        public event EventHandler DisplayRectangleUpdated;
        public event EventHandler Activated;
        public event EventHandler LoadAnnotationsAsked;
        public event EventHandler SaveAnnotationsAsked;
        public event EventHandler SaveAnnotationsAsAsked;
        public event EventHandler SaveDefaultPlayerAnnotationsAsked;
        public event EventHandler SaveDefaultCaptureAnnotationsAsked;
        public event EventHandler UnloadAnnotationsAsked;
        public event EventHandler ReloadDefaultCaptureAnnotationsAsked;
        public event EventHandler ReloadLinkedAnnotationsAsked;
        public event EventHandler CloseAsked;
        //public event EventHandler KVAImported;
        //public event EventHandler ExportImageAsked;
        //public event EventHandler ExportVideoAsked;

        /// <summary>
        /// Event raised when we are moving an object from this viewport.
        /// </summary>
        public event EventHandler Moving;
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

        public Metadata Metadata
        {
            get { return metadata; }
            set { metadata = value; }
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

        public string LastExportedKVA
        {
            get { return lastExportedKVA; }
            set { lastExportedKVA = value; }
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
        private Metadata metadata;
        private MetadataRenderer metadataRenderer;
        private MetadataManipulator metadataManipulator;
        private bool allowContextMenu = true;
        private string lastExportedKVA;

        #region Context menu
        private ContextMenuStrip popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuConfigure = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuBackground = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuCopyPic = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuPastePic = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuPasteDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLoadAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSaveAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSaveAnnotationsAs = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSaveDefaultPlayerAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSaveDefaultCaptureAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuUnloadAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuReloadDefaultCaptureAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuReloadLinkedAnnotations = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuExportVideo = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuExportImage = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCloseScreen = new ToolStripMenuItem();

        private ContextMenuStrip popMenuDrawings = new ContextMenuStrip();
        private ToolStripMenuItem mnuConfigureDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuConfigureOpacity = new ToolStripMenuItem();
        private ToolStripSeparator mnuSepDrawing = new ToolStripSeparator();
        private ToolStripSeparator mnuSepDrawing2 = new ToolStripSeparator();
        private ToolStripSeparator mnuSepDrawing3 = new ToolStripSeparator();
        //private ToolStripMenuItem mnuCutDrawing = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuCopyDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteDrawing = new ToolStripMenuItem();
        #endregion
        #endregion

        #region Construction/Destruction
        public ViewportController(bool allowContextMenu = true, bool dblClickZoom = true, bool showRecordingIndicator = true)
        {
            view = new Viewport(this, dblClickZoom, showRecordingIndicator);
            this.allowContextMenu = allowContextMenu;

            BuildContextMenus();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~ViewportController()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                view.SetContextMenu(null);

                popMenu.Dispose();
                popMenuDrawings.Dispose();
            }
        }
        #endregion

        public void ReloadUICulture()
        {
            ReloadMenusCulture();
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

            bool handled = metadataManipulator.ContinueMove(e, modifiers, imageLocation, imageZoom);
            Moving?.Invoke(this, EventArgs.Empty);
            
            return handled;
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
            // This is the equivalent of SurfaceScreen_RightDown in the player.

            if (metadataManipulator == null || metadata == null)
                return;

            Poke();

            if (!allowContextMenu)
                return;

            metadata.DeselectAll();
            AbstractDrawing hitDrawing = null;

            IImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF mouseInImage = transformer.Untransform(mouse);
            int fixedKeyframe = 0;
            long fixedTimestamp = 0;
            
            if (metadata.IsOnDrawing(fixedKeyframe, mouseInImage, fixedTimestamp))
            {
                AbstractDrawing drawing = metadata.HitDrawing;
                PrepareDrawingContextMenu(drawing, popMenuDrawings);

                popMenuDrawings.Items.Add(mnuDeleteDrawing);
                view.SetContextMenu(popMenuDrawings);
            }
            else if ((hitDrawing = metadata.IsOnDetachedDrawing(mouseInImage, fixedTimestamp)) != null)
            {
                if (metadata.IsChronoLike(hitDrawing))
                {
                    // Unsupported.
                }
                else if (hitDrawing is DrawingTrack)
                {
                    // Unsupported.
                }
                else if (hitDrawing is DrawingCoordinateSystem || hitDrawing is DrawingTestGrid)
                {
                    PrepareDrawingContextMenu(hitDrawing, popMenuDrawings);
                    view.SetContextMenu(popMenuDrawings);
                }
                else if (hitDrawing is AbstractMultiDrawing)
                {
                    // Unsupported for now.
                }
            }
            else
            {
                PrepareBackgroundContextMenu(popMenu);

                //mnuBackground.Visible = true;
                //mnuBackground.Enabled = true;
                //mnuPasteDrawing.Visible = true;
                //mnuPasteDrawing.Enabled = DrawingClipboard.HasContent;
                //mnuPastePic.Visible = true;
                //mnuPastePic.Enabled = Clipboard.ContainsImage();

                view.SetContextMenu(popMenu);
            }
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
        public long CurrentTimestamp
        {
            //get { return 0; }
            get { return timestamp; }
        }

        public Bitmap CurrentImage
        {
            get { return bitmap; }
        }

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

        public void UpdateFramesMarkers()
        {
            // No implementation needed.
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Set up images and event handlers for all context menus.
        /// </summary>
        private void BuildContextMenus()
        {
            // Background context menu.
            mnuConfigure.Image = Properties.Capture.settings;
            mnuLoadAnnotations.Image = Properties.Resources.notes2_16;
            mnuSaveAnnotations.Image = Properties.Resources.save_16;
            mnuSaveAnnotationsAs.Image = Properties.Resources.save_as_16;
            mnuSaveDefaultPlayerAnnotations.Image = Properties.Resources.save_player_16;
            mnuSaveDefaultCaptureAnnotations.Image = Properties.Resources.save_capture_16;
            mnuUnloadAnnotations.Image = Properties.Resources.delete_notes;
            mnuReloadDefaultCaptureAnnotations.Image = Properties.Resources.notes2_16;
            mnuReloadLinkedAnnotations.Image = Properties.Resources.notes2_16;
            mnuCloseScreen.Image = Properties.Capture.camera_close;

            mnuConfigure.Click += (s, e) => ConfigureAsked?.Invoke(this, e);
            mnuLoadAnnotations.Click += (s, e) => LoadAnnotationsAsked?.Invoke(this, e);
            mnuSaveAnnotations.Click += (s, e) => SaveAnnotationsAsked?.Invoke(this, e);
            mnuSaveAnnotationsAs.Click += (s, e) => SaveAnnotationsAsAsked?.Invoke(this, e);
            mnuSaveDefaultPlayerAnnotations.Click += (s, e) => SaveDefaultPlayerAnnotationsAsked?.Invoke(this, e);
            mnuSaveDefaultCaptureAnnotations.Click += (s, e) => SaveDefaultCaptureAnnotationsAsked?.Invoke(this, e);
            mnuUnloadAnnotations.Click += (s, e) => UnloadAnnotationsAsked?.Invoke(this, e);
            mnuReloadDefaultCaptureAnnotations.Click += (s, e) => ReloadDefaultCaptureAnnotationsAsked?.Invoke(this, e);
            mnuReloadLinkedAnnotations.Click += (s, e) => ReloadLinkedAnnotationsAsked?.Invoke(this, e);
            mnuCloseScreen.Click += (s, e) => CloseAsked?.Invoke(this, e);
            
            // Drawings context menu.
            mnuConfigureDrawing.Click += mnuConfigureDrawing_Click;
            mnuConfigureDrawing.Image = Properties.Drawings.configure;
            //mnuConfigureOpacity.Click += mnuConfigureOpacity_Click;
            //mnuConfigureOpacity.Image = Properties.Drawings.persistence;
            mnuDeleteDrawing.Click += mnuDeleteDrawing_Click;
            mnuDeleteDrawing.Image = Properties.Drawings.delete;

            // The right context menu and its content will be choosen on MouseDown.
            view.SetContextMenu(popMenu);

            // Load the menu labels.
            ReloadMenusCulture();
        }

        /// <summary>
        /// Set up the text for all context menus.
        /// </summary>
        private void ReloadMenusCulture()
        {
            // Background context menu
            mnuConfigure.Text = ScreenManagerLang.ToolTip_ConfigureCamera;
            //mnuBackground.Text = ScreenManagerLang.PlayerScreenUserInterface_Background;
            //mnuPasteDrawing.Text = ScreenManagerLang.mnuPasteDrawing;
            //mnuPasteDrawing.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.PasteDrawing);
            mnuLoadAnnotations.Text = ScreenManagerLang.mnuLoadAnalysis;
            mnuSaveAnnotations.Text = ScreenManagerLang.Generic_SaveKVA;
            mnuSaveAnnotationsAs.Text = ScreenManagerLang.Generic_SaveKVAAs;
            mnuSaveDefaultPlayerAnnotations.Text = "Save as default player annotations";
            mnuSaveDefaultCaptureAnnotations.Text = "Save as default capture annotations";
            mnuUnloadAnnotations.Text = "Unload annotations";
            mnuReloadDefaultCaptureAnnotations.Text = "Reload default capture annotations";
            mnuReloadLinkedAnnotations.Text = "Reload linked annotations";
            //mnuExportVideo.Text = ScreenManagerLang.Generic_ExportVideo;
            //mnuExportImage.Text = ScreenManagerLang.Generic_SaveImage;
            //mnuCopyPic.Text = ScreenManagerLang.mnuCopyImageToClipboard;
            //mnuCopyPic.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.CopyImage);
            //mnuPastePic.Text = ScreenManagerLang.mnuPasteImage;
            mnuCloseScreen.Text = ScreenManagerLang.mnuCloseScreen;
            mnuCloseScreen.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.Close);

            // Drawings context menu
            mnuConfigureDrawing.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
            mnuConfigureOpacity.Text = ScreenManagerLang.Generic_Opacity;
            mnuDeleteDrawing.Text = ScreenManagerLang.mnuDeleteDrawing;
        }

        /// <summary>
        /// Signal to the screen manager that this is the active screen.
        /// This is to update the top level menus that depends on which screen is active.
        /// It should be called when we mous down inside the viewport.
        /// </summary>
        private void Poke()
        {
            if (Activated != null)
                Activated(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Set up the correct list of menus to show and customize the text.
        /// </summary>
        private void PrepareBackgroundContextMenu(ContextMenuStrip popMenu)
        {
            // Inject the target file name to avoid surprises.
            if (!string.IsNullOrEmpty(metadata.LastKVAPath))
            {
                string filename = Path.GetFileName(metadata.LastKVAPath);
                mnuSaveAnnotations.Text = string.Format("{0} ({1})", 
                    ScreenManagerLang.Generic_SaveKVA, filename);
            }
            else
            {
                mnuSaveAnnotations.Text = ScreenManagerLang.Generic_SaveKVA;
            }

            bool hasDefaultCaptureKVA = !string.IsNullOrEmpty(PreferencesManager.CapturePreferences.CaptureKVA);
            mnuReloadDefaultCaptureAnnotations.Enabled = hasDefaultCaptureKVA;

            if (!string.IsNullOrEmpty(lastExportedKVA))
            {
                string filename = Path.GetFileName(lastExportedKVA);
                mnuReloadLinkedAnnotations.Text = string.Format("{0} ({1})",
                    "Reload linked annotations", filename);
                mnuReloadLinkedAnnotations.Enabled = true;
            }
            else
            {
                mnuReloadLinkedAnnotations.Text = "Reload linked annotations";
                mnuReloadLinkedAnnotations.Enabled = false;
            }

            popMenu.Items.Clear();
            popMenu.Items.AddRange(new ToolStripItem[]
            {
                mnuConfigure,
                //mnuBackground,
                new ToolStripSeparator(),
                //mnuCopyPic,
                //mnuPastePic,
                //mnuPasteDrawing,
                //new ToolStripSeparator(),
                //new ToolStripSeparator(),
                mnuLoadAnnotations,
                mnuReloadDefaultCaptureAnnotations,
                mnuReloadLinkedAnnotations,
                new ToolStripSeparator(),
                mnuSaveAnnotations,
                mnuSaveAnnotationsAs,
                mnuSaveDefaultPlayerAnnotations,
                mnuSaveDefaultCaptureAnnotations,
                new ToolStripSeparator(),
                mnuUnloadAnnotations,
                new ToolStripSeparator(),
                //mnuExportVideo,
                //mnuExportImage,
                //new ToolStripSeparator(),
                mnuCloseScreen
            });
        }

        /// <summary>
        /// Set up the correct list of menus to show and customize the text.
        /// </summary>
        private void PrepareDrawingContextMenu(AbstractDrawing drawing, ContextMenuStrip popMenu)
        {
            popMenu.Items.Clear();

            // Generic menus based on the drawing capabilities: configuration (style), visibility, tracking.
            if (!metadata.DrawingInitializing)
                PrepareDrawingContextMenuCapabilities(drawing, popMenu);

            // Custom menu handlers implemented by the drawing itself.
            // These change the drawing core state. (ex: angle orientation, measurement display option, start/stop chrono, etc.).
            bool hasExtraMenus = AddDrawingCustomMenus(drawing, popMenu.Items);

            // "Goto parent keyframe" menu: not implemented.

            // Below the custom menus and the goto keyframe we have the generic copy-paste and the delete menu.
            // Some singleton drawings cannot be deleted nor copy-pasted, so they don't need this.
            if (drawing is DrawingCoordinateSystem || drawing is DrawingTestGrid)
                return;

            if (hasExtraMenus)
                popMenu.Items.Add(mnuSepDrawing2);

            if (drawing.IsCopyPasteable)
            {
                //popMenuDrawings.Items.Add(mnuCutDrawing);
                //popMenuDrawings.Items.Add(mnuCopyDrawing);
                //popMenuDrawings.Items.Add(mnuSepDrawing3);
            }
        }

        private void PrepareDrawingContextMenuCapabilities(AbstractDrawing drawing, ContextMenuStrip popMenu)
        {
            if ((drawing.Caps & DrawingCapabilities.ConfigureColor) == DrawingCapabilities.ConfigureColor ||
                   (drawing.Caps & DrawingCapabilities.ConfigureColorSize) == DrawingCapabilities.ConfigureColorSize)
            {
                mnuConfigureDrawing.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
                popMenu.Items.Add(mnuConfigureDrawing);
                popMenu.Items.Add(mnuSepDrawing);
            }

            // The following are in the player but not currently implemented in capture.
            // - Visibility menus for fading drawings.
            // - Visibility for transparent drawings (bitmaps).
            // - Tracking.
        }

        private bool AddDrawingCustomMenus(AbstractDrawing drawing, ToolStripItemCollection menuItems)
        {
            List<ToolStripItem> extraMenu = drawing.ContextMenu;

            bool hasExtraMenu = (extraMenu != null && extraMenu.Count > 0);
            if (!hasExtraMenu)
                return false;

            foreach (ToolStripItem tsmi in drawing.ContextMenu)
            {
                ToolStripMenuItem menuItem = tsmi as ToolStripMenuItem;

                // Inject a dependency on this screen into the drawing.
                // Since the drawing now owns a piece of the UI, it may need to call back into functions here.
                // This is used to invalidate the view and complete operations that are normally handled here and
                // require calls to other objects that the drawing itself doesn't have access to, like when the
                // polyline drawing handles InitializeEnd and needs to remove the last point added to tracking.
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
            DrawingText label = e.Drawing as DrawingText;
            if(label == null)
                return;
                
            label.ContainerScreen = view;
            view.Controls.Add(label.EditBox);
            label.EditBox.BringToFront();
            label.EditBox.Focus();
        }
        #endregion

        #region Menu handlers
        private void mnuConfigureDrawing_Click(object sender, EventArgs e)
        {
            IDecorable decorable = metadata.HitDrawing as IDecorable;
            if(decorable == null || decorable.StyleElements == null || decorable.StyleElements.Elements.Count == 0)
                return;

            AbstractDrawing drawing = metadata.HitDrawing;
            AbstractDrawingManager owner = metadata.HitDrawingOwner;
            HistoryMementoModifyDrawing memento = null;
            if (owner != null)
                memento = new HistoryMementoModifyDrawing(metadata, owner.Id, drawing.Id, drawing.Name, SerializationFilter.Style);

            FormConfigureDrawing2 fcd = new FormConfigureDrawing2(decorable, Refresh);
            FormsHelper.Locate(fcd);
            fcd.ShowDialog();

            if (fcd.DialogResult == DialogResult.OK)
            {
                if (memento != null)
                {
                    memento.UpdateCommandName(drawing.Name);
                    metadata.HistoryStack.PushNewCommand(memento);
                }

                // Update the style preset for the parent tool of this drawing
                // so the next time we use this tool it will have the style we just set.
                ToolManager.SetToolStyleFromDrawing(metadata.HitDrawing, decorable.StyleElements);
                ToolManager.SavePresets();
                InvalidateCursor();
            }

            fcd.Dispose();

            Refresh();
        }
        
        private void mnuDeleteDrawing_Click(object sender, EventArgs e)
        {
            Keyframe keyframe = metadata.HitKeyframe;
            AbstractDrawing drawing = metadata.HitDrawing;

            if (keyframe == null || drawing == null)
                return;

            HistoryMemento memento = new HistoryMementoDeleteDrawing(metadata, keyframe.Id, drawing.Id, drawing.Name);
            metadata.DeleteDrawing(keyframe.Id, drawing.Id);
            metadata.HistoryStack.PushNewCommand(memento);

            Refresh();
        }
        #endregion
    }
}
