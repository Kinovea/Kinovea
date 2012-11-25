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

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// This dialog let the user specify how many real-world-units long is this line.
	/// This will have an impact on all lines stored in this metadata.
	/// Note that it is not possible to map pixels to pixels. 
	/// Pixel are used exclusively internally. 
	/// </summary>
    public partial class FormCalibrateLine : Form
    {
    	#region Members
        private CalibrationHelper calibrationHelper;
        private DrawingLine2D line;
        private float pixelLength;
        private float calibratedLength;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Construction & Initialization
        public FormCalibrateLine(CalibrationHelper calibrationHelper, DrawingLine2D line)
        {
        	this.calibrationHelper = calibrationHelper;
        	this.line = line;
        	
        	pixelLength = this.line.Length();
        	calibratedLength = calibrationHelper.GetLength(PointF.Empty, new PointF(pixelLength, 0));
        	log.Debug(calibrationHelper.GetLengthText(PointF.Empty, new PointF(pixelLength, 0), true, true));
        	
            InitializeComponent();
            LocalizeForm();
        }
        private void LocalizeForm()
        {
            this.Text = "   " + ScreenManagerLang.dlgConfigureMeasure_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            lblRealSize.Text = ScreenManagerLang.dlgConfigureMeasure_lblRealSize.Replace("\\n", "\n");
            
            // Combo Units (MUST be filled in the order of the enum)
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Centimeters + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Centimeters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Meters + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Meters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Inches + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Inches) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Feet + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Feet) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Yards + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Yards) + ")");
            cbUnit.Items.Add("Percentage" + " (" + UnitHelper.LengthAbbreviation(LengthUnits.Percentage) + ")");
            
            // Update with current values.
            if(!calibrationHelper.IsCalibrated)
            {
                // Default to 50 cm if no unit selected yet.
            	tbMeasure.Text = "50";
            	cbUnit.SelectedIndex = (int)LengthUnits.Centimeters;
            }
            else
            {
            	tbMeasure.Text = String.Format("{0:0.00}", calibratedLength);
            	cbUnit.SelectedIndex = (int)calibrationHelper.LengthUnit;
            }
            
        }
        #endregion

        #region User choices handlers
        private void tbFPSOriginal_KeyPress(object sender, KeyPressEventArgs e)
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
        	if(tbMeasure.Text.Length > 0)
        	{
            	// Save value.
	            try
	            {
	            	float fRealWorldMeasure = float.Parse(tbMeasure.Text);
	            
	            	if(fRealWorldMeasure > 0 && calibratedLength > 0)
	            	{
	            	    calibrationHelper.CalibrationByLine_SetPixelToUnit(fRealWorldMeasure / pixelLength);
	            	    calibrationHelper.LengthUnit = (LengthUnits)cbUnit.SelectedIndex;
	            	}
	            }
	            catch
	            {
	                // Failed : do nothing.
	                log.Error(String.Format("Error while parsing measure. ({0}).", tbMeasure.Text));
	            } 
        	}
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
             // Nothing more to do.           
        }
        #endregion
    }
}