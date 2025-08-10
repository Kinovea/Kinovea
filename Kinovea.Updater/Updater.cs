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

        public UpdaterKernel()
        {
            log.Debug("Module Construction: Updater.");
        }

        #region IKernel Implementation
        public void BuildSubTree()
        {
            // No sub modules.
        }
        public void ExtendMenu(ToolStrip menu)
        {
            //Catch the Help menu.  
            ToolStripMenuItem mnuCatchParent = new ToolStripMenuItem();            
            mnuCatchParent.MergeIndex = 7;
            mnuCatchParent.MergeAction = MergeAction.MatchOnly;

            mnuCheckForUpdates.Image = Properties.Resources.software_update;
            mnuCheckForUpdates.Click += new EventHandler(mnuCheckForUpdatesOnClick);

            mnuCheckForUpdates.MergeIndex = 1;
            mnuCheckForUpdates.MergeAction = MergeAction.Insert;

            mnuCatchParent.DropDownItems.AddRange(new ToolStripItem[] { mnuCheckForUpdates });

            MenuStrip ThisMenu = new MenuStrip();
            ThisMenu.Items.AddRange(new ToolStripItem[] { mnuCatchParent });
            ThisMenu.AllowMerge = true;

            ToolStripManager.Merge(ThisMenu, menu);

            RefreshUICulture();
        }
        public void ExtendToolBar(ToolStrip _toolbar) {}
        public void ExtendStatusBar(ToolStrip _statusbar) {}
        public void ExtendUI() {}

        public void RefreshUICulture()
        {
            mnuCheckForUpdates.Text = UpdaterLang.mnuCheckForUpdates;
        }
        public bool CloseSubModules()
        {
            return false;
        }
        public void PreferencesUpdated()
        {
            RefreshUICulture();
        }
        #endregion

        #region Menu Event Handlers
        private void mnuCheckForUpdatesOnClick(object sender, EventArgs e)
        {
            NotificationCenter.RaiseStopPlayback(this);

            // Download the update configuration file from the webserver.
            HelpIndex hiRemote = new HelpIndex(Software.RemoteHelpIndex);
            if (!hiRemote.LoadSuccess)
            {
                MessageBox.Show(UpdaterLang.Updater_InternetError, UpdaterLang.Updater_Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            
            ThreePartsVersion currentVersion = new ThreePartsVersion(Software.Version);
            if (hiRemote.AppInfos.Version > currentVersion)
            {
                // We are not up to date, display the full dialog.
                UpdateDialog2 ud = new UpdateDialog2(hiRemote);
                ud.ShowDialog();
                ud.Dispose();
            }
            else
            {
                MessageBox.Show(UpdaterLang.Updater_UpToDate, UpdaterLang.Updater_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

    }
}
