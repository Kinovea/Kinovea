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
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Computes the angle value in calibrated space and define positions of bounding box for the sweep zone and text position.
    /// </summary>
    public class AngleHelper
    {
        /// <summary>
        /// A helper class for drawing the pie section in image space.
        /// </summary>
        public SweepAngle SweepAngle { get; private set;}
        
        /// <summary>
        /// The actual angular value in radians in the range [-π..+π].
        /// </summary>
        public float CalibratedAngle { get; private set; }

        private const double TAU = Math.PI * 2;
        private int textDistance;
        private int radius;
        private bool tenth;
        private string symbol;
        private static readonly int defaultTextDistance = 40;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public AngleHelper() :
            this(defaultTextDistance, 0, true, "")
        {
        }

        public AngleHelper(int textDistance, int radius, bool tenth, string symbol)
        {
            this.textDistance = textDistance;
            this.radius = radius;
            this.tenth = tenth;
            this.symbol = symbol;

            SweepAngle = new SweepAngle();
        }

        /// <summary>
        /// Takes point in image space and compute various values necessary to measure and draw the angle.
        /// </summary>
        public void Update(PointF o, PointF a, PointF b, bool signed, bool ccw, bool supplementary, CalibrationHelper calibration)
        {
            if(o == a || o == b)
                return;

            if (supplementary)
            {
                // Supplementary angle to 180°.
                // Point symmetry around o to find the actual second leg.
                PointF c = new PointF(2 * o.X - a.X, 2 * o.Y - a.Y);

                // Both drawing and value are impacted by this directly so we can just swap the new legs in.
                a = b;
                b = c;
            }

            SweepAngle.Update(o, a, b, (float)radius, signed, ccw);
            CalibratedAngle = ComputeCalibratedAngle(o, a, b, signed, ccw, calibration);
        }

        public void DrawText(Graphics canvas, double opacity, SolidBrush brushFill, PointF o, IImageToViewportTransformer transformer, CalibrationHelper calibrationHelper, StyleData styleHelper)
        {
            float value = calibrationHelper.ConvertAngle(CalibratedAngle);

            string label = "";
            if (tenth || calibrationHelper.AngleUnit == AngleUnit.Radian)
                label = string.Format("{0:0.0} {1}", value, calibrationHelper.GetAngleAbbreviation());
            else
                label = string.Format("{0} {1}", (int)Math.Round(value), calibrationHelper.GetAngleAbbreviation());

            if (!string.IsNullOrEmpty(symbol))
                label = string.Format("{0} = {1}", symbol, label);

            SolidBrush fontBrush = styleHelper.GetForegroundBrush((int)(opacity * 255));
            
            Font tempFont = styleHelper.GetFont(1.0F);
            SizeF labelSize = canvas.MeasureString(label, tempFont);

            Font tempFontTransformed = styleHelper.GetFont((float)transformer.Scale);
            SizeF labelSizeTransformed = canvas.MeasureString(label, tempFontTransformed);

            // Background
            PointF textPosition = GetTextPosition(textDistance, labelSize);
            textPosition = textPosition.Scale((float)transformer.Scale);

            PointF backgroundOrigin = o.Translate(textPosition.X, textPosition.Y);
            RectangleF backRectangle = new RectangleF(backgroundOrigin, labelSizeTransformed);
            RoundedRectangle.Draw(canvas, backRectangle, brushFill, tempFontTransformed.Height / 4, false, false, null);

            // Text
            canvas.DrawString(label, tempFontTransformed, fontBrush, backgroundOrigin);

            tempFont.Dispose();
            tempFontTransformed.Dispose();
            fontBrush.Dispose();
        }

        private float ComputeCalibratedAngle(PointF o, PointF a, PointF b, bool signed, bool ccw, CalibrationHelper calibration)
        {
            if (calibration == null)
                throw new InvalidProgramException();

            PointF o2 = calibration.GetPoint(o);
            PointF a2 = calibration.GetPoint(a);
            PointF b2 = calibration.GetPoint(b);

            float value = 0;
            if (ccw)
                value = GeometryHelper.GetAngle(o2, a2, b2);
            else
                value = GeometryHelper.GetAngle(o2, b2, a2);

            if (!signed && value < 0)
                value = (float)(TAU + value);

            return value;
        }

        /// <summary>
        /// Get the relative position of the text in image space.
        /// The center of the text is placed at a fixed distance on the bissector of the angle.
        /// </summary>
        public PointF GetTextPosition(int textDistance, SizeF labelSize)
        {
            // The sweep is going clockwise in drawpie conventions so the y-axis is downwards, 
            // which is also the direction of the y-axis of the image so the y variable doesn't need to be inverted here.
            double angle = MathHelper.Radians(SweepAngle.Start + SweepAngle.Sweep / 2);
            double x = Math.Cos(angle) * textDistance;
            double y = Math.Sin(angle) * textDistance;
            
            x -= (labelSize.Width/2);
            y -= (labelSize.Height/2);

            return new PointF((float)x, (float)y);
        }
    
        public void UpdateTextDistance(float factor)
        {
            this.textDistance = (int)(defaultTextDistance * factor);
        }
    }
}
