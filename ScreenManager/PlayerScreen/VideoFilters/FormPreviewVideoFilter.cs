#region License
/*
Copyright © Joan Charmant 2009.
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
		#region Members
        private Bitmap m_bmpPreview = null;
		#endregion
        
        public formPreviewVideoFilter(Bitmap _bmpPreview, string _windowTitle)
        {
        	m_bmpPreview = _bmpPreview;
            InitializeComponent();

            // Culture
            ResourceManager resManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            
			this.Text = "   " + _windowTitle;
            btnOK.Text = resManager.GetString("Generic_Apply", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = resManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
        }
        private void formFilterTuner_Load(object sender, EventArgs e)
        {
            RatioStretch();
            picPreview.Invalidate();
        }

        private void picPreview_Paint(object sender, PaintEventArgs e)
        {
            // Afficher l'image.
            if (m_bmpPreview != null)
            {
                e.Graphics.DrawImage(m_bmpPreview, 0, 0, picPreview.Width, picPreview.Height);
            }     
        }
        private void RatioStretch()
        {
            // Agrandi la picturebox pour maximisation dans le panel.
            if (m_bmpPreview != null)
            {
                float WidthRatio = (float)m_bmpPreview.Width / pnlPreview.Width;
                float HeightRatio = (float)m_bmpPreview.Height / pnlPreview.Height;

                //Redimensionner l'image selon la dimension la plus proche de la taille du panel.
                if (WidthRatio > HeightRatio)
                {
                    picPreview.Width = pnlPreview.Width;
                    picPreview.Height = (int)((float)m_bmpPreview.Height / WidthRatio);
                }
                else
                {
                    picPreview.Width = (int)((float)m_bmpPreview.Width / HeightRatio);
                    picPreview.Height = pnlPreview.Height;
                }

                //recentrer
                picPreview.Left = (pnlPreview.Width / 2) - (picPreview.Width / 2);
                picPreview.Top = (pnlPreview.Height / 2) - (picPreview.Height / 2);
            }
        }
    }
}
