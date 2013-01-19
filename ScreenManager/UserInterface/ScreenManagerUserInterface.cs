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

        #region Properties
        public bool CommonControlsVisible 
        {
            get { return !splitScreensPanel.Panel2Collapsed; }
        }
        public bool ThumbnailsViewerVisible
        {
            get { return thumbnailsViewer.Visible;}
        }
        #endregion
        
        #region Members
        public ThumbListView thumbnailsViewer = new ThumbListView();
        private List<String> filenames = new List<String>();
        private bool thumbnailsWereVisible;
        private IScreenManagerUIContainer controller;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
        
		public ScreenManagerUserInterface(IScreenManagerUIContainer controller)
        {
        	log.Debug("Constructing ScreenManagerUserInterface.");

        	this.controller = controller;

            InitializeComponent();
            ComCtrls.ScreenManagerUIContainer = controller;
            
            BackColor = Color.White;
            Dock = DockStyle.Fill;
            
            InitializeThumbnailsViewer();
            
            delegateUpdateTrackerFrame = UpdateTrkFrame;

            // Registers our exposed functions to the DelegatePool.
            DelegatesPool dp = DelegatesPool.Instance();
            dp.DisplayThumbnails = DoDisplayThumbnails;

            // Thumbs are enabled by default.
            thumbnailsWereVisible = true;
            thumbnailsViewer.BringToFront();

            pnlScreens.BringToFront();
            pnlScreens.Dock     = DockStyle.Fill;

            Application.Idle += this.IdleDetector;
        }
		
        
        #region Public methods
        public void RefreshUICulture()
        {
            ComCtrls.RefreshUICulture();
            thumbnailsViewer.RefreshUICulture();
        }
        public void UpdateSyncPosition(long position)
        {
        	ComCtrls.trkFrame.UpdateSyncPointMarker(position);
        	ComCtrls.trkFrame.Invalidate();
        }
        public void SetupTrkFrame(long min, long max, long pos)
        {
            ComCtrls.trkFrame.Minimum = min;
            ComCtrls.trkFrame.Maximum = max;
            ComCtrls.trkFrame.Position = pos;
            ComCtrls.trkFrame.Invalidate();
        }
        public void UpdateTrkFrame(long position)
        {
            ComCtrls.trkFrame.Position = position;
            ComCtrls.trkFrame.Invalidate();
        }
        public void ShowCommonControls(bool show)
        {
            splitScreensPanel.Panel2Collapsed = show;
        }
        public void ToggleCommonControls()
        {
            splitScreensPanel.Panel2Collapsed = !splitScreensPanel.Panel2Collapsed;
        }
        public void DisplayAsPaused()
        {
            ComCtrls.Playing = false;
        }
        public bool OnKeyPress(Keys key)
        {
            bool bWasHandled = false;
            switch (key)
            {
                case Keys.Space:
                case Keys.Return:
                {
                    ComCtrls.buttonPlay_Click(null, EventArgs.Empty);
                    bWasHandled = true;
                    break;
                }
                case Keys.Left:
                {
                    ComCtrls.buttonGotoPrevious_Click(null, EventArgs.Empty);
                    bWasHandled = true;
                    break;
                }
                case Keys.Right:
                {
                    ComCtrls.buttonGotoNext_Click(null, EventArgs.Empty);
                    bWasHandled = true;
                    break;
                }
                case Keys.End:
                {
                    ComCtrls.buttonGotoLast_Click(null, EventArgs.Empty);
                    bWasHandled = true;
                    break;
                }
                case Keys.Home:
                {
                    ComCtrls.buttonGotoFirst_Click(null, EventArgs.Empty);
                    bWasHandled = true;
                    break;
                }
                default:
                    break;
            }
            
            return bWasHandled;
        }
        public void OrganizeScreens(List<AbstractScreen> screenList)
        {
            if(screenList.Count == 0)
            {
                pnlScreens.Visible = false;
                this.AllowDrop = true;
                ClearLeftScreen();
                ClearRightScreen();

                thumbnailsViewer.Visible = true;
                this.Cursor = Cursors.WaitCursor;
                thumbnailsViewer.DisplayThumbnails(filenames);
                this.Cursor = Cursors.Default;
            }
            else
            {
                pnlScreens.Visible = true;
                this.AllowDrop = false;
                
                thumbnailsViewer.Visible = false;
                thumbnailsViewer.StopLoading();
                
                splitScreens.Panel1.Controls.Clear();
                PrepareLeftScreen(screenList[0].UI);
                
                if(screenList.Count == 2)
                    PrepareRightScreen(screenList[1].UI);
                else
                    ClearRightScreen();
            }
        }
        #endregion

        private void InitializeThumbnailsViewer()
        {
            thumbnailsViewer.SetScreenManagerUIContainer(controller);
            thumbnailsViewer.Top = 0;
            thumbnailsViewer.Left = 0;
            thumbnailsViewer.Width = Width;
            thumbnailsViewer.Height = Height - pbLogo.Height - 10;
            thumbnailsViewer.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            thumbnailsViewer.Visible = true;
            this.Controls.Add(thumbnailsViewer);
		}
        private void IdleDetector(object sender, EventArgs e)
		{
			log.Debug("Application is idle in ScreenManagerUserInterface.");
			
			// This is a one time only routine.
			Application.Idle -= new EventHandler(this.IdleDetector);
			
			// Launch file.
			string filePath = CommandLineArgumentManager.Instance().InputFile;
			if(filePath != null && File.Exists(filePath))
				controller.DropLoadMovie(filePath, -1);
			
// ----------- TEMPORARY -----------------------------------.
			// Check for cameras connected to the system.
			CameraTypeManager.DiscoverCameras();
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
        
        #region DragDrop
        private void ScreenManagerUserInterface_DragOver(object sender, DragEventArgs e)
        {
        	e.Effect = controller.GetDragDropEffects(-1);
        }
        private void ScreenManagerUserInterface_DragDrop(object sender, DragEventArgs e)
        {
            CommitDrop(e, -1);
        }
        private void splitScreens_Panel1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = controller.GetDragDropEffects(0);
        }
        private void splitScreens_Panel1_DragDrop(object sender, DragEventArgs e)
        {
            CommitDrop(e, 1);
        }
        private void splitScreens_Panel2_DragOver(object sender, DragEventArgs e)
        {
        	e.Effect = controller.GetDragDropEffects(1);
        }
        private void splitScreens_Panel2_DragDrop(object sender, DragEventArgs e)
        {
            CommitDrop(e, 2);
        }
        private void CommitDrop(DragEventArgs e, int _iScreen)
        {
            //-----------------------------------------------------------
            // An object has been dropped.
            // Support drag & drop from the FileExplorer module (listview)
            // or from the Windows Explorer.
            // Not between screens.
            //-----------------------------------------------------------
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                // String. Coming from the file explorer.
                string filePath = (string)e.Data.GetData(DataFormats.StringFormat);
                controller.DropLoadMovie(filePath, _iScreen);
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // File. Coming from Windows Explorer.
                Array fileArray = (Array)e.Data.GetData(DataFormats.FileDrop);

                if (fileArray != null)
                {
                    //----------------------------------------------------------------
                    // Extract string from first array element
                    // (ignore all files except first if number of files are dropped).
                    //----------------------------------------------------------------
                    string filePath = fileArray.GetValue(0).ToString();
                    controller.DropLoadMovie(filePath, _iScreen);
                }
            }

        }
        #endregion

        private void btnShowThumbView_Click(object sender, EventArgs e)
        {
            thumbnailsViewer.Visible = true;
            this.Cursor = Cursors.WaitCursor;
            thumbnailsViewer.DisplayThumbnails(filenames);
            this.Cursor = Cursors.Default;
        }
        private void DoDisplayThumbnails(List<String> _fileNames, bool _bRefreshNow)
        {
        	// Keep track of the files, in case we need to bring them back
        	// after closing a screen.
            filenames = _fileNames;

            if(_bRefreshNow)
            {
	            if (_fileNames.Count > 0)
	            {
	            	thumbnailsViewer.Height = Height - 20; // margin for cosmetic
	                
	            	// We keep the Kinovea logo until there is at least 1 thumbnail to show.
	            	// After that we never display it again.
	                pbLogo.Visible = false;
	            }
	            else
	            {
	                thumbnailsViewer.Height = 1;
	            }
	
	            if (thumbnailsViewer.Visible)
	            {
	                this.Cursor = Cursors.WaitCursor;
	                thumbnailsViewer.DisplayThumbnails(_fileNames);
	                this.Cursor = Cursors.Default;
	            }
	            else if (thumbnailsWereVisible)
	            {
	                // Thumbnail pane was hidden to show player screen
	                // Then we changed folder and we don't have anything to show. 
	                // Let's clean older thumbnails now.
	                thumbnailsViewer.CleanupThumbnails();
	            }
            }
        }
    }
}
