#region License
/*
Copyright © Joan Charmant 2010.
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
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// MessageToaster.
    /// A class to encapsulate the display of a message to be painted directly on a canvas, for a given duration.
    /// Used to indicate a change in the state of a screen for example. (Pause, Zoom factor).
    /// The same object is reused for various messages. Each screen should have its own instance.
    /// 
    /// Exemple use:
    /// MessageToaster toaster = new MessageToaster(control);
    /// 
    /// In the OnPaint event of the control:
    /// toaster.Draw(e.Graphics);
    /// 
    /// To display a message:
    /// toaster.SetDuration(750);
    /// toaster.Show("hello");
    /// </summary>
    public class MessageToaster
    {
        #region Properties
        public bool Enabled
        {
            get { return enabled; }
        }
        #endregion
        
        #region Members
        private string message;
        private Timer timer = new Timer();
        private Font font;
        private bool enabled;
        private Control canvasHolder;
        private static readonly int defaultDuration = 1000;
        private static readonly int defaultFontSize = 24;
        private Brush foreBrush = new SolidBrush(Color.FromArgb(255, Color.White));
        private Brush backBrush = new SolidBrush(Color.FromArgb(128, Color.Black));
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public MessageToaster(Control canvasHolder)
        {
            this.canvasHolder = canvasHolder;
            font = new Font("Arial", defaultFontSize, FontStyle.Bold);
            timer.Interval = defaultDuration;
            timer.Tick += new EventHandler(Timer_OnTick); 
        }
        #endregion
        
        #region Public Methods
        public void SetDuration(int duration)
        {
            timer.Interval = duration;
        }
        public void Show(string message)
        {
            log.Debug(String.Format("Toasting message: {0}", message));
            this.message = message;
            enabled = true;
            StartStopTimer();
        }
        public void Draw(Graphics canvas)
        {
            if(!enabled || string.IsNullOrEmpty(message) || canvasHolder == null)
                return;
            
            SizeF bgSize = canvas.MeasureString(message, font);
            bgSize = new SizeF(bgSize.Width, bgSize.Height + 3);
            PointF location = new PointF((canvasHolder.Width - bgSize.Width)/2, (canvasHolder.Height - bgSize.Height)/2);
            RectangleF bg = new RectangleF(location.X - 5, location.Y - 5, bgSize.Width + 10, bgSize.Height + 5);
            int radius = (int)(font.Size / 2);
            RoundedRectangle.Draw(canvas, bg, (SolidBrush)backBrush, radius, false, false, null);
            canvas.DrawString(message, font, foreBrush, location.X, location.Y);
        }
        #endregion
        
        #region Private methods
        private void Timer_OnTick(object sender, EventArgs e)
        {
            // Timer fired : Time to hide the message.
            enabled = false;
            StartStopTimer();
        }
        private void StartStopTimer()
        {
            if(enabled)
            {
                if(timer.Enabled)
                    timer.Stop();
                
                timer.Start();
            }
            else
            {
                timer.Stop();
                if(canvasHolder != null)
                    canvasHolder.Invalidate();
            }
        }
        #endregion
    }
}
