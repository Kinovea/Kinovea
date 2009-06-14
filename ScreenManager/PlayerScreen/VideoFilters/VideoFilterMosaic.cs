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
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using AForge.Imaging;
using AForge.Imaging.Filters;
using Videa.Services;
using VideaPlayerServer;

namespace Videa.ScreenManager
{
	/// <summary>
	/// VideoFilterMosaic.
	/// - Input			: Subset of all images (or all key images (?)).
	/// - Output		: One image, same size.
	/// - Operation 	: Combine input images into a single view.
	/// - Type 			: Called at draw time.
	/// - Previewable 	: No.
	/// </summary>
	public class VideoFilterMosaic : AbstractVideoFilter
	{
		#region Properties
		public override ToolStripMenuItem Menu
		{
			get { return m_Menu; }
		}	
		public override List<DecompressedFrame> FrameList
        {
			set { m_FrameList = value; }
        }
		public override bool Experimental 
		{
			get { return true; }
		}
		#endregion
		
		#region Members
		private ToolStripMenuItem m_Menu;
		private List<DecompressedFrame> m_FrameList;
		private List<Bitmap> m_InputFrames;
		//private bool m_bKeyImagesOnly = false;
		private bool m_bIsRightToLeft = false;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public VideoFilterMosaic()
		{
			ResourceManager resManager = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            
			// Menu
            m_Menu = new ToolStripMenuItem();
            m_Menu.Tag = new ItemResourceInfo(resManager, "VideoFilterMosaic_FriendlyName");
            m_Menu.Text = ((ItemResourceInfo)m_Menu.Tag).resManager.GetString(((ItemResourceInfo)m_Menu.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            m_Menu.Click += new EventHandler(Menu_OnClick);
            m_Menu.MergeAction = MergeAction.Append;
		}
		#endregion
		
		#region AbstractVideoFilter Implementation
		public override void Menu_OnClick(object sender, EventArgs e)
        {
			// 1. Display configuration dialog box.
			formConfigureMosaic fcm = new formConfigureMosaic(m_FrameList.Count);
			if (fcm.ShowDialog() == DialogResult.OK)
			{
				SetInputFrames(fcm.FramesToExtract);
				m_bIsRightToLeft = fcm.IsRightToLeft;
				
				DrawtimeFilterOutput dfo = new DrawtimeFilterOutput();
				dfo.Draw = new DelegateDraw(Draw);
				
				// Notify the ScreenManager that we are done.
				ProcessingOver(dfo);
			}
			fcm.Dispose();
        }
		protected override void Process()
		{
			// Not implemented.
			// This filter process its imput at draw time only. See Draw().
		}
		#endregion
		
		#region Draw Implementation
		public void Draw(Graphics g, Size _iNewSize)
		{
			// This method will be called by a player screen at draw time.
			
			if(m_InputFrames != null && m_InputFrames.Count > 0)
			{
				g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
				g.CompositingQuality = CompositingQuality.HighSpeed;
				g.InterpolationMode = InterpolationMode.Bilinear;
				g.SmoothingMode = SmoothingMode.None;
				
				//---------------------------------------------------------------------------
				// We reserve n² placeholders, so we have exactly as many images on width than on height.
				// Example: 
				// - 32 images as input.
				// - We get up to next square root : 36. iSide is thus 6.
				// - This will be 6x6 images (with the last 4 not filled)
				// - Each image must be scaled down by a factor of 1/6.
				//---------------------------------------------------------------------------
				
				int iSide = (int)Math.Ceiling(Math.Sqrt((double)m_InputFrames.Count));
				int iThumbWidth = _iNewSize.Width / iSide;
				int iThumbHeight = _iNewSize.Height / iSide;
					
				Rectangle rSrc = new Rectangle(0, 0, m_InputFrames[0].Width, m_InputFrames[0].Height);
		
				for(int i=0;i<iSide;i++)
				{
					for(int j=0;j<iSide;j++)
					{
						int iImageIndex = j*iSide + i;
						if(iImageIndex < m_InputFrames.Count && m_InputFrames[iImageIndex] != null)
						{
							// compute left coord depending on "RightToLeft" status.
							int iLeft;
							if(m_bIsRightToLeft)
							{
								iLeft = (iSide - 1 - i)*iThumbWidth;
							}
							else
							{
								iLeft = i*iThumbWidth;
							}
							
							Rectangle rDst = new Rectangle(iLeft, j*iThumbHeight, iThumbWidth, iThumbHeight);
							g.DrawImage(m_InputFrames[iImageIndex], rDst, rSrc, GraphicsUnit.Pixel);
						}
					}
				}
			}
		}
		#endregion
		
		#region Private methods
		private void SetInputFrames(int _iFramesToExtract)
		{
			// Get the subset of images we will be using for the mosaic.
			
			m_InputFrames = new List<Bitmap>();
			double fExtractStep = (double)m_FrameList.Count / _iFramesToExtract;

			int iExtracted = 0;
			for(int i=0;i<m_FrameList.Count;i++)
			{
				if(i >= iExtracted * fExtractStep)
				{
					m_InputFrames.Add(m_FrameList[i].BmpImage);
					iExtracted++;
				}
			}
		}
		#endregion
	}
}


