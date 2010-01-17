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
        //private Track m_Track;
        private Point m_ScaledOrigin = new Point(-1,-1);	// Selected point in display size coordinates.
        private Point m_RealOrigin = new Point(-1,-1);		// Selected point in image size coordinates.
        private float m_fRatio = 1.0f;
        private Pen m_PenCurrent;
        private Pen m_PenSelected;
        private string m_Text;								// Current mouse coord in the new coordinate system.
        private Point m_CurrentMouse;
        private Metadata m_ParentMetadata;
        #endregion
		
        #region Constructor
		public formSetTrajectoryOrigin(Bitmap _bmpPreview, Metadata _ParentMetadata)
		{
			// Init data.
			m_ParentMetadata = _ParentMetadata;
			m_bmpPreview = _bmpPreview;
			m_PenSelected = new Pen(Color.Red);
			m_PenCurrent = new Pen(Color.Red);
			m_PenCurrent.DashStyle = DashStyle.Dot;
			 
			if(m_ParentMetadata.CalibrationHelper.CoordinatesOrigin.X >= 0 && m_ParentMetadata.CalibrationHelper.CoordinatesOrigin.Y >= 0)
            {
            	m_RealOrigin = m_ParentMetadata.CalibrationHelper.CoordinatesOrigin;
            }
			
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
            UpdateScaledOrigin();
            picPreview.Invalidate();
		}
		private void picPreview_MouseMove(object sender, MouseEventArgs e)
		{
			// Save current mouse position and coordinates in the new system.
			m_CurrentMouse = new Point(e.X, e.Y);
			
			int iCoordX = (int)((float)e.X * m_fRatio) - m_RealOrigin.X;
			int iCoordY = m_RealOrigin.Y - (int)((float)e.Y * m_fRatio);
			string textX = m_ParentMetadata.CalibrationHelper.GetLengthText((double)iCoordX, false, false);
			string textY = m_ParentMetadata.CalibrationHelper.GetLengthText((double)iCoordY, false, false);
			m_Text = String.Format("{{{0};{1}}} {2}", textX, textY, m_ParentMetadata.CalibrationHelper.GetAbbreviation());
			
			picPreview.Invalidate();
		}
		private void picPreview_Paint(object sender, PaintEventArgs e)
		{
			// Afficher l'image.
            if (m_bmpPreview != null)
            {
                e.Graphics.DrawImage(m_bmpPreview, 0, 0, picPreview.Width, picPreview.Height);
            	
                if(m_ScaledOrigin.X >= 0 && m_ScaledOrigin.Y >= 0)
                {
        			// Selected Coordinate system.
					e.Graphics.DrawLine(m_PenSelected, 0, m_ScaledOrigin.Y, e.ClipRectangle.Width, m_ScaledOrigin.Y);
               		e.Graphics.DrawLine(m_PenSelected, m_ScaledOrigin.X, 0, m_ScaledOrigin.X, e.ClipRectangle.Height);
                
               		// Current Mouse system.
               		e.Graphics.DrawLine(m_PenCurrent, 0, m_CurrentMouse.Y, e.ClipRectangle.Width, m_CurrentMouse.Y);
               		e.Graphics.DrawLine(m_PenCurrent, m_CurrentMouse.X, 0, m_CurrentMouse.X, e.ClipRectangle.Height);
                
                	// Current pos.
                	Font fontText = new Font("Arial", 8, FontStyle.Bold);
        			SolidBrush fontBrush = new SolidBrush(m_PenSelected.Color);
                	e.Graphics.DrawString(m_Text, fontText, fontBrush, m_CurrentMouse.X - 67,m_CurrentMouse.Y + 2);
                }
            }
		}
		private void pnlPreview_Resize(object sender, EventArgs e)
		{
			RatioStretch();
			UpdateScaledOrigin();
			picPreview.Invalidate();
		}
		#endregion
		
		#region User triggered events
		private void picPreview_MouseClick(object sender, MouseEventArgs e)
		{
			// User selected an origin point.
			m_ScaledOrigin = new Point(e.X, e.Y);

			int iLeft = (int)((float)e.X * m_fRatio);
			int iTop = (int)((float)e.Y * m_fRatio);
			m_RealOrigin = new Point(iLeft, iTop);
			
			picPreview.Invalidate();
		}
		private void btnOK_Click(object sender, EventArgs e)
		{
			m_ParentMetadata.CalibrationHelper.CoordinatesOrigin = m_RealOrigin;
		}
		#endregion
		
		#region low level
		private void RatioStretch()
        {
			// Resizes the picture box to maximize the image in the panel.
			
			// This method directly pasted from FormPreviewVideoFilter.
			// Todo: avoid duplication and factorize the two dialogs ?
			
            if (m_bmpPreview != null)
            {
                float WidthRatio = (float)m_bmpPreview.Width / pnlPreview.Width;
                float HeightRatio = (float)m_bmpPreview.Height / pnlPreview.Height;

                //Redimensionner l'image selon la dimension la plus proche de la taille du panel.
                if (WidthRatio > HeightRatio)
                {
                    picPreview.Width = pnlPreview.Width;
                    picPreview.Height = (int)((float)m_bmpPreview.Height / WidthRatio);
                	m_fRatio = WidthRatio;
                }
                else
                {
                    picPreview.Width = (int)((float)m_bmpPreview.Width / HeightRatio);
                    picPreview.Height = pnlPreview.Height;
                    m_fRatio = HeightRatio;
                }

                // Centering.
                picPreview.Left = (pnlPreview.Width / 2) - (picPreview.Width / 2);
                picPreview.Top = (pnlPreview.Height / 2) - (picPreview.Height / 2);
            }
        }
		private void UpdateScaledOrigin()
		{
			if(m_RealOrigin.X >= 0 && m_RealOrigin.Y >= 0)
            {
				int iLeft = (int)((float)m_RealOrigin.X / m_fRatio);
				int iTop = (int)((float)m_RealOrigin.Y / m_fRatio);
				m_ScaledOrigin = new Point(iLeft, iTop);
			}
			else
			{
				m_ScaledOrigin = new Point(-1, -1);
			}
		}
		#endregion
	}
}
