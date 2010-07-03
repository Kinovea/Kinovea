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
            m_Menu.Image = Properties.Resources.mosaic;
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
				// 2010-05-19 - Do not display configuration box anymore. 
				// The user can now directly scroll to change the parameter. (excep for right to left though.)
				DrawtimeFilterOutput dfo = new DrawtimeFilterOutput((int)VideoFilterType.Mosaic, true);
				dfo.PrivateData = new Parameters(ExtractBitmapList(m_FrameList), 16, false);
				dfo.Draw = new DelegateDraw(Draw);
				dfo.IncreaseZoom = new DelegateIncreaseZoom(IncreaseZoom);
				dfo.DecreaseZoom = new DelegateDecreaseZoom(DecreaseZoom);
				ProcessingOver(dfo);
			}
        }
		protected override void Process()
		{
			// Not implemented.
			// This filter process its imput frames at draw time only. See Draw().
		}
		#endregion
		
		#region DrawtimeFilterOutput Implementation
		public static void Draw(Graphics g, Size _iNewSize, object _privateData)
		{
			//-----------------------------------------------------------------------------------
			// This method will be called by a player screen at draw time.
			// static: the DrawingtimeFilterObject contains all that is needed to use the method.
			// Most notably, the _privateData parameters contains references to the frames
			// to be combined, the zoom level and if the composite is right to left or not.
			//-----------------------------------------------------------------------------------
			
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			Parameters parameters = _privateData as Parameters;
			
			if(parameters != null)
			{
				// We recompute the image at each draw time.
				// We could have only computed it on first creation and on resize, 
				// but in the end it doesn't matter.
				List<Bitmap> selectedFrames = GetInputFrames(parameters.FrameList, parameters.FramesToExtract);	
				
				if(selectedFrames != null && selectedFrames.Count > 0)
				{
					g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
					g.CompositingQuality = CompositingQuality.HighSpeed;
					g.InterpolationMode = InterpolationMode.Bilinear;
					g.SmoothingMode = SmoothingMode.HighQuality;
					
					//---------------------------------------------------------------------------
					// We reserve n² placeholders, so we have exactly as many images on width than on height.
					// Example: 
					// - 32 images as input.
					// - We get up to next square root : 36. iSide is thus 6.
					// - This will be 6x6 images (with the last 4 not filled)
					// - Each image must be scaled down by a factor of 1/6.
					//---------------------------------------------------------------------------
					
					// to test:
					// 1. Lock all images and get all BitmapData in an array.
					// 2. Loop on final image pixels, and fill in by interpolating from the source images.
					// This should get down to a few ms instead of more than 500 ms for HD vids.
					
					int iSide = (int)Math.Ceiling(Math.Sqrt((double)selectedFrames.Count));
					int iThumbWidth = _iNewSize.Width / iSide;
					int iThumbHeight = _iNewSize.Height / iSide;
						
					Rectangle rSrc = new Rectangle(0, 0, selectedFrames[0].Width, selectedFrames[0].Height);
			
					// Configure font for image numbers.
					Font f = new Font("Arial", GetFontSize(iThumbWidth), FontStyle.Bold);
					
					for(int i=0;i<iSide;i++)
					{
						for(int j=0;j<iSide;j++)
						{
							int iImageIndex = j*iSide + i;
							if(iImageIndex < selectedFrames.Count && selectedFrames[iImageIndex] != null)
							{
								// compute left coord depending on "RightToLeft" status.
								int iLeft;
								if(parameters.RightToLeft)
								{
									iLeft = (iSide - 1 - i)*iThumbWidth;
								}
								else
								{
									iLeft = i*iThumbWidth;
								}
								
								Rectangle rDst = new Rectangle(iLeft, j*iThumbHeight, iThumbWidth, iThumbHeight);
								g.DrawImage(selectedFrames[iImageIndex], rDst, rSrc, GraphicsUnit.Pixel);
								
								// Draw the image number.
								DrawImageNumber(g, iImageIndex, rDst, f);
							}
						}
					}
				}					
			}
			
			sw.Stop();
			log.Debug(String.Format("Mosaic Draw : {0} ms.", sw.ElapsedMilliseconds));
		}
		public static void IncreaseZoom(object _privateData)
		{
			Parameters parameters = _privateData as Parameters;
			if(parameters != null)
			{
				parameters.ChangeFrameCount(true);
			}
		}
		public static void DecreaseZoom(object _privateData)
		{
			Parameters parameters = _privateData as Parameters;
			if(parameters != null)
			{
				parameters.ChangeFrameCount(false);
			}
		}
		#endregion
		
		#region Private methods
		private static List<Bitmap> GetInputFrames(List<Bitmap> _frameList, int _iFramesToExtract)
		{
			// Get the subset of images we will be using for the mosaic.
			
		 	List<Bitmap> inputFrames = new List<Bitmap>();
			double fExtractStep = (double)_frameList.Count / _iFramesToExtract;

			int iExtracted = 0;
			for(int i=0;i<_frameList.Count;i++)
			{
				if(i >= iExtracted * fExtractStep)
				{
					inputFrames.Add(_frameList[i]);
					iExtracted++;
				}
			}
			
			return inputFrames;
		}
		private List<Bitmap> ExtractBitmapList(List<DecompressedFrame> _frameList)
		{
			// Simply create a list of bitmaps from the list of decompressed frames.
			
			List<Bitmap> inputFrames = new List<Bitmap>();
			for(int i=0;i<_frameList.Count;i++)
			{
				inputFrames.Add(_frameList[i].BmpImage);
			}
			
			return inputFrames;	
		}
		private static int GetFontSize(int _iThumbWidth)
		{
			// Return the font size for the image number based on the thumb width.
			int fontSize = 18;
			
			if(_iThumbWidth >= 200)
			{
				fontSize = 18;
			}
			else if(_iThumbWidth >= 150)
			{
				fontSize = 14;
			}
			else
			{
				fontSize = 10;
			}
			
			return fontSize;
		}
		private static void DrawImageNumber(Graphics _canvas, int _iImageIndex, Rectangle _rDst, Font _font)
		{
			string number = String.Format(" {0}", _iImageIndex + 1);
			SizeF bgSize = _canvas.MeasureString(number, _font);
			bgSize = new SizeF(bgSize.Width + 6, bgSize.Height + 2);
            
			// 1. Draw background.
            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();
            gp.AddLine(_rDst.Left, _rDst.Top, _rDst.Left + bgSize.Width, _rDst.Top);
            gp.AddLine(_rDst.Left + bgSize.Width, _rDst.Top, _rDst.Left + bgSize.Width, _rDst.Top + (bgSize.Height / 2));
            gp.AddArc(_rDst.Left, _rDst.Top, bgSize.Width, bgSize.Height, 0, 90);
            gp.AddLine(_rDst.Left + (bgSize.Width/2), _rDst.Top + bgSize.Height, _rDst.Left, _rDst.Top + bgSize.Height);
            gp.CloseFigure();
            _canvas.FillPath(Brushes.Black, gp);
            
            // 2. Draw image number.
			_canvas.DrawString(number, _font, Brushes.White, _rDst.Location);
		}
		#endregion
		
		/// <summary>
		/// Class to hold the private parameters needed to draw the image.
		/// </summary>
		private class Parameters
		{
			#region Properties
			public List<Bitmap> FrameList
			{
				get { return m_FrameList; }
			}
			public int FramesToExtract
			{
				// This value is always side². (4, 9, 16, 25, 49, etc.)
				get { return m_iFramesToExtract; }
			}
			public bool RightToLeft
			{
				get { return m_bRightToLeft; }
			}
			#endregion
			
			#region Members
			private bool m_bRightToLeft;
			private List<Bitmap> m_FrameList;
			private int m_iFramesToExtract;

			#endregion
			
			public Parameters(List<Bitmap> _frameList, int _iFramesToExtract, bool _bRightToLeft)
			{
				m_FrameList = _frameList;
				m_iFramesToExtract = _iFramesToExtract;
				m_bRightToLeft = _bRightToLeft;
			}
			
			public void ChangeFrameCount(bool _bIncrease)
			{
				// Increase the number of frames to take into account for the mosaic.
				int side = (int)Math.Sqrt((double)m_iFramesToExtract);
				side = _bIncrease ? Math.Min(10, side + 1) : Math.Max(2, side - 1);
				m_iFramesToExtract = side * side;
			}
		}
	}
	
	
	
}


