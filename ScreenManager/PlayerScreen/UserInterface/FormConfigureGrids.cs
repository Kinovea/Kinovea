#region License
/*
Copyright © Joan Charmant 2009.
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
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// The dialog let the user configure an instance of a grid.
	/// We work with the actual grid to update it in real time.
	/// If the user chooses to cancel, we fall back to a memo.
	/// 
	/// The main difference with the formConfigureDrawings or formConfigureChrono
	/// is that we have to update the preferences xml file along the way.
	/// </summary>
    public partial class formConfigureGrids : Form
    {
    	#region Members
    	private bool m_bManualClose = false;
        
        private PictureBox m_SurfaceScreen; // Used to update the image while configuring.
        private Plane3D m_Grid;
        private Plane3D m_MemoGrid;
        private ColorPicker m_ColorPicker = new ColorPicker();
		#endregion
        
		#region Construction and Initialization
        public formConfigureGrids(Plane3D _grid, PictureBox _SurfaceScreen)
        {
            InitializeComponent();
         	
            m_ColorPicker.Top = 18;
			m_ColorPicker.Left = 9;
			m_ColorPicker.ColorPicked += new ColorPickedHandler(colorPicker_ColorPicked);
			grpConfig.Controls.Add(m_ColorPicker);
			
            m_SurfaceScreen = _SurfaceScreen;
            
            m_Grid = _grid;
            m_MemoGrid = new Plane3D(0, m_Grid.Divisions, m_Grid.Support3D);
            m_MemoGrid.GridColor = m_Grid.GridColor;

            // Show current values:
            cmbDivisions.Text = m_Grid.Divisions.ToString();

            // Localize
            this.Text = "   " + ScreenManagerLang.dlgConfigureGrids_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            lblDivisions.Text = ScreenManagerLang.dlgConfigureGrids_Divisions;
        }
        #endregion
        
        #region User choice handlers
        private void colorPicker_ColorPicked(object sender, EventArgs e)
        {
            m_Grid.GridColor = m_ColorPicker.PickedColor;

            // We must change the color in the preferences aswell.
            PreferencesManager pm = PreferencesManager.Instance();
            if (m_Grid.Support3D)
            {
                pm.Plane3DColor = m_ColorPicker.PickedColor;
            }
            else
            {
                pm.GridColor = m_ColorPicker.PickedColor;
            }
            PreferencesManager.Instance().AddRecentColor(m_ColorPicker.PickedColor);
            m_SurfaceScreen.Invalidate();
        }
        private void cmbDivisions_SelectedValueChanged(object sender, EventArgs e)
        {
            m_Grid.Divisions = int.Parse((string)cmbDivisions.Items[cmbDivisions.SelectedIndex]);
            m_SurfaceScreen.Invalidate();
        }
        #endregion
        
        #region OK/Cancel handlers
        private void btnOK_Click(object sender, EventArgs e)
        {
            // OK. object has been updated already.
            PreferencesManager pm = PreferencesManager.Instance();
            pm.Export();
            m_bManualClose = true;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            FallBack();
            m_bManualClose = true;
        }
        private void formConfigureGrids_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!m_bManualClose)
            {
                FallBack();
            }
        }
        private void FallBack()
        {
            // Fall back to memo.
            m_Grid.Divisions = m_MemoGrid.Divisions;
            m_Grid.GridColor = m_MemoGrid.GridColor;

            PreferencesManager pm = PreferencesManager.Instance();
            if (m_Grid.Support3D)
            {
                pm.Plane3DColor = m_MemoGrid.GridColor;
            }
            else
            {
                pm.GridColor = m_MemoGrid.GridColor;
            }
        }
        #endregion
        
    }
}