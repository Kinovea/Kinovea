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
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Reflection;
using System.Drawing;
using System.Xml;
using System.Collections.Specialized;
using System.Configuration;
using Kinovea.Services;


[assembly: CLSCompliant(true)]
namespace Kinovea.Updater
{
    public class UpdaterKernel : IKernel 
    {
        #region Properties
        public ResourceManager resManager
        {
            get { return _resManager; }
            set { _resManager = value; }
        }
        #endregion

        #region Members
        private ResourceManager _resManager;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public UpdaterKernel()
        {
            log.Debug("Module Construction : Updater.");
            resManager = new ResourceManager("Kinovea.Updater.Languages.UpdaterLang", Assembly.GetExecutingAssembly());
        }
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
            mnuSep.MergeIndex = 1;
            mnuSep.MergeAction = MergeAction.Insert;

            //Update
            ToolStripMenuItem mnuCheckForUpdates = new ToolStripMenuItem();
            mnuCheckForUpdates.Tag = new ItemResourceInfo(resManager, "mnuCheckForUpdates");
            mnuCheckForUpdates.Text = ((ItemResourceInfo)mnuCheckForUpdates.Tag).resManager.GetString(((ItemResourceInfo)mnuCheckForUpdates.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuCheckForUpdates.Click += new EventHandler(mnuCheckForUpdatesOnClick);

            mnuCheckForUpdates.MergeIndex = 2;
            mnuCheckForUpdates.MergeAction = MergeAction.Insert;


            mnuCatchOptions.DropDownItems.AddRange(new ToolStripItem[] { mnuSep, mnuCheckForUpdates });

            MenuStrip ThisMenu = new MenuStrip();
            ThisMenu.Items.AddRange(new ToolStripItem[] { mnuCatchOptions });
            ThisMenu.AllowMerge = true;

            ToolStripManager.Merge(ThisMenu, _menu);

            // No sub modules.
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
            // Nothing at this level.
            // No sub modules.
        }
        public void CloseSubModules()
        {
            // No sub modules to close.
            // Nothing more to do here.
        }
        #endregion

        #region Menu Event Handlers
        private void mnuCheckForUpdatesOnClick(object sender, EventArgs e)
        {
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.StopPlaying != null)
            {
                dp.StopPlaying();
            }

            // Force update channel to standard.
            // There is also an update channel for experimental builds, but it is not used for now.
            int iUpdateChannel = 0;

            //------------------------------------------------
            // Lecture des configuration locales et distantes.
            //------------------------------------------------
            PreferencesManager pm = PreferencesManager.Instance();
            ResourceManager SharedResources = PreferencesManager.ResourceManager;

            HelpIndex hiLocal = new HelpIndex(Application.StartupPath + "\\" + SharedResources.GetString("URILocalHelpIndex"));
            HelpIndex hiRemote;
            switch (iUpdateChannel)
            {
                case 0:
                    hiRemote = new HelpIndex(SharedResources.GetString("URIRemoteHelpIndex"));
                    break;
                case 1:
                    hiRemote = new HelpIndex(SharedResources.GetString("URIRemoteHelpIndexBeta"));
                    break;
                default:
                    hiRemote = new HelpIndex(SharedResources.GetString("URIRemoteHelpIndex"));
                    break;
            }
            
            if (hiRemote.LoadSuccess && hiLocal.LoadSuccess)
            {
                if (dp.DeactivateKeyboardHandler != null)
                {
                    dp.DeactivateKeyboardHandler();
                }

                UpdateDialog ud = new UpdateDialog(resManager, hiLocal, hiRemote);
                ud.ShowDialog();
                ud.Dispose();

                if (dp.ActivateKeyboardHandler != null)
                {
                    dp.ActivateKeyboardHandler();
                }
            }
            else if (!hiRemote.LoadSuccess)
            {
                // MessageBox demandant de vérifier la connexion à internet.
                // Cause possible : FIREWALL.
                MessageBox.Show(resManager.GetString("Updater_InternetError", Thread.CurrentThread.CurrentUICulture),
                                resManager.GetString("Updater_Title", Thread.CurrentThread.CurrentUICulture),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
            }
            else
            {
                // TODO: Local file is missing, rebuild it ?
                log.Error("Cannot load local help index.");
            }
        }
        #endregion

    }
}
