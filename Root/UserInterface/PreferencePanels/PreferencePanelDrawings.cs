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
        private Color m_GridColor;
        private Color m_PerspectiveGridColor;
        private InfosFading m_DefaultFading;
        private bool m_bDrawOnPlay;
        
        private PreferencesManager m_prefManager;
		#endregion
		
		#region Construction & Initialization
		public PreferencePanelDrawings()
		{
			InitializeComponent();
			this.BackColor = Color.White;
			
			m_prefManager = PreferencesManager.Instance();
			
			m_Description = RootLang.dlgPreferences_btnDrawings;
			m_Icon = Resources.drawings;
			
			ImportPreferences();
			InitPage();
		}
		private void ImportPreferences()
        {
			m_bDrawOnPlay = m_prefManager.DrawOnPlay;
			m_GridColor = m_prefManager.GridColor;
            m_PerspectiveGridColor = m_prefManager.Plane3DColor;
            m_DefaultFading = new InfosFading(0, 0);
		}
		private void InitPage()
		{
			tabGeneral.Text = RootLang.dlgPreferences_ButtonGeneral;
			chkDrawOnPlay.Text = RootLang.dlgPreferences_chkDrawOnPlay;
			grpColors.Text = RootLang.dlgPreferences_GroupColors;
            lblGrid.Text = RootLang.dlgPreferences_LabelGrid;
            lblPlane3D.Text = RootLang.dlgPreferences_LabelPlane3D;	
            
            tabPersistence.Text = RootLang.dlgPreferences_grpPersistence;
            chkEnablePersistence.Text = RootLang.dlgPreferences_chkEnablePersistence;
			chkAlwaysVisible.Text = RootLang.dlgPreferences_chkAlwaysVisible;
			
			chkDrawOnPlay.Checked = m_bDrawOnPlay;
			btnGridColor.BackColor = m_GridColor;
            btn3DPlaneColor.BackColor = m_PerspectiveGridColor;
            FixColors();
            chkEnablePersistence.Checked = m_DefaultFading.Enabled;
            trkFading.Maximum = m_prefManager.MaxFading;
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
		private void btnGridColor_Click(object sender, EventArgs e)
        {
        	FormColorPicker picker = new FormColorPicker();
        	if(picker.ShowDialog() == DialogResult.OK)
        	{
        		btnGridColor.BackColor = picker.PickedColor;
                m_GridColor = picker.PickedColor;
                FixColors();
        	}
        	picker.Dispose();
        }
        private void btn3DPlaneColor_Click(object sender, EventArgs e)
        {
        	FormColorPicker picker = new FormColorPicker();
        	if(picker.ShowDialog() == DialogResult.OK)
        	{
        		btn3DPlaneColor.BackColor = picker.PickedColor;
                m_PerspectiveGridColor = picker.PickedColor;
                FixColors();
        	}
        	picker.Dispose();
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
		
		private void FixColors()
        {
            // Put a black frame around white rectangles.
            // set the mouse over color to the same color.

            btnGridColor.FlatAppearance.MouseOverBackColor = btnGridColor.BackColor;
            if (Color.Equals(btnGridColor.BackColor, Color.FromArgb(255, 255, 255)) || Color.Equals(btnGridColor.BackColor, Color.White))
            {
                btnGridColor.FlatAppearance.BorderSize = 1;
            }
            else
            {
                btnGridColor.FlatAppearance.BorderSize = 0;
            }

            btn3DPlaneColor.FlatAppearance.MouseOverBackColor = btn3DPlaneColor.BackColor;
            if (Color.Equals(btn3DPlaneColor.BackColor, Color.FromArgb(255, 255, 255)) || Color.Equals(btn3DPlaneColor.BackColor, Color.White))
            {
                btn3DPlaneColor.FlatAppearance.BorderSize = 1;
            }
            else
            {
                btn3DPlaneColor.FlatAppearance.BorderSize = 0;
            }

        }
		private void EnableDisableFadingOptions()
        {
            trkFading.Enabled = chkEnablePersistence.Checked;
            lblFading.Enabled = chkEnablePersistence.Checked;
            chkAlwaysVisible.Enabled = chkEnablePersistence.Checked;
        }
	
		public void CommitChanges()
		{
			m_prefManager.DrawOnPlay = m_bDrawOnPlay;
			m_prefManager.GridColor = m_GridColor;
            m_prefManager.Plane3DColor = m_PerspectiveGridColor;
			m_prefManager.DefaultFading.FromInfosFading(m_DefaultFading);
		}
	}
}
