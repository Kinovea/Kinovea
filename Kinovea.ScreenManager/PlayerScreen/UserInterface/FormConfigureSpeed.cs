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
        public double UserInterval
        {
            get { return userInterval; }
        }

        public double CaptureInterval
        {
            get { return captureInterval; }
        }
        #endregion
        
        #region Members
        private double fileInterval;
       
        private double userInterval;
        private double minUserFPS = 1;
        private double maxUserFPS = 1000;
        
        private double captureInterval;
        private double minCaptureFPS = 0.1;
        private double maxCaptureFPS = 1000000; // 1MHz.
        
        private bool manualUpdate;
        #endregion

        public formConfigureSpeed(double fileInterval, double userInterval, double captureInterval)
        {
            this.fileInterval = fileInterval;
            this.userInterval = userInterval;
            this.captureInterval = captureInterval;

            InitializeComponent();
            LocalizeForm();
        }
        private void LocalizeForm()
        {
            //this.Text = "   " + ScreenManagerLang.dlgConfigureSpeed_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            //grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            //lblCaptureFPS.Text = ScreenManagerLang.dlgConfigureSpeed_lblFPSCaptureTime.Replace("\\n", "\n");
            //toolTips.SetToolTip(btnReset, ScreenManagerLang.dlgConfigureSpeed_ToolTip_Reset);
            //lblVideoFPS.Text = string.Format(ScreenManagerLang.dlgConfigureSpeed_lblFPSDisplayTime, videoFPS);

            UpdateCaptureText();
            UpdateUserText();

            string format = "Framerate read in the file: {0:0.##} fps";
            lblFile.Text = string.Format(format, 1000 / fileInterval);
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
            captureInterval = userInterval;
            UpdateCaptureText();
        }
        #endregion

        #region Video
        private void tbUser_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!IsNumerical(e.KeyChar))
                e.Handled = true;
        }
        private void tbUser_TextChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            // Text is parsed using the current culture.
            double result;
            bool parsed = double.TryParse(tbUser.Text, out result);
            if (!parsed)
                return;

            double userFPS = Math.Max(Math.Min(result, maxUserFPS), minUserFPS);
            userInterval = 1000 / userFPS;

            if (userFPS != result)
                UpdateUserText();
        }
        private void UpdateUserText()
        {
            manualUpdate = true;
            tbUser.Text = String.Format("{0:0.##}", 1000 / userInterval);
            manualUpdate = false;
        }
        private void btnResetUser_Click(object sender, EventArgs e)
        {
            userInterval = fileInterval;
            UpdateUserText();
        }
        #endregion


        private bool IsNumerical(char key)
        {
            return (key >= '0' && key <= '9') || key == ',' || key == '.' || key == '\b';
        }

    }
}