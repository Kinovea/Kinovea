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
using Videa.Services;

namespace Videa.Root
{
    public partial class KinoveaMainWindow : Form
    {
        #region Properties
        public SupervisorUserInterface SupervisorControl
        {
            get { return sui; }
            set { sui = value;}
        }
        #endregion

        #region Members
        private RootKernel mRootKernel;
        private SupervisorUserInterface sui;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public KinoveaMainWindow(RootKernel _RootKernel)
        {
            mRootKernel = _RootKernel;
            InitializeComponent();
            
            // Window title
            Text = " Kinovea";


            SupervisorControl = new SupervisorUserInterface(mRootKernel);
            this.Controls.Add(SupervisorControl);

            SupervisorControl.Dock = DockStyle.Fill;
            SupervisorControl.BringToFront();

            //log.Debug("Register Application Idle handler");
            //Application.Idle += new EventHandler(Application_Idle);
        }
        #endregion

        #region EventHandlers
        /*private void Application_Idle(object sender, EventArgs e)
        {
            log.Debug("Application is Idle.");
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.CompilePlayerScreen != null)
            {
                dp.CompilePlayerScreen();
            }
            Application.Idle -= new EventHandler(Application_Idle);
            log.Debug("We should be ready to get user input.");
        }*/
        private void UserInterface_FormClosing(object sender, FormClosingEventArgs e)
        {
            mRootKernel.CloseSubModules();
        }
        #endregion
    }
}