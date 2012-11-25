#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public partial class FormCalibratePlane : Form
    {
        private CalibrationHelper calibrationHelper;
        private DrawingPlane plane;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        
        public FormCalibratePlane(CalibrationHelper calibrationHelper, DrawingPlane plane)
        {
            this.calibrationHelper = calibrationHelper;
            this.plane = plane;
            
            InitializeComponent();
            LocalizeForm();
        }
        
        private void LocalizeForm()
        {
            //this.Text = "   " + ScreenManagerLang.dlgConfigureMeasure_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            //grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            
            // Combo Units (MUST be filled in the order of the enum)
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Centimeters + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Centimeters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Meters + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Meters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Inches + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Inches) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Feet + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Feet) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Yards + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Yards) + ")");
            cbUnit.Items.Add("Percentage" + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Percentage) + ")");
            
            // Update with current values.
            /*if(!calibrationHelper.IsCalibrated)
            {
                // Default to 50 cm if no unit selected yet.
            	tbMeasure.Text = "50";
            	cbUnit.SelectedIndex = (int)LengthUnits.Centimeters;
            }
            else
            {
            	tbMeasure.Text = String.Format("{0:0.00}", calibratedLength);
            	cbUnit.SelectedIndex = (int)calibrationHelper.LengthUnit;
            }*/
            
            cbUnit.SelectedIndex = (int)LengthUnits.Centimeters;
        }
        
        private void btnOK_Click(object sender, EventArgs e)
        {
            if(tbA.Text.Length == 0 || tbB.Text.Length == 0)
                return;
            
            try
            {
                float a = float.Parse(tbA.Text);
                float b = float.Parse(tbB.Text);
                
                plane.SetUsedForCalibration(true);
                calibrationHelper.SetCalibratorFromType(CalibratorType.Plane);
                calibrationHelper.CalibrationByPlane_InitSize(new SizeF(a, b));
                calibrationHelper.LengthUnit = (LengthUnits)cbUnit.SelectedIndex;
            }
            catch
            {
                // Failed : do nothing.
                log.Error(String.Format("Error while parsing measure. ({0}x{1}).", tbA.Text, tbB.Text));
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
        }
        
    }
}
