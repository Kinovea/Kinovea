#region License
/*
Copyright © Joan Charmant 2011.
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
using Kinovea.Services;
using System;
using System.Drawing;
//using SystemBitmap = System.Drawing.Bitmap;

namespace Kinovea.Video.Bitmap
{
    /// <summary>
    /// Provide access to a single image. Used to turn an image into a video.
    /// </summary>
    public class FrameGeneratorImageFile : IFrameGenerator, IDisposable
    {
        public Size OriginalSize {
            get { return originalSize; }
        }

        public Size ReferenceSize {
            get { return referenceSize; }
        }   

        private string filename;
        private Size originalSize;
        private Size referenceSize;
        private System.Drawing.Bitmap image;
        private System.Drawing.Bitmap errorImage;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Construction/Destruction
        public FrameGeneratorImageFile()
        {
            errorImage = new System.Drawing.Bitmap(4, 4);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FrameGeneratorImageFile()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (image != null)
                    image.Dispose();

                if (errorImage != null)
                    errorImage.Dispose();
            }
        }
        #endregion

        public OpenVideoResult Open(string filename)
        {
            return Open(filename, ImageRotation.Rotate0);
        }

        public void Close()
        {
            if (image != null)
                image.Dispose();
        }

        public System.Drawing.Bitmap Generate(long timestamp)
        {
            return image??errorImage;
        }

        public System.Drawing.Bitmap Generate(long timestamp, Size maxWidth)
        {
            return Generate(timestamp);
        }

        public void DisposePrevious(System.Drawing.Bitmap previous) 
        {
            // We do not dispose anything here since we only have a single copy of the image that we constantly return.
        }

        public void SetRotation(ImageRotation rotation)
        {
            // Re-initialize with different rotation.
            Open(filename, rotation);
        }

        private OpenVideoResult Open(string filename, ImageRotation rotation)
        {

            this.filename = filename;
            OpenVideoResult res = OpenVideoResult.NotSupported;

            if (image != null)
                image.Dispose();

            try
            {
                image = new System.Drawing.Bitmap(filename);
                if (image != null)
                {
                    res = OpenVideoResult.Success;
                    originalSize = image.Size;

                    // Force input to be 96 dpi like GDI+ so we don't have any surprises when calling Graphics.DrawImageUnscaled().
                    image.SetResolution(96, 96);

                    switch (rotation)
                    {
                        case ImageRotation.Rotate90:
                            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case ImageRotation.Rotate180:
                            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case ImageRotation.Rotate270:
                            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        default:
                            break;
                    }

                    referenceSize = image.Size;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("An error occured while trying to open {0}", filename);
                log.Error(e);
            }

            return res;
        }
    }
}
