#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Kinovea.Camera;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Host the current thumbnail viewer and exposes some common controls to change size,
    /// type of content, etc.
    /// </summary>
    public partial class ThumbnailViewerContainer : UserControl
    {
        public event EventHandler<LoadAskedEventArgs> LoadAsked;
        
        #region Members
        private Selector selector;
        private ThumbnailViewerContent currentContent = ThumbnailViewerContent.Files;
        private List<string> files;
        private ThumbnailViewerFiles viewerFiles = new ThumbnailViewerFiles();
        private ThumbnailViewerFiles viewerShortcuts = new ThumbnailViewerFiles();
        private ThumbnailViewerCameras viewerCameras = new ThumbnailViewerCameras();
        private UserControl viewer;
        #endregion

        public ThumbnailViewerContainer()
        {
            InitializeComponent();
            InitializeSelector();
            InitializeViewers();
            progressBar.Left = selector.Right + 10;
            progressBar.Visible = false;
            
            // Registers our exposed functions to the DelegatePool.
            DelegatesPool dp = DelegatesPool.Instance();
            dp.CurrentDirectoryChanged = CurrentDirectoryChanged;
            //dp.CameraListChanged
            
            viewer = viewerFiles;
            this.splitMain.Panel2.Controls.Add(viewer);
            viewer.BringToFront();
            
            CameraTypeManager.CamerasDiscovered += CameraTypeManager_CamerasDiscovered;
            CameraTypeManager.CameraImageReceived += CameraTypeManager_CameraImageReceived;
            
            // TODO: not necessarily the final place for this call.
            CameraTypeManager.DiscoverCameras();
        }

        private void CameraTypeManager_CameraImageReceived(object sender, CameraImageReceivedEventArgs e)
        {
            //e.Image.Save(string.Format("{0}.bmp", e.Summary.Alias));
            
            // TODO: this still runs in a worker thread.
            
            
        }
        
        #region Public methods
        public void RefreshUICulture()
        {
            // tool tips of content type buttons.
            // Forward to viewer.
        }
        public void CurrentDirectoryChanged(bool shortcuts, List<string> files, bool refresh)
        {
            this.files = files;
            
            if(!refresh)
                return;
            
            ThumbnailViewerContent newContent = shortcuts ? ThumbnailViewerContent.Shortcuts : ThumbnailViewerContent.Files;
            SwitchContent(newContent);
            
            if(shortcuts)
                viewerShortcuts.CurrentDirectoryChanged(files);
            else
                viewerFiles.CurrentDirectoryChanged(files);
        }
        
        /*public void DisplayThumbnailsFiles(bool shortcuts, List<string> files, bool refresh)
        {
            // Keep track of the files since we'll have to bring them back.
            this.files = files;
            
            if(!refresh)
                return;
            
            /*if (files.Count > 0)
            {
            	this.Height = Height - 20; // margin for cosmetic
                
            	// We keep the Kinovea logo until there is at least 1 thumbnail to show.
            	// After that we never display it again.
                pbLogo.Visible = false;
            }
            else
            {
                thumbnailViewerContainer.Height = 1;
            }* /

            if(this.Visible)
            {
                this.Cursor = Cursors.WaitCursor;
                DisplayThumbnails(_fileNames);
                this.Cursor = Cursors.Default;
            }

            if (thumbnailViewerContainer.Visible)
            {
                
            }
            else
            {
                // Thumbnail pane was hidden to show player screen
                // Then we changed folder and we don't have anything to show.
                // Let's clean older thumbnails now.
                thumbnailViewerContainer.CleanupThumbnails();
            }
        }*/
        public void HideContent()
        {
            this.Visible = false;
            
            if (currentContent == ThumbnailViewerContent.Files)
                viewerFiles.CancelLoading();
            else if (currentContent == ThumbnailViewerContent.Shortcuts)
                viewerShortcuts.CancelLoading();
            //else
                //viewerCameras.CancelLoading();
        }
        public void Unhide()
        {
            this.Visible = true;
            
            this.Cursor = Cursors.WaitCursor;
            
            if (currentContent == ThumbnailViewerContent.Files)
                viewerFiles.CurrentDirectoryChanged(files);
            else if (currentContent == ThumbnailViewerContent.Shortcuts)
                viewerShortcuts.CurrentDirectoryChanged(files);
            //else
                //viewerCameras.Update();
                
            this.Cursor = Cursors.Default;
        }
        public void CleanupThumbnails()
        {
            /*selectedThumbnail = null;
            foreach(ThumbListViewItem tlvi in thumbnails)
            tlvi.DisposeImages();
            thumbnails.Clear();
            splitResizeBar.Panel2.Controls.Clear();*/
        }
        public bool OnKeyPress(Keys _keycode)
        {
            return false;
            // Method called from the Screen Manager's PreFilterMessage.
            /*bool bWasHandled = false;
            if(splitResizeBar.Panel2.Controls.Count > 0 && !editMode)
            {
            // Note that ESC key to cancel editing is handled directly in
            // each thumbnail item.
            switch (_keycode)
            {
            case Keys.Left:
            {
            if (selectedThumbnail == null )
            {
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
            }
            else
            {
            int index = (int)selectedThumbnail.Tag;
            int iRow = index / columns;
            int iCol = index - (iRow * columns);
            
            if (iCol > 0)
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[index - 1]).SetSelected();
            }
            break;
            }
            case Keys.Right:
            {
            if (selectedThumbnail == null)
            {
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
            }
            else
            {
            int index = (int)selectedThumbnail.Tag;
            int iRow = index / columns;
            int iCol = index - (iRow * columns);
            
            if (iCol < columns - 1 && index + 1 < splitResizeBar.Panel2.Controls.Count)
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[index + 1]).SetSelected();
            }
            break;
            }
            case Keys.Up:
            {
            if (selectedThumbnail == null)
            {
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
            }
            else
            {
            int index = (int)selectedThumbnail.Tag;
            int iRow = index / columns;
            int iCol = index - (iRow * columns);
            
            if (iRow > 0)
            {
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[index - columns]).SetSelected();
            }
            }
            splitResizeBar.Panel2.ScrollControlIntoView(selectedThumbnail);
            break;
            }
            case Keys.Down:
            {
            if (selectedThumbnail == null)
            {
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
            }
            else
            {
            int index = (int)selectedThumbnail.Tag;
            int iRow = index / columns;
            int iCol = index - (iRow * columns);
            
            if ((iRow < splitResizeBar.Panel2.Controls.Count / columns) && index + columns  < splitResizeBar.Panel2.Controls.Count)
            {
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[index + columns]).SetSelected();
            }
            }
            splitResizeBar.Panel2.ScrollControlIntoView(selectedThumbnail);
            break;
            }
            case Keys.Return:
            {
            if (selectedThumbnail != null && !selectedThumbnail.IsError && LoadAsked != null)
            LoadAsked(this, new LoadAskedEventArgs(selectedThumbnail.FileName, -1));
            break;
            }
            case Keys.Add:
            {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            UpSizeThumbs();
            break;
            }
            case Keys.Subtract:
            {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            DownSizeThumbs();
            break;
            }
            case Keys.F2:
            {
            if(selectedThumbnail != null && !selectedThumbnail.IsError)
            selectedThumbnail.StartRenaming();
            break;
            }
            default:
            break;
            }
            }
            return bWasHandled;*/
        }
        #endregion
        
        #region Private methods
        private void CameraTypeManager_CamerasDiscovered(object sender,  CamerasDiscoveredEventArgs e)
        {
            //if(currentContent != ThumbnailViewerContent.Cameras)
            //    return;
                
            viewerCameras.CamerasDiscovered(e.Summaries);
        }
        private void InitializeSelector()
        {
            SelectorOption optionFiles = new SelectorOption(ScreenManager.Properties.Resources.explorer_video, "", ThumbnailViewerContent.Files);
            SelectorOption optionShortcuts = new SelectorOption(ScreenManager.Properties.Resources.explorer_shortcut, "", ThumbnailViewerContent.Shortcuts);
            SelectorOption optionCameras = new SelectorOption(ScreenManager.Properties.Resources.explorer_camera, "", ThumbnailViewerContent.Cameras);
            
            List<SelectorOption> options = new List<SelectorOption>();
            options.Add(optionFiles);
            options.Add(optionShortcuts);
            options.Add(optionCameras);
            
            selector = new Selector(options, 0);
            selector.Location = new Point(160, 6);
            selector.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            selector.SelectionChanged += Selector_SelectionChanged;
            
            this.Controls.Add(selector);
            selector.BringToFront();
        }
        private void Selector_SelectionChanged(object sender, EventArgs e)
        {
            SelectorOption option = selector.Selected;
            ThumbnailViewerContent selectedContent = (ThumbnailViewerContent)option.Data;
            SwitchContent(selectedContent);
        }
        private void InitializeViewers()
        {
            viewerFiles.LoadAsked += Viewer_FileLoadAsked;
            viewerFiles.BeforeLoad += Viewer_BeforeLoad;
            viewerFiles.ProgressChanged += Viewer_ProgressChanged;
            viewerFiles.AfterLoad += Viewer_AfterLoad;
            
            viewerShortcuts.LoadAsked += Viewer_FileLoadAsked;
            viewerShortcuts.BeforeLoad += Viewer_BeforeLoad;
            viewerShortcuts.ProgressChanged += Viewer_ProgressChanged;
            viewerShortcuts.AfterLoad += Viewer_AfterLoad;
            
            // TODO: Events of the camera viewer.
        }
        private void Viewer_FileLoadAsked(object sender, LoadAskedEventArgs e)
        {
            if(LoadAsked != null)
                LoadAsked(sender, e);
        }
        private void Viewer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }
        private void Viewer_BeforeLoad(object sender, EventArgs e)
        {
            progressBar.Value = 0;
            progressBar.Visible = true;
        }
        private void Viewer_AfterLoad(object sender, EventArgs e)
        {
            progressBar.Value = 100;
            progressBar.Visible = false;
        }
        private void SwitchContent(ThumbnailViewerContent newContent)
        {
            if(currentContent == newContent)
                return;
            
            if(currentContent == ThumbnailViewerContent.Files)
            {
                viewerFiles.CancelLoading();
                viewerFiles.Clear();
            }
            else if(currentContent == ThumbnailViewerContent.Shortcuts)
            {
                viewerShortcuts.CancelLoading();
                viewerShortcuts.Clear();
            }
            
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.ChangeFileExplorerTab != null)
                dp.ChangeFileExplorerTab(GetFileExplorerTab(newContent));
            
            this.splitMain.Panel2.Controls.Clear();
            
            // TODO: switch on current content if needed to dispose resources, etc.
            
            switch(newContent)
            {
                case ThumbnailViewerContent.Files:
                    {
                        viewer = viewerFiles;
                        break;
                    }
                case ThumbnailViewerContent.Shortcuts:
                    {
                        viewer = viewerShortcuts;
                        break;
                    }
                case ThumbnailViewerContent.Cameras:
                    {
                        viewer = viewerCameras;
                        break;
                    }
            }
            
            this.splitMain.Panel2.Controls.Add(viewer);
            viewer.BringToFront();
            currentContent = newContent;
        }
        private ActiveFileBrowserTab GetFileExplorerTab(ThumbnailViewerContent content)
        {
            ActiveFileBrowserTab tab = ActiveFileBrowserTab.Explorer;
            
            switch(content)
            {
            case ThumbnailViewerContent.Files: 
                tab = ActiveFileBrowserTab.Explorer;
                break;
            case ThumbnailViewerContent.Shortcuts: 
                tab = ActiveFileBrowserTab.Shortcuts;
                break;
            case ThumbnailViewerContent.Cameras: 
                tab = ActiveFileBrowserTab.Cameras;
                break;
            }
            
            return tab;
        }
        #endregion
    }
}
