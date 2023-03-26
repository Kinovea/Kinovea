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

            lblAxis.Text = ScreenManagerLang.dlgCalibrateLine_CoordinateSystemAlignment;

            // Combo axis.
            cbAxis.Items.Add(ScreenManagerLang.dlgCalibrateLine_AxisHorizontal);
            cbAxis.Items.Add(ScreenManagerLang.dlgCalibrateLine_AxisVertical);
            cbAxis.Items.Add(ScreenManagerLang.dlgCalibrateLine_AxesImage);
        }
        private void InitializeValues()
        {
            bool calibrated = false;
            if(calibrationHelper.IsCalibrated && calibrationHelper.CalibratorType == CalibratorType.Line)
            {
                string text = calibrationHelper.GetLengthText(line.A, line.B, true, false);
                float value;
                bool parsed = float.TryParse(text, out value);
                if (parsed)
                {
                    nudMeasure.Value = (decimal)value;
                    cbUnit.SelectedIndex = (int)calibrationHelper.LengthUnit;
                    cbAxis.SelectedIndex = (int)calibrationHelper.CalibrationAxis;
                    calibrated = true;
                }
            }

            if (!calibrated)
            {
                nudMeasure.Value = 100;
                cbUnit.SelectedIndex = (int)LengthUnit.Centimeters;
                cbAxis.SelectedIndex = (int)CalibrationAxis.LineHorizontal;
            }

            NudHelper.FixNudScroll(nudMeasure);
        }
        #endregion

        #region OK/Cancel Handlers
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                float length = (float)nudMeasure.Value;
                if (length <= 0)
                    return;
                
                calibrationHelper.SetCalibratorFromType(CalibratorType.Line);
                calibrationHelper.LengthUnit = (LengthUnit)cbUnit.SelectedIndex;
                calibrationHelper.CalibrationByLine_Initialize(line.Id, length, line.A, line.B, (CalibrationAxis)cbAxis.SelectedIndex);
            }
            catch
            {
                // Failed : do nothing.
                log.Error(String.Format("Error while parsing calibration measure."));
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
        }
        #endregion
    }
}