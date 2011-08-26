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
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using AForge.Imaging;
using AForge.Imaging.Filters;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// VideoFilterSandbox.
	/// This filter is for testing purposes only.
	/// It may be used to test a particular code or experiment. 
	/// It should never be available to the end-user.
	/// </summary>
	public class VideoFilterSandbox : AbstractVideoFilter
	{
		#region Properties
		public override string Name
		{
		    get { return "Sandbox"; }
		}
		public override Bitmap Icon
		{
		    get { return Properties.Resources.controller; }
		}	
		public override bool Experimental 
		{
			get { return true; }
		}
		#endregion
		
		#region Members
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region AbstractVideoFilter Implementation
		public override void Menu_OnClick(object sender, EventArgs e)
        {
			StartProcessing();
        }
		protected override void Process()
		{
			// Method called back from AbstractVideoFilter after a call to StartProcessing().

			//TestFindSurfFeatures();
			//TestCreateYTSlices();
			//TestFrameInterpolation();
		}
		#endregion
		
		#region YTSlices
		/*private void TestCreateYTSlices()
		{
			// Create a number of YT-Slice images.
			// I call YT-Slice the image created over time by a specific column of pixel at X coordinate in the video.
			// A bit like a finish-line image, we get to see what happened at this X during the video.
			// Currently used in experimentations on frame interpolation for slow motion.
			
			string testDirectory = @"C:\Documents and Settings\Administrateur\Mes documents\Dev  Prog\Videa\Video Testing\YT images\test output\";
			
			// Clean up output folder.
    		string[] outFiles = Directory.GetFiles(testDirectory, "*.bmp");
    		foreach (string f in outFiles)
		    {
		        File.Delete(f);
		    }
			
			// Get column of each image and output it in the resulting image.
			int imgHeight = FrameList[0].BmpImage.Height;
			int imgWidth = FrameList[0].BmpImage.Width;
			int iTotalImages = FrameList.Count;
			iTotalImages = Math.Min(100, FrameList.Count);
			for(int iCurrentX=0;iCurrentX<imgWidth;iCurrentX++)
			{
				CreateYTSlice(iCurrentX, iTotalImages, imgHeight, imgWidth, testDirectory);
				m_BackgroundWorker.ReportProgress(iCurrentX, imgWidth);
			}
			
			// Switch lists.
			/*for(int i=0;i<iTotalImages;i++)
			{
				if(i<imgWidth)
				{
					m_FrameList[i].BmpImage.Dispose();
					m_FrameList[i].BmpImage = m_TempImageList[i];
				}
				else
				{
					// Black out image.
				}
			}* /
		}*/
		/*private Bitmap CreateYTSlice(int iCurrentX, int iTotalImages, int imgHeight, int imgWidth, string testDirectory)
		{
			// Create the lateral image.
			// Gather the same column in all images and paint it on a new image.
			
			/*
	       	//1. Mode same 3D space-time block.
	       	// We try to keep the same 3D space-time block.
	       	Bitmap ytImage = new Bitmap(imgWidth, imgHeight, PixelFormat.Format24bppRgb);
	       	Graphics g = Graphics.FromImage(ytImage);
			
	       	int iScope = Math.Min(iTotalImages, imgWidth);
			for(int i=0;i<iScope;i++)
	       	{
	       		//Rectangle rSrc = new Rectangle(iXCoord, 0, 1, imgHeight);
	       		//Rectangle rDst = new Rectangle(i, 0, 1, imgHeight);
				Rectangle rSrc = new Rectangle(iCurrentX, 0, 1, imgHeight);
	       		Rectangle rDst = new Rectangle(i, 0, 1, imgHeight);
	       		g.DrawImage(m_FrameList[i].BmpImage, rDst, rSrc, GraphicsUnit.Pixel);
	       	}* /
			
			
			// 2. Mode full output.
			Bitmap ytImage = new Bitmap(iTotalImages, imgHeight, PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(ytImage);
			
			// loop on all t.
			int iScope = iTotalImages;
			for(int i=0;i<iScope;i++)
	       	{
	       		//Rectangle rSrc = new Rectangle(iXCoord, 0, 1, imgHeight);
	       		//Rectangle rDst = new Rectangle(i, 0, 1, imgHeight);
				Rectangle rSrc = new Rectangle(iCurrentX, 0, 1, imgHeight);
	       		Rectangle rDst = new Rectangle(i, 0, 1, imgHeight);
	       		g.DrawImage(FrameList[i].BmpImage, rDst, rSrc, GraphicsUnit.Pixel);
	       	}
		       			
	        ytImage.Save(testDirectory + String.Format("test-X{0:000}", iCurrentX) + ".bmp");
		        
	        return ytImage;
		}*/
		#endregion
		
		#region Frame Interpolation
		/*private void TestFrameInterpolation()
		{
			int imgStart = 60;
			int range = 10;
			
			List<Bitmap> interpolatedList = new List<Bitmap>();
			for(int i=imgStart;i<imgStart + range;i++)
			{
				Bitmap bmp = ELAVertInterpolation(FrameList[i].BmpImage, FrameList[i+1].BmpImage, 5);
				interpolatedList.Add(bmp);
				m_BackgroundWorker.ReportProgress(i-imgStart, range);
			}
			
			// Interleave.
			List<Bitmap> interleavedList = new List<Bitmap>();
			for(int i=0;i<FrameList.Count;i++)
			{
				interleavedList.Add(FrameList[i].BmpImage);
				if(i>=imgStart && i<imgStart + range)
				{
					interleavedList.Add(interpolatedList[i-imgStart]);
				}
			}
				
			// Reconstruct original list.
			int totalImages = FrameList.Count;
			for(int i=0;i<FrameList.Count;i++)
			{
				FrameList[i].BmpImage = interleavedList[i];
			}
			
			// Dispose the extra images.
			for(int i=FrameList.Count;i<interleavedList.Count;i++)
			{
				interleavedList[i].Dispose();
			}
		}*/
		private static unsafe Bitmap ELAVertInterpolation(Bitmap _src1, Bitmap _src2, int _aperture)
		{
			//----------------------------------------------------
			// Performs ELA between two adjacent (in time) images.
			// Works only on columns.
			//----------------------------------------------------
			
			Bitmap src1 = _src1;
			Bitmap src2 = _src2;
			Bitmap dst = new Bitmap(src1.Width, src1.Height, src1.PixelFormat);
			
			// Lock images.
			BitmapData src1Data = src1.LockBits(new Rectangle( 0, 0, src1.Width, src1.Height ), ImageLockMode.ReadOnly, src1.PixelFormat );
			BitmapData src2Data = src2.LockBits(new Rectangle( 0, 0, src1.Width, src1.Height ), ImageLockMode.ReadOnly, src1.PixelFormat );
			BitmapData dstData = dst.LockBits(new Rectangle( 0, 0, src1.Width, src1.Height ), ImageLockMode.ReadWrite, src1.PixelFormat );
			
			// Get unmanaged images.
			UnmanagedImage src1Unmanaged = new UnmanagedImage( src1Data );
			UnmanagedImage src2Unmanaged = new UnmanagedImage( src2Data );
			UnmanagedImage dstUnmanaged = new UnmanagedImage( dstData );
			
			// Dimensions.
            int width  = src1Unmanaged.Width;
            int height = src1Unmanaged.Height;
            int stride = src1Unmanaged.Stride;
            //int dstStride = dstUnmanaged.Stride;
			 
			byte* pSrc1 = (byte*) src1Unmanaged.ImageData.ToPointer( );
			byte* pSrc2 = (byte*) src2Unmanaged.ImageData.ToPointer( );
			byte* pDst = (byte*) dstUnmanaged.ImageData.ToPointer( );
			
			//--
			// for each line
            for ( int y = 0; y < height; y++ )
            {
                // for each pixel
                for ( int x = 0; x < width; x++)
                {
                	int pos = (y*(stride)) + (x*3);
                	
                	// Find minimum difference of lines in the aperture window.
                	int minDiff = 255 * 3;
					int top = y - (_aperture/2);
					int minRow = top + (_aperture/2);
                	
                	for(int row = top; row < top + _aperture; row++)
	                {
	                	int row2 = (top + _aperture - 1) - (row - top);
	                	
	                	if(((row < 0) || (row >= height)) || ((row2 < 0) || (row2 >= height)) || (x+1 >= width))
	                 	{
	                 		continue;
	                 	}
	                 	else
	                 	{                 			
                 			int pos1 = (row * stride) + x*3;
                 			int pos2 = (row2 * stride) + x*3;
                 			
                 			byte b1 = pSrc1[pos1];
                 			byte b2 = pSrc2[pos2];
                 			byte g1 = pSrc1[pos1+1];
                 			byte g2 = pSrc2[pos2+1];
                 			byte r1 = pSrc1[pos1+2];
                 			byte r2 = pSrc2[pos2+2];
                 			
                 			int diff = System.Math.Abs(b1 - b2);
                 			diff += System.Math.Abs(g1 - g2);
                 			diff += System.Math.Abs(r1 - r2);
	                 		
	                 		if(minDiff > diff)
	                 		{
	                 			minDiff = diff;
	                 			minRow = row;
	                 		}	
                 		}
	                }
					
                	// minPos has the best pos.
	                int minPos1 = (minRow * stride) + x*3;
	                int minPos2 = ( ((top + _aperture - 1) - (minRow - top)) * stride) + x*3;
	                
                 	pDst[pos] = (byte)(((int)pSrc1[minPos1] + (int)pSrc2[minPos2]) / 2);
                	pDst[pos+1] = (byte)(((int)pSrc1[minPos1 + 1] + (int)pSrc2[minPos2 + 1]) / 2);
                	pDst[pos+2] = (byte)(((int)pSrc1[minPos1 + 2] + (int)pSrc2[minPos2 + 2]) / 2);
                }
			}            
			//--
			
			src1.UnlockBits( src1Data );
            src2.UnlockBits( src2Data );
			dst.UnlockBits( dstData );
            
			return dst;
		}
		#endregion
	}
}


