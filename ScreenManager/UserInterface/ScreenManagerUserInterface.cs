#region License
/*
Copyright © Joan Charmant 2008.
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
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.Camera;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class ScreenManagerUserInterface : UserControl
    {
        #region Delegates
        public DelegateUpdateTrackerFrame delegateUpdateTrackerFrame;
        #endregion

        public event EventHandler<FileLoadAskedEventArgs> LoadAsked;
        
        #region Properties
        public bool CommonControlsVisible 
        {
            get { return !splitScreensPanel.Panel2Collapsed; }
        }
        public bool CommonPlaying
        {
            get { return commonControls.Playing; }
            set { commonControls.Playing = value; }
        }
        public bool Merging
        {
            get { return commonControls.SyncMerging; }
            set { commonControls.SyncMerging = value; }
        }
        #endregion
        
        #region Members
        private ThumbnailViewerContainer thumbnailViewerContainer = new ThumbnailViewerContainer();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
        
		public ScreenManagerUserInterface(IScreenManagerUIContainer controller)
        {
        	log.Debug("Constructing ScreenManagerUserInterface.");
            InitializeComponent();
            commonControls.Controller = controller;
            
            BackColor = Color.White;
            Dock = DockStyle.Fill;
            
            InitializeThumbnailsContainer();
            
            delegateUpdateTrackerFrame = UpdateTrkFrame;

            // Thumbs are enabled by default.
            thumbnailViewerContainer.BringToFront();
            pnlScreens.BringToFront();
            pnlScreens.Dock     = DockStyle.Fill;

            Application.Idle += this.IdleDetector;
        }
        
        #region Public methods
        public void RefreshUICulture()
        {
            commonControls.RefreshUICulture();
            thumbnailViewerContainer.RefreshUICulture();
        }
        public void ShowCommonControls(bool show)
        {
            splitScreensPanel.Panel2Collapsed = !show;
        }
        public void ToggleCommonControls()
        {
            splitScreensPanel.Panel2Collapsed = !splitScreensPanel.Panel2Collapsed;
        }
        public bool OnKeyPress(Keys key)
        {
            if(!thumbnailViewerContainer.Visible)
                return false;
            
            return thumbnailViewerContainer.OnKeyPress(key);
        }
        public void OrganizeScreens(List<AbstractScreen> screenList)
        {
            if(screenList.Count == 0)
            {
                pnlScreens.Visible = false;
                this.AllowDrop = true;
                ClearLeftScreen();
                ClearRightScreen();

                thumbnailViewerContainer.Unhide();
            }
            else
            {
                pnlScreens.Visible = true;
                this.AllowDrop = false;
                
                thumbnailViewerContainer.HideContent();
                
                splitScreens.Panel1.Controls.Clear();
                PrepareLeftScreen(screenList[0].UI);
                
                if(screenList.Count == 2)
                    PrepareRightScreen(screenList[1].UI);
                else
                    ClearRightScreen();
            }
        }
        
        #region Forwarded to common controls
        public void UpdateSyncPosition(long position)
        {
            commonControls.UpdateSyncPosition(position);
        }
        public void SetupTrkFrame(long min, long max, long pos)
        {
            commonControls.SetupTrkFrame(min, max, pos);
        }
        public void UpdateTrkFrame(long position)
        {
            commonControls.UpdateTrkFrame(position);
        }
        public void DisplayAsPaused()
        {
            commonControls.Playing = false;
        }
        public bool CommonKeyPress(Keys key)
        {
            return commonControls.OnKeyPress(key);
        }
        #endregion

        #endregion

        private void InitializeThumbnailsContainer()
        {
            thumbnailViewerContainer.Dock = DockStyle.Fill;
            thumbnailViewerContainer.Visible = true;
            thumbnailViewerContainer.LoadAsked += (s,e) => {
                if(LoadAsked != null)
                    LoadAsked(this, e);
            };
            
            this.Controls.Add(thumbnailViewerContainer);
		}
        private void IdleDetector(object sender, EventArgs e)
		{
			log.Debug("Application is idle in ScreenManagerUserInterface.");
			
			// This is a one time only routine.
			Application.Idle -= new EventHandler(this.IdleDetector);
			
			// Launch file.
			string filePath = CommandLineArgumentManager.Instance().InputFile;
			if(filePath != null && File.Exists(filePath) && LoadAsked != null)
			    LoadAsked(this, new FileLoadAskedEventArgs(filePath, -1));
		}
        private void pnlScreens_Resize(object sender, EventArgs e)
        {
            // Reposition Common Controls panel so it doesn't take more space than necessary.
            splitScreensPanel.SplitterDistance = pnlScreens.Height - 50;
        }
        private void ScreenManagerUserInterface_DoubleClick(object sender, EventArgs e)
        {
         	DelegatesPool dp = DelegatesPool.Instance();
            if (dp.OpenVideoFile != null)
                dp.OpenVideoFile();
        }

        #region Screen management
        private void PrepareLeftScreen(UserControl screenUI)
        {
            splitScreens.Panel1Collapsed = false;
            splitScreens.Panel1.AllowDrop = true;
            splitScreens.Panel1.Controls.Add(screenUI);
        }
        private void PrepareRightScreen(UserControl screenUI)
        {
            splitScreens.Panel2Collapsed = false;
            splitScreens.Panel2.AllowDrop = true;
            splitScreens.Panel2.Controls.Add(screenUI);
        }
        private void ClearLeftScreen()
        {
            splitScreens.Panel1.Controls.Clear();
            splitScreens.Panel1Collapsed = true;
            splitScreens.Panel1.AllowDrop = false;
        }
        private void ClearRightScreen()
        {
            splitScreens.Panel2.Controls.Clear();
            splitScreens.Panel2Collapsed = true;
            splitScreens.Panel2.AllowDrop = false;
        }
        #endregion
        
        #region DragDrop
        private void DroppableArea_DragOver(object sender, DragEventArgs e)
        {
        	e.Effect = DragDropEffects.All;
        }
        private void ScreenManagerUserInterface_DragDrop(object sender, DragEventArgs e)
        {
            if(LoadAsked != null)
                LoadAsked(this, new FileLoadAskedEventArgs(GetDroppedObject(e), -1));
        }
        private void splitScreens_Panel1_DragDrop(object sender, DragEventArgs e)
        {
            if(LoadAsked != null)
                LoadAsked(this, new FileLoadAskedEventArgs( GetDroppedObject(e), 1));
        }
        private void splitScreens_Panel2_DragDrop(object sender, DragEventArgs e)
        {
            if(LoadAsked != null)
                LoadAsked(this, new FileLoadAskedEventArgs(GetDroppedObject(e), 2));
        }
        private string GetDroppedObject(DragEventArgs e)
        {
            string result = "";
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                result = (string)e.Data.GetData(DataFormats.StringFormat);
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Array fileArray = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (fileArray != null)
                   result = fileArray.GetValue(0).ToString();
            }
            
            return result;
        }
        #endregion

        /*private void DoDisplayThumbnails(List<String> _fileNames, bool _bRefreshNow)
        {
        	// Keep track of the files, in case we need to bring them back
        	// after closing a screen.
            filenames = _fileNames;

            if(!_bRefreshNow)
                return;
            
            if (_fileNames.Count > 0)
            {
            	thumbnailViewerContainer.Height = Height - 20; // margin for cosmetic
                
            	// We keep the Kinovea logo until there is at least 1 thumbnail to show.
            	// After that we never display it again.
                pbLogo.Visible = false;
            }
            else
            {
                thumbnailViewerContainer.Height = 1;
            }

            if (thumbnailViewerContainer.Visible)
            {
                this.Cursor = Cursors.WaitCursor;
                thumbnailViewerContainer.DisplayThumbnails(_fileNames);
                this.Cursor = Cursors.Default;
            }
            else
            {
                // Thumbnail pane was hidden to show player screen
                // Then we changed folder and we don't have anything to show.
                // Let's clean older thumbnails now.
                thumbnailViewerContainer.CleanupThumbnails();
            }
        }*/
    }
}
