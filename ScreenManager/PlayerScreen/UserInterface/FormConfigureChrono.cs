#region License
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
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Threading;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// The dialog lets the user configure a chronometer instance.
	/// Some of the logic is the same as for formConfigureDrawing.
	/// Specifically, we work and update the actual instance in real time. 
	/// If the user finally decide to cancel there's a "fallback to memo" mechanism. 
	/// </summary>
    public partial class formConfigureChrono : Form
    {
    	#region Members
    	// Generic
    	private ResourceManager m_ResourceManager;
    	private bool m_bManualClose = false;
        
    	// Specific to the configure page.
    	private PictureBox m_SurfaceScreen; 	// Used to update the image while configuring.
    	private DrawingChrono m_Chrono;
    	private StaticColorPicker m_ColPicker;
        #endregion
        
        #region Construction & Initialization
        public formConfigureChrono(DrawingChrono _chrono, PictureBox _SurfaceScreen)
        {
            InitializeComponent();
            m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());

            m_SurfaceScreen = _SurfaceScreen;
            m_Chrono = _chrono;
            
            // Save the current state in case we need to recall it later.
            m_Chrono.MemorizeDecoration();

            // Show current values:
            cmbFontSize.Text = m_Chrono.FontSize.ToString();
            btnChronoColor.BackColor = m_Chrono.BackgroundColor;
            FixColors();
            tbLabel.Text = m_Chrono.Label;
            chkShowLabel.Checked = m_Chrono.ShowLabel;

            // Configure color picker
            m_ColPicker = new StaticColorPicker();
            m_ColPicker.MouseLeft += new StaticColorPicker.DelegateMouseLeft(ColorPicker_MouseLeft);
            m_ColPicker.ColorPicked += new StaticColorPicker.DelegateColorPicked(ColorPicker_ColorPicked);
            m_ColPicker.Visible = false;
            this.Controls.Add(m_ColPicker);
            m_ColPicker.BringToFront();

            // Localize
            this.Text = "   " + m_ResourceManager.GetString("dlgConfigureChrono_Title", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
            btnOK.Text = m_ResourceManager.GetString("Generic_Apply", Thread.CurrentThread.CurrentUICulture);
            grpConfig.Text = m_ResourceManager.GetString("Generic_Configuration", Thread.CurrentThread.CurrentUICulture);
            lblColor.Text = m_ResourceManager.GetString("Generic_ColorPicker", Thread.CurrentThread.CurrentUICulture);
            lblFontSize.Text = m_ResourceManager.GetString("Generic_FontSizePicker", Thread.CurrentThread.CurrentUICulture);
            lblLabel.Text = m_ResourceManager.GetString("dlgConfigureChrono_Label", Thread.CurrentThread.CurrentUICulture);
            chkShowLabel.Text = m_ResourceManager.GetString("dlgConfigureChrono_chkShowLabel", Thread.CurrentThread.CurrentUICulture);
        
        }
        #endregion

        #region ColorPicker Handling
        private void btnChronoColor_Click(object sender, EventArgs e)
        {
            m_ColPicker.Top = grpConfig.Top + btnChronoColor.Top;
            m_ColPicker.Left = grpConfig.Left + btnChronoColor.Left + btnChronoColor.Width - m_ColPicker.Width;
            m_ColPicker.Visible = true;
        }
        private void ColorPicker_ColorPicked(object sender, EventArgs e)
        {
            btnChronoColor.BackColor = m_ColPicker.PickedColor;
            m_Chrono.UpdateDecoration(m_ColPicker.PickedColor);
            FixColors();
            m_ColPicker.Visible = false;

            m_SurfaceScreen.Invalidate();
        }
        private void ColorPicker_MouseLeft(object sender, EventArgs e)
        {
            m_ColPicker.Visible = false;
        }
        private void FixColors()
        {
            // Fix the color of the button.
            btnChronoColor.FlatAppearance.MouseOverBackColor = btnChronoColor.BackColor;

            // Put a black frame around white rectangles.
            if (Color.Equals(btnChronoColor.BackColor, Color.FromArgb(255, 255, 255)) || Color.Equals(btnChronoColor.BackColor, Color.White))
            {
                btnChronoColor.FlatAppearance.BorderSize = 1;
            }
            else
            {
                btnChronoColor.FlatAppearance.BorderSize = 0;
            }
        }
        #endregion

        #region m_FontSize & Label Handling
        private void cmbFontSize_SelectedValueChanged(object sender, EventArgs e)
        {
        	m_Chrono.UpdateDecoration(int.Parse((string)cmbFontSize.Items[cmbFontSize.SelectedIndex]));
            m_SurfaceScreen.Invalidate();
        }
        private void tbLabel_TextChanged(object sender, EventArgs e)
        {
            m_Chrono.Label = tbLabel.Text;
            m_SurfaceScreen.Invalidate();
        }
        private void chkShowLabel_CheckedChanged(object sender, EventArgs e)
        {
            m_Chrono.ShowLabel = chkShowLabel.Checked;
            m_SurfaceScreen.Invalidate();
        }
        #endregion

        #region OK/Cancel Handlers
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Nothing more to do. The chrono has been updated already.
            m_bManualClose = true;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
        	// Fall back to memorized decoration.
        	m_Chrono.RecallDecoration();
        	m_SurfaceScreen.Invalidate();
            m_bManualClose = true;
        }
        private void formConfigureChrono_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!m_bManualClose) 
            {
            	m_Chrono.RecallDecoration();
            	m_SurfaceScreen.Invalidate();
            }
        }
        #endregion 
    }
}