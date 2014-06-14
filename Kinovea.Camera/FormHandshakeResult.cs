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
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Kinovea.Camera
{
    public partial class FormHandshakeResult : Form
    {
        public FormHandshakeResult(Bitmap bitmap)
        {
            InitializeComponent();
            
            float ratioHeight = (float)bitmap.Height / pbImage.Height;
            float ratioWidth = (float)bitmap.Width / pbImage.Width;
            float ratio = Math.Max(ratioHeight, ratioWidth);
            Bitmap stretched = new Bitmap((int)(bitmap.Width / ratio), (int)(bitmap.Height / ratio), bitmap.PixelFormat);
            Graphics g = Graphics.FromImage(stretched);
            g.DrawImage(bitmap, 0, 0, stretched.Width, stretched.Height);
            
            pbImage.BackgroundImage = stretched;
        }
    }
}
