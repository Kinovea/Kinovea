#region License
/*
Copyright © Joan Charmant 2013.
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public class SliderLogScale : Control
    {
        #region Events
        public event EventHandler ValueChanged;
        #endregion
    
        #region Properties
        public double Minimum
        {
            get { return min;}
            set 
            { 
                min = value;
                Remap();
            }
        }
        
        public double Maximum
        {
            get { return max;}
            set 
            { 
                max = value <= min ? max = min + 1 : value;
                if(val > max)
                    val = max;
                Remap();
            }
        }
        
        public double Value
        {
            get { return val;}
            set 
            {
                val = Math.Min(Math.Max(value, min), max);

                Remap();
            }
        }
        #endregion
    
        #region Members
        private double minPix;
        private double maxPix;
        private double valPix;
        
        private double min;
        private double max;
        private double val;
        
        private Bitmap cursor;
        private Bitmap gutterLeft;
        private Bitmap gutterRight;
        private Bitmap gutterCenter;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public SliderLogScale()
        {
            this.Cursor = Cursors.Hand;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            ComponentResourceManager resources = new ComponentResourceManager(typeof(SliderLogScale));
            cursor = (Bitmap)resources.GetObject("cursor");
            gutterLeft = (Bitmap)resources.GetObject("gutter_left");
            gutterRight = (Bitmap)resources.GetObject("gutter_right");
            gutterCenter = (Bitmap)resources.GetObject("gutter_center");
            
            minPix = cursor.Width / 2;
            maxPix = this.Width - (cursor.Width / 2);
            maxPix = 300;
            
            min = 0;
            max = 100;
            val = 0;
            
            Remap();
            
            this.Height = gutterCenter.Height;
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (double.IsNaN(valPix))
                return;
            
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            
            e.Graphics.DrawImageUnscaled(gutterLeft, Point.Empty);
            e.Graphics.DrawImageUnscaled(gutterRight, this.Width - gutterRight.Width, 0);
            e.Graphics.DrawImage(gutterCenter, gutterLeft.Width, 0, this.Width - gutterRight.Width - gutterLeft.Width, gutterCenter.Height);
            e.Graphics.DrawImageUnscaled(cursor, (int)(valPix - (cursor.Width/2)), 0);
            
            //e.Graphics.DrawString(val.ToString("0.00"), SystemFonts.DefaultFont, Brushes.Red, 0, 0);
        }
        
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            maxPix = this.Width - (cursor.Width / 2);
            if(maxPix <= minPix)
                maxPix = minPix;

            Remap();
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if(e.Button != MouseButtons.Left)
                return;
            
            valPix = Math.Max(Math.Min(e.X, maxPix), minPix);
            val = PixelToValue(valPix);
            Invalidate();
            
            if(ValueChanged!=null)
                ValueChanged(this, EventArgs.Empty);
        }
        
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            
            valPix = Math.Max(Math.Min(e.X, maxPix), minPix);
            val = PixelToValue(valPix);
            Invalidate();
            
            if(ValueChanged!=null)
                ValueChanged(this, EventArgs.Empty);
        }
        
        private void Remap()
        {
            valPix = ValueToPixel(val);
        }
        
        private double ValueToPixel(double a)
        {
            double safeMin = min+1;
            double safeVal = a+1;
            double safeMax = max+1;
            
            double rangePix = maxPix - minPix;
            double range = Math.Log10(safeMax/safeMin);
            
            double p = minPix + (Math.Log10(safeVal/safeMin) * (rangePix/range));
            return p;
        }
        
        private double PixelToValue(double a)
        {
            double safeMin = min+1;
            double safeMax = max+1;
            double rangePix = maxPix - minPix;
            double range = Math.Log10(safeMax/safeMin);
            
            double p = Math.Log10(safeMin) + ((a - minPix) / (rangePix/range));
            double safeValue = Math.Pow(10, p);
            return safeValue - 1;
        }
    }
}
