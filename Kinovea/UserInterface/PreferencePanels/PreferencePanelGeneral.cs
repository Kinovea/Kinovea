#region License
/*
Copyright © Joan Charmant 2011.
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
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using Kinovea.Root.Languages;
using Kinovea.Root.Properties;
using Kinovea.Services;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using log4net;

namespace Kinovea.Root
{
    /// <summary>
    /// PreferencePanelGeneral.
    /// </summary>
    public partial class PreferencePanelGeneral : UserControl, IPreferencePanel
    {
        #region IPreferencePanel properties
        public string Description
        {
            get { return description;}
        }
        public Bitmap Icon
        {
            get { return icon;}
        }
        public List<PreferenceTab> Tabs
        {
            get { return tabs; }
        }
        #endregion
        
        #region Members
        private string description;
        private Bitmap icon;
        private List<PreferenceTab> tabs = new List<PreferenceTab> { PreferenceTab.General_General };
        private string uiCultureName;
        private int maxRecentFiles;
        private bool enableDebugLogs;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction & Initialization
        public PreferencePanelGeneral()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            
            description = RootLang.dlgPreferences_tabGeneral;
            icon = Resources.tools_30;
            
            ImportPreferences();
            InitPage();
        }

        public void OpenTab(PreferenceTab tab)
        {
        }

        public void Close()
        {
        }

        private void ImportPreferences()
        {
            uiCultureName = LanguageManager.GetCurrentCultureName();
            maxRecentFiles = PreferencesManager.FileExplorerPreferences.MaxRecentFiles;
            enableDebugLogs = PreferencesManager.GeneralPreferences.EnableDebugLog;
        }
        private void InitPage()
        {
            // Localize and fill possible values
            
            lblLanguage.Text = RootLang.dlgPreferences_Player_lblLanguages;
            cmbLanguage.Items.Clear();
            foreach(KeyValuePair<string, string> lang in LanguageManager.Languages)
            {
                cmbLanguage.Items.Add(new LanguageIdentifier(lang.Key, lang.Value));
            }
            
            lblHistoryCount.Text = RootLang.dlgPreferences_General_lblHistoryCount;

            SelectCurrentLanguage();
            cmbHistoryCount.SelectedIndex = maxRecentFiles;

            cbEnableDebugLogs.Text = "Enable debug logs";
            cbEnableDebugLogs.Checked = enableDebugLogs;
        }
        private void SelectCurrentLanguage()
        {
            bool found = false;
            for(int i=0;i<cmbLanguage.Items.Count;i++)
            {
                LanguageIdentifier li = (LanguageIdentifier)cmbLanguage.Items[i];
                
                if (li.Culture.Equals(uiCultureName))
                {
                    // Matching
                    cmbLanguage.SelectedIndex = i;            
                    found = true;
                }
            }

            if(!found)
                cmbLanguage.SelectedIndex = 0;   
        }
        #endregion
        
        #region Handlers
        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            uiCultureName = ((LanguageIdentifier)cmbLanguage.Items[cmbLanguage.SelectedIndex]).Culture;
        }
        private void cmbHistoryCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            maxRecentFiles = cmbHistoryCount.SelectedIndex;
        }
        private void ChkEnableDebugLog_CheckedChanged(object sender, EventArgs e)
        {
            enableDebugLogs = cbEnableDebugLogs.Checked;

            // Immediately change the log level.
            Software.UpdateLogLevel(enableDebugLogs);
        }
        #endregion

        public void CommitChanges()
        {
            PreferencesManager.GeneralPreferences.SetCulture(uiCultureName);
            PreferencesManager.FileExplorerPreferences.MaxRecentFiles = maxRecentFiles;
            PreferencesManager.GeneralPreferences.EnableDebugLog = enableDebugLogs;
        }
    }
}
