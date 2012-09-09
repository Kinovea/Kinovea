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
	/// PreferencePanelPlayer.
	/// </summary>
	public partial class PreferencePanelPlayer : UserControl, IPreferencePanel
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
        private bool m_bDeinterlaceByDefault;
        private int m_iWorkingZoneSeconds;
        private int m_iWorkingZoneMemory;
		#endregion
		
		#region Construction & Initialization
		public PreferencePanelPlayer()
		{
			InitializeComponent();
			this.BackColor = Color.White;
			
			m_Description = RootLang.dlgPreferences_ButtonPlayAnalyze;
			m_Icon = Resources.video;
			
			ImportPreferences();
			InitPage();
		}
		private void ImportPreferences()
        {
 			m_bDeinterlaceByDefault = PreferencesManager.PlayerPreferences.DeinterlaceByDefault;
 			m_iWorkingZoneSeconds = PreferencesManager.PlayerPreferences.WorkingZoneSeconds;
 			m_iWorkingZoneMemory = PreferencesManager.PlayerPreferences.WorkingZoneMemory;
		}
		private void InitPage()
		{
            chkDeinterlace.Text = RootLang.dlgPreferences_DeinterlaceByDefault;   
            grpSwitchToAnalysis.Text = RootLang.dlgPreferences_GroupAnalysisMode;
            lblWorkingZoneLogic.Text = RootLang.dlgPreferences_LabelLogic;
            
            // Fill in initial values.            
            chkDeinterlace.Checked = m_bDeinterlaceByDefault;
            trkWorkingZoneSeconds.Value = m_iWorkingZoneSeconds;
            lblWorkingZoneSeconds.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneSeconds, trkWorkingZoneSeconds.Value);
            trkWorkingZoneMemory.Value = m_iWorkingZoneMemory;
            lblWorkingZoneMemory.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneMemory, trkWorkingZoneMemory.Value);
		}
		#endregion
		
		#region Handlers
        private void ChkDeinterlaceCheckedChanged(object sender, EventArgs e)
        {
        	m_bDeinterlaceByDefault = chkDeinterlace.Checked;
        }
        private void trkWorkingZoneSeconds_ValueChanged(object sender, EventArgs e)
        {
            lblWorkingZoneSeconds.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneSeconds, trkWorkingZoneSeconds.Value);
            m_iWorkingZoneSeconds = trkWorkingZoneSeconds.Value;
        }
        private void trkWorkingZoneMemory_ValueChanged(object sender, EventArgs e)
        {
            lblWorkingZoneMemory.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneMemory, trkWorkingZoneMemory.Value);
            m_iWorkingZoneMemory = trkWorkingZoneMemory.Value;
        }
		#endregion
		
		public void CommitChanges()
		{
            PreferencesManager.PlayerPreferences.DeinterlaceByDefault = m_bDeinterlaceByDefault;
            PreferencesManager.PlayerPreferences.WorkingZoneSeconds = m_iWorkingZoneSeconds;
            PreferencesManager.PlayerPreferences.WorkingZoneMemory = m_iWorkingZoneMemory;
		}
	}
}
