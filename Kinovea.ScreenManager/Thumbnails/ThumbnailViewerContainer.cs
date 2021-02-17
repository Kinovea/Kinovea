#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Windows.Forms;

using Kinovea.Camera;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Host the current thumbnail viewer and exposes some common controls to change size, type of content, etc.
    /// </summary>
    public partial class ThumbnailViewerContainer : KinoveaControl
    {
        public event EventHandler<FileLoadAskedEventArgs> FileLoadAsked;
        
        #region Members
        private SizeSelector sizeSelector = new SizeSelector();
        private ThumbnailViewerContent currentContent = ThumbnailViewerContent.Files;
        private List<string> files;
        private ThumbnailViewerFiles viewerFiles = new ThumbnailViewerFiles();
        private ThumbnailViewerFiles viewerShortcuts = new ThumbnailViewerFiles();
        private ThumbnailViewerCameras viewerCameras = new ThumbnailViewerCameras();
        private UserControl viewer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public ThumbnailViewerContainer()
        {
            InitializeComponent();
            InitializeSizeSelector();
            InitializeViewers();
            progressBar.Left = sizeSelector.Right + 10;
            progressBar.Visible = false;

            NotificationCenter.CurrentDirectoryChanged += NotificationCenter_CurrentDirectoryChanged;
            NotificationCenter.ExplorerTabChanged += (s, e) => SwitchContent(Convert(e.Tab));
            
            CameraTypeManager.CamerasDiscovered += CameraTypeManager_CamerasDiscovered;
            CameraTypeManager.CameraSummaryUpdated += CameraTypeManager_CameraSummaryUpdated;
            CameraTypeManager.CameraForgotten += CameraTypeManager_CameraForgotten; 
            CameraTypeManager.CameraThumbnailProduced += CameraTypeManager_CameraThumbnailProduced;

            // Switch immediately to the right tab, don't wait for the file explorer to load.
            // In particular if the active tab is the cameras we want to get the thumbnails as soon as possible, even if not displaying yet.
            SwitchContent(Convert(PreferencesManager.FileExplorerPreferences.ActiveTab));
                    
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys("ThumbnailViewerContainer");
        }

        #region Public methods
        public void RefreshUICulture()
        {
            viewerFiles.RefreshUICulture();
            viewerShortcuts.RefreshUICulture();
            viewerCameras.RefreshUICulture();
        }
        
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
        #endregion
        
        #region Private methods
        private void NotificationCenter_CurrentDirectoryChanged(object sender, CurrentDirectoryChangedEventArgs e)
        {
            this.files = e.Files;

            if (!e.Refresh || !this.Visible)
                return;

            if (currentContent != ThumbnailViewerContent.Files && currentContent != ThumbnailViewerContent.Shortcuts)
                return;

            if (e.Shortcuts)
                viewerShortcuts.CurrentDirectoryChanged(files);
            else
                viewerFiles.CurrentDirectoryChanged(files);
        }

        private void CameraTypeManager_CamerasDiscovered(object sender,  CamerasDiscoveredEventArgs e)
        {
            viewerCameras.CamerasDiscovered(e.Summaries);
        }
        
        private void CameraTypeManager_CameraSummaryUpdated(object sender, CameraSummaryUpdatedEventArgs e)
        {
            viewerCameras.CameraSummaryUpdated(e.Summary);
        }

        private void CameraTypeManager_CameraForgotten(object sender, EventArgs<CameraSummary> e)
        {
            viewerCameras.CameraForgotten(e.Value);
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
        
        private void Selector_SelectionChanged(object sender, EventArgs e)
        {
            ViewerSelectorOption option = viewerSelector.Selected;
            ThumbnailViewerContent selectedContent = (ThumbnailViewerContent)option.Data;
            SwitchContent(selectedContent);
            NotificationCenter.RaiseExplorerTabChanged(this, Convert(selectedContent));
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
            int progress = Math.Min(e.ProgressPercentage, progressBar.Maximum);
            progressBar.Value = progress;
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

            log.DebugFormat("Switching from {0} to {1}.", currentContent, newContent);
            
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
                        viewerCameras.BeforeSwitch();
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
        private ActiveFileBrowserTab Convert(ThumbnailViewerContent content)
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
        private ThumbnailViewerContent Convert(ActiveFileBrowserTab tab)
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
        private void CameraTypeManager_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            if(!e.HadError && !e.Cancelled)
                viewerCameras.CameraImageReceived(e.Summary, e.Thumbnail);
        }
        private void ThumbnailViewerContainer_Load(object sender, EventArgs e)
        {
            CameraTypeManager.StartDiscoveringCameras();
        }
        #endregion

        #region Commands
        protected override bool ExecuteCommand(int cmd)
        {
            ThumbnailViewerContainerCommands command = (ThumbnailViewerContainerCommands)cmd;

            switch (command)
            {
                case ThumbnailViewerContainerCommands.DecreaseSize:
                    CommandDecreaseSize();
                    break;
                case ThumbnailViewerContainerCommands.IncreaseSize:
                    CommandIncreaseSize();
                    break;
                default:
                    return base.ExecuteCommand(cmd);
            }

            return true;
        }

        private void CommandIncreaseSize()
        {
            sizeSelector.Increase();
        }

        private void CommandDecreaseSize()
        {
            sizeSelector.Decrease();
        }
        #endregion

        private void btnCloseFullscreen_Click(object sender, EventArgs e)
        {
            NotificationCenter.RaiseFullScreenToggle(this);
        }
    }
}
