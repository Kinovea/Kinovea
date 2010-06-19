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
        public ICommonControlsHandler CommonControlsHandler
		{
			set { m_CommonControlsHandler = value; }
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

        public int SyncOffset
        {
            set 
            {
                int iValue = value;
                lblSyncOffset.Text = "SyncOffset : " + iValue;
                lblSyncOffset.Invalidate();
            }
        }
        #endregion

        #region Members
        private bool m_bPlaying;
        private bool m_bSyncMerging;
		private long m_iOldPosition;
		private ICommonControlsHandler m_CommonControlsHandler;
		private Button btnSnapShot = new Button();
        #endregion

        #region Construction & Culture
        public CommonControls()
        {
            InitializeComponent();
            
			// PostInit
            BackColor = Color.White;
            
        	btnSnapShot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	btnSnapShot.BackColor = System.Drawing.Color.Transparent;
        	btnSnapShot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	btnSnapShot.Cursor = System.Windows.Forms.Cursors.Hand;
        	btnSnapShot.FlatAppearance.BorderSize = 0;
        	btnSnapShot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	btnSnapShot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	btnSnapShot.Image = ScreenManager.Properties.Resources.snapsingle_1;
        	btnSnapShot.Location = new System.Drawing.Point(trkFrame.Left + trkFrame.Width + 10, btnMerge.Top);
        	btnSnapShot.MinimumSize = new System.Drawing.Size(25, 25);
        	btnSnapShot.Name = "btnSnapShot";
        	btnSnapShot.Size = new System.Drawing.Size(30, 25);
        	btnSnapShot.UseVisualStyleBackColor = false;
        	btnSnapShot.Click += new System.EventHandler(btnSnapshot_Click);
            
        	this.Controls.Add(btnSnapShot);
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
            
            RefreshMergeTooltip();
		}
		#endregion
        
        #region Buttons Handlers
        public void buttonGotoFirst_Click(object sender, EventArgs e)
        {
        	if(m_CommonControlsHandler != null)
        	{
        		m_CommonControlsHandler.CommonCtrl_GotoFirst();
        		trkFrame.Position = trkFrame.Minimum;
            	PlayStopped();
        	}
        }
        public void buttonGotoPrevious_Click(object sender, EventArgs e)
        {
            if(m_CommonControlsHandler != null)
            { 
            	m_CommonControlsHandler.CommonCtrl_GotoPrev(); 
            	trkFrame.Position--;
            }
        }
        public void buttonPlay_Click(object sender, EventArgs e)
        {
            if(m_CommonControlsHandler != null)
        	{
                m_bPlaying = !m_bPlaying;
                RefreshPlayButton();
                m_CommonControlsHandler.CommonCtrl_Play(); 
            }
        }
        public void buttonGotoNext_Click(object sender, EventArgs e)
        {
            if(m_CommonControlsHandler != null)
        	{ 
            	m_CommonControlsHandler.CommonCtrl_GotoNext(); 
            	trkFrame.Position++;
            }
        }
        public void buttonGotoLast_Click(object sender, EventArgs e)
        {
            if(m_CommonControlsHandler != null)
        	{
            	m_CommonControlsHandler.CommonCtrl_GotoLast(); 
            	trkFrame.Position = trkFrame.Maximum;
            }
        }
        private void btnSwap_Click(object sender, EventArgs e)
        {
            if(m_CommonControlsHandler != null)
        	{ 
            	m_CommonControlsHandler.CommonCtrl_Swap(); 
            }
        }
        private void btnSync_Click(object sender, EventArgs e)
        {
            if(m_CommonControlsHandler != null)
        	{ 
            	m_CommonControlsHandler.CommonCtrl_Sync(); 
            }
        }
        private void btnMerge_Click(object sender, EventArgs e)
        {
       		if(m_CommonControlsHandler != null)
        	{ 
       			m_bSyncMerging = !m_bSyncMerging;
       			m_CommonControlsHandler.CommonCtrl_Merge();
       			RefreshMergeTooltip();
       		}
        }
        private void btnSnapshot_Click(object sender, EventArgs e)
        {
       		if(m_CommonControlsHandler != null)
        	{ 
       			m_CommonControlsHandler.CommonCtrl_Snapshot();
       		}
        }
        
        #endregion
        
        #region TrkFrame Handlers
        private void trkFrame_PositionChanged(object sender, long _iPosition)
        {
            if(_iPosition != m_iOldPosition)
        	{
        		m_iOldPosition = _iPosition;
            	if(m_CommonControlsHandler != null)
        		{ 
            		m_CommonControlsHandler.CommonCtrl_PositionChanged(_iPosition); 
            	}
        	}
        }
        #endregion
        
        #region Lower level helpers
        private void UpdateDebug()
        {
            lblTrkFrameInfos.Text = "Min : " + trkFrame.Minimum + ", Max : " + trkFrame.Maximum + ", Pos : " + trkFrame.Position;
            lblTrkFrameInfos.Invalidate();
        }
        private void RefreshPlayButton()
        {
            if (m_bPlaying)
            {
                buttonPlay.BackgroundImage = Kinovea.ScreenManager.Properties.Resources.liqpause6;
            }
            else
            {
                buttonPlay.BackgroundImage = Kinovea.ScreenManager.Properties.Resources.liqplay17;
            }
        }
        private void PlayStopped()
        {
            buttonPlay.BackgroundImage = Kinovea.ScreenManager.Properties.Resources.liqplay17;
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
