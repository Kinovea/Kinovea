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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Threading;
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
        #endregion

        #region Contructors
        public formKeyframeComments(PlayerScreenUserInterface _psui)
        {
            InitializeComponent();
            RefreshUICulture();
            m_bUserActivated = false;
            m_psui = _psui;
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
            ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            this.Text = "   " + rm.GetString("dlgKeyframeComment_Title", Thread.CurrentThread.CurrentUICulture);
        }
        #endregion

        #region Event Handlers
        private void formKeyframeComments_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // If the user close the mini window we only hide it.
                e.Cancel = true;
                m_bUserActivated = false;
                SaveInfos();
                ActivateKeyboardHandler();
            }
            else
            {
                // == CloseReason.FormOwnerClosing
            }

            this.Visible = false;
        }
        private void formKeyframeComments_MouseEnter(object sender, EventArgs e)
        {
            DeactivateKeyboardHandler();
        }
        private void formKeyframeComments_MouseLeave(object sender, EventArgs e)
        {
            CheckMouseLeave();
        }
        #endregion

        #region Lower level helpers
        private void CheckMouseLeave()
        {
            // We really leave only if we left the whole control.
            // we have to do this because placing the mouse over the text boxes will raise a
            // formKeyframeComments_MouseLeave event...
            if (!this.Bounds.Contains(Control.MousePosition))
            {
                ActivateKeyboardHandler(); 
            }
        }
        private void DeactivateKeyboardHandler()
        {
            // Mouse enters the info box : deactivate the keyboard handling for the screens
            // so we can use <space>, <return>, etc. here.
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.DeactivateKeyboardHandler != null)
            {
                dp.DeactivateKeyboardHandler();
            }
        }
        private void ActivateKeyboardHandler()
        {
            // Mouse leave the info box : reactivate the keyboard handling for the screens
            // so we can use <space>, <return>, etc. as player shortcuts
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.ActivateKeyboardHandler != null)
            {
                dp.ActivateKeyboardHandler();
            }
        }
        private void LoadInfos()
        {
            // Update
            txtTitle.Text = m_Keyframe.Title;
            rtbComment.Clear();
            foreach (string line in m_Keyframe.Comments)
            {
                rtbComment.AppendText(line);
            }
        }
        private void SaveInfos()
        {
            // Commit changes to the keyframe
            // This must not be called at each info modification otherwise the update routine breaks...
            if (m_Keyframe != null)
            {
                m_Keyframe.Title = txtTitle.Text;

                m_Keyframe.Comments.Clear();
                foreach (string line in rtbComment.Lines)
                {
                    m_Keyframe.Comments.Add(line + "\n");
                }

                m_psui.OnKeyframesTitleChanged();
            }
        }
        #endregion

       

        

    }
}