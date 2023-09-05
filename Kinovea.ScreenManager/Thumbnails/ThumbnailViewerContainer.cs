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
using System.IO;

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
        private ThumbnailViewerType currentViewerType = ThumbnailViewerType.Files;
        private string path;
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
            InitializeControlBar();
            
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
            
            if (currentViewerType == ThumbnailViewerType.Files)
                viewerFiles.CancelLoading();
            else if (currentViewerType == ThumbnailViewerType.Shortcuts)
                viewerShortcuts.CancelLoading();
            //else
                //viewerCameras.CancelLoading();
        }
        public void Unhide()
        {
            this.Visible = true;

            this.Cursor = Cursors.WaitCursor;
            
            if (currentViewerType == ThumbnailViewerType.Files)
                viewerFiles.CurrentDirectoryChanged(path, files);
            else if (currentViewerType == ThumbnailViewerType.Shortcuts)
                viewerShortcuts.CurrentDirectoryChanged(path, files);
            else if(currentViewerType == ThumbnailViewerType.Cameras)
                viewerCameras.Unhide();
                
            this.Cursor = Cursors.Default;
        }
        #endregion
        
        #region Private methods
        private void NotificationCenter_CurrentDirectoryChanged(object sender, CurrentDirectoryChangedEventArgs e)
        {
            this.path = e.Path;
            this.files = e.Files;
            

            if (!e.DoRefresh || !this.Visible)
                return;

            lblAddress.Text = path;

            if (currentViewerType != ThumbnailViewerType.Files && currentViewerType != ThumbnailViewerType.Shortcuts)
                return;

            if (e.IsShortcuts)
                viewerShortcuts.CurrentDirectoryChanged(path, files);
            else
                viewerFiles.CurrentDirectoryChanged(path, files);
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

        private void SizeSelector_SelectionChanged(object sender, EventArgs e)
        {
            if (currentViewerType == ThumbnailViewerType.Files)
                viewerFiles.UpdateThumbnailsSize(sizeSelector.SelectedSize);
            else if (currentViewerType == ThumbnailViewerType.Shortcuts)
                viewerShortcuts.UpdateThumbnailsSize(sizeSelector.SelectedSize);
            else
                viewerCameras.UpdateThumbnailsSize(sizeSelector.SelectedSize);
                
            PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize = sizeSelector.SelectedSize;
            PreferencesManager.Save();
        }
        
        private void Selector_SelectionChanged(object sender, EventArgs e)
        {
            ViewerSelectorOption option = viewerSelector.Selected;
            ThumbnailViewerType selectedContent = (ThumbnailViewerType)option.Data;
            SwitchContent(selectedContent);
            NotificationCenter.RaiseExplorerTabChanged(this, Convert(selectedContent));
        }
        
        private void InitializeControlBar()
        {
            // Right aligned controls
            // right to left: explorer tab selector, size selector, progress bar.
            ExplorerThumbSize size = PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize;
            sizeSelector.ForceSelect(size);
            
            viewerFiles.UpdateThumbnailsSize(size);
            viewerFiles.FileLoadAsked += Viewer_FileLoadAsked;
            viewerFiles.BeforeLoad += Viewer_BeforeLoad;
            viewerFiles.ProgressChanged += Viewer_ProgressChanged;
            viewerFiles.AfterLoad += Viewer_AfterLoad;
            
            viewerShortcuts.UpdateThumbnailsSize(size);
            viewerShortcuts.FileLoadAsked += Viewer_FileLoadAsked;
            viewerShortcuts.BeforeLoad += Viewer_BeforeLoad;
            viewerShortcuts.ProgressChanged += Viewer_ProgressChanged;
            viewerShortcuts.AfterLoad += Viewer_AfterLoad;
            
            viewerCameras.UpdateThumbnailsSize(size);
            viewerCameras.BeforeLoad += Viewer_BeforeLoad;
            viewerCameras.ProgressChanged += Viewer_ProgressChanged;
            viewerCameras.AfterLoad += Viewer_AfterLoad;

            sizeSelector.Left = viewerSelector.Left - sizeSelector.Width - 10;
            sizeSelector.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sizeSelector.SelectionChanged += SizeSelector_SelectionChanged;
            splitMain.Panel1.Controls.Add(sizeSelector);

            progressBar.Left = sizeSelector.Left - progressBar.Width - 10;
            progressBar.Visible = false;
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
        private void SwitchContent(ThumbnailViewerType viewerType)
        {
            if(viewer != null && currentViewerType == viewerType)
                return;

            log.DebugFormat("Switching from {0} to {1}.", currentViewerType, viewerType);
            
            ClearContent();
            ShowHideAddressBar(false);
            this.splitMain.Panel2.Controls.Clear();
            
            switch(viewerType)
            {
                case ThumbnailViewerType.Files:
                    {
                        viewerFiles.UpdateThumbnailsSize(sizeSelector.SelectedSize);
                        viewer = viewerFiles;
                        ShowHideAddressBar(true);
                        break;
                    }
                case ThumbnailViewerType.Shortcuts:
                    {
                        viewerShortcuts.UpdateThumbnailsSize(sizeSelector.SelectedSize);
                        viewer = viewerShortcuts;
                        ShowHideAddressBar(true);
                        break;
                    }
                case ThumbnailViewerType.Cameras:
                    {
                        viewerCameras.BeforeSwitch();
                        viewerCameras.UpdateThumbnailsSize(sizeSelector.SelectedSize);
                        viewer = viewerCameras;
                        break;
                    }
            }
            
            this.splitMain.Panel2.Controls.Add(viewer);
            viewer.BringToFront();
            currentViewerType = viewerType;
        }
        private void ClearContent()
        {
            if(currentViewerType == ThumbnailViewerType.Files)
            {
                viewerFiles.CancelLoading();
                viewerFiles.Clear();
            }
            else if(currentViewerType == ThumbnailViewerType.Shortcuts)
            {
                viewerShortcuts.CancelLoading();
                viewerShortcuts.Clear();
            }
            else
            {
                 CameraTypeManager.StopDiscoveringCameras();
            }

        }
        private void ShowHideAddressBar(bool visible)
        {
            btnBack.Visible = visible;
            btnForward.Visible = visible;
            btnUp.Visible = visible;
            lblAddress.Visible = visible;
        }
        private ActiveFileBrowserTab Convert(ThumbnailViewerType content)
        {
            ActiveFileBrowserTab tab = ActiveFileBrowserTab.Explorer;
            
            switch(content)
            {
            case ThumbnailViewerType.Files: 
                tab = ActiveFileBrowserTab.Explorer;
                break;
            case ThumbnailViewerType.Shortcuts: 
                tab = ActiveFileBrowserTab.Shortcuts;
                break;
            case ThumbnailViewerType.Cameras: 
                tab = ActiveFileBrowserTab.Cameras;
                break;
            }
            
            return tab;
        }
        private ThumbnailViewerType Convert(ActiveFileBrowserTab tab)
        {
            ThumbnailViewerType content = ThumbnailViewerType.Files;
            
            switch(tab)
            {
                case ActiveFileBrowserTab.Explorer:
                    content = ThumbnailViewerType.Files;
                    break;
                case ActiveFileBrowserTab.Shortcuts:
                    content = ThumbnailViewerType.Shortcuts;
                    break;
                case ActiveFileBrowserTab.Cameras:
                    content = ThumbnailViewerType.Cameras;
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
            // Do not start discovering cameras on every load, wait until we actually switch to the camera tab.
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

        #region Folder navigation
        private void btnUp_Click(object sender, EventArgs e)
        {
            if (currentViewerType != ThumbnailViewerType.Files && currentViewerType != ThumbnailViewerType.Shortcuts)
                return;

            if (!Directory.Exists(path))
                return;

            DirectoryInfo info = Directory.GetParent(path);
            if (info == null)
                return;

            NotificationCenter.RaiseFolderChangeAsked(this, info.FullName);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            NotificationCenter.RaiseFolderNavigationAsked(this, FolderNavigationType.Backward);
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            NotificationCenter.RaiseFolderNavigationAsked(this, FolderNavigationType.Forward);
        }
        #endregion

    }
}
