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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.Services;
using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager
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
			get { return false; }
		}
		#endregion
		
		#region Members
		private ToolStripMenuItem m_Menu;
		private List<DecompressedFrame> m_FrameList;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public VideoFilterMosaic()
		{
			ResourceManager resManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            
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
			if(m_Menu.Checked)
			{
				// Toggle off filter.
				DrawtimeFilterOutput dfo = new DrawtimeFilterOutput((int)VideoFilterType.Mosaic, false);
				ProcessingOver(dfo);
			}
			else
			{
				// 1. Display configuration dialog box.
				formConfigureMosaic fcm = new formConfigureMosaic(m_FrameList.Count);
				if (fcm.ShowDialog() == DialogResult.OK)
				{
					
					DrawtimeFilterOutput dfo = new DrawtimeFilterOutput((int)VideoFilterType.Mosaic, true);
					
					// Set up the output object so it becomes independant from this filter instance.
					// (otherwise another use of this filter on a video in the second screen will interfere)
					dfo.InputFrames = GetInputFrames(fcm.FramesToExtract);
					dfo.PrivateData = fcm.IsRightToLeft;
					dfo.Draw = new DelegateDraw(Draw);
					
					// Notify the ScreenManager that we are done.
					ProcessingOver(dfo);
					
				}
				fcm.Dispose();
			}
        }
		protected override void Process()
		{
			// Not implemented.
			// This filter process its imput at draw time only. See Draw().
		}
		#endregion
		
		#region Draw Implementation
		public static void Draw(Graphics g, Size _iNewSize, List<Bitmap> _inputFrames, object _privateData)
		{
			// This method will be called by a player screen at draw time.
			// static: the DrawingtimeFilterObject contains all that is needed to use the method.
			
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			if(_inputFrames != null && _inputFrames.Count > 0 && _privateData is bool )
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
				
				// Todo:
				// 1. Lock all images and get all BitmapData in an array.
				// 2. Loop on final image row and cols, and fill pixels by interpolating from the source images.
				// This should get down to a few ms instead of more than 500 ms for HD vids.
				
				int iSide = (int)Math.Ceiling(Math.Sqrt((double)_inputFrames.Count));
				int iThumbWidth = _iNewSize.Width / iSide;
				int iThumbHeight = _iNewSize.Height / iSide;
					
				Rectangle rSrc = new Rectangle(0, 0, _inputFrames[0].Width, _inputFrames[0].Height);
		
				for(int i=0;i<iSide;i++)
				{
					for(int j=0;j<iSide;j++)
					{
						int iImageIndex = j*iSide + i;
						if(iImageIndex < _inputFrames.Count && _inputFrames[iImageIndex] != null)
						{
							// compute left coord depending on "RightToLeft" status.
							int iLeft;
							if((bool)_privateData)
							{
								iLeft = (iSide - 1 - i)*iThumbWidth;
							}
							else
							{
								iLeft = i*iThumbWidth;
							}
							
							Rectangle rDst = new Rectangle(iLeft, j*iThumbHeight, iThumbWidth, iThumbHeight);
							g.DrawImage(_inputFrames[iImageIndex], rDst, rSrc, GraphicsUnit.Pixel);
						}
					}
				}
			}
			
			sw.Stop();
			log.Debug(String.Format("Mosaic Draw : {0} ms.", sw.ElapsedMilliseconds));
		}
		#endregion
		
		#region Private methods
		private List<Bitmap> GetInputFrames(int _iFramesToExtract)
		{
			// Get the subset of images we will be using for the mosaic.
			
		 	List<Bitmap> inputFrames = new List<Bitmap>();
			double fExtractStep = (double)m_FrameList.Count / _iFramesToExtract;

			int iExtracted = 0;
			for(int i=0;i<m_FrameList.Count;i++)
			{
				if(i >= iExtracted * fExtractStep)
				{
					inputFrames.Add(m_FrameList[i].BmpImage);
					iExtracted++;
				}
			}
			
			return inputFrames;
		}
		#endregion
	}
}


