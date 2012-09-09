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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Kinovea.Root.Languages;
using Kinovea.Root.Properties;
using Kinovea.ScreenManager;
using Kinovea.Services;

namespace Kinovea.Root
{
	/// <summary>
	/// Description of PreferencePanelDrawings.
	/// </summary>
	public partial class PreferencePanelDrawings : UserControl, IPreferencePanel
	{
		#region IPreferencePanel properties
		public string Description
		{
			get { return m_Description;}
		}
		public Bitmap Icon
		{
			get { return m_Icon;}
		}
		private string m_Description;
		private Bitmap m_Icon;
		#endregion
		
		#region Members
        private InfosFading m_DefaultFading;
        private bool m_bDrawOnPlay;
		#endregion
		
		#region Construction & Initialization
		public PreferencePanelDrawings()
		{
			InitializeComponent();
			this.BackColor = Color.White;
			
			m_Description = RootLang.dlgPreferences_btnDrawings;
			m_Icon = Resources.drawings;
			
			ImportPreferences();
			InitPage();
		}
		private void ImportPreferences()
        {
			m_bDrawOnPlay = PreferencesManager.PlayerPreferences.DrawOnPlay;
            m_DefaultFading = new InfosFading(0, 0);
		}
		private void InitPage()
		{
			tabGeneral.Text = RootLang.dlgPreferences_ButtonGeneral;
			chkDrawOnPlay.Text = RootLang.dlgPreferences_chkDrawOnPlay;
			
			tabPersistence.Text = RootLang.dlgPreferences_grpPersistence;
            chkEnablePersistence.Text = RootLang.dlgPreferences_chkEnablePersistence;
			chkAlwaysVisible.Text = RootLang.dlgPreferences_chkAlwaysVisible;
			
			chkDrawOnPlay.Checked = m_bDrawOnPlay;
			chkEnablePersistence.Checked = m_DefaultFading.Enabled;
            trkFading.Maximum = PreferencesManager.PlayerPreferences.MaxFading;
            trkFading.Value = Math.Min(m_DefaultFading.FadingFrames, trkFading.Maximum);
            chkAlwaysVisible.Checked = m_DefaultFading.AlwaysVisible;
            EnableDisableFadingOptions();
            lblFading.Text = String.Format(RootLang.dlgPreferences_lblFading, trkFading.Value);
		}
		#endregion
		
		#region Handlers
		#region General
		private void chkDrawOnPlay_CheckedChanged(object sender, EventArgs e)
        {
            m_bDrawOnPlay = chkDrawOnPlay.Checked;
        }
		#endregion
		
		#region Persistence
		private void chkFading_CheckedChanged(object sender, EventArgs e)
        {
            m_DefaultFading.Enabled = chkEnablePersistence.Checked;
            EnableDisableFadingOptions();
        }
        private void trkFading_ValueChanged(object sender, EventArgs e)
        {
            lblFading.Text = String.Format(RootLang.dlgPreferences_lblFading, trkFading.Value);
            m_DefaultFading.FadingFrames = trkFading.Value;
            chkAlwaysVisible.Checked = false;
        }
        private void chkAlwaysVisible_CheckedChanged(object sender, EventArgs e)
        {
        	m_DefaultFading.AlwaysVisible = chkAlwaysVisible.Checked;	
        }
        #endregion
		#endregion
		
		private void EnableDisableFadingOptions()
        {
            trkFading.Enabled = chkEnablePersistence.Checked;
            lblFading.Enabled = chkEnablePersistence.Checked;
            chkAlwaysVisible.Enabled = chkEnablePersistence.Checked;
        }
	
		public void CommitChanges()
		{
			PreferencesManager.PlayerPreferences.DrawOnPlay = m_bDrawOnPlay;
			PreferencesManager.PlayerPreferences.DefaultFading.FromInfosFading(m_DefaultFading);
		}
	}
}
