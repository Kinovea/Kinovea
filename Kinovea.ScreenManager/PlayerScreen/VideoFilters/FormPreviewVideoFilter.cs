#region License
/*
Copyright © Joan Charmant 2009.
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
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// formPreviewVideoFilter is a dialog to let the user decide 
    /// if he really wants to apply a given filter.
    /// No configuration is needed for the filter but the operation may be destrcutive
    /// so we better ask him to confirm.
    /// </summary>
    public partial class formPreviewVideoFilter : Form
    {
        private Bitmap bmpPreview = null;
        
        public formPreviewVideoFilter(Bitmap bmpPreview, string windowTitle)
        {
            this.bmpPreview = bmpPreview;
            InitializeComponent();

            this.Text = "   " + windowTitle;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        private void formFilterTuner_Load(object sender, EventArgs e)
        {
            RatioStretch();
            picPreview.Invalidate();
        }
        private void picPreview_Paint(object sender, PaintEventArgs e)
        {
            if (bmpPreview != null)
                e.Graphics.DrawImage(bmpPreview, 0, 0, picPreview.Width, picPreview.Height);
        }
        private void RatioStretch()
        {
            if (bmpPreview == null)
                return;

            Rectangle r = UIHelper.RatioStretch(bmpPreview.Size, pnlPreview.Size);
            picPreview.Size = r.Size;
            picPreview.Location = r.Location;
        }
    }
}
