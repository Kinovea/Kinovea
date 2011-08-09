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
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.Root
{
    public partial class KinoveaMainWindow : Form
    {
        #region Properties
        public SupervisorUserInterface SupervisorControl
        {
            get { return sui; }
            set { sui = value;}
        }
        public bool FullScreen
        {
            get { return m_bFullScreen; }
        }
        protected override CreateParams CreateParams 
        {
            // Fix flickering of controls during resize.
            // Ref. http://social.msdn.microsoft.com/forums/en-US/winforms/thread/aaed00ce-4bc9-424e-8c05-c30213171c2c/
            get 
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }
        #endregion

        #region Members
        private RootKernel mRootKernel;
        private SupervisorUserInterface sui;
        private bool m_bFullScreen;
        private Rectangle m_MemoBounds;
        private FormWindowState m_MemoWindowState;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public KinoveaMainWindow(RootKernel _RootKernel)
        {
            log.Debug("Create main UI window.");
            
            mRootKernel = _RootKernel;
            InitializeComponent();
            
            this.Text = " Kinovea";
            SupervisorControl = new SupervisorUserInterface(mRootKernel);
            this.Controls.Add(SupervisorControl);
            SupervisorControl.Dock = DockStyle.Fill;
            SupervisorControl.BringToFront();
        }
        #endregion

        public void ToggleFullScreen()
        {
            // TODO: Does this work for multiple monitors ?
            
            this.SuspendLayout();
            
            m_bFullScreen = !m_bFullScreen;
            
            if(m_bFullScreen)
            {
                m_MemoBounds = this.Bounds;
                m_MemoWindowState = this.WindowState;
                
                // Go full screen. We switch to normal state first, otherwise it doesn't work each time.
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                Screen screen = Screen.FromControl(this);
                this.Bounds = screen.Bounds;
                
                this.menuStrip.Visible = false;    
                this.toolStrip.Visible = false;
                this.statusStrip.Visible = false;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = m_MemoWindowState;
                this.Bounds = m_MemoBounds;
                
                this.menuStrip.Visible = true;
                this.toolStrip.Visible = true;
                this.statusStrip.Visible = true;
            }
            
            this.ResumeLayout();
        }
        
        #region Event Handlers
        private void UserInterface_FormClosing(object sender, FormClosingEventArgs e)
        {
            mRootKernel.CloseSubModules();
        }
        #endregion
    }
}