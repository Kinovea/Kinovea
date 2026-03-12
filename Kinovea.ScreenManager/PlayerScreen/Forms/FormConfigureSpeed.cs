#region License
/*
Copyright © Joan Charmant 2009.
jcharmant@gmail.com 
 
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
    /// Dialog to configure the capture frame rate of the video.
    /// Defines the scale of the time axis.
    /// </summary>
    public partial class formConfigureSpeed : Form
    {
        #region Properties
        /// <summary>
        /// Frame interval at capture time in milliseconds.
        /// </summary>
        public double CaptureInterval
        {
            get { return captureInterval; }
        }
        #endregion
        
        #region Members
        private double fileInterval;
        private double captureInterval;
        private double minCaptureFPS = 0.1;
        private double maxCaptureFPS = 1000000; // 1MHz.
        private bool manualUpdate;
        #endregion

        public formConfigureSpeed(double fileInterval, double captureInterval)
        {
            this.fileInterval = fileInterval;
            this.captureInterval = captureInterval;

            InitializeComponent();
            LocalizeForm();
        }
        private void LocalizeForm()
        {
            this.Text = ScreenManagerLang.dlgTimebase_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;

            grpHighSpeedCamera.Text = ScreenManagerLang.dlgTimebase_GroupCapture;
            lblCapture.Text = ScreenManagerLang.dlgTimebase_lblCapture;
            toolTips.SetToolTip(btnResetCapture, ScreenManagerLang.dlgTimebase_ToolTip_Reset);
            tbCaptureInfo.Clear();
            tbCaptureInfo.AppendText(ScreenManagerLang.dlgTimebase_InfoCapture);

            UpdateCaptureText();
        }

        #region High speed camera
        private void tbCapture_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!IsNumerical(e.KeyChar))
                e.Handled = true;
        }
        private void tbCapture_TextChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            // Text is parsed using the current culture.
            double result;
            bool parsed = double.TryParse(tbCapture.Text, out result);
            if (!parsed)
                return;

            double captureFPS = Math.Max(Math.Min(result, maxCaptureFPS), minCaptureFPS);
            captureInterval = 1000 / captureFPS;

            if (captureFPS != result)
                UpdateCaptureText();
        }
        private void UpdateCaptureText()
        {
            manualUpdate = true;
            tbCapture.Text = String.Format("{0:0.##}", 1000 / captureInterval);
            manualUpdate = false;
        }
        private void btnResetCapture_Click(object sender, EventArgs e)
        {
            captureInterval = fileInterval;
            UpdateCaptureText();
        }
        #endregion

        private bool IsNumerical(char key)
        {
            return (key >= '0' && key <= '9') || key == ',' || key == '.' || key == '\b';
        }

    }
}