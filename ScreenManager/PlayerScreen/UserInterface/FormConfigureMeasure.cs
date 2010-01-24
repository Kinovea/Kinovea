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
        private ResourceManager m_ResourceManager;
        private Metadata m_Metadata; 
        private DrawingLine2D m_Line;
        private double m_fCurrentLengthPixels;
        private double m_fCurrentLengthReal;			// The current length of the segment. Might be expressed in pixels.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Construction & Initialization
        public formConfigureMeasure(Metadata _metadata, DrawingLine2D _line)
        {
        	m_Metadata = _metadata;
        	m_Line =_line;
        	
        	m_fCurrentLengthPixels = Math.Sqrt(((m_Line.m_StartPoint.X - m_Line.m_EndPoint.X) * (m_Line.m_StartPoint.X - m_Line.m_EndPoint.X)) + ((m_Line.m_StartPoint.Y - m_Line.m_EndPoint.Y) * (m_Line.m_StartPoint.Y - m_Line.m_EndPoint.Y)));
        	m_fCurrentLengthReal = m_Metadata.CalibrationHelper.GetLengthInUserUnit(m_Line.m_StartPoint, m_Line.m_EndPoint);
            	
        	log.Debug(String.Format("Initial length:{0:0.00} {1}", m_fCurrentLengthReal, m_Metadata.CalibrationHelper.CurrentLengthUnit.ToString()));
        	
            InitializeComponent();
            m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            
            LocalizeForm();
        }
        private void LocalizeForm()
        {
            this.Text = "   " + m_ResourceManager.GetString("dlgConfigureMeasure_Title", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
            btnOK.Text = m_ResourceManager.GetString("Generic_Apply", Thread.CurrentThread.CurrentUICulture);
            grpConfig.Text = m_ResourceManager.GetString("Generic_Configuration", Thread.CurrentThread.CurrentUICulture);
            lblRealSize.Text = m_ResourceManager.GetString("dlgConfigureMeasure_lblRealSize", Thread.CurrentThread.CurrentUICulture).Replace("\\n", "\n");
            
            // Combo Units (MUST be filled in the order of the enum)
            cbUnit.Items.Add(m_ResourceManager.GetString("LengthUnit_Centimeters", Thread.CurrentThread.CurrentUICulture) + " (" + CalibrationHelper.GetLengthAbbreviationFromUnit(CalibrationHelper.LengthUnits.Centimeters) + ")");
            cbUnit.Items.Add(m_ResourceManager.GetString("LengthUnit_Meters", Thread.CurrentThread.CurrentUICulture) + " (" + CalibrationHelper.GetLengthAbbreviationFromUnit(CalibrationHelper.LengthUnits.Meters) + ")");
            cbUnit.Items.Add(m_ResourceManager.GetString("LengthUnit_Inches", Thread.CurrentThread.CurrentUICulture) + " (" + CalibrationHelper.GetLengthAbbreviationFromUnit(CalibrationHelper.LengthUnits.Inches) + ")");
            cbUnit.Items.Add(m_ResourceManager.GetString("LengthUnit_Feet", Thread.CurrentThread.CurrentUICulture) + " (" + CalibrationHelper.GetLengthAbbreviationFromUnit(CalibrationHelper.LengthUnits.Feet) + ")");
            cbUnit.Items.Add(m_ResourceManager.GetString("LengthUnit_Yards", Thread.CurrentThread.CurrentUICulture) + " (" + CalibrationHelper.GetLengthAbbreviationFromUnit(CalibrationHelper.LengthUnits.Yards) + ")");
            
            // Update with current values.
            if(m_Metadata.CalibrationHelper.CurrentLengthUnit == CalibrationHelper.LengthUnits.Pixels)
            {
            	// Default to 50 cm if no unit selected yet.
            	tbMeasure.Text = "50";
            	cbUnit.SelectedIndex = (int)CalibrationHelper.LengthUnits.Centimeters;
            }
            else
            {
            	tbMeasure.Text = String.Format("{0:0.00}",m_fCurrentLengthReal);
            	cbUnit.SelectedIndex = (int)m_Metadata.CalibrationHelper.CurrentLengthUnit;
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
	            	double fRealWorldMeasure = double.Parse(tbMeasure.Text, CultureInfo.InvariantCulture);
	            
	            	if(fRealWorldMeasure > 0 && m_fCurrentLengthReal > 0)
	            	{
	                	m_Metadata.CalibrationHelper.PixelToUnit = fRealWorldMeasure / m_fCurrentLengthPixels;
	                	m_Metadata.CalibrationHelper.CurrentLengthUnit = (CalibrationHelper.LengthUnits)cbUnit.SelectedIndex;
	            	
	                	log.Debug(String.Format("Selected length:{0:0.00} {1}", fRealWorldMeasure, m_Metadata.CalibrationHelper.CurrentLengthUnit.ToString()));
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