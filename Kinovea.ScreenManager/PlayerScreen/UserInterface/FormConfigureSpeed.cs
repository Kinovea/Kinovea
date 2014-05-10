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
    /// Configure capture speed of video.
    /// The capture FPS is only updated when the user actually enters something in the textbox.
    /// FIXME: store capture speed everywhere instead of precomputing the "slowFactor" ratio.
    /// </summary>
    public partial class formConfigureSpeed : Form
    {
        #region Properties
        public double SlowFactor
        {
            get 
            {
                return captureFPS < 1 ? memoSlowFactor : captureFPS / videoFPS;
            }
        }
        #endregion
        
        #region Members
        private readonly double videoFPS;				
        private double captureFPS;
        private double memoSlowFactor;
        private const double maxCaptureFPS = 10000;
        private const double minCaptureFPS = 1;
        private bool internalUpdate;
        #endregion

        public formConfigureSpeed(double videoFPS, double memoSlowFactor)
        {
            this.videoFPS = videoFPS;
            this.memoSlowFactor = memoSlowFactor;
            captureFPS = videoFPS * memoSlowFactor;
            
            InitializeComponent();
            LocalizeForm();
        }
        private void LocalizeForm()
        {
            this.Text = "   " + ScreenManagerLang.dlgConfigureSpeed_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            lblCaptureFPS.Text = ScreenManagerLang.dlgConfigureSpeed_lblFPSCaptureTime.Replace("\\n", "\n");
            toolTips.SetToolTip(btnReset, ScreenManagerLang.dlgConfigureSpeed_ToolTip_Reset);
            
            lblVideoFPS.Text = string.Format(ScreenManagerLang.dlgConfigureSpeed_lblFPSDisplayTime, videoFPS);

            UpdateCaptureFPSText();
            UpdateSlowFactorText();
        }

        private void tbCaptureFPS_TextChanged(object sender, EventArgs e)
        {
            if (internalUpdate)
                return;

            // Text is parsed using the current culture.
            double result;
            bool parsed = double.TryParse(tbCaptureFPS.Text, out result);
            if (!parsed)
                return;

            captureFPS = Math.Min(maxCaptureFPS, result);

            if (captureFPS < minCaptureFPS)
                captureFPS = videoFPS;

            if (captureFPS != result)
                UpdateCaptureFPSText();

            UpdateSlowFactorText();            
        }
        private void tbCaptureFPS_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!IsNumerical(e.KeyChar))
                e.Handled = true;
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            captureFPS = videoFPS;
            UpdateCaptureFPSText();
        }
        private void UpdateCaptureFPSText()
        {
            internalUpdate = true;
            tbCaptureFPS.Text = String.Format("{0:0.00}", captureFPS);
            internalUpdate = false;
        }
        private void UpdateSlowFactorText()
        {
            int slowingFactor = (int)(captureFPS / videoFPS);
            lblSlowFactor.Visible = slowingFactor > 1;
            lblSlowFactor.Text = string.Format(ScreenManagerLang.dlgConfigureSpeed_lblSlowFactor, slowingFactor);
        }
        private bool IsNumerical(char key)
        {
            return (key >= '0' && key <= '9') || key == ',' || key == '.' || key == '\b';
        }
    }
}