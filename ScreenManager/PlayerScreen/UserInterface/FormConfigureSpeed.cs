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
using System.Resources;
using System.Threading;
using System.Reflection;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// This dialog let the user specify the original speed of the camera used.
	/// This is used when the camera was filming at say, 1000 fps, 
	/// and the resulting movie created at 24 fps. 
	/// 
	/// The result of this dialog is only a change in the way we compute the times.
	/// The value stored in the PlayerScreen UI is not updated in real time. 
	/// </summary>
    public partial class formConfigureSpeed : Form
    {
    	#region Members
        private ResourceManager m_ResourceManager;
        private PlayerScreenUserInterface m_psui;   	// Just to access the SlowFactor property.
        
        private double m_fFps;						// This is the fps read in the video. (ex: 24 fps)
        private double m_fSlowFactor;					// The current slow factor. (if we already used the dialog)
        private double m_fOriginalFps;				// The current fps modified value (ex: 1000 fps).
        #endregion
        
        #region Construction & Initialization
        public formConfigureSpeed(double _fFps, PlayerScreenUserInterface _psui)
        {
            InitializeComponent();
            m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            m_psui = _psui;
            m_fSlowFactor = m_psui.SlowFactor;
            m_fFps = _fFps;
            m_fOriginalFps = m_fFps * m_fSlowFactor;
            
            LocalizeForm();
        }
        private void LocalizeForm()
        {
            this.Text = "   " + m_ResourceManager.GetString("dlgConfigureSpeed_Title", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
            btnOK.Text = m_ResourceManager.GetString("Generic_Apply", Thread.CurrentThread.CurrentUICulture);
            grpConfig.Text = m_ResourceManager.GetString("Generic_Configuration", Thread.CurrentThread.CurrentUICulture);
            lblFPSCaptureTime.Text = m_ResourceManager.GetString("dlgConfigureSpeed_lblFPSCaptureTime", Thread.CurrentThread.CurrentUICulture).Replace("\\n", "\n");
            toolTips.SetToolTip(btnReset, m_ResourceManager.GetString("dlgConfigureSpeed_ToolTip_Reset", Thread.CurrentThread.CurrentUICulture));
            
            // Update text box with current value. (Will update computed values too)
            tbFPSOriginal.Text = String.Format("{0:0.00}", m_fOriginalFps);
        }
        #endregion

        #region User choices handlers
        private void UpdateValues()
        {
            lblFPSDisplayTime.Text = String.Format(m_ResourceManager.GetString("dlgConfigureSpeed_lblFPSDisplayTime", Thread.CurrentThread.CurrentUICulture), m_fFps);
            int timesSlower = (int)(m_fOriginalFps / m_fFps);
            lblSlowFactor.Visible = timesSlower > 1;
            lblSlowFactor.Text = String.Format(m_ResourceManager.GetString("dlgConfigureSpeed_lblSlowFactor", Thread.CurrentThread.CurrentUICulture), timesSlower);
        }
        private void tbFPSOriginal_TextChanged(object sender, EventArgs e)
        {
            try
            {
            	// FIXME: check how this play with culture variations on decimal separator.
                m_fOriginalFps = double.Parse(tbFPSOriginal.Text);
                if (m_fOriginalFps > 2000)
                {
                    tbFPSOriginal.Text = "2000";
                }
                else if (m_fOriginalFps < 1)
                {
                    m_fOriginalFps = m_fFps;
                }
            }
            catch
            {
                // Failed : do nothing. 
            }

            UpdateValues();
        }
        private void tbFPSOriginal_KeyPress(object sender, KeyPressEventArgs e)
        {
        	// We only accept numbers, points and coma in there.
            char key = e.KeyChar;
            if (((key < '0') || (key > '9')) && (key != ',') && (key != '.') && (key != '\b'))
            {
                e.Handled = true;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // Fall back To original.
            tbFPSOriginal.Text = String.Format("{0:0.00}", m_fFps);
        }
        #endregion
        
        #region OK/Cancel Handlers
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Save value.
            if (m_fOriginalFps < 1)
            {
                // Fall back to original.
                m_psui.SlowFactor = m_fFps;
            }
            else
            {
                m_psui.SlowFactor = m_fOriginalFps / m_fFps;
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
             // Nothing more to do.           
        }
        #endregion

    }
}