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
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class CommonControls : UserControl
    {
        #region Properties
        public IScreenManagerUIContainer ScreenManagerUIContainer
		{
			set { m_ScreenManagerUIContainer = value; }
		}
        public bool Playing
        {
            get { return m_bPlaying;  }
            set 
            { 
                m_bPlaying = value;
                RefreshPlayButton();
            }
        }
        public bool SyncMerging
		{
			get { return m_bSyncMerging; }
			set 
			{ 
				m_bSyncMerging = value; 
				RefreshMergeTooltip();
			}
		}
        #endregion

        #region Members
        private bool m_bPlaying;
        private bool m_bSyncMerging;
		private long m_iOldPosition;
		private IScreenManagerUIContainer m_ScreenManagerUIContainer;
		private Button btnSnapShot = new Button();
		private Button btnDualVideo = new Button();
        #endregion

        #region Construction & Culture
        public CommonControls()
        {
            InitializeComponent();
            PostInit();
        }
        private void PostInit()
        {
        	BackColor = Color.White;
            
        	btnSnapShot.BackColor = System.Drawing.Color.Transparent;
        	btnSnapShot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	btnSnapShot.Cursor = System.Windows.Forms.Cursors.Hand;
        	btnSnapShot.FlatAppearance.BorderSize = 0;
        	btnSnapShot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	btnSnapShot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	btnSnapShot.Image = ScreenManager.Properties.Resources.snapsingle_1;
        	btnSnapShot.Location = new System.Drawing.Point(trkFrame.Right + 10, btnMerge.Top);
        	btnSnapShot.MinimumSize = new System.Drawing.Size(25, 25);
        	btnSnapShot.Name = "btnSnapShot";
        	btnSnapShot.Size = new System.Drawing.Size(30, 25);
        	btnSnapShot.UseVisualStyleBackColor = false;
        	btnSnapShot.Click += new System.EventHandler(btnSnapshot_Click);
        	
        	btnDualVideo.BackColor = System.Drawing.Color.Transparent;
        	btnDualVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	btnDualVideo.Cursor = System.Windows.Forms.Cursors.Hand;
        	btnDualVideo.FlatAppearance.BorderSize = 0;
        	btnDualVideo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	btnDualVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	btnDualVideo.Image = ScreenManager.Properties.Resources.savevideo;
        	btnDualVideo.Location = new System.Drawing.Point(btnSnapShot.Right + 10, btnSnapShot.Top);
        	btnDualVideo.MinimumSize = new System.Drawing.Size(25, 25);
        	btnDualVideo.Name = "btnDualVideo";
        	btnDualVideo.Size = new System.Drawing.Size(30, 25);
        	btnDualVideo.UseVisualStyleBackColor = false;
        	btnDualVideo.Click += new System.EventHandler(btnDualVideo_Click);
            
        	this.Controls.Add(btnSnapShot);
        	this.Controls.Add(btnDualVideo);
        }
        public void RefreshUICulture()
        {
            // Labels
            lblInfo.Text = ScreenManagerLang.lblInfo_Text;

            // ToolTips
            toolTips.SetToolTip(buttonGotoFirst, ScreenManagerLang.buttonGotoFirst_ToolTip);
            toolTips.SetToolTip(buttonGotoLast, ScreenManagerLang.buttonGotoLast_ToolTip);
            toolTips.SetToolTip(buttonGotoNext, ScreenManagerLang.buttonGotoNext_ToolTip);
            toolTips.SetToolTip(buttonGotoPrevious, ScreenManagerLang.buttonGotoPrevious_ToolTip);
            toolTips.SetToolTip(buttonPlay, ScreenManagerLang.buttonPlay_ToolTip);
            toolTips.SetToolTip(btnSwap, ScreenManagerLang.mnuSwapScreens);
            toolTips.SetToolTip(btnSync, ScreenManagerLang.btnSync_ToolTip);
            toolTips.SetToolTip(btnSnapShot, ScreenManagerLang.ToolTip_SideBySideSnapshot);
            toolTips.SetToolTip(btnDualVideo, ScreenManagerLang.ToolTip_SideBySideVideo);
            
            RefreshMergeTooltip();
		}
		#endregion
        
		private void CommonControls_Resize(object sender, EventArgs e)
		{
			btnSnapShot.Location = new System.Drawing.Point(trkFrame.Right + 10, btnMerge.Top);
			btnDualVideo.Location = new System.Drawing.Point(btnSnapShot.Right + 10, btnMerge.Top);
		}
		
        #region Buttons Handlers
        public void buttonGotoFirst_Click(object sender, EventArgs e)
        {
        	if(m_ScreenManagerUIContainer != null)
        	{
        		m_ScreenManagerUIContainer.CommonCtrl_GotoFirst();
        		trkFrame.Position = trkFrame.Minimum;
        		trkFrame.Invalidate();
            	PlayStopped();
        	}
        }
        public void buttonGotoPrevious_Click(object sender, EventArgs e)
        {
            if(m_ScreenManagerUIContainer != null)
            { 
            	m_ScreenManagerUIContainer.CommonCtrl_GotoPrev(); 
            	trkFrame.Position--;
            	trkFrame.Invalidate();
            }
        }
        public void buttonPlay_Click(object sender, EventArgs e)
        {
            if(m_ScreenManagerUIContainer != null)
        	{
                m_bPlaying = !m_bPlaying;
                RefreshPlayButton();
                m_ScreenManagerUIContainer.CommonCtrl_Play(); 
            }
        }
        public void buttonGotoNext_Click(object sender, EventArgs e)
        {
            if(m_ScreenManagerUIContainer != null)
        	{ 
            	m_ScreenManagerUIContainer.CommonCtrl_GotoNext(); 
            	trkFrame.Position++;
            	trkFrame.Invalidate();
            }
        }
        public void buttonGotoLast_Click(object sender, EventArgs e)
        {
            if(m_ScreenManagerUIContainer != null)
        	{
            	m_ScreenManagerUIContainer.CommonCtrl_GotoLast(); 
            	trkFrame.Position = trkFrame.Maximum;
            	trkFrame.Invalidate();
            }
        }
        private void btnSwap_Click(object sender, EventArgs e)
        {
            if(m_ScreenManagerUIContainer != null)
        	{ 
            	m_ScreenManagerUIContainer.CommonCtrl_Swap(); 
            }
        }
        private void btnSync_Click(object sender, EventArgs e)
        {
            if(m_ScreenManagerUIContainer != null)
        	{ 
            	m_ScreenManagerUIContainer.CommonCtrl_Sync(); 
            }
        }
        private void btnMerge_Click(object sender, EventArgs e)
        {
       		if(m_ScreenManagerUIContainer != null)
        	{ 
       			m_bSyncMerging = !m_bSyncMerging;
       			m_ScreenManagerUIContainer.CommonCtrl_Merge();
       			RefreshMergeTooltip();
       		}
        }
        private void btnSnapshot_Click(object sender, EventArgs e)
        {
       		if(m_ScreenManagerUIContainer != null)
        	{ 
       			m_ScreenManagerUIContainer.CommonCtrl_Snapshot();
       		}
        }
        private void btnDualVideo_Click(object sender, EventArgs e)
        {
       		if(m_ScreenManagerUIContainer != null)
        	{ 
       			m_ScreenManagerUIContainer.CommonCtrl_DualVideo();
       		}
        }
        
        
        #endregion
        
        #region TrkFrame Handlers
        private void trkFrame_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            if(e.Position != m_iOldPosition)
        	{
        		m_iOldPosition = e.Position;
            	if(m_ScreenManagerUIContainer != null)
        		{ 
            		m_ScreenManagerUIContainer.CommonCtrl_PositionChanged(e.Position); 
            	}
        	}
        }
        #endregion
        
        #region Lower level helpers
        private void RefreshPlayButton()
        {
            if (m_bPlaying)
            {
                buttonPlay.Image = Kinovea.ScreenManager.Properties.Resources.liqpause6;
            }
            else
            {
                buttonPlay.Image = Kinovea.ScreenManager.Properties.Resources.liqplay17;
            }
        }
        private void PlayStopped()
        {
            buttonPlay.Image = Kinovea.ScreenManager.Properties.Resources.liqplay17;
        }
        private void RefreshMergeTooltip()
        {
        	if(m_bSyncMerging)
            {
            	toolTips.SetToolTip(btnMerge, ScreenManagerLang.ToolTip_CommonCtrl_DisableMerge);
            }
            else
            {
            	toolTips.SetToolTip(btnMerge, ScreenManagerLang.ToolTip_CommonCtrl_EnableMerge);	
            }
        }
		#endregion
    }
}
