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
using System.ComponentModel;
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
        private BrowserContentSnapshot browserContent;
        private bool showingScreen = false;
        private BrowserContentType currentViewerType = BrowserContentType.FileSystem;
        private ThumbnailViewerFiles viewerFiles = new ThumbnailViewerFiles();
        private ThumbnailViewerCameras viewerCameras = new ThumbnailViewerCameras();
        private UserControl viewer;
        private SizeSelector sizeSelector = new SizeSelector();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public ThumbnailViewerContainer()
        {
            InitializeComponent();
            Populate();
            
            NotificationCenter.BeforeLoadVideo += NotificationCenter_BeforeLoadVideo;
            NotificationCenter.BrowserContentUpdated += NotificationCenter_BrowserContentUpdated;
            NotificationCenter.BrowserContentTypeChanged += NotificationCenter_BrowserContentTypeChanged;
            
            CameraTypeManager.CamerasDiscovered += CameraTypeManager_CamerasDiscovered;
            CameraTypeManager.CameraForgotten += CameraTypeManager_CameraForgotten; 
            CameraTypeManager.CameraThumbnailProduced += CameraTypeManager_CameraThumbnailProduced;

            viewerFiles.FileLoadAsked += Viewer_FileLoadAsked;
            viewerFiles.BeforeLoad += Viewer_BeforeLoad;
            viewerFiles.ProgressChanged += Viewer_ProgressChanged;
            viewerFiles.AfterLoad += Viewer_AfterLoad;

            viewerCameras.BeforeLoad += Viewer_BeforeLoad;
            viewerCameras.ProgressChanged += Viewer_ProgressChanged;
            viewerCameras.AfterLoad += Viewer_AfterLoad;

            ShowHideAddressBar(false);
            UpdateThumbnailsSize();

            this.Hotkeys = HotkeySettingsManager.ActiveBindings.GetCommandBindings("ThumbnailViewerContainer");
        }

        private void Populate()
        {
            // Right aligned controls
            // right to left: explorer tab selector, size selector, progress bar.
            ExplorerThumbSize size = PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize;
            sizeSelector.ForceSelect(size);

            sizeSelector.Top = 10;
            sizeSelector.Left = viewerSelector.Left - sizeSelector.Width - 30;
            sizeSelector.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sizeSelector.SelectionChanged += SizeSelector_SelectionChanged;
            splitMain.Panel1.Controls.Add(sizeSelector);

            sizeSelector.Visible = true;
        }

        #region Public methods
        public void RefreshUICulture()
        {
            viewerFiles.RefreshUICulture();
            viewerCameras.RefreshUICulture();
        }

        /// <summary>
        /// Hide the panel and cancel any ongoing loading operation.
        /// </summary>
        public void HideContent()
        {
            if (this.showingScreen)
                return;

            log.DebugFormat("Hiding thumbnails panel.");
            viewerFiles.CancelLoading();
            viewerCameras.SetHidden();
            this.Visible = false;
            this.showingScreen = true;
        }

        /// <summary>
        /// Unhide the panel and refresh the content if needed.
        /// </summary>
        public void UnhideContent()
        {
            log.DebugFormat("Unhiding thumbnails panel to {0}", currentViewerType.ToString());
            this.Visible = true;
            this.showingScreen = false;

            this.Cursor = Cursors.WaitCursor;
            
            if (currentViewerType == BrowserContentType.FileSystem)
            {
                viewerFiles.BrowserContentUpdated(browserContent);
            }
            else if(currentViewerType == BrowserContentType.Cameras)
            {
                viewerCameras.Unhide();
            }
                
            this.Cursor = Cursors.Default;
        }
        
        public void SetFullScreen(bool fullScreen)
        {
            btnCloseFullscreen.Image = fullScreen ? Properties.Resources.collapse_16 : Properties.Resources.expand_16;
        }

        public string GetStatusString()
        {
            if (viewer == null)
                return "";

            if (currentViewerType == BrowserContentType.FileSystem)
                return browserContent == null ? "" : browserContent.Location.Key;
            else
                return "Camera list";
        }
        #endregion

        #region Event handlers

        private void NotificationCenter_BeforeLoadVideo(object sender, EventArgs e)
        {
            HideContent();
        }

        private void NotificationCenter_BrowserContentTypeChanged(object sender, EventArgs<BrowserContentType> e)
        {
            SwitchContent(e.Value);
        }
        private void NotificationCenter_BrowserContentUpdated(object sender, EventArgs<BrowserContentSnapshot> e)
        {
            // The navigation pane has changed location.
            BrowserContentSnapshot snapshot = e.Value;
            if (snapshot == null)
                return;

            // Remember the content even if we are not visible.
            // We'll show it later in UnhideContent().
            browserContent = snapshot;
            
            if (!this.Visible)
                return;

            if (currentViewerType == BrowserContentType.Cameras)
                return;

            lblAddress.Text = browserContent.Location.Key;
            viewerFiles.BrowserContentUpdated(snapshot);
        }

        private void CameraTypeManager_CamerasDiscovered(object sender,  CamerasDiscoveredEventArgs e)
        {
            viewerCameras.CamerasDiscovered(e.Summaries);
        }
        
        private void CameraTypeManager_CameraForgotten(object sender, EventArgs<CameraSummary> e)
        {
            viewerCameras.CameraForgotten(e.Value);
        }

        private void CameraTypeManager_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            if (!e.HadError && !e.Cancelled)
                viewerCameras.CameraImageReceived(e.Summary, e.Thumbnail);
        }

        private void SizeSelector_SelectionChanged(object sender, EventArgs e)
        {
            UpdateThumbnailsSize();
            PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize = sizeSelector.SelectedSize;
        }

        private void ViewerSelector_SelectionChanged(object sender, EventArgs e)
        {
            ViewerSelectorOption option = viewerSelector.Selected;
            BrowserContentType selectedContent = (BrowserContentType)option.Data;
            SwitchContent(selectedContent);
            NotificationCenter.RaiseBrowserContentTypeChanged(this, selectedContent);
            NotificationCenter.RaiseUpdateStatus();
        }
        
        private void Viewer_FileLoadAsked(object sender, FileLoadAskedEventArgs e)
        {
            if(FileLoadAsked != null)
                FileLoadAsked(sender, e);
        }
        private void Viewer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //int progress = Math.Min(e.ProgressPercentage, progressBar.Maximum);
            //progressBar.Value = progress;
        }
        private void Viewer_BeforeLoad(object sender, EventArgs e)
        {
            //progressBar.Value = 0;
            //progressBar.Visible = true;
        }
        private void Viewer_AfterLoad(object sender, EventArgs e)
        {
            //progressBar.Value = 100;
            //progressBar.Visible = false;
        }
        private void btnCloseFullscreen_Click(object sender, EventArgs e)
        {
            NotificationCenter.RaiseFullScreenToggle();
        }
        #endregion

        #region Private methods
        private void UpdateThumbnailsSize()
        {
            if (currentViewerType == BrowserContentType.FileSystem)
                viewerFiles.UpdateThumbnailsSize(sizeSelector.SelectedSize);
            else
                viewerCameras.UpdateThumbnailsSize(sizeSelector.SelectedSize);
        }

        /// <summary>
        /// Switch to the correct type of viewer.
        /// For files this does not trigger any loading operation.
        /// For cameras it starts the discovery process (only if we are visible).
        /// </summary>
        private void SwitchContent(BrowserContentType viewerType)
        {
            if(viewer != null && currentViewerType == viewerType)
                return;

            log.DebugFormat("Switching from {0} to {1}.", viewer == null ? "null" : currentViewerType.ToString(), viewerType);
            
            ClearContent();
            this.splitMain.Panel2.Controls.Clear();
            
            switch(viewerType)
            {
                case BrowserContentType.FileSystem:
                    {
                        viewerFiles.UpdateThumbnailsSize(sizeSelector.SelectedSize);
                        viewer = viewerFiles;
                        break;
                    }
                case BrowserContentType.Cameras:
                    {
                        viewerCameras.BeforeSwitch();
                        viewerCameras.UpdateThumbnailsSize(sizeSelector.SelectedSize);
                        viewer = viewerCameras;

                        if (this.Visible)
                        {
                            CameraTypeManager.StartDiscoveringCameras();
                        }
                        break;
                    }
            }
            
            this.splitMain.Panel2.Controls.Add(viewer);
            viewer.BringToFront();
            currentViewerType = viewerType;
        }
        private void ClearContent()
        {
            if(currentViewerType == BrowserContentType.FileSystem)
            {
                viewerFiles.CancelLoading();
                viewerFiles.Clear();
            }
            else
            {
                CameraTypeManager.StopDiscoveringCameras();
                viewerCameras.SetHidden();
            }

        }
        private void ShowHideAddressBar(bool visible)
        {
            btnBack.Visible = visible;
            btnForward.Visible = visible;
            btnUp.Visible = visible;
            lblAddress.Visible = visible;
        }
        #endregion

        #region Commands
        protected override bool ExecuteCommand(string name)
        {
            ThumbnailViewerContainerCommands command = (ThumbnailViewerContainerCommands)Enum.Parse(typeof(ThumbnailViewerContainerCommands), name);

            switch (command)
            {
                case ThumbnailViewerContainerCommands.DecreaseSize:
                    CommandDecreaseSize();
                    break;
                case ThumbnailViewerContainerCommands.IncreaseSize:
                    CommandIncreaseSize();
                    break;
                default:
                    return base.ExecuteCommand(name);
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

        #region Folder navigation
        private void btnUp_Click(object sender, EventArgs e)
        {
            if (currentViewerType == BrowserContentType.Cameras)
                return;

            //if (!Directory.Exists(path))
            //    return;

            //DirectoryInfo info = Directory.GetParent(path);
            //if (info == null)
            //    return;

            //NotificationCenter.RaiseFolderChangeAsked(info.FullName);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            //NotificationCenter.RaiseFolderNavigationAsked(FolderNavigationType.Backward);
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            //NotificationCenter.RaiseFolderNavigationAsked(FolderNavigationType.Forward);
        }
        #endregion

    }
}
