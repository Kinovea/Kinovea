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
using Kinovea.Services;

namespace Kinovea.Root
{
	/// <summary>
	/// PreferencePanelCapture.
	/// </summary>
	public partial class PreferencePanelCapture : UserControl, IPreferencePanel
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
		private PreferencesManager m_prefManager;
		#endregion
		
		#region Construction & Initialization
		public PreferencePanelCapture()
		{
			InitializeComponent();
			this.BackColor = Color.White;
			
			m_prefManager = PreferencesManager.Instance();
			
			m_Description = "Capture"; // RootLang.dlgPreferences_btnDrawings; FIXME
			m_Icon = Resources.pref_capture;
			
			ImportPreferences();
			InitPage();
		}
		private void ImportPreferences()
        {
			
		}
		private void InitPage()
		{
			tabGeneral.Text = RootLang.dlgPreferences_ButtonGeneral;	
		}
		#endregion
		
		#region Handlers
		#endregion
		
		public void CommitChanges()
		{
			
		}
	}
}
