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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// FormSetTrajectoryOrigin is a dialog that let the user specify the 
	/// trajectory's coordinate system origin.
	/// This is then used in spreadsheet export.
	/// </summary>
	public partial class formSetTrajectoryOrigin : Form
	{
		#region Members
        private Bitmap m_bmpPreview;
        private Size m_OriginalSize;
        private Point m_Origin = new Point(-1,-1);
        private double m_Scale = 1.0f;
        private Pen m_PenCurrent;
        private Pen m_PenSelected = Pens.Red;
        private Point m_MousePosition;
        private CalibrationHelper m_CalibrationHelper;
        private Font m_FontText = new Font("Arial", 8, FontStyle.Bold);
        #endregion
		
        #region Constructor
		public formSetTrajectoryOrigin(Bitmap _bmpPreview, CalibrationHelper _calibrationHelper, Size _originalSize)
		{
			m_bmpPreview = _bmpPreview;
			m_CalibrationHelper = _calibrationHelper;
			m_OriginalSize = _originalSize;
			
			m_PenCurrent = new Pen(m_PenSelected.Color);
			m_PenCurrent.DashStyle = DashStyle.Dot;
			 
			if(_calibrationHelper.CoordinatesOrigin.X >= 0 && _calibrationHelper.CoordinatesOrigin.Y >= 0)
            	m_Origin = _calibrationHelper.CoordinatesOrigin;
			
			InitializeComponent();
			
			// Culture
            this.Text = "   " + ScreenManagerLang.dlgSetTrajectoryOrigin_Title;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
		}
		#endregion

		#region Auto Events
		private void formSetTrajectoryOrigin_Load(object sender, EventArgs e)
		{
            RatioStretch();
            picPreview.Invalidate();
		}
		private void picPreview_MouseMove(object sender, MouseEventArgs e)
		{
		    m_MousePosition = e.Location;
		    picPreview.Invalidate();
		}
		private void picPreview_Paint(object sender, PaintEventArgs e)
		{
			// Afficher l'image.
            if (m_bmpPreview == null)
                return;
            
            e.Graphics.DrawImage(m_bmpPreview, 0, 0, picPreview.Width, picPreview.Height);
            
            if(m_Origin.X < 0 || m_Origin.Y < 0)
                return;
            
            // Selected system
            Point scaledOrigin = m_Origin.Scale(m_Scale, m_Scale);
            e.Graphics.DrawLine(Pens.Red, 0, scaledOrigin.Y, picPreview.Width, scaledOrigin.Y);
            e.Graphics.DrawLine(Pens.Red, scaledOrigin.X, 0, scaledOrigin.X, picPreview.Height);
            
            // On the fly system
            e.Graphics.DrawLine(m_PenCurrent, 0, m_MousePosition.Y, picPreview.Width, m_MousePosition.Y);
            e.Graphics.DrawLine(m_PenCurrent, m_MousePosition.X, 0, m_MousePosition.X, picPreview.Height);
            
            // Text
            Point descaledMouse = Descale(m_MousePosition);
            Point descaledDelta = new Point(descaledMouse.X - m_Origin.X, descaledMouse.Y - m_Origin.Y);
            string textX = m_CalibrationHelper.GetLengthText((double)descaledDelta.X, false, false);
			string textY = m_CalibrationHelper.GetLengthText((double)descaledDelta.Y, false, false);
			string text = String.Format("{{{0};{1}}} {2}", textX, textY, m_CalibrationHelper.GetLengthAbbreviation());
			
			int textCoordX = m_MousePosition.X - 70;
			int textCoordY = m_MousePosition.Y + 2;
			if(textCoordX < 0)
			    textCoordX = m_MousePosition.X;
			if(textCoordY > picPreview.Height - 15)
			    textCoordY = m_MousePosition.Y - 15;
			
			e.Graphics.DrawString(text, m_FontText, (SolidBrush)Brushes.Red, (float)textCoordX, (float)textCoordY);
		}
		private void pnlPreview_Resize(object sender, EventArgs e)
		{
			RatioStretch();
			picPreview.Invalidate();
		}
		#endregion
		
		#region User triggered events
		private void picPreview_MouseClick(object sender, MouseEventArgs e)
		{
		    if(e.Button != MouseButtons.Left)
		        return;
		    
		    m_Origin = Descale(e.Location);
			picPreview.Invalidate();
		}
		private void btnOK_Click(object sender, EventArgs e)
		{
			m_CalibrationHelper.CoordinatesOrigin = m_Origin;
		}
		#endregion
		
		#region low level
		private void RatioStretch()
        {
			// Resizes the picture box to maximize the image in the panel.
			// This method might be similar to others, notably in FormPreviewVideoFilter.

			if (m_bmpPreview == null)
                return;
            
            double widthRatio = (double)pnlPreview.Width / m_bmpPreview.Width;
            double heightRatio = (double)pnlPreview.Height / m_bmpPreview.Height;

            if (widthRatio < heightRatio)
                picPreview.Size = new Size(pnlPreview.Width, (int)(m_bmpPreview.Height * widthRatio));
            else
                picPreview.Size = new Size((int)(m_bmpPreview.Width * heightRatio), pnlPreview.Height);

            m_Scale = (double)picPreview.Width / m_OriginalSize.Width;
            
            picPreview.Left = (pnlPreview.Width - picPreview.Width) / 2;
            picPreview.Top = (pnlPreview.Height - picPreview.Height) / 2;
        }
		private Point Descale(Point _point)
		{
		    return new Point((int)(_point.X / m_Scale), (int)(_point.Y / m_Scale));
		}
		#endregion
	}
}
