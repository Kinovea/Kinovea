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
    public class SliderLinear : Control
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
        
        public bool Sticky
        {
            get { return sticky; }
            set { sticky = value; }
        }

        public double StickyValue
        {
            get { return stickyValue; }
            set { stickyValue = value; }
        }
        #endregion
    
        #region Members
        private double minPix;
        private double maxPix;
        private double valPix;
        
        private double min;
        private double max;
        private double val;
        private bool sticky;
        private double stickyValue;
        private double stickyRadius;

        private Bitmap cursor;
        private Bitmap gutterLeft;
        private Bitmap gutterRight;
        private Bitmap gutterCenter;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public SliderLinear()
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
        
        public void Initialize(double val)
        {
            min = 0;
            max = 1000;
            this.val = val;
            stickyValue = val;
            sticky = true;
            stickyRadius = 0.05 * (max - min);
            
            Remap();
        }

        /// <summary>
        /// Move to the next spot that is a whole divisor of the asked target.
        /// The sign of relativeTarget indicates if we are moving forward or backward.
        /// The value is normalized.
        /// </summary>
        public void StepJump(double relativeTarget)
        {
            double stepSize = (max - min) * Math.Abs(relativeTarget);
            double current = (val - min) / stepSize;
            double target = val;

            if (relativeTarget > 0)
                target = min + ((Math.Floor(current) + 1) * stepSize);
            else
                target = min + ((Math.Ceiling(current) - 1) * stepSize);

            val = Math.Max(Math.Min(target, max), min);
            valPix = ValueToPixel(val);
            Remap();
            Invalidate();

            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Set a new value and raises event back.
        /// Used to programmatically set the value, for automatic decrease for example.
        /// </summary>
        public void Force(double value)
        {
            val = Math.Max(Math.Min(value, max), min);
            valPix = ValueToPixel(val);
            Remap();
            Invalidate();

            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (double.IsNaN(valPix))
                return;
            
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            int top = -3;

            e.Graphics.DrawImageUnscaled(gutterLeft, 0, top);
            e.Graphics.DrawImageUnscaled(gutterRight, this.Width - gutterRight.Width, top);
            e.Graphics.DrawImage(gutterCenter, gutterLeft.Width, top, this.Width - gutterRight.Width - gutterLeft.Width, gutterCenter.Height);
            e.Graphics.DrawImageUnscaled(cursor, (int)(valPix - (cursor.Width/2)), top);
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
            Stick();
            Invalidate();
            
            if(ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }
        
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            
            valPix = Math.Max(Math.Min(e.X, maxPix), minPix);
            val = PixelToValue(valPix);
            Stick();
            Invalidate();
            
            if(ValueChanged!=null)
                ValueChanged(this, EventArgs.Empty);
        }
        
        private void Stick()
        {
            if (sticky && val >= stickyValue - stickyRadius && val <= stickyValue + stickyRadius)
            {
                val = stickyValue;
                valPix = ValueToPixel(val);
            }
        }

        private void Remap()
        {
            valPix = ValueToPixel(val);
        }
        
        private double ValueToPixel(double v)
        {
            v = Math.Min(Math.Max(v, min), max);
            double vNormalized = (v - min) / (max - min);
            double p = minPix + (vNormalized * (maxPix - minPix));
            return p;
        }
        
        private double PixelToValue(double p)
        {
            p = Math.Min(Math.Max(p, minPix), maxPix);
            double pNormalized = (p - minPix) / (maxPix - minPix);
            double v = min + (pNormalized * (max - min));
            return v;
        }
    }
}
