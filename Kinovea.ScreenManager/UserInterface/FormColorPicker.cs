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
using Kinovea.ScreenManager.Languages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// FormColorPicker. Let the user choose a color.
    /// The color picker control itself is based on the one from Greenshot. 
    /// </summary>
    public partial class FormColorPicker : Form
    {
        #region Properties
        public Color PickedColor
        {
            get { return pickedColor; }
        }
        #endregion

        #region Members
        private ColorPicker colorPicker;
        private Color pickedColor;
        private Color currentColor;
        private List<Color> recentColors;
        #endregion
        
        #region Construction and Initialization
        public FormColorPicker(Color currentColor)
        {
            this.currentColor = currentColor;
            this.SuspendLayout();
            InitializeComponent();
            colorPicker = new ColorPicker(currentColor);
            colorPicker.Top = 5;
            colorPicker.Left = 5;
            colorPicker.ColorPicked += colorPicker_ColorPicked;
            
            Controls.Add(colorPicker);
            this.ResumeLayout();
            
            // Recent colors.
            recentColors = PreferencesManager.PlayerPreferences.RecentColors;
            
            colorPicker.DisplayRecentColors(recentColors);
            this.Height = colorPicker.Bottom + 20;
        }
        #endregion
        
        #region event handlers
        private void colorPicker_ColorPicked(object sender, System.EventArgs e)
        {
            pickedColor = colorPicker.PickedColor;
            PreferencesManager.PlayerPreferences.AddRecentColor(colorPicker.PickedColor);
            DialogResult = DialogResult.OK;
            Close();
        }
        #endregion
    }
}
