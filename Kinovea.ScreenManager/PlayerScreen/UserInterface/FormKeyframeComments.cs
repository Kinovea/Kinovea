/*
Copyright © Joan Charmant 2008.
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

using Kinovea.ScreenManager.Languages;
using System;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class formKeyframeComments : Form
    {

        // This is an info box common to all keyframes.
        // It can be activated or deactivated by the user.
        // When activated, it only display itself if we are stopped on a keyframe.
        // The content is then updated with keyframe content.

        #region Properties
        public bool UserActivated
        {
            get { return m_bUserActivated; }
            set { m_bUserActivated = value; }
        }
        
        #endregion

        #region Members
        private bool m_bUserActivated;
        private Keyframe m_Keyframe;
        private PlayerScreenUserInterface m_psui;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Contructors
        public formKeyframeComments(PlayerScreenUserInterface _psui)
        {
            InitializeComponent();
            RefreshUICulture();
            m_bUserActivated = false;
            m_psui = _psui;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                m_psui = null;
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Public Interface
        public void UpdateContent(Keyframe _keyframe)
        {
            // We keep only one window, and the keyframe it displays is swaped.

            if (m_Keyframe != _keyframe)
            {
                SaveInfos();
                m_Keyframe = _keyframe;
                LoadInfos();
            }
        }
        public void CommitChanges()
        {
            SaveInfos();  
        }
        public void RefreshUICulture()
        {
            this.Text = "   " + ScreenManagerLang.dlgKeyframeComment_Title;
            toolTips.SetToolTip(btnBold, ScreenManagerLang.ToolTip_RichText_Bold);
            toolTips.SetToolTip(btnItalic, ScreenManagerLang.ToolTip_RichText_Italic);
            toolTips.SetToolTip(btnUnderline, ScreenManagerLang.ToolTip_RichText_Underline);
            toolTips.SetToolTip(btnStrike, ScreenManagerLang.ToolTip_RichText_Strikeout);
            toolTips.SetToolTip(btnForeColor, ScreenManagerLang.ToolTip_RichText_ForeColor);
            toolTips.SetToolTip(btnBackColor, ScreenManagerLang.ToolTip_RichText_BackColor);
        }
        #endregion

        #region Form event handlers
        private void formKeyframeComments_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // If the user close the mini window we only hide it.
                e.Cancel = true;
                m_bUserActivated = false;
                SaveInfos();
            }
            else
            {
                // == CloseReason.FormOwnerClosing
            }

            this.Visible = false;
        }
        #endregion
        
        #region Styling event handlers
        private void btnBold_Click(object sender, EventArgs e)
        {
            int style = GetSelectionStyle();
            style = rtbComment.SelectionFont.Bold ? style - (int)FontStyle.Bold : style + (int)FontStyle.Bold;
            rtbComment.SelectionFont = new Font(rtbComment.SelectionFont.FontFamily, rtbComment.SelectionFont.Size, (FontStyle)style);
        }
        private void btnItalic_Click(object sender, EventArgs e)
        {
            int style = GetSelectionStyle();
            style = rtbComment.SelectionFont.Italic ? style - (int)FontStyle.Italic : style + (int)FontStyle.Italic;
            rtbComment.SelectionFont = new Font(rtbComment.SelectionFont.FontFamily, rtbComment.SelectionFont.Size, (FontStyle)style);
        }
        private void btnUnderline_Click(object sender, EventArgs e)
        {
            int style = GetSelectionStyle();
            style = rtbComment.SelectionFont.Underline ? style - (int)FontStyle.Underline : style + (int)FontStyle.Underline;
            rtbComment.SelectionFont = new Font(rtbComment.SelectionFont.FontFamily, rtbComment.SelectionFont.Size, (FontStyle)style);
        }
        private void btnStrike_Click(object sender, EventArgs e)
        {
            int style = GetSelectionStyle();
            style = rtbComment.SelectionFont.Strikeout ? style - (int)FontStyle.Strikeout : style + (int)FontStyle.Strikeout;
            rtbComment.SelectionFont = new Font(rtbComment.SelectionFont.FontFamily, rtbComment.SelectionFont.Size, (FontStyle)style);
        }
        private void btnForeColor_Click(object sender, EventArgs e)
        {
            FormColorPicker picker = new FormColorPicker(rtbComment.SelectionColor);
            FormsHelper.Locate(picker);
            if(picker.ShowDialog() == DialogResult.OK)
            {
                rtbComment.SelectionColor = picker.PickedColor;
            }
            picker.Dispose();
        }
        private void btnBackColor_Click(object sender, EventArgs e)
        {
            FormColorPicker picker = new FormColorPicker(rtbComment.SelectionBackColor);
            FormsHelper.Locate(picker);
            if(picker.ShowDialog() == DialogResult.OK)
            {
                rtbComment.SelectionBackColor = picker.PickedColor;
            }
            picker.Dispose();
        }
        #endregion

        #region Lower level helpers
        private void LoadInfos()
        {
            // Update
            txtTitle.Text = m_Keyframe.Title;
            rtbComment.Clear();
            rtbComment.Rtf = m_Keyframe.Comments;
        }
        private void SaveInfos()
        {
            // Commit changes to the keyframe
            // This must not be called at each info modification otherwise the update routine breaks...
            
            log.Debug("Saving comment and title");
            if (m_Keyframe != null)
            {
                m_Keyframe.Comments = rtbComment.Rtf;
    
                if(m_Keyframe.Title != txtTitle.Text)
                {
                    m_Keyframe.Title = txtTitle.Text;	
                    m_psui.OnKeyframesTitleChanged();
                }
            }
        }
        private int GetSelectionStyle()
        {
            // Combine all the styles into an int, to have generic toggles methods.
            int bold = rtbComment.SelectionFont.Bold ? (int)FontStyle.Bold : 0;
            int italic = rtbComment.SelectionFont.Italic ? (int)FontStyle.Italic : 0;
            int underline = rtbComment.SelectionFont.Underline ? (int)FontStyle.Underline : 0;
            int strikeout = rtbComment.SelectionFont.Strikeout ? (int)FontStyle.Strikeout : 0;
            
            return bold + italic + underline + strikeout;
        }
        #endregion

    }
}