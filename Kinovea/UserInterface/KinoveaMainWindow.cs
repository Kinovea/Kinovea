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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;
using Kinovea.Services;


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
        private const string COMMAND_TRIGGERCAPTURE = "2b0576a5-43fb-4b92-8e55-a13aea656ee5";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public KinoveaMainWindow(RootKernel rootKernel)
        {
            log.Debug("Creating main UI window.");

            this.rootKernel = rootKernel;
            InitializeComponent();

            string title = " Kinovea";
            if (!string.IsNullOrEmpty(Software.InstanceName))
                title += string.Format(" [{0}]", Software.InstanceName);

            this.Text = title;
            
            this.FormClosing += KinoveaMainWindow_FormClosing;
            supervisorView = new SupervisorUserInterface(rootKernel);
            this.Controls.Add(supervisorView);
            supervisorView.Dock = DockStyle.Fill;
            supervisorView.BringToFront();

            log.DebugFormat("Restoring window state: {0}, window rectangle: {1}", PreferencesManager.GeneralPreferences.WindowState, PreferencesManager.GeneralPreferences.WindowRectangle);
            if (Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(PreferencesManager.GeneralPreferences.WindowRectangle)))
            {
                // The screen it was on is still here, move it to this screen and then restore the state.
                this.StartPosition = FormStartPosition.Manual;
                this.DesktopBounds = PreferencesManager.GeneralPreferences.WindowRectangle;
                this.WindowState = PreferencesManager.GeneralPreferences.WindowState;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
                
            EnableCopyData();
        }

        private void KinoveaMainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            PreferencesManager.GeneralPreferences.WindowState = this.WindowState;
            PreferencesManager.GeneralPreferences.WindowRectangle = this.DesktopBounds;
            PreferencesManager.Save();
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

        private void EnableCopyData()
        {
            NativeMethods.CHANGEFILTERSTRUCT changeFilter = new NativeMethods.CHANGEFILTERSTRUCT();
            changeFilter.size = (uint)Marshal.SizeOf(changeFilter);
            changeFilter.info = 0;
            if (!NativeMethods.ChangeWindowMessageFilterEx(this.Handle, NativeMethods.WM_COPYDATA, NativeMethods.ChangeWindowMessageFilterExAction.Allow, ref changeFilter))
            {
                int error = Marshal.GetLastWin32Error();
                log.ErrorFormat("Error while trying to enable WM_COPYDATA: {0}", error);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg != NativeMethods.WM_COPYDATA)
            {
                base.WndProc(ref m);
                return;
            }

            //-------------------------------
            // Handle WM_COPYDATA.
            // Supported commands: 
            // - Trigger capture.
            //-------------------------------
            log.DebugFormat("Received WM_COPYDATA.");
                
            NativeMethods.COPYDATASTRUCT copyData = (NativeMethods.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.COPYDATASTRUCT));
            int dataType = (int)copyData.dwData;
            if (dataType != 0)
                return;

            if (Marshal.PtrToStringUni(copyData.lpData) == COMMAND_TRIGGERCAPTURE || Marshal.PtrToStringAnsi(copyData.lpData) == COMMAND_TRIGGERCAPTURE)
            {
                log.DebugFormat("Received capture trigger command.");
                NotificationCenter.RaiseCaptureTriggered(this);
            }
            else
            {
                log.ErrorFormat("Unrecognized command.");
            }
    
            return;
        }
        #endregion
    }
}