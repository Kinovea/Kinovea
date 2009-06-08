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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using Videa.Services;
using System.Threading;

namespace Videa.ScreenManager
{
    public partial class ScreenManagerUserInterface : UserControl
    {
        #region Delegates

        // Déclarations de Types
        public delegate void GotoFirstHandler(object sender, EventArgs e);
        public delegate void GotoLastHandler(object sender, EventArgs e);
        public delegate void GotoPrevHandler(object sender, EventArgs e);
        public delegate void GotoNextHandler(object sender, EventArgs e);
        public delegate void PlayHandler(object sender, EventArgs e);
        public delegate void SwapHandler(object sender, EventArgs e);
        public delegate void SyncHandler(object sender, EventArgs e);
        public delegate void PositionChangedHandler(object sender, long _iPosition);
        public delegate void CallbackDropLoadMovie(string _FilePath, int _iScreen);


        // Déclarations des variables
        public GotoFirstHandler         GotoFirst;
        public GotoLastHandler          GotoLast;
        public GotoPrevHandler          GotoPrev;
        public GotoNextHandler          GotoNext;
        public PlayHandler              Play;
        public SwapHandler              Swap;
        public SyncHandler              Sync;
        public PositionChangedHandler   PositionChanged;
        
        public CallbackDropLoadMovie    m_CallbackDropLoadMovie;

        public delegate void DelegateUpdateTrkFrame(int _iFrame);
        public DelegateUpdateTrkFrame m_DelegateUpdateTrkFrame;

        #endregion

        public ThumbListView m_ThumbsViewer;
        public List<String> m_FolderFileNames;
        public bool m_bThumbnailsWereVisible;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ScreenManagerUserInterface()
        {
        	log.Debug("Constructing ScreenManagerUserInterface.");
        	 
            InitializeComponent();
            
            m_FolderFileNames = new List<String>();

            m_ThumbsViewer = new ThumbListView();
            m_ThumbsViewer.Top = 0;
            m_ThumbsViewer.Left = 0;
            m_ThumbsViewer.Width = Width;
            m_ThumbsViewer.Height = Height - pbLogo.Height - 10;
            m_ThumbsViewer.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

            // Thumbs are enabled by default.
            m_ThumbsViewer.Visible = true;
            m_bThumbnailsWereVisible = true;

            m_ThumbsViewer.Closing += new ThumbListView.DelegateClosing(ThumbsViewer_Closing);
            this.Controls.Add(m_ThumbsViewer);
            
            m_ThumbsViewer.BringToFront();
            
            pnlScreens.BringToFront();
            pnlScreens.Dock     = DockStyle.Fill;            

            BackColor = Color.White;
            Dock = DockStyle.Fill;

            m_DelegateUpdateTrkFrame = new DelegateUpdateTrkFrame(UpdateTrkFrame);

            // Registers our exposed functions to the DelegatePool.
            DelegatesPool dp = DelegatesPool.Instance();
            dp.DisplayThumbnails = DoDisplayThumbnails;
        }

        #region public, called from Kernel
        public void RefreshUICulture(ResourceManager _resManager)
        {
            ComCtrls.RefreshUICulture(_resManager);
            btnShowThumbView.Text = _resManager.GetString("btnShowThumbView", Thread.CurrentThread.CurrentUICulture);
            m_ThumbsViewer.RefreshUICulture(_resManager);
        }
        public void DisplaySyncLag(int _iOffset)
        {
            ComCtrls.SyncOffset = _iOffset;
        }
        public void SetupTrkFrame(int _iMinimum, int _iMaximum, int _iPosition)
        {
            ComCtrls.trkFrame.Minimum = _iMinimum;
            ComCtrls.trkFrame.Maximum = _iMaximum;
            ComCtrls.trkFrame.Position = _iPosition;   
        }
        public void UpdateTrkFrame(int _iPosition)
        {
            // This shouldn't be visible outside debug mode.

            //ComCtrls.trkFrame.Position = _iPosition;
            //ComCtrls.UpdateDebug();
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
            e.Effect = DragDropEffects.All;
        }
        private void ScreenManagerUserInterface_DragDrop(object sender, DragEventArgs e)
        {
                CommitDrop(e, -1);
        }
        
        private void splitScreens_Panel1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }
        private void splitScreens_Panel1_DragDrop(object sender, DragEventArgs e)
        {
            CommitDrop(e, 1);
        }
        
        private void splitScreens_Panel2_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }
        private void splitScreens_Panel2_DragDrop(object sender, DragEventArgs e)
        {
            CommitDrop(e, 2);
        }

        private void CommitDrop(DragEventArgs e, int _iScreen)
        {
            //-----------------------------------------------------------
            // Un objet vient d'être déposé
            // On supporte le drag&drop depuis le FileExplorer (listview)
            // depuis l'explorateur windows,
            // mais pas entre les écrans. (swap)
            //-----------------------------------------------------------
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                //--------------------------------------
                // Chaîne en provenance du FileExplorer.
                //--------------------------------------
                string filePath = (string)e.Data.GetData(DataFormats.StringFormat);
                //if(filePath.Equals("swap"))
                //{
                //    // DragDrop entre les deux écrans désactivé : intérfère avec les
                //    // outils de dessins...
                //}
                //else
                //{
                    m_CallbackDropLoadMovie(filePath, _iScreen);
                //}
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //------------------------------------------------
                // Fichier en provenance de l'explorateur windows.
                //------------------------------------------------
                Array fileArray = (Array)e.Data.GetData(DataFormats.FileDrop);

                if (fileArray != null)
                {
                    //----------------------------------------------------------------
                    // Extract string from first array element
                    // (ignore all files except first if number of files are dropped).
                    //----------------------------------------------------------------
                    string filePath = fileArray.GetValue(0).ToString();

                    m_CallbackDropLoadMovie(filePath, _iScreen);
                }
            }

        }
        #endregion

        #region Delegates en provenance du ComCtrls
        private void ComCtrls_GotoFirst(object sender, EventArgs e)
        {
            if (GotoFirst != null) { GotoFirst(this, EventArgs.Empty); }
        }
        private void ComCtrls_GotoLast(object sender, EventArgs e)
        {
            if (GotoLast != null) { GotoLast(this, EventArgs.Empty); }
        }
        private void ComCtrls_GotoNext(object sender, EventArgs e)
        {
            if (GotoNext != null) { GotoNext(this, EventArgs.Empty); }
        }
        private void ComCtrls_GotoPrev(object sender, EventArgs e)
        {
            if (GotoPrev != null) { GotoPrev(this, EventArgs.Empty); }
        }
        private void ComCtrls_Play(object sender, EventArgs e)
        {
            if (Play != null) { Play(this, EventArgs.Empty); }
        }
        private void ComCtrls_Swap(object sender, EventArgs e)
        {
            if (Swap != null) { Swap(this, EventArgs.Empty); }
        }
        private void ComCtrls_Sync(object sender, EventArgs e)
        {
            if (Sync != null) { Sync(this, EventArgs.Empty); }
        }
        private void ComCtrls_PositionChanged(object sender, long _iPosition)
        {
            if (PositionChanged != null) { PositionChanged(sender, _iPosition); }
        }
        #endregion

        private void btnShowThumbView_Click(object sender, EventArgs e)
        {
            m_ThumbsViewer.Visible = true;
            this.Cursor = Cursors.WaitCursor;
            m_ThumbsViewer.DisplayThumbnails(m_FolderFileNames);
            this.Cursor = Cursors.Default;
        }
        private void ThumbsViewer_Closing(object sender)
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
            if (m_ThumbsViewer.Visible)
            {
                m_bThumbnailsWereVisible = true;
                m_ThumbsViewer.Visible = false;
            }
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
