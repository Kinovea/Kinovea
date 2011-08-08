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

using Kinovea.ScreenManager.Languages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class ScreenManagerUserInterface : UserControl
    {
        #region Delegates
        public delegate void DelegateUpdateTrkFrame(int _iFrame);
        public DelegateUpdateTrkFrame m_DelegateUpdateTrkFrame;
        #endregion

        public ThumbListView m_ThumbsViewer = new ThumbListView();
        
        #region Members
        private List<String> m_FolderFileNames = new List<String>();
        private bool m_bThumbnailsWereVisible;
        private IScreenManagerUIContainer m_ScreenManagerUIContainer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
        
		public ScreenManagerUserInterface(IScreenManagerUIContainer _ScreenManagerUIContainer)
        {
        	log.Debug("Constructing ScreenManagerUserInterface.");

        	m_ScreenManagerUIContainer = _ScreenManagerUIContainer;
        	
            InitializeComponent();
            ComCtrls.ScreenManagerUIContainer = m_ScreenManagerUIContainer;
            m_ThumbsViewer.SetScreenManagerUIContainer(m_ScreenManagerUIContainer);
            
            BackColor = Color.White;
            Dock = DockStyle.Fill;
            
            m_ThumbsViewer.Top = 0;
            m_ThumbsViewer.Left = 0;
            m_ThumbsViewer.Width = Width;
            m_ThumbsViewer.Height = Height - pbLogo.Height - 10;
            m_ThumbsViewer.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
			m_ThumbsViewer.Closing += ThumbsViewer_Closing;
            this.Controls.Add(m_ThumbsViewer);

            m_DelegateUpdateTrkFrame = new DelegateUpdateTrkFrame(UpdateTrkFrame);

            // Registers our exposed functions to the DelegatePool.
            DelegatesPool dp = DelegatesPool.Instance();
            dp.DisplayThumbnails = DoDisplayThumbnails;
                        
            // Thumbs are enabled by default.
            m_ThumbsViewer.Visible = true;
            m_bThumbnailsWereVisible = true;
            m_ThumbsViewer.BringToFront();
            
            pnlScreens.BringToFront();
            pnlScreens.Dock     = DockStyle.Fill;
             
            Application.Idle += new EventHandler(this.IdleDetector);
        }
		private void IdleDetector(object sender, EventArgs e)
		{
			log.Debug("Application is idle in ScreenManagerUserInterface.");
			
			// This is a one time only routine.
			Application.Idle -= new EventHandler(this.IdleDetector);
			
			// Launch file.
			string filePath = CommandLineArgumentManager.Instance().InputFile;
			if(filePath != null && File.Exists(filePath))
			{
				m_ScreenManagerUIContainer.DropLoadMovie(filePath, -1);
			}
		}
        
        #region public, called from Kernel
        public void RefreshUICulture()
        {
            ComCtrls.RefreshUICulture();
            btnShowThumbView.Text = ScreenManagerLang.btnShowThumbView;
            m_ThumbsViewer.RefreshUICulture();
        }
        public void DisplaySyncLag(int _iOffset)
        {
            ComCtrls.SyncOffset = _iOffset;
        }
        public void UpdateSyncPosition(int _iPosition)
        {
        	ComCtrls.trkFrame.UpdateSyncPointMarker(_iPosition);
        	ComCtrls.trkFrame.Invalidate();
        }
        public void SetupTrkFrame(int _iMinimum, int _iMaximum, int _iPosition)
        {
            ComCtrls.trkFrame.Minimum = _iMinimum;
            ComCtrls.trkFrame.Maximum = _iMaximum;
            ComCtrls.trkFrame.Position = _iPosition;   
        }
        public void UpdateTrkFrame(int _iPosition)
        {
            ComCtrls.trkFrame.Position = _iPosition;
        }
        public void OrganizeMenuProxy(Delegate _method)
        {
            _method.DynamicInvoke();
        }
        public void DisplayAsPaused()
        {
            ComCtrls.Playing = false;
        }
        #endregion

        private void pnlScreens_Resize(object sender, EventArgs e)
        {
            // Reposition Common Controls panel so it doesn't take 
            // more space than necessary.
            splitScreensPanel.SplitterDistance = pnlScreens.Height - 50;
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            // Hide Common Controls Panel
            IUndoableCommand ctcc = new CommandToggleCommonControls(splitScreensPanel);
            CommandManager cm = CommandManager.Instance();
            cm.LaunchUndoableCommand(ctcc);
        }
        private void ScreenManagerUserInterface_DoubleClick(object sender, EventArgs e)
        {
         	DelegatesPool dp = DelegatesPool.Instance();
            if (dp.OpenVideoFile != null)
            {
                dp.OpenVideoFile();
            }   
        }

        #region DragDrop
        private void ScreenManagerUserInterface_DragOver(object sender, DragEventArgs e)
        {
        	e.Effect = m_ScreenManagerUIContainer.GetDragDropEffects(-1);
        }
        private void ScreenManagerUserInterface_DragDrop(object sender, DragEventArgs e)
        {
                CommitDrop(e, -1);
        }
        private void splitScreens_Panel1_DragOver(object sender, DragEventArgs e)
        {
        	e.Effect = m_ScreenManagerUIContainer.GetDragDropEffects(0);
        }
        private void splitScreens_Panel1_DragDrop(object sender, DragEventArgs e)
        {
            CommitDrop(e, 1);
        }
        private void splitScreens_Panel2_DragOver(object sender, DragEventArgs e)
        {
        	e.Effect = m_ScreenManagerUIContainer.GetDragDropEffects(1);
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
                m_ScreenManagerUIContainer.DropLoadMovie(filePath, _iScreen);
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
                    m_ScreenManagerUIContainer.DropLoadMovie(filePath, _iScreen);
                }
            }

        }
        #endregion

        private void btnShowThumbView_Click(object sender, EventArgs e)
        {
            m_ThumbsViewer.Visible = true;
            this.Cursor = Cursors.WaitCursor;
            m_ThumbsViewer.DisplayThumbnails(m_FolderFileNames);
            this.Cursor = Cursors.Default;
        }
        private void ThumbsViewer_Closing(object sender, EventArgs e)
        {
            m_ThumbsViewer.Visible = false;
            m_bThumbnailsWereVisible = false;
        }
        private void DoDisplayThumbnails(List<String> _fileNames, bool _bRefreshNow)
        {
        	// Keep track of the files, in case we need to bring them back
        	// after closing a screen.
            m_FolderFileNames = _fileNames;

            if(_bRefreshNow)
            {
	            if (_fileNames.Count > 0)
	            {
	            	m_ThumbsViewer.Height = Height - 20; // margin for cosmetic
	                btnShowThumbView.Visible = true;
	                
	            	// We keep the Kinovea logo until there is at least 1 thumbnail to show.
	            	// After that we never display it again.
	                pbLogo.Visible = false;
	            }
	            else
	            {
	                // If no thumbs are to be displayed, enable the drag & drop and double click on background.
	                m_ThumbsViewer.Height = 1;
	                btnShowThumbView.Visible = false;
	
	                // TODO: info message.
	                //"No files to display in this folder."
	            }
	
	            if (m_ThumbsViewer.Visible)
	            {
	                this.Cursor = Cursors.WaitCursor;
	                m_ThumbsViewer.DisplayThumbnails(_fileNames);
	                this.Cursor = Cursors.Default;
	            }
	            else if (m_bThumbnailsWereVisible)
	            {
	                // Thumbnail pane was hidden to show player screen
	                // Then we changed folder and we don't have anything to show. 
	                // Let's clean older thumbnails now.
	                m_ThumbsViewer.CleanupThumbnails();
	            }
            }
        }
        public void CloseThumbnails()
        {
            // This happens when the Thumbnail view is closed by another component
            // (e.g: When we need to show screens)
            log.Debug("Closing thumbnails to display screen.");
            if (m_ThumbsViewer.Visible)
            {
                m_bThumbnailsWereVisible = true;
            }
            
            m_ThumbsViewer.Visible = false;
        }
        public void BringBackThumbnails()
        {
            if (m_bThumbnailsWereVisible)
            {
                m_ThumbsViewer.Visible = true;
                this.Cursor = Cursors.WaitCursor;
                m_ThumbsViewer.DisplayThumbnails(m_FolderFileNames);
                this.Cursor = Cursors.Default;
            }
        }
        
    }
}
