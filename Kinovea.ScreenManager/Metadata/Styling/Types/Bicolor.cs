#region License
/*
Copyright © Joan Charmant 2011.
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
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A simple wrapper around two color values.
    /// When setting the background color, the foreground color is automatically adjusted 
    /// to black or white depending on the luminosity of the background color.
    /// </summary>
    public struct Bicolor
    {
        public Color Foreground
        {
            get { return foreground; }
        }
        public Color Background
        {
            get { return background; }
            set
            {
                background = value;
                foreground = value.GetBrightness() >= 0.5 ? Color.Black : Color.White;
            }
        }
        public int ContentHash
        {
            get
            {
                return background.GetHashCode() ^ foreground.GetHashCode();
            }
        }

        private Color foreground;
        private Color background;

        public Bicolor(Color backColor)
        {
            background = backColor;
            foreground = backColor.GetBrightness() >= 0.5 ? Color.Black : Color.White;
        }
    }
}
