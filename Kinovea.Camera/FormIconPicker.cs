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
using System.Linq;
using System.Windows.Forms;

namespace Kinovea.Camera
{
    public partial class FormIconPicker : Form
    {
        public Bitmap PickedIcon
        {
            get { return pickedIcon;}
        }
        
        private Bitmap pickedIcon;
        private IEnumerable<Bitmap> icons;
        private int columns;
        
        public FormIconPicker(IEnumerable<Bitmap> icons, int columns, string title)
        {
            this.icons = icons;
            this.columns = columns;
            this.Text = title;
            
            InitializeComponent();
            InitializeButtons();
        }
        
        private void InitializeButtons()
        {
            int buttonSize = 16;
            int internalMargin = 5;
            int externalMargin = 10;
            int cols = columns;
            
            int index = 0;
            foreach(Bitmap bitmap in icons)
            {
                Button button = new Button();
                button.BackgroundImage = bitmap;
                button.FlatStyle = FlatStyle.Flat;
                button.BackColor = Color.Transparent;
                button.FlatAppearance.BorderSize = 0;
                
                int row = index / cols;
                int col = index - (row * cols);
                int left = externalMargin + ((buttonSize + internalMargin) * col);
                int top = externalMargin + ((buttonSize + internalMargin) * row);
                
                button.Top = top;
                button.Left = left;
                button.Width = buttonSize;
                button.Height = buttonSize;
                
                button.Tag = bitmap;
                button.Click += Button_Click;
                
                this.Controls.Add(button);
                
                index++;
            }
            
            Rectangle screenRectangle = RectangleToScreen(this.ClientRectangle);
            int titleBarHeight = screenRectangle.Top - this.Top;

            int buttonTotalWidth = ((buttonSize + internalMargin) * cols) - internalMargin;
            int totalRows = (icons.Count() / cols) + 1;
            int buttonTotalHeight = ((buttonSize + internalMargin) * totalRows) - internalMargin;
            
            this.Width = externalMargin + buttonTotalWidth + externalMargin;
            this.Height = titleBarHeight + externalMargin + buttonTotalHeight + externalMargin;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            
            Button button = sender as Button;
            if(button == null)
                return;
            Bitmap bitmap = button.Tag as Bitmap;
            if(bitmap != null)
                pickedIcon = bitmap;
        }
    }
}
