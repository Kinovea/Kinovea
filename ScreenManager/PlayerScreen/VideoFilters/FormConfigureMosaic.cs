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
using System.IO;
using System.Threading;
using System.Resources;
using System.Reflection;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// This form lets the user choose how many images will be visible when in the mosaic.
	/// The user can also choose to view only keyframes (temporary disabled).
	/// </summary>
    public partial class formConfigureMosaic : Form
    {
    	#region Properties        
		public int FramesToExtract 
		{
			get { return m_iFramesToExtract; }
		}    	
		public bool IsRightToLeft
		{
			get { return cbRTL.Checked; }
		}
    	#endregion
    	
        #region Members
        //private double m_fDurationInSeconds;
        private int m_iDurationinFrames;
        private static readonly int m_iDefaultFramesToExtract = 25;
        private int m_iFramesToExtract = m_iDefaultFramesToExtract;
        private ResourceManager m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
        #endregion

        public formConfigureMosaic(int _iTotalImages)
        {
        	m_iDurationinFrames = _iTotalImages;
        	
       		InitializeComponent();
       		SetupUICulture();
       		SetupData();
       		UpdateLabels();
        }
        private void SetupUICulture()
        {
            // Window
            this.Text = "   " + m_ResourceManager.GetString("dlgConfigureMosaic_Title", Thread.CurrentThread.CurrentUICulture);
            
            // Group Config
            grpboxConfig.Text = m_ResourceManager.GetString("Generic_Configuration", Thread.CurrentThread.CurrentUICulture);
            rbKeyframes.Text = m_ResourceManager.GetString("dlgConfigureMosaic_radioKeyframes", Thread.CurrentThread.CurrentUICulture);
            rbFrequency.Text = m_ResourceManager.GetString("dlgConfigureMosaic_radioFrequency", Thread.CurrentThread.CurrentUICulture);
            cbRTL.Text = m_ResourceManager.GetString("dlgConfigureMosaic_cbRightToLeft", Thread.CurrentThread.CurrentUICulture);
            
            // Buttons
            btnOK.Text = m_ResourceManager.GetString("Generic_Apply", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
        }
        private void SetupData()
        {
        	rbKeyframes.Checked = false;
        	rbFrequency.Checked = true;
        	rbKeyframes.Enabled = false;
        	cbRTL.Checked = false;
        	
        	// Default slider.
        	trkInterval.Minimum = 4;
        	trkInterval.Maximum = 100;
        	
        	// Adapt slider to actual values.
        	if(m_iDurationinFrames < trkInterval.Maximum)
        	{
        		trkInterval.Maximum = m_iDurationinFrames;
        	}
        	
        	if(m_iDefaultFramesToExtract <= trkInterval.Maximum)
        	{
        		trkInterval.Value = m_iDefaultFramesToExtract;
        	}
        	else
        	{
        		trkInterval.Value = trkInterval.Maximum;
        	}
        }
        private void RbFrequencyCheckedChanged(object sender, EventArgs e)
        {
        	trkInterval.Enabled = rbFrequency.Checked;
        	lblInfosTotalFrames.Enabled = rbFrequency.Checked;
        	lblInfosFrequency.Enabled = rbFrequency.Checked;
        }
        private void trkInterval_ValueChanged(object sender, EventArgs e)
        {
        	int iRoot = (int)(Math.Sqrt((double)trkInterval.Value));
        	m_iFramesToExtract = iRoot * iRoot;
            UpdateLabels();
        }
        private void UpdateLabels()
        {
        	// Number of frames
            lblInfosTotalFrames.Text = String.Format(m_ResourceManager.GetString("dlgConfigureMosaic_LabelImages", Thread.CurrentThread.CurrentUICulture), " " + m_iFramesToExtract);            
        	
            // Frequency
            /*double fInterval = m_fDurationInSeconds / (double)m_iFramesToExtract;
            lblInfosFrequency.Text = m_ResourceManager.GetString("dlgConfigureMosaic_LabelFrequencyRoot", Thread.CurrentThread.CurrentUICulture) + " ";
            if (fInterval < 1)
            {
                int iHundredth = (int)(fInterval * 100);
                lblInfosFrequency.Text += String.Format(m_ResourceManager.GetString("dlgRafaleExport_LabelFrequencyHundredth", Thread.CurrentThread.CurrentUICulture), iHundredth);
            }
            else
            {
                lblInfosFrequency.Text += String.Format(m_ResourceManager.GetString("dlgRafaleExport_LabelFrequencySeconds", Thread.CurrentThread.CurrentUICulture), fInterval);
            }*/
        }
    }
}