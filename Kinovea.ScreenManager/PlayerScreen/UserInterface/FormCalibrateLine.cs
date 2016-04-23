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
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This dialog lets the user specify a mapping between pixels (undistorted) and real world units.
    /// This type of calibration assumes all points are on the same plane, orthogonal to the camera optical axis.
    /// </summary>
    public partial class FormCalibrateLine : Form
    {
        #region Members
        private CalibrationHelper calibrationHelper;
        private DrawingLine line;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Construction & Initialization
        public FormCalibrateLine(CalibrationHelper calibrationHelper, DrawingLine line)
        {
            this.calibrationHelper = calibrationHelper;
            this.line = line;
            
            InitializeComponent();
            LocalizeForm();
            InitializeValues();
        }
        private void LocalizeForm()
        {
            this.Text = "   " + ScreenManagerLang.dlgCalibrateLine_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Calibration;
            lblRealSize.Text = ScreenManagerLang.dlgConfigureMeasure_lblRealSize.Replace("\\n", "\n");
            
            // Combo Units (MUST be filled in the order of the enum)
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Millimeters + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Millimeters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Centimeters + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Centimeters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Meters + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Meters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Inches + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Inches) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Feet + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Feet) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Yards + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Yards) + ")");

            string customLengthUnit = PreferencesManager.PlayerPreferences.CustomLengthUnit;
            string customLengthAbbreviation = PreferencesManager.PlayerPreferences.CustomLengthAbbreviation;
            if (string.IsNullOrEmpty(customLengthUnit))
            {
                customLengthUnit = ScreenManagerLang.LengthUnit_Percentage;
                customLengthAbbreviation = "%";
            }
            
            cbUnit.Items.Add(customLengthUnit + " (" + customLengthAbbreviation + ")");
        }
        private void InitializeValues()
        {
            if(calibrationHelper.IsCalibrated && calibrationHelper.CalibratorType == CalibratorType.Line)
            {
                string text = calibrationHelper.GetLengthText(line.A, line.B, true, false);
                tbMeasure.Text = text;
                
                cbUnit.SelectedIndex = (int)calibrationHelper.LengthUnit;
            }
            else
            {
                tbMeasure.Text = "50";
                cbUnit.SelectedIndex = (int)LengthUnit.Centimeters;
            }
        }
        #endregion

        #region User choices handlers
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // We only accept numbers, points and coma in there.
            char key = e.KeyChar;
            if (((key < '0') || (key > '9')) && (key != ',') && (key != '.') && (key != '\b'))
            {
                e.Handled = true;
            }
        }
        #endregion
        
        #region OK/Cancel Handlers
        private void btnOK_Click(object sender, EventArgs e)
        {
            if(tbMeasure.Text.Length == 0)
                return;
            
            try
            {
                float length = float.Parse(tbMeasure.Text);
                if(length <= 0)
                    return;

                PointF a = calibrationHelper.DistortionHelper.Undistort(line.A);
                PointF b = calibrationHelper.DistortionHelper.Undistort(line.B);
                float pixelLength = GeometryHelper.GetDistance(a, b);

                float ratio = length / pixelLength;
                
                calibrationHelper.SetCalibratorFromType(CalibratorType.Line);
                calibrationHelper.LengthUnit = (LengthUnit)cbUnit.SelectedIndex;
                calibrationHelper.CalibrationByLine_Initialize(ratio);
            }
            catch
            {
                // Failed : do nothing.
                log.Error(String.Format("Error while parsing measure. ({0}).", tbMeasure.Text));
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
        }
        #endregion
    }
}