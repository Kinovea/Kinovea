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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Videa.ScreenManager
{
	/// <summary>
	/// The mosaic class encapsulate the behavior of the mosaic view mode.
	/// In this mode, the video is shown as a table of thumbnails.
	/// Thumbnails are either retrieved from kayframes or from a frequency sampling of the video.
	/// </summary>
	public class Mosaic
	{
		#region Properties
        public bool Enabled
        {
            get { return m_bEnabled; }
            set { m_bEnabled = value; }
        }        
		public bool KeyImagesOnly 
		{
			get { return m_bKeyImagesOnly; }
			set { m_bKeyImagesOnly = value; }
		}        
        public bool MemoStretchMode
        {
            get { return m_bMemoStretchMode; }
            set { m_bMemoStretchMode = value; }
        }
        public int LastImagesCount 
        {
			get { return m_iLastImagesCount; }
			set { m_iLastImagesCount = value; }
		}
		public bool RightToLeft 
		{
			get { return m_bRightToLeft; }
			set { m_bRightToLeft = value; }
		}
        #endregion
        
        #region Members
        private bool m_bEnabled = false;
        private bool m_bKeyImagesOnly = false;
        private bool m_bMemoStretchMode = false;
        private int m_iLastImagesCount = -1;
        private bool m_bRightToLeft = false;
        private List<Bitmap> m_Images = new List<Bitmap>();
        #endregion
		
		public Mosaic()
		{
		}
		
		public void Disable()
		{
			m_bEnabled = false;
			m_Images.Clear();
		}
		public void Load(List<Bitmap> _images)
		{
			m_Images.Clear();
			m_Images.AddRange(_images);
			
			if(m_Images.Count > 0)
			{
				m_bEnabled = true;
				m_iLastImagesCount = m_Images.Count;
			}	
			else
			{
				m_iLastImagesCount = -1;
			}
		}
		public void Draw(Graphics g, Size _iNewSize)
		{
			// Organize and draw all images on the canvas.
			
			if(m_Images.Count > 0 && m_Images[0] != null)
			{
				g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
				g.CompositingQuality = CompositingQuality.HighSpeed;
				g.InterpolationMode = InterpolationMode.Bilinear;
				g.SmoothingMode = SmoothingMode.None;
				
				//---------------------------------------------------------------------------
				// We know that they all have the main size.
				// We reserve n² placeholders, so we have exactly as many images on width than on height.
				// Example: 
				// - 32 images.
				// - We get up to next square root : 36, iSide is 6.
				// - This will be 6x6 images (with the last 4 not filled)
				// - Each image must be scaled down by a factor of 1/6.
				//---------------------------------------------------------------------------
				
				int iSide = (int)Math.Ceiling(Math.Sqrt((double)m_Images.Count));
				int iThumbWidth = _iNewSize.Width / iSide;
				int iThumbHeight = _iNewSize.Height / iSide;
				
				Rectangle rSrc = new Rectangle(0, 0, m_Images[0].Width, m_Images[0].Height);

				for(int i=0;i<iSide;i++)
				{
					for(int j=0;j<iSide;j++)
					{
						int iImageIndex = j*iSide + i;
						if(iImageIndex < m_Images.Count && m_Images[iImageIndex] != null)
						{
							// compute left coord depending on "RightToLeft" status.
							int iLeft;
							if(m_bRightToLeft)
							{
								iLeft = (iSide - 1 - i)*iThumbWidth;
							}
							else
							{
								iLeft = i*iThumbWidth;
							}
							
							Rectangle rDst = new Rectangle(iLeft, j*iThumbHeight, iThumbWidth, iThumbHeight);
							g.DrawImage(m_Images[iImageIndex], rDst, rSrc, GraphicsUnit.Pixel);
						}
					}
				}
			}			
		}
	}
}
