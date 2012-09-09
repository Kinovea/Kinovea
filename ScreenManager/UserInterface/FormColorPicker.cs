#region License
/*
Copyright © Joan Charmant 2010.
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
using Kinovea.ScreenManager.Languages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// FormColorPicker. Let the user choose a color.
	/// The color picker control itself is based on the one from Greenshot. 
	/// </summary>
	public partial class FormColorPicker : Form
	{
		#region Properties
		public Color PickedColor
        {
            get { return m_PickedColor; }
        }
		#endregion
		
		#region Members
		private ColorPicker m_ColorPicker = new ColorPicker();
		private Color m_PickedColor;
		private List<Color> m_RecentColors;
		#endregion
		
		#region Construction and Initialization
		public FormColorPicker()
		{
			this.SuspendLayout();
			InitializeComponent();
			m_ColorPicker.Top = 5;
			m_ColorPicker.Left = 5;
			m_ColorPicker.ColorPicked += colorPicker_ColorPicked;
			
			Controls.Add(m_ColorPicker);
			this.Text = "   " + ScreenManagerLang.dlgColorPicker_Title;
			this.ResumeLayout();
			
			// Recent colors.
			m_RecentColors = PreferencesManager.PlayerPreferences.RecentColors;
			
			m_ColorPicker.DisplayRecentColors(m_RecentColors);
			this.Height = m_ColorPicker.Bottom + 20;
		}
		#endregion
		
		#region event handlers
		private void colorPicker_ColorPicked(object sender, System.EventArgs e)
		{
			m_PickedColor = m_ColorPicker.PickedColor;
			PreferencesManager.PlayerPreferences.AddRecentColor(m_ColorPicker.PickedColor);
			PreferencesManager.Save();
			DialogResult = DialogResult.OK;
			Close();
		}
		#endregion
	}
}
