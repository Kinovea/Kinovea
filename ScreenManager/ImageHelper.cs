#region License
/*
Copyright © Joan Charmant 2010.
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
using System.IO;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// A static class with hepler functions related to Images, conversions, etc.
	/// </summary>
	public static class ImageHelper
	{
	    public static void Save(string _fileName, Bitmap _image)
		{
			string filenameToLower = _fileName.ToLower();
			
			if (filenameToLower.EndsWith("jpg") || filenameToLower.EndsWith("jpeg"))
			{
				Bitmap jpgImage = ImageHelper.ConvertToJPG(_image, 100);
				jpgImage.Save(_fileName, ImageFormat.Jpeg);
				jpgImage.Dispose();
			}
			else if (filenameToLower.EndsWith("bmp"))
			{
				_image.Save(_fileName, ImageFormat.Bmp);
			}
			else if (filenameToLower.EndsWith("png"))
			{
				_image.Save(_fileName, ImageFormat.Png);
			}
			else
			{
				// the user may have put a filename in the form : "filename.ext"
				// where ext is unsupported. Or he misunderstood and put ".00.00"
				// We force format to jpg and we change back the extension to ".jpg".
				string fileName = Path.GetDirectoryName(_fileName) + "\\" + Path.GetFileNameWithoutExtension(_fileName) + ".jpg";

				Bitmap jpgImage = ImageHelper.ConvertToJPG(_image, 100);
				jpgImage.Save(fileName, ImageFormat.Jpeg);
				jpgImage.Dispose();
			}
		}
		public static Bitmap ConvertToJPG(Bitmap _image, long _quality)
		{
			// Intermediate MemoryStream for the conversion.
			MemoryStream memStr = new MemoryStream();

			//Get the list of available encoders
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

			//find the encoder with the image/jpeg mime-type
			ImageCodecInfo ici = null;
			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.MimeType == "image/jpeg")
				{
					ici = codec;
				}
			}

			if (ici != null)
			{
				//Create a collection of encoder parameters (we only need one in the collection)
				EncoderParameters ep = new EncoderParameters();
				ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_quality);

				_image.Save(memStr, ici, ep);
			}
			else
			{
				// No JPG encoder found (is that common ?) Use default system.
				_image.Save(memStr, ImageFormat.Jpeg);
			}

			return new Bitmap(memStr);
		}
		public static Bitmap GetSideBySideComposite(Bitmap _leftImage, Bitmap _rightImage, bool _video, bool _horizontal)
		{
			Bitmap composite = null;
			
			if(_horizontal)
			{
				// Create the output image.
				int height = Math.Max(_leftImage.Height, _rightImage.Height);
				int width = _leftImage.Width + _rightImage.Width;
					
				// For video export, only even heights are valid.			
				if(_video && (height % 2 != 0))
				{
					height++;
				}
				
				composite = new Bitmap(width, height, _leftImage.PixelFormat);
				
				// Vertically center the shortest image.
				int leftTop = 0;
				if(_leftImage.Height < height)
				{
					leftTop = (height - _leftImage.Height) / 2;
				}
				int rightTop = 0;
				if(_rightImage.Height < height)
				{
					rightTop = (height - _rightImage.Height) / 2;
				}
				
				// Draw the images on the output.
				Graphics g = Graphics.FromImage(composite);
				g.DrawImage(_leftImage, 0, leftTop);
				g.DrawImage(_rightImage, _leftImage.Width, rightTop);
			}
			else
			{
				// Create the output image.
				int height = _leftImage.Height + _rightImage.Height;
				int width = Math.Max(_leftImage.Width, _rightImage.Width);
				
				// For video export, only even heights are valid.			
				if(_video && (height % 2 != 0))
				{
					height++;
				}
				
				composite = new Bitmap(width, height, _leftImage.PixelFormat);
				
				// Horizontally center the shortest image.
				int firstLeft = 0;
				if(_leftImage.Width < width)
				{
					firstLeft = (width - _leftImage.Width) / 2;
				}
				int secondLeft = 0;
				if(_rightImage.Width < width)
				{
					secondLeft = (width - _rightImage.Width) / 2;
				}
				
				// Draw the images on the output.
				Graphics g = Graphics.FromImage(composite);
				g.DrawImage(_leftImage, firstLeft, 0);
				g.DrawImage(_rightImage, secondLeft, _leftImage.Height);	
			}
			
			return composite;
		}
	}
}
