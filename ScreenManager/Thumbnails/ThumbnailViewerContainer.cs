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
        public event EventHandler<FileLoadAskedEventArgs> FileLoadAsked;
        
        #region Members
        private Selector selector;
        private SizeSelector sizeSelector = new SizeSelector();
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
            InitializeSizeSelector();
            InitializeViewerSelector();
            InitializeViewers();
            progressBar.Left = selector.Right + 10;
            progressBar.Visible = false;
            
            // Registers our exposed functions to the DelegatePool.
            DelegatesPool dp = DelegatesPool.Instance();
            dp.CurrentDirectoryChanged = CurrentDirectoryChanged;
            dp.ExplorerTabChanged = ExplorerTab_Changed;
            
            CameraTypeManager.CamerasDiscovered += CameraTypeManager_CamerasDiscovered;
            CameraTypeManager.CameraSummaryUpdated += CameraTypeManager_CameraSummaryUpdated;
            CameraTypeManager.CameraImageReceived += CameraTypeManager_CameraImageReceived;
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
            
            if(currentContent != ThumbnailViewerContent.Files && currentContent != ThumbnailViewerContent.Shortcuts)
                return;
            
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
            else if(currentContent == ThumbnailViewerContent.Cameras)
                viewerCameras.Unhide();
                
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
        public bool OnKeyPress(Keys keyCode)
        {
            bool handled = false;
            
            switch(keyCode)
            {
                case Keys.Add:
                {
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        sizeSelector.Increase();
                        handled = true;
                    }
                    
                    break;
                }
                case Keys.Subtract:
                {
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        sizeSelector.Decrease();
                        handled = true;
                    }
                    
                    break;
                }
            }
            
            if(handled)
                return true;
                
            if (currentContent == ThumbnailViewerContent.Files)
                return viewerFiles.OnKeyPress(keyCode);
            else if (currentContent == ThumbnailViewerContent.Shortcuts)
                return viewerShortcuts.OnKeyPress(keyCode);
            else
                return viewerCameras.OnKeyPress(keyCode);
        }
        #endregion
        
        #region Private methods
        private void ExplorerTab_Changed(ActiveFileBrowserTab newTab)
        {
            SwitchContent(GetThumbnailViewerContent(newTab));
        }

        private void CameraTypeManager_CamerasDiscovered(object sender,  CamerasDiscoveredEventArgs e)
        {
            if(currentContent != ThumbnailViewerContent.Cameras)
                return;
                
            viewerCameras.CamerasDiscovered(e.Summaries);
        }
        
        private void CameraTypeManager_CameraSummaryUpdated(object sender, CameraSummaryUpdatedEventArgs e)
        {
            if(currentContent != ThumbnailViewerContent.Cameras)
                return;
                
            viewerCameras.CameraSummaryUpdated(e.Summary);
        }

        private void InitializeSizeSelector()
        {
            sizeSelector.Left = 30;
            sizeSelector.SelectionChanged += SizeSelector_SelectionChanged;
            splitMain.Panel1.Controls.Add(sizeSelector);
        }

        private void SizeSelector_SelectionChanged(object sender, EventArgs e)
        {
            if (currentContent == ThumbnailViewerContent.Files)
                viewerFiles.UpdateThumbnailsSize(sizeSelector.SelectedSize);
            else if (currentContent == ThumbnailViewerContent.Shortcuts)
                viewerShortcuts.UpdateThumbnailsSize(sizeSelector.SelectedSize);
            else
                viewerCameras.UpdateThumbnailsSize(sizeSelector.SelectedSize);
                
            PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize = sizeSelector.SelectedSize;
            PreferencesManager.Save();
        }
        
        private void InitializeViewerSelector()
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
            
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.ChangeFileExplorerTab != null)
                dp.ChangeFileExplorerTab(GetFileExplorerTab(selectedContent));
        }
        
        private void InitializeViewers()
        {
            ExplorerThumbSize newSize = PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize;
            sizeSelector.ForceSelect(newSize);
            
            viewerFiles.UpdateThumbnailsSize(newSize);
            viewerFiles.FileLoadAsked += Viewer_FileLoadAsked;
            viewerFiles.BeforeLoad += Viewer_BeforeLoad;
            viewerFiles.ProgressChanged += Viewer_ProgressChanged;
            viewerFiles.AfterLoad += Viewer_AfterLoad;
            
            viewerShortcuts.UpdateThumbnailsSize(newSize);
            viewerShortcuts.FileLoadAsked += Viewer_FileLoadAsked;
            viewerShortcuts.BeforeLoad += Viewer_BeforeLoad;
            viewerShortcuts.ProgressChanged += Viewer_ProgressChanged;
            viewerShortcuts.AfterLoad += Viewer_AfterLoad;
            
            viewerCameras.UpdateThumbnailsSize(newSize);
            viewerCameras.BeforeLoad += Viewer_BeforeLoad;
            viewerCameras.ProgressChanged += Viewer_ProgressChanged;
            viewerCameras.AfterLoad += Viewer_AfterLoad;
        }
        
        private void Viewer_FileLoadAsked(object sender, FileLoadAskedEventArgs e)
        {
            if(FileLoadAsked != null)
                FileLoadAsked(sender, e);
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
            if(viewer != null && currentContent == newContent)
                return;
            
            ClearContent();
            this.splitMain.Panel2.Controls.Clear();
            
            switch(newContent)
            {
                case ThumbnailViewerContent.Files:
                    {
                        viewerFiles.UpdateThumbnailsSize(sizeSelector.SelectedSize);
                        viewer = viewerFiles;
                        break;
                    }
                case ThumbnailViewerContent.Shortcuts:
                    {
                        viewerShortcuts.UpdateThumbnailsSize(sizeSelector.SelectedSize);
                        viewer = viewerShortcuts;
                        break;
                    }
                case ThumbnailViewerContent.Cameras:
                    {
                        CameraTypeManager.DiscoverCameras();
                        viewerCameras.UpdateThumbnailsSize(sizeSelector.SelectedSize);
                        viewer = viewerCameras;
                        break;
                    }
            }
            
            this.splitMain.Panel2.Controls.Add(viewer);
            viewer.BringToFront();
            currentContent = newContent;
        }
        private void ClearContent()
        {
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
            else
            {
                 CameraTypeManager.StopDiscoveringCameras();
            }

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
        private ThumbnailViewerContent GetThumbnailViewerContent(ActiveFileBrowserTab tab)
        {
            ThumbnailViewerContent content = ThumbnailViewerContent.Files;
            
            switch(tab)
            {
                case ActiveFileBrowserTab.Explorer:
                    content = ThumbnailViewerContent.Files;
                    break;
                case ActiveFileBrowserTab.Shortcuts:
                    content = ThumbnailViewerContent.Shortcuts;
                    break;
                case ActiveFileBrowserTab.Cameras:
                    content = ThumbnailViewerContent.Cameras;
                    break;
            }
            
            return content;
        }
        private void CameraTypeManager_CameraImageReceived(object sender, CameraImageReceivedEventArgs e)
        {
            if(currentContent == ThumbnailViewerContent.Cameras)
                viewerCameras.CameraImageReceived(e.Summary, e.Image);
        }
        private void ThumbnailViewerContainer_Load(object sender, EventArgs e)
        {
            // TODO: not necessarily the final place for this call.
            CameraTypeManager.DiscoverCameras();
        }
        #endregion
    }
}
