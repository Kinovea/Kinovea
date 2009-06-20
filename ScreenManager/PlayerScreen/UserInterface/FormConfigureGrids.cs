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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Kinovea.Services;
using System.Resources;
using System.Reflection;
using System.Threading;

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
    	private ResourceManager m_ResourceManager;
        private bool m_bManualClose = false;
        
        private PictureBox m_SurfaceScreen; // Used to update the image while configuring.
        private Plane3D m_Grid;
        private Plane3D m_MemoGrid;
        private StaticColorPicker colPicker;
		#endregion
        
		#region Construction and Initialization
        public formConfigureGrids(Plane3D _grid, PictureBox _SurfaceScreen)
        {
            InitializeComponent();
            
            // Custom Controls - moved here for #Develop designer.
            colPicker = new StaticColorPicker();
            grpConfig.Controls.Add(this.colPicker);
            colPicker.BackColor = System.Drawing.Color.WhiteSmoke;
            colPicker.Location = new System.Drawing.Point(22, 23);
            colPicker.Name = "colPicker";
            colPicker.Size = new System.Drawing.Size(160, 120);
            colPicker.TabIndex = 5;
            colPicker.ColorPicked += new Kinovea.ScreenManager.StaticColorPicker.DelegateColorPicked(this.colPicker_ColorPicked);
            
            m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
			m_SurfaceScreen = _SurfaceScreen;
            
            m_Grid = _grid;
            m_MemoGrid = new Plane3D(0, m_Grid.Divisions, m_Grid.Support3D);
            m_MemoGrid.GridColor = m_Grid.GridColor;

            // Show current values:
            cmbDivisions.Text = m_Grid.Divisions.ToString();

            // Localize
            this.Text = "   " + m_ResourceManager.GetString("dlgConfigureGrids_Title", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
            btnOK.Text = m_ResourceManager.GetString("Generic_Apply", Thread.CurrentThread.CurrentUICulture);
            grpConfig.Text = m_ResourceManager.GetString("Generic_Configuration", Thread.CurrentThread.CurrentUICulture);
            lblDivisions.Text = m_ResourceManager.GetString("dlgConfigureGrids_Divisions", Thread.CurrentThread.CurrentUICulture);
        }
        #endregion
        
        #region User choice handlers
        private void colPicker_ColorPicked(object sender, EventArgs e)
        {
            m_Grid.GridColor = colPicker.PickedColor;

            // We must change the color in the preferences aswell.
            PreferencesManager pm = PreferencesManager.Instance();
            if (m_Grid.Support3D)
            {
                pm.Plane3DColor = colPicker.PickedColor;
            }
            else
            {
                pm.GridColor = colPicker.PickedColor;
            }
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