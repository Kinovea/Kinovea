#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A class that will animate several controls independently.
    /// </summary>
    public class ControlAnimator
    {
        public event EventHandler AnimationsFinished;
        
        private Timer timer = new Timer();
        private List<ControlAnimation> animations = new List<ControlAnimation>();
        
        public ControlAnimator()
        {
            timer.Interval = 15;
            timer.Tick += new EventHandler(Timer_Tick);
        }

        public void Animate(Control control, Point motion, int duration)
        {
            if(motion == Point.Empty)
                return;

            ControlAnimation anim = new ControlAnimation(control, control.Location, motion, duration);
            anim.StartTime = DateTime.Now;
            animations.Add(anim);
            if(!timer.Enabled)
                timer.Enabled = true;
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            bool stillRunning = false;
            foreach(ControlAnimation anim in animations)
            {
                bool running = AnimateControl(anim);
                if(running)
                    stillRunning = true;
            }
            
            if(!stillRunning)
            {
                timer.Enabled = false;
                if(AnimationsFinished != null)
                    AnimationsFinished(this, EventArgs.Empty);
            }
        }
        
        private bool AnimateControl(ControlAnimation anim)
        {
            double span = (DateTime.Now - anim.StartTime).TotalMilliseconds;
            double timePosition = span / anim.Duration;
            if(timePosition > 1.0)
                timePosition = 1.0;
            
            // Quartic easing in.
            double factor = timePosition * timePosition * timePosition * timePosition;
            
            Point shift = anim.Motion.Scale(factor, factor);
            anim.Control.Location = anim.Start.Translate(shift.X, shift.Y);
            
            return timePosition < 1.0;
        }
        
        public void Clear()
        {
            animations.Clear();
        }
    }
}
