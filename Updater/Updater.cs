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

using Kinovea.Updater.Languages;
using System;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.Updater
{
    public class UpdaterKernel : IKernel 
    {
        #region Members
        private ToolStripMenuItem mnuCheckForUpdates = new ToolStripMenuItem();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region IKernel Implementation
        public void BuildSubTree()
        {
            // No sub modules.
        }
        public void ExtendMenu(ToolStrip _menu)
        {
            //Catch Options Menu (5)  
            ToolStripMenuItem mnuCatchOptions = new ToolStripMenuItem();            
            mnuCatchOptions.MergeIndex = 5;
            mnuCatchOptions.MergeAction = MergeAction.MatchOnly;

            // sep    
            ToolStripSeparator mnuSep = new ToolStripSeparator();
            mnuSep.MergeIndex = 2;
            mnuSep.MergeAction = MergeAction.Insert;

            //Update
            mnuCheckForUpdates.Image = Properties.Resources.software_update;
            mnuCheckForUpdates.Click += new EventHandler(mnuCheckForUpdatesOnClick);

            mnuCheckForUpdates.MergeIndex = 3;
            mnuCheckForUpdates.MergeAction = MergeAction.Insert;

            mnuCatchOptions.DropDownItems.AddRange(new ToolStripItem[] { mnuSep, mnuCheckForUpdates });

            MenuStrip ThisMenu = new MenuStrip();
            ThisMenu.Items.AddRange(new ToolStripItem[] { mnuCatchOptions });
            ThisMenu.AllowMerge = true;

            ToolStripManager.Merge(ThisMenu, _menu);

            RefreshUICulture();
        }
        public void ExtendToolBar(ToolStrip _toolbar)
        {
            // Nothing at this level.
            // No sub modules.
        }
        public void ExtendStatusBar(ToolStrip _statusbar)
        {
            // Nothing at this level.
            // No sub modules.
        }
        public void ExtendUI()
        {
            // No sub modules.
        }
        public void RefreshUICulture()
        {
            mnuCheckForUpdates.Text = UpdaterLang.mnuCheckForUpdates;
        }
        public bool CloseSubModules()
        {
            // No sub modules to close.
            // Nothing more to do here.
            return false;
        }
        #endregion

        #region Menu Event Handlers
        private void mnuCheckForUpdatesOnClick(object sender, EventArgs e)
        {
        	// Stop playing if needed.
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.StopPlaying != null)
                dp.StopPlaying();

            // Download the update configuration file from the webserver.
            HelpIndex hiRemote = new HelpIndex(Software.RemoteHelpIndex);
			
            if (hiRemote.LoadSuccess)
            {
                if (dp.DeactivateKeyboardHandler != null)
                {
                    dp.DeactivateKeyboardHandler();
                }

                // Check if we are up to date.
                bool testUpdate = false;
                //testUpdate = true;
                ThreePartsVersion currentVersion = new ThreePartsVersion(Software.Version);
                if (hiRemote.AppInfos.Version > currentVersion || testUpdate)
            	{
                	// We are not up to date, display the full dialog.
                	// The dialog is responsible for displaying the download success msg box.
	                UpdateDialog2 ud = new UpdateDialog2(hiRemote);
	                ud.ShowDialog();
	               	ud.Dispose();
                }
                else
                {
                	// We are up to date, display a simple confirmation box.
                	MessageBox.Show(UpdaterLang.Updater_UpToDate, UpdaterLang.Updater_Title, 
                	                MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
	            }
                
                if (dp.ActivateKeyboardHandler != null)
                {
                    dp.ActivateKeyboardHandler();
                }
            }
            else
            {
                // Remote connection failed, we are probably firewalled.
                MessageBox.Show(UpdaterLang.Updater_InternetError, UpdaterLang.Updater_Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion

    }
}
