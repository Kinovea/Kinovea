#region License
/*
Copyright © Joan Charmant 2011.
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
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using Kinovea.Root.Languages;
using Kinovea.Root.Properties;
using Kinovea.ScreenManager;
using Kinovea.Services;
using Kinovea.Video;

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
		#endregion
		
		#region Members
		private string description;
		private Bitmap icon;
		private string uiCultureName;
		private int maxRecentFiles;
        #endregion
		
		#region Construction & Initialization
		public PreferencePanelGeneral()
		{
			InitializeComponent();
			this.BackColor = Color.White;
			
			description = RootLang.dlgPreferences_ButtonGeneral;
			icon = Resources.pref_general;
			
			ImportPreferences();
			InitPage();
		}
		private void ImportPreferences()
        {
            CultureInfo ci = PreferencesManager.GeneralPreferences.GetSupportedCulture();
            uiCultureName = ci.IsNeutralCulture ? ci.Name : ci.Parent.Name;
            maxRecentFiles = PreferencesManager.FileExplorerPreferences.MaxRecentFiles;
        }
		private void InitPage()
		{
			// Localize and fill possible values
			
			lblLanguage.Text = RootLang.dlgPreferences_LabelLanguages;
            cmbLanguage.Items.Clear();
            foreach(KeyValuePair<string, string> lang in LanguageManager.Languages)
            {
                cmbLanguage.Items.Add(new LanguageIdentifier(lang.Key, lang.Value));
            }
            
            lblHistoryCount.Text = RootLang.dlgPreferences_LabelHistoryCount;

            
            // Fill current values
            SelectCurrentLanguage();
            cmbHistoryCount.SelectedIndex = maxRecentFiles;
            
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
            {
                // The supported language is not in the combo box. (error).
                cmbLanguage.SelectedIndex = 0;   
            }
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
		#endregion
		
		public void CommitChanges()
		{
		    PreferencesManager.GeneralPreferences.SetCulture(uiCultureName);
			PreferencesManager.FileExplorerPreferences.MaxRecentFiles = maxRecentFiles;
		}
	}
}
