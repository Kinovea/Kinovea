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
using System.Drawing.Imaging;
using AForge.Imaging.Filters;
using Kinovea.ScreenManager.Languages;
using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager
{
	public class VideoFilterSharpen : AdjustmentFilter
	{
		public override string Name {
		    get { return ScreenManagerLang.VideoFilterSharpen_FriendlyName; }
		}
		public override ImageProcessor ImageProcessor {
		    get { return ProcessSingleImage; }
		}
		private Bitmap ProcessSingleImage(Bitmap _src)
		{
			Bitmap img = (_src.PixelFormat == PixelFormat.Format24bppRgb) ? _src : CloneTo24bpp(_src);
			Sharpen sharpenFilter = new Sharpen();
			sharpenFilter.ApplyInPlace(img);
			
			if(_src.PixelFormat != PixelFormat.Format24bppRgb)
			{
            	Graphics g = Graphics.FromImage(_src);
            	g.DrawImageUnscaled(img, 0, 0);
            	img.Dispose();
            }
			
			return _src;
		}
	}
}
