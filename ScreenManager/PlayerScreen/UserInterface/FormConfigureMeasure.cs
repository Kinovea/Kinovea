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
    public partial class formConfigureMeasure : Form
    {
    	#region Members
        private CalibrationHelper calibrationHelper;        
        private DrawingLine2D m_Line;
        private double m_fCurrentLengthPixels;
        private double m_fCurrentLengthReal;			// The current length of the segment. Might be expressed in pixels.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Construction & Initialization
        public formConfigureMeasure(CalibrationHelper calibrationHelper, DrawingLine2D _line)
        {
        	this.calibrationHelper = calibrationHelper;
        	m_Line =_line;
        	
        	m_fCurrentLengthPixels = (double)m_Line.Length();
        	m_fCurrentLengthReal = calibrationHelper.GetLengthInUserUnit(m_fCurrentLengthPixels);
            	
        	log.Debug(String.Format("Initial length:{0:0.00} {1}", m_fCurrentLengthReal, calibrationHelper.CurrentLengthUnit.ToString()));
        	
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
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Centimeters + " (" + CalibrationHelper.GetLengthAbbreviationFromUnit(LengthUnits.Centimeters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Meters + " (" + CalibrationHelper.GetLengthAbbreviationFromUnit(LengthUnits.Meters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Inches + " (" + CalibrationHelper.GetLengthAbbreviationFromUnit(LengthUnits.Inches) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Feet + " (" + CalibrationHelper.GetLengthAbbreviationFromUnit(LengthUnits.Feet) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Yards + " (" + CalibrationHelper.GetLengthAbbreviationFromUnit(LengthUnits.Yards) + ")");
            cbUnit.Items.Add("Percentage" + " (" + CalibrationHelper.GetLengthAbbreviationFromUnit(LengthUnits.Percentage) + ")");
            
            // Update with current values.
            if(calibrationHelper.CurrentLengthUnit == LengthUnits.Pixels)
            {
            	// Default to 50 cm if no unit selected yet.
            	tbMeasure.Text = "50";
            	cbUnit.SelectedIndex = (int)LengthUnits.Centimeters;
            }
            else
            {
            	tbMeasure.Text = String.Format("{0:0.00}",m_fCurrentLengthReal);
            	cbUnit.SelectedIndex = (int)calibrationHelper.CurrentLengthUnit;
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
	            	double fRealWorldMeasure = double.Parse(tbMeasure.Text);
	            
	            	if(fRealWorldMeasure > 0 && m_fCurrentLengthReal > 0)
	            	{
	                	calibrationHelper.PixelToUnit = fRealWorldMeasure / m_fCurrentLengthPixels;
	                	calibrationHelper.CurrentLengthUnit = (LengthUnits)cbUnit.SelectedIndex;
	            	
	                	log.Debug(String.Format("Selected length:{0:0.00} {1}", fRealWorldMeasure, calibrationHelper.CurrentLengthUnit.ToString()));
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