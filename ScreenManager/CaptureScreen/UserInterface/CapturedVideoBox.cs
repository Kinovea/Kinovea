#region License
/*
Copyright © Joan Charmant 2009.
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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Microsoft.VisualBasic.FileIO;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Represent a recently saved video as a thumbnail.
	/// (saved during the live of a Capture Screen)
	/// The box display a thumbnail of the video and allows the user to change
	/// the file name of the video and to launch it in a PlayerScreen.
	/// </summary>
	public partial class CapturedVideoBox : UserControl
	{
		
		#region EventDelegates
        // Déclarations de Types
        public delegate void CloseThumbHandler(object sender, EventArgs e);
        public delegate void ClickThumbHandler(object sender, EventArgs e);
        public delegate void LaunchVideoHandler(object sender, EventArgs e);
        
        // Déclarations des évènements
        [Category("Action"), Browsable(true)]
        public event CloseThumbHandler CloseThumb;
        [Category("Action"), Browsable(true)]
        public event ClickThumbHandler ClickThumb;
        [Category("Action"), Browsable(true)]
        public event LaunchVideoHandler LaunchVideo;
        #endregion
        
        #region Properties
        public string FilePath
        {
        	get { return m_CapturedVideo.Filepath; }
        }
        #endregion
        
		#region Members
		private CapturedVideo m_CapturedVideo;
		private bool m_bAutoUpdatingTitle;
		
		#region Context menu
		private ContextMenuStrip popMenu = new ContextMenuStrip();
		private ToolStripMenuItem mnuLoadVideo = new ToolStripMenuItem();
		private ToolStripMenuItem mnuHide = new ToolStripMenuItem();
		private ToolStripMenuItem mnuDelete = new ToolStripMenuItem();
		#endregion
		
		#endregion
		
		public void RefreshUICulture()
		{
			ReloadMenusCulture();
		}
		
		#region Construction & initialization
		public CapturedVideoBox(CapturedVideo _cv)
		{
			m_CapturedVideo = _cv;
			InitializeComponent();
			
            btnClose.Parent = pbThumbnail;
            
            m_bAutoUpdatingTitle = true;
            tbTitle.Text = Path.GetFileName(m_CapturedVideo.Filepath);
            m_bAutoUpdatingTitle = false;
            
            BuildContextMenus();
            ReloadMenusCulture();
		}
		private void BuildContextMenus()
		{
			mnuLoadVideo.Click += new EventHandler(mnuLoadVideo_Click);
			mnuLoadVideo.Image = Properties.Resources.film_go;
			mnuHide.Click += new EventHandler(mnuHide_Click);
			mnuHide.Image = Properties.Resources.hide;
			mnuDelete.Click += new EventHandler(mnuDelete_Click);
			mnuDelete.Image = Properties.Resources.delete;
			popMenu.Items.AddRange(new ToolStripItem[] { mnuLoadVideo, new ToolStripSeparator(), mnuHide, mnuDelete });	
			this.ContextMenuStrip = popMenu;
		}
		#endregion
		
		#region Event Handlers - Mouse Enter / Leave
		private void pbThumbnail_MouseMove(object sender, MouseEventArgs e)
        {
        	if(e.Button == MouseButtons.Left)
        	{
        		DoDragDrop(m_CapturedVideo.Filepath, DragDropEffects.Copy);
        	}
        }
        private void Controls_MouseEnter(object sender, EventArgs e)
        {
        	ShowButtons();
        }
        private void Controls_MouseLeave(object sender, EventArgs e)
        {
            // We hide the close button only if we left the whole control.
            Point clientMouse = PointToClient(Control.MousePosition);
            if(!pbThumbnail.ClientRectangle.Contains(clientMouse))
            {
                HideButtons();
                StopEditing();
            }
        }
        #endregion
        
        #region Event Handlers - Buttons / Text
        private void btnClose_Click(object sender, EventArgs e)
        {
            if (CloseThumb != null) CloseThumb(this, e);
        }
        private void pbThumbnail_Click(object sender, EventArgs e)
        {
            if (ClickThumb != null) ClickThumb(this, e);
        }
        private void pbThumbnail_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        	if (LaunchVideo != null) LaunchVideo(this, e);
        }
        private void TbTitleTextChanged(object sender, EventArgs e)
        {
        	if(!m_bAutoUpdatingTitle)
        	{
        		// Update the file
        		//m_CapturedVideo.RenameFile(tbTitle.Text);
        		//m_Keyframe.Title = tbTitle.Text;
        		//UpdateToolTip();
        	}
        }
        private void TbTitleEnter(object sender, EventArgs e)
        {
        	//DeactivateKeyboardHandler();
        }
        #endregion
        
        #region Event Handlers - Menu
        private void mnuLoadVideo_Click(object sender, EventArgs e)
		{
			if (LaunchVideo != null) LaunchVideo(this, e);	
		}
        private void mnuHide_Click(object sender, EventArgs e)
		{
			if (CloseThumb != null) CloseThumb(this, e);
		}
        private void mnuDelete_Click(object sender, EventArgs e)
		{
			// Use the built-in dialogs to confirm (or not).
			// Delete is done through moving to recycle bin.
			if(File.Exists(m_CapturedVideo.Filepath))
			{
				try
				{
					FileSystem.DeleteFile(m_CapturedVideo.Filepath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
				}
				catch(OperationCanceledException)
				{
					// User cancelled confirmation box.
				}
	
				// Other possible error case: the file couldn't be deleted because it's still in use.
				
				// If file was effectively moved to trash, hide the thumb and reload the folder.
				if(!File.Exists(m_CapturedVideo.Filepath))
				{
					if (CloseThumb != null) CloseThumb(this, e);
					
					// Ask the Explorer tree to refresh itself...
					// This will in turn refresh the thumbnails pane.
		            DelegatesPool dp = DelegatesPool.Instance();
		            if (dp.RefreshFileExplorer != null)
		            {
		                dp.RefreshFileExplorer(true);
		            }
				}
			}
			
		}
        #endregion
        
        #region Private helpers
        private void ReloadMenusCulture()
		{
        	// Reload the text for each menu.
			// this is done at construction time and at RefreshUICulture time.
			mnuLoadVideo.Text = ScreenManagerLang.mnuThumbnailPlay;
			mnuHide.Text = ScreenManagerLang.mnuGridsHide;
			mnuDelete.Text = ScreenManagerLang.mnuThumbnailDelete;
        }
        private void ShowButtons()
        {
            //btnClose.Visible = true;
        }
        private void HideButtons()
        {
            btnClose.Visible = false;
        }
        private void DeactivateKeyboardHandler()
        {
            // Mouse enters the box : deactivate the keyboard handling for the screens
            // so we can use <space>, <return>, etc. here.
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.DeactivateKeyboardHandler != null)
            {
                dp.DeactivateKeyboardHandler();
            }
        }
        private void ActivateKeyboardHandler()
        {
            // Mouse leave the box : reactivate the keyboard handling for the screens
            // so we can use <space>, <return>, etc. as player shortcuts
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.ActivateKeyboardHandler != null)
            {
                dp.ActivateKeyboardHandler();
            }
        }
        private void StopEditing()
        {
        	ActivateKeyboardHandler();
        	
        	if(tbTitle.Text.Length == 0)
    		{
    			// We reseted the title. We should now display the timecode.
    			//m_bAutoUpdatingTitle = true;
    			//tbTitle.Text = m_Keyframe.Title;
    			//m_bAutoUpdatingTitle = false;
    		}
        	else
        	{
        		//m_CapturedVideo.CommitFileName(tbTitle.Text);
        	}
        }
        #endregion

        
        
	}
}
