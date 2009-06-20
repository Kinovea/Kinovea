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
	/// The dialog lets the user configure the fading / persistence option for a given drawing.
	/// We work with the actual drawing to display the change in real time.
	/// If the user decide to cancel, there's a "fallback to memo" mechanism.
	/// </summary>
    public partial class formConfigureFading : Form
    {
    	#region Members
        private ResourceManager m_ResourceManager;
    	private bool m_bManualClose = false;
        
    	private PictureBox m_SurfaceScreen;        // Used to update the image while configuring.
        private AbstractDrawing m_Drawing;			// Instance of the drawing we are modifying.
        private InfosFading m_MemoInfosFading;		// Memo to fallback to on cancel.
        #endregion
        
        #region Construction & Initialization
        public formConfigureFading(AbstractDrawing _drawing, PictureBox _SurfaceScreen)
        {
            InitializeComponent();
            m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());

            m_SurfaceScreen = _SurfaceScreen;
            m_Drawing = _drawing;
            m_MemoInfosFading = _drawing.infosFading.Clone();
            ConfigureForm();
            LocalizeForm();
        }
        private void ConfigureForm()
        {
        	// Display current values.
            trkValue.Value = m_Drawing.infosFading.FadingFrames;
            chkDefault.Checked = m_Drawing.infosFading.UseDefault;
            chkAlwaysVisible.Checked = m_Drawing.infosFading.AlwaysVisible;
            chkEnable.Checked = m_Drawing.infosFading.Enabled;
        }
        private void LocalizeForm()
        {
            this.Text = "   " + m_ResourceManager.GetString("dlgConfigureFading_Title", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
            btnOK.Text = m_ResourceManager.GetString("Generic_Apply", Thread.CurrentThread.CurrentUICulture);
            grpConfig.Text = m_ResourceManager.GetString("Generic_Configuration", Thread.CurrentThread.CurrentUICulture);

            chkEnable.Text = m_ResourceManager.GetString("dlgConfigureFading_chkEnable", Thread.CurrentThread.CurrentUICulture);
            chkDefault.Text = String.Format(m_ResourceManager.GetString("dlgConfigureFading_chkDefault", Thread.CurrentThread.CurrentUICulture), PreferencesManager.Instance().DefaultFading.FadingFrames);
            UpdateValueLabel();
            chkAlwaysVisible.Text = m_ResourceManager.GetString("dlgConfigureFading_chkAlwaysVisible", Thread.CurrentThread.CurrentUICulture);
        }
        #endregion

        #region User choices handlers
        private void chkEnable_CheckedChanged(object sender, EventArgs e)
        {
            m_Drawing.infosFading.Enabled = chkEnable.Checked;
            EnableDisable();
            m_SurfaceScreen.Invalidate();
        }
        private void chkDefault_CheckedChanged(object sender, EventArgs e)
        {
            m_Drawing.infosFading.UseDefault = chkDefault.Checked;
            EnableDisable();
            m_SurfaceScreen.Invalidate();
        }
        private void chkAlwaysVisible_CheckedChanged(object sender, EventArgs e)
        {
            m_Drawing.infosFading.AlwaysVisible = chkAlwaysVisible.Checked;
            EnableDisable();
            m_SurfaceScreen.Invalidate();
        }
        private void trkValue_ValueChanged(object sender, EventArgs e)
        {
            m_Drawing.infosFading.FadingFrames = trkValue.Value;
            UpdateValueLabel();
            chkAlwaysVisible.Checked = false; 
            m_SurfaceScreen.Invalidate();
        }
        private void UpdateValueLabel()
        {
            lblValue.Text = String.Format(m_ResourceManager.GetString("dlgConfigureFading_lblValue", Thread.CurrentThread.CurrentUICulture), m_Drawing.infosFading.FadingFrames.ToString());
        }
        private void EnableDisable()
        {
            if (!chkEnable.Checked)
            {
                chkDefault.Enabled = false;
                lblValue.Enabled = false;
                trkValue.Enabled = false;
                chkAlwaysVisible.Enabled = false;
            }
            else if (chkDefault.Checked)
            {
                chkDefault.Enabled = true;
                lblValue.Enabled = false;
                trkValue.Enabled = false;
                chkAlwaysVisible.Enabled = false;
            }
            else
            {
                // We keep the slider enabled even when the user checked "Always visible".
                // We will automatically uncheck it if he moves the slider.
                chkDefault.Enabled = true;
                lblValue.Enabled = true;
                trkValue.Enabled = true;
                chkAlwaysVisible.Enabled = true;
            }
        }
        #endregion
        
        #region OK/Cancel Handlers
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Nothing special to do, the drawing has already been updated.   
            m_bManualClose = true;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Fall back to memo.
            m_Drawing.infosFading = m_MemoInfosFading.Clone();
            m_SurfaceScreen.Invalidate();
            m_bManualClose = true;
        }
        private void formConfigureFading_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!m_bManualClose)
            {
            	m_Drawing.infosFading = m_MemoInfosFading.Clone();
                m_SurfaceScreen.Invalidate();
            }
        }
        #endregion
    }
}