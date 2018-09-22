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
            get { return supervisorView; }
            set { supervisorView = value;}
        }
        public bool FullScreen
        {
            get { return fullScreen; }
        }
        #endregion

        #region Members
        private RootKernel rootKernel;
        private SupervisorUserInterface supervisorView;
        private bool fullScreen;
        private Rectangle memoBounds;
        private FormWindowState memoWindowState;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public KinoveaMainWindow(RootKernel rootKernel, bool firstInstance)
        {
            log.Debug("Create main UI window.");

            this.rootKernel = rootKernel;
            InitializeComponent();

            string title = " Kinovea";
            if (!firstInstance)
            {
                // Add random characters at the end of the window title so it can be identified by automation scripts.
                string id = Guid.NewGuid().ToString().Substring(0, 4);
                title += " [" + id + "]";
            }

            this.Text = title;
            supervisorView = new SupervisorUserInterface(rootKernel);
            this.Controls.Add(supervisorView);
            supervisorView.Dock = DockStyle.Fill;
            supervisorView.BringToFront();
        }
        #endregion

        public void ToggleFullScreen()
        {
            // TODO: Does this work for multiple monitors ?
            
            this.SuspendLayout();
            
            fullScreen = !fullScreen;
            
            if(fullScreen)
            {
                memoBounds = this.Bounds;
                memoWindowState = this.WindowState;
                
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
                this.WindowState = memoWindowState;
                this.Bounds = memoBounds;
                
                this.menuStrip.Visible = true;
                this.toolStrip.Visible = true;
                this.statusStrip.Visible = true;
            }
            
            this.ResumeLayout();
        }
        public void PlugUI(UserControl fileExplorer, UserControl screenManager)
        {
            supervisorView.PlugUI(fileExplorer, screenManager);
        }

        #region Event Handlers
        private void UserInterface_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = rootKernel.CloseSubModules();
        }
        #endregion
    }
}