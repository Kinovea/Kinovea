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
	/// The dialog lets the user configure the opacity option for image type drawing (SVG or Bitmap).
	/// We work with the actual drawing to display the change in real time.
	/// If the user decide to cancel, there's a "fallback to memo" mechanism.
	/// </summary>
    public partial class formConfigureOpacity : Form
    {
    	#region Members
       private bool m_bManualClose = false;
        
    	private PictureBox m_SurfaceScreen;        // Used to update the image while configuring.
        private AbstractDrawing m_Drawing;			// Instance of the drawing we are modifying.
        private InfosFading m_MemoInfosFading;		// Memo to fallback to on cancel.
        #endregion
        
        #region Construction & Initialization
        public formConfigureOpacity(AbstractDrawing _drawing, PictureBox _SurfaceScreen)
        {
        	m_SurfaceScreen = _SurfaceScreen;
            m_Drawing = _drawing;
            m_MemoInfosFading = _drawing.infosFading.Clone();
            
            InitializeComponent();
            ConfigureForm();
            LocalizeForm();
        }
        private void ConfigureForm()
        {
        	// Display current values.
        	trkValue.Value = (int)Math.Ceiling(m_Drawing.infosFading.MasterFactor * 100);
        }
        private void LocalizeForm()
        {
            this.Text = "   " + ScreenManagerLang.dlgConfigureOpacity_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;

            UpdateValueLabel();
        }
        #endregion

        #region User choices handlers
        private void trkValue_ValueChanged(object sender, EventArgs e)
        {
        	m_Drawing.infosFading.MasterFactor = (float)trkValue.Value / 100;
            UpdateValueLabel();
            m_SurfaceScreen.Invalidate();
        }
        private void UpdateValueLabel()
        {
            lblValue.Text = String.Format(ScreenManagerLang.dlgConfigureOpacity_lblValue, m_Drawing.infosFading.MasterFactor * 100);
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
        private void formConfigureOpacity_FormClosing(object sender, FormClosingEventArgs e)
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
