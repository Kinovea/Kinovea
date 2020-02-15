#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class FormCalibratePlane : Form
    {
        private CalibrationHelper calibrationHelper;
        private DrawingPlane drawingPlane;
        private QuadrilateralF quadrilateral;
        private QuadrilateralF miniQuadrilateral;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormCalibratePlane(CalibrationHelper calibrationHelper, DrawingPlane drawingPlane)
        {
            this.calibrationHelper = calibrationHelper;
            this.drawingPlane = drawingPlane;
            this.quadrilateral = drawingPlane.QuadImage;
            
            InitializeComponent();
            LocalizeForm();
            InitializeValues();
        }
        
        private void LocalizeForm()
        {
            this.Text = "   " + ScreenManagerLang.dlgCalibratePlane_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Calibration;
            
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
            if(calibrationHelper.IsCalibrated && calibrationHelper.CalibratorType == CalibratorType.Plane)
            {
                SizeF size = calibrationHelper.CalibrationByPlane_GetRectangleSize();
                tbA.Text = String.Format("{0:0.00}", size.Height);
                tbB.Text = String.Format("{0:0.00}", size.Width);
                
                cbUnit.SelectedIndex = (int)calibrationHelper.LengthUnit;
            }
            else
            {
                tbA.Text = "100";
                tbB.Text = "100";
                cbUnit.SelectedIndex = (int)LengthUnit.Centimeters;
            }
            
            // Prepare drawing.
            RectangleF bbox = quadrilateral.GetBoundingBox();
            SizeF usableSize = new SizeF(pnlQuadrilateral.Width * 0.9f, pnlQuadrilateral.Height * 0.9f);
            float ratioWidth = bbox.Width / usableSize.Width;
            float ratioHeight = bbox.Height / usableSize.Height;
            float ratio = Math.Max(ratioWidth, ratioHeight);
            
            float width = bbox.Width / ratio;
            float height = bbox.Height / ratio;
            float top = (pnlQuadrilateral.Height - height) / 2;
            float left = (pnlQuadrilateral.Width - width) / 2;
            
            miniQuadrilateral = new QuadrilateralF();
            for(int i = 0; i<4; i++)
            {
                PointF p = quadrilateral[i].Translate(-bbox.Left, -bbox.Top);
                p = p.Scale(1/ratio, 1/ratio);
                p = p.Translate(left, top);
                miniQuadrilateral[i] = p;
            }
        }
        
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only accept numbers, decimal separator and backspace.
            // TODO: move to a helper.
            
            NumberFormatInfo nfi = Thread.CurrentThread.CurrentCulture.NumberFormat;
            string decimalSeparator = nfi.NumberDecimalSeparator;
            
            char key = e.KeyChar;
            if (((key < '0') || (key > '9')) && (key != decimalSeparator[0]) && (key != '\b'))
            {
                e.Handled = true;
            }
        }
        
        private void btnOK_Click(object sender, EventArgs e)
        {
            if(tbA.Text.Length == 0 || tbB.Text.Length == 0)
                return;
            
            try
            {
                float a = float.Parse(tbA.Text);
                float b = float.Parse(tbB.Text);
                if(a <= 0 || b <= 0)
                    return;
                
                SizeF size = new SizeF(b, a);
                
                drawingPlane.UpdateMapping(size);
                
                calibrationHelper.SetCalibratorFromType(CalibratorType.Plane);
                calibrationHelper.CalibrationByPlane_Initialize(drawingPlane.Id, size, drawingPlane.QuadImage);
                calibrationHelper.LengthUnit = (LengthUnit)cbUnit.SelectedIndex;
            }
            catch
            {
                // Failed : do nothing.
                log.Error(String.Format("Error while parsing size. ({0}x{1}).", tbA.Text, tbB.Text));
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
        }
        
        
        private void pnlQuadrilateral_Paint(object sender, PaintEventArgs e)
        {
            Graphics canvas = e.Graphics;
            canvas.CompositingQuality = CompositingQuality.HighQuality;
            canvas.InterpolationMode = InterpolationMode.Bicubic;
            canvas.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Edges
            Pen p = new Pen(pnlQuadrilateral.ForeColor);
            canvas.DrawLine(p, miniQuadrilateral.A, miniQuadrilateral.B);
            canvas.DrawLine(p, miniQuadrilateral.B, miniQuadrilateral.C);
            canvas.DrawLine(p, miniQuadrilateral.C, miniQuadrilateral.D);
            canvas.DrawLine(p, miniQuadrilateral.D, miniQuadrilateral.A);
            p.Dispose();
            
            // Side indicators
            DrawIndicator(canvas, " b ", miniQuadrilateral.A, miniQuadrilateral.B);
            DrawIndicator(canvas, " a ", miniQuadrilateral.B, miniQuadrilateral.C);
            DrawIndicator(canvas, " b ", miniQuadrilateral.C, miniQuadrilateral.D);
            DrawIndicator(canvas, " a ", miniQuadrilateral.D, miniQuadrilateral.A);
        }
        
        private void DrawIndicator(Graphics canvas, string label, PointF a, PointF b)
        {
            PointF middle = GeometryHelper.GetMiddlePoint(a, b);
            
            Font tempFont = new Font("Arial", 9, FontStyle.Regular);
            SizeF labelSize = canvas.MeasureString(label, tempFont);
            
            PointF textOrigin = new PointF(middle.X - labelSize.Width/2, middle.Y - labelSize.Height/2);
            
            SolidBrush brushBack = new SolidBrush(pnlQuadrilateral.BackColor);
            canvas.FillRectangle(brushBack, textOrigin.X, textOrigin.Y, labelSize.Width, labelSize.Height);
            
            SolidBrush brushFont = new SolidBrush(pnlQuadrilateral.ForeColor);
            canvas.DrawString(label, tempFont, brushFont, textOrigin);

            tempFont.Dispose();
            brushBack.Dispose();
            brushFont.Dispose();
        }
    }
}
