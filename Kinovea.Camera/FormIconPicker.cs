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
        
        public FormIconPicker(IEnumerable<Bitmap> icons, int columns = 0)
        {
            this.icons = icons;
            
            InitializeComponent();
            InitializeButtons();
            this.Text = "";
        }
        
        private void InitializeButtons()
        {
            int buttonSize = 20;
            foreach (Bitmap bitmap in icons)
            {
                Button button = new Button();
                button.Image = bitmap;
                button.ImageAlign = ContentAlignment.MiddleCenter;
                button.FlatStyle = FlatStyle.Flat;
                button.BackColor = Color.Transparent;
                button.FlatAppearance.BorderSize = 0;
                button.Cursor = Cursors.Hand;
                
                button.Width = buttonSize;
                button.Height = buttonSize;
                button.Margin = new Padding(0, 0, 10, 10);

                button.Tag = bitmap;
                button.Click += Button_Click;

                pnlIcons.Controls.Add(button);
            }
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
