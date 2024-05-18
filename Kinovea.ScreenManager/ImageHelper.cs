#region License
/*
Copyright © Joan Charmant 2010.
jcharmant@gmail.com 
 
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
        public static void Save(string filename, Bitmap image)
        {
            string directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string filenameToLower = filename.ToLower();

            if (filenameToLower.EndsWith("jpg") || filenameToLower.EndsWith("jpeg"))
            {
                Bitmap jpgImage = ImageHelper.ConvertToJPG(image, 100);
                jpgImage.Save(filename, ImageFormat.Jpeg);
                jpgImage.Dispose();
            }
            else if (filenameToLower.EndsWith("bmp"))
            {
                image.Save(filename, ImageFormat.Bmp);
            }
            else if (filenameToLower.EndsWith("png"))
            {
                image.Save(filename, ImageFormat.Png);
            }
            else
            {
                // the user may have put a filename in the form : "filename.ext"
                // where ext is unsupported. Or he misunderstood and put ".00.00"
                // We force format to jpg and we change back the extension to ".jpg".
                string newFilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + ".jpg";

                Bitmap jpgImage = ImageHelper.ConvertToJPG(image, 100);
                jpgImage.Save(newFilename, ImageFormat.Jpeg);
                jpgImage.Dispose();
            }
        }
        public static Bitmap ConvertToJPG(Bitmap image, long quality)
        {
            MemoryStream memStr = new MemoryStream();
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            ImageCodecInfo ici = null;
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == "image/jpeg")
                {
                    ici = codec;
                    break;
                }
            }

            if (ici != null)
            {
                //Create a collection of encoder parameters (we only need one in the collection)
                EncoderParameters ep = new EncoderParameters();
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                image.Save(memStr, ici, ep);
            }
            else
            {
                // No JPG encoder found (is that common ?) Use default system.
                image.Save(memStr, ImageFormat.Jpeg);
            }

            return new Bitmap(memStr);
        }


        /// <summary>
        /// Compute the size of the final composite.
        /// If `isVideo` is true, the final width and height may be extended by one to three pixels.
        /// </summary>
        public static Size GetSideBySideCompositeSize(Size left, Size right, bool isVideo, bool merging, bool horizontal)
        {
            int width;
            int height;

            if (merging)
            {
                width = left.Width;
                height = left.Height;
            }
            else if (horizontal)
            {
                width = left.Width + right.Width;
                height = Math.Max(left.Height, right.Height);
            }
            else
            {
                width = Math.Max(left.Width, right.Width);
                height = left.Height + right.Height;
            }

            if (isVideo)
            {
                if (width % 4 != 0)
                    width += 4 - (width % 4);
                
                if (height % 2 != 0)
                    height++;
            }

            return new Size(width, height);
        }

        /// <summary>
        /// Composite the two images side by side either horizontally or vertically.
        /// </summary>
        public static void PaintSideBySideComposite(Bitmap composite, Bitmap leftImage, Bitmap rightImage, bool horizontal)
        {
            Graphics g = Graphics.FromImage(composite);

            if (horizontal)
            {
                // Vertically center the shortest image.
                int leftTop = 0;
                if(leftImage.Height < composite.Height)
                    leftTop = (composite.Height - leftImage.Height) / 2;

                int rightTop = 0;
                if(rightImage.Height < composite.Height)
                    rightTop = (composite.Height - rightImage.Height) / 2;
                
                // Draw the images on the output.
                g.DrawImage(leftImage, 0, leftTop);
                g.DrawImage(rightImage, leftImage.Width, rightTop);
            }
            else
            {
                // Horizontally center the shortest image.
                int firstLeft = 0;
                if(leftImage.Width < composite.Width)
                    firstLeft = (composite.Width - leftImage.Width) / 2;

                int secondLeft = 0;
                if(rightImage.Width < composite.Width)
                    secondLeft = (composite.Width - rightImage.Width) / 2;
                
                // Draw the images on the output.
                g.DrawImage(leftImage, firstLeft, 0);
                g.DrawImage(rightImage, secondLeft, leftImage.Height);	
            }
        }
    }
}
