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
        private const string EXTERNAL_COMMAND_IDENTIFIER = "Kinovea";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public KinoveaMainWindow(RootKernel rootKernel)
        {
            log.Debug("Creating main UI window.");

            this.rootKernel = rootKernel;
            InitializeComponent();

            UpdateTitle();
            
            this.FormClosing += KinoveaMainWindow_FormClosing;
            supervisorView = new SupervisorUserInterface(rootKernel);
            this.Controls.Add(supervisorView);
            supervisorView.Dock = DockStyle.Fill;
            supervisorView.BringToFront();

            log.DebugFormat("Restoring window state: {0}, window rectangle: {1}", WindowManager.ActiveWindow.WindowState, WindowManager.ActiveWindow.WindowRectangle);
            if (Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(WindowManager.ActiveWindow.WindowRectangle)))
            {
                // The monitor it was on is still here, move it to this monitor and then restore the state.
                this.StartPosition = FormStartPosition.Manual;
                this.DesktopBounds = WindowManager.ActiveWindow.WindowRectangle;
                this.WindowState = WindowManager.ActiveWindow.WindowState;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
                
            EnableCopyData();
        }

        /// <summary>
        /// Update the title after the window name has changed.
        /// </summary>
        public void UpdateTitle()
        {
            string title = "Kinovea";
            if (!string.IsNullOrEmpty(WindowManager.TitleName))
                title += string.Format(" [{0}]", WindowManager.TitleName);

            this.Text = title;
        }

        private void KinoveaMainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            
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
        public void PlugUI(UserControl navigationPanel, UserControl screenManager)
        {
            supervisorView.PlugUI(navigationPanel, screenManager);
        }

        #region Event Handlers
        private void UserInterface_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save the screen list if needed.
            // We must do this here and not in KinoveaMainWindow_FormClosing as it will be too late.
            WindowManager.ActiveWindow.WindowState = this.WindowState;
            WindowManager.ActiveWindow.WindowRectangle = this.DesktopBounds;

            if (WindowManager.ActiveWindow.StartupMode == WindowStartupMode.Continue)
            {
                var descriptors = rootKernel.ScreenManager.GetScreenDescriptors();
                WindowManager.ActiveWindow.ReplaceScreens(descriptors);
            }

            WindowManager.SaveActiveWindow();

            // Start the close process. It may be cancelled if the user had unsaved changes.
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

            log.DebugFormat("Received WM_COPYDATA.");
                
            NativeMethods.COPYDATASTRUCT copyData = (NativeMethods.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.COPYDATASTRUCT));
            int dataType = (int)copyData.dwData;
            if (dataType != 0)
            {
                log.DebugFormat("Malformed command.");
                return;
            }

            string message = Marshal.PtrToStringUni(copyData.lpData);
            bool parsed = message.StartsWith(EXTERNAL_COMMAND_IDENTIFIER);
            if (!parsed)
            {
                message = Marshal.PtrToStringAnsi(copyData.lpData);
                parsed = message.StartsWith(EXTERNAL_COMMAND_IDENTIFIER);
            }

            if (!parsed)
            {
                log.ErrorFormat("Unrecognized command.");
                return;
            }
            
            int commandIndex = message.IndexOf(':');
            if (commandIndex < 0)
            {
                log.ErrorFormat("Malformed command. Separator not found.");
                return;
            }

            string command = message.Substring(commandIndex + 1, message.Length - (commandIndex + 1));
            log.DebugFormat("Received external command:\"{0}\"", command);

            NotificationCenter.RaiseExternalCommand(this, command);
        }
        #endregion
    }
}