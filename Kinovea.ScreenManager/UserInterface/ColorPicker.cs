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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// ColorPicker. Let the user choose a color.
    /// 
    /// This color picker is heavily inspired by the Color Picker from Greenshot.
    /// The code for generating the palette is taken from Greenshot with almost no modifications.
    /// http://greenshot.sourceforge.net/
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        #region Events
        [Category("Action"), Browsable(true)]
        public event EventHandler ColorPicked;
        #endregion
        
        #region Properties
        public Color PickedColor
        {
            get { return pickedColor; }
        }
        #endregion
        
        #region Members
        private List<Button> buttons = new List<Button>();
        private Color pickedColor;
        private Color currentColor;
        private Button currentColorButton;
        private static readonly int buttonSize = 15;
        #endregion
        
        #region Construction and Initialization
        public ColorPicker(Color currentColor)
        {
            this.currentColor = currentColor;
            this.SuspendLayout();
            InitializeComponent();
            GeneratePalette(0, 0);
            this.ResumeLayout();
        }
        private void GeneratePalette(int left, int top) 
        {
            int shades = 11;
            
            createColorButtonColumn(255, 0, 0, left, top, shades);
            
            left += buttonSize;
            createColorButtonColumn(255, 255/2, 0, left, top, shades);
            
            left += buttonSize;
            createColorButtonColumn(255, 255, 0, left, top, shades);
            
            left += buttonSize;
            createColorButtonColumn(255/2, 255, 0, left, top, shades);
            
            left += buttonSize;
            createColorButtonColumn(0, 255, 0, left, top, shades);
            
            left += buttonSize;
            createColorButtonColumn(0, 255, 255/2, left, top, shades);
            
            left += buttonSize;
            createColorButtonColumn(0,255,255, left, top, shades);
            
            left += buttonSize;
            createColorButtonColumn(0,255/2,255, left, top, shades);
            
            left += buttonSize;
            createColorButtonColumn(0,0,255, left, top, shades);
            
            left += buttonSize;
            createColorButtonColumn(255/2,0,255, left, top, shades);
            
            left += buttonSize;
            createColorButtonColumn(255,0,255, left, top, shades);
            
            left += buttonSize;
            createColorButtonColumn(255,0,255/2, left, top, shades);
            
            // Grayscale column.
            left += buttonSize + 5;
            createColorButtonColumn(255/2,255/2,255/2, left, top, shades);
            
            Controls.AddRange(buttons.ToArray());
        }
        private void createColorButtonColumn(int red, int green, int blue, int x, int y, int shades) 
        {
            int shadedColorsNum = (shades - 1) / 2;
            
            for(int i=0; i <= shadedColorsNum; i++)
            {
                buttons.Add(createColorButton(red * i / shadedColorsNum, green * i / shadedColorsNum, blue * i / shadedColorsNum, x, y + i * buttonSize));

                if (i > 0)
                    buttons.Add(createColorButton(red + (255 - red) * i / shadedColorsNum, green + (255 - green) * i / shadedColorsNum, blue + (255 - blue) * i / shadedColorsNum, x, y + (i + shadedColorsNum) * buttonSize));
            }
        }

        private Button createColorButton(int red, int green, int blue, int x, int y) 
        {
            return createColorButton(Color.FromArgb(255, red, green, blue), x, y);
        }

        private Button createColorButton(Color color, int x, int y) 
        {
            Button b = new Button();
            
            b.BackColor = color;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = color;
            b.FlatAppearance.BorderColor = Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
            b.FlatStyle = FlatStyle.Flat;
            b.Location = new Point(x,y);
            b.Size = new Size(buttonSize, buttonSize);
            b.TabStop = false;

            if (currentColorButton == null && currentColor == color)
            {
                b.FlatAppearance.BorderSize = 1;
                currentColorButton = b;
            }

            b.Click += colorButton_Click;
            b.MouseEnter += colorButton_MouseEnter;
            b.MouseLeave += colorButton_MouseLeave;
            
            return b;
        }
        #endregion
        
        public void DisplayRecentColors(List<Color> _recentColors)
        {
            if(_recentColors.Count > 0)
            {
                Label lblRecent = new Label();
                lblRecent.AutoSize = true;
                
                lblRecent.Text = ScreenManagerLang.RecentlyUsedColors;
                lblRecent.Top = this.Margin.Top + (11 * buttonSize) + 30;
                Controls.Add(lblRecent);
                
                List<Button> recentButtons = new List<Button>();
                int x = 0;
                int y = lblRecent.Bottom + 5;
                for(int i=0; i<_recentColors.Count;i++)
                {
                    Button b = createColorButton(_recentColors[i], x, y);
                    recentButtons.Add(b);
                    x += buttonSize;
                }
                
                Controls.AddRange(recentButtons.ToArray());
            }
            
        }
        
        #region event handlers
        private void colorButton_Click(object sender, System.EventArgs e) 
        {
            Button b = sender as Button;
            if (b == null)
                return;

            pickedColor = b.BackColor;
            if(ColorPicked != null)
                ColorPicked(this, e);
        }
        private void colorButton_MouseEnter(object sender, System.EventArgs e) 
        {
            Button b = sender as Button;
            if (b != null)
                b.FlatAppearance.BorderSize = 1;
        }
        private void colorButton_MouseLeave(object sender, System.EventArgs e) 
        {
            Button b = sender as Button;

            if (currentColorButton == b)
                return;

            if (b != null)
                b.FlatAppearance.BorderSize = 0;
        }
        #endregion
    }
}
