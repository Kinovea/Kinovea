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
        
        public PointF TextPosition { get; private set;}
        public bool Tenth { get; private set;}

        /// <summary>
        /// A symbol used to identify this specific angle in a drawing containing several.
        /// </summary>
        public string Symbol { get; private set;}

        public Color Color { get; private set;}

        private const double TAU = Math.PI * 2;
        private int textDistance;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public AngleHelper(int textDistance, bool tenth, string symbol)
        {
            this.textDistance = textDistance;
            this.Tenth = tenth;
            this.Symbol = symbol;

            SweepAngle = new SweepAngle();
        }

        /// <summary>
        /// Takes point in image space and compute various values necessary to measure and draw the angle.
        /// </summary>
        public void Update(PointF o, PointF a, PointF b, int radius, AngleOptions angleOptions, Color color, CalibrationHelper calibration, IImageToViewportTransformer transformer)
        {
            if(o == a || o == b)
                return;

            Color = color;

            if (angleOptions.Complement)
            {
                // Complement to 180°.
                // Point symmetry around o to find the actual second leg.
                PointF c = new PointF(2 * o.X - a.X, 2 * o.Y - a.Y);

                // Both drawing and value are impacted by this directly so we can just swap the new legs in.
                a = b;
                b = c;
            }

            SweepAngle.Update(o, a, b, (float)radius, angleOptions.Signed, angleOptions.CCW);
            CalibratedAngle = ComputeCalibratedAngle(o, a, b, angleOptions.Signed, angleOptions.CCW, calibration);
            UpdateTextPosition(transformer);
        }

        public void DrawText(Graphics canvas, double opacity, SolidBrush brushFill, PointF o, IImageToViewportTransformer transformer, CalibrationHelper calibrationHelper, StyleHelper styleHelper)
        {
            float value = calibrationHelper.ConvertAngle(CalibratedAngle);

            string label = "";
            if (Tenth || calibrationHelper.AngleUnit == AngleUnit.Radian)
                label = string.Format("{0:0.0} {1}", value, calibrationHelper.GetAngleAbbreviation());
            else
                label = string.Format("{0} {1}", (int)Math.Round(value), calibrationHelper.GetAngleAbbreviation());

            if (!string.IsNullOrEmpty(Symbol))
                label = string.Format("{0} = {1}", Symbol, label);

            SolidBrush fontBrush = styleHelper.GetForegroundBrush((int)(opacity * 255));
            Font tempFont = styleHelper.GetFont(Math.Max((float)transformer.Scale, 1.0F));
            SizeF labelSize = canvas.MeasureString(label, tempFont);

            // Background
            float shiftx = (float)(transformer.Scale * TextPosition.X);
            float shifty = (float)(transformer.Scale * TextPosition.Y);
            
            PointF textOrigin = new PointF(o.X + shiftx - (labelSize.Width / 2), o.Y + shifty - (labelSize.Height / 2));
            RectangleF backRectangle = new RectangleF(textOrigin, labelSize);
            RoundedRectangle.Draw(canvas, backRectangle, brushFill, tempFont.Height / 4, false, false, null);

            // Text
            canvas.DrawString(label, tempFont, fontBrush, backRectangle.Location);

            tempFont.Dispose();
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

        private void UpdateTextPosition(IImageToViewportTransformer transformer)
        {
            int imageTextDistance = transformer.Untransform(textDistance);
            
            // The sweep is going clockwise in drawpie conventions so the y-axis is downwards, 
            // which is also the direction of the y-axis of the image so y doesn't need to be inverted here.
            double angle = (SweepAngle.Start + SweepAngle.Sweep / 2) * MathHelper.DegreesToRadians;
            double x = Math.Cos(angle) * imageTextDistance;
            double y = Math.Sin(angle) * imageTextDistance;
            
            TextPosition = new PointF((float)x, (float)y);
        }
    }
}
