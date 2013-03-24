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
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public class ControlAnimation
    {
        public DateTime StartTime { get; set; }
        public int Duration { get; private set; }
        public Point Motion { get; private set; }
        public Point Start { get; private set; }
        public Control Control { get; private set; }
        
        public ControlAnimation(Control control, Point start, Point motion, int duration)
        {
            this.Control = control;
            this.Start = start;
            this.Motion = motion;
            this.Duration = duration;
        }
    }
}
