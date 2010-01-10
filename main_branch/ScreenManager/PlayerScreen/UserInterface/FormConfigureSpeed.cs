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
    	#region Properties
    	
    	#endregion
    	public double SlowFactor
    	{
    		get 
    		{ 
	    		if (m_fRealWorldFps < 1)
	            {
	                // Fall back to original.
	                return m_fSlowFactor;
	            }
	            else
	            {
	                return m_fRealWorldFps / m_fVideoFps;
	            }
    		}
    	}
    	#region Members
        private double m_fVideoFps;					// This is the fps read in the video. (ex: 24 fps)
        private double m_fRealWorldFps;				// The current fps modified value (ex: 1000 fps).
        private double m_fSlowFactor;					// The current slow factor. (if we already used the dialog)
        #endregion
        
        #region Construction & Initialization
        public formConfigureSpeed(double _fFps, double _fSlowFactor)
        {
            m_fSlowFactor = _fSlowFactor;
            m_fVideoFps = _fFps;
            m_fRealWorldFps = m_fVideoFps * m_fSlowFactor;
            
            InitializeComponent();
            LocalizeForm();
        }
        private void LocalizeForm()
        {
            this.Text = "   " + ScreenManagerLang.dlgConfigureSpeed_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            lblFPSCaptureTime.Text = ScreenManagerLang.dlgConfigureSpeed_lblFPSCaptureTime.Replace("\\n", "\n");
            toolTips.SetToolTip(btnReset, ScreenManagerLang.dlgConfigureSpeed_ToolTip_Reset);
            
            // Update text box with current value. (Will update computed values too)
            tbFPSRealWorld.Text = String.Format("{0:0.00}", m_fRealWorldFps);
        }
        #endregion

        #region User choices handlers
        private void UpdateValues()
        {
            lblFPSDisplayTime.Text = String.Format(ScreenManagerLang.dlgConfigureSpeed_lblFPSDisplayTime, m_fVideoFps);
            int timesSlower = (int)(m_fRealWorldFps / m_fVideoFps);
            lblSlowFactor.Visible = timesSlower > 1;
            lblSlowFactor.Text = String.Format(ScreenManagerLang.dlgConfigureSpeed_lblSlowFactor, timesSlower);
        }
        private void tbFPSRealWorld_TextChanged(object sender, EventArgs e)
        {
            try
            {
            	// FIXME: check how this play with culture variations on decimal separator.
                m_fRealWorldFps = double.Parse(tbFPSRealWorld.Text);
                if (m_fRealWorldFps > 2000)
                {
                    tbFPSRealWorld.Text = "2000";
                }
                else if (m_fRealWorldFps < 1)
                {
                    m_fRealWorldFps = m_fVideoFps;
                }
            }
            catch
            {
                // Failed : do nothing. 
            }

            UpdateValues();
        }
        private void tbFPSRealWorld_KeyPress(object sender, KeyPressEventArgs e)
        {
        	// We only accept numbers, points and coma in there.
            char key = e.KeyChar;
            if (((key < '0') || (key > '9')) && (key != ',') && (key != '.') && (key != '\b'))
            {
                e.Handled = true;
            }
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            // Fall back To original.
            tbFPSRealWorld.Text = String.Format("{0:0.00}", m_fVideoFps);
        }
        #endregion
    }
}