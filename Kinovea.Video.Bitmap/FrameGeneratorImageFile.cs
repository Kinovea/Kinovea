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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

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
        private System.Drawing.Bitmap originalBitmap;   // originalBitmap contains the image at its original size but possibly rotated.
        private System.Drawing.Bitmap currentBitmap;    // current returned image, same aspect ratio as reference size but possibly scaled.
        private System.Drawing.Bitmap errorBitmap;
        private Size originalSize;                      // Original size of the image on disk, unrotated. In general this shouldn't be used for anything.
        private Size referenceSize;                     // Image size after possible rotation. The reference opened image is kept at this size.
        private bool hasGenerated = false;
        private Stopwatch stopwatch = new Stopwatch();
        
        private float maxDecodingFactor = 2.0f;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Construction/Destruction
        public FrameGeneratorImageFile()
        {
            errorBitmap = new System.Drawing.Bitmap(4, 4);
            currentBitmap = new System.Drawing.Bitmap(640, 480);
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
                if (originalBitmap != null)
                    originalBitmap.Dispose();

                if (errorBitmap != null)
                    errorBitmap.Dispose();

                if (currentBitmap != null)
                    currentBitmap.Dispose();
            }
        }
        #endregion

        public OpenVideoResult Open(string filepath)
        {
            return Open(filepath, ImageRotation.Rotate0);
        }

        public void Close()
        {
            hasGenerated = false;

            if (currentBitmap != null)
                currentBitmap.Dispose();

            if (originalBitmap != null)
               originalBitmap.Dispose();

            if (errorBitmap != null)
                errorBitmap.Dispose();
        }

        public System.Drawing.Bitmap Generate(long timestamp)
        {
            //return originalBitmap ?? errorBitmap;

            if (!hasGenerated || currentBitmap.Width != referenceSize.Width)
                Render(referenceSize);

            return currentBitmap ?? errorBitmap;
        }

        public System.Drawing.Bitmap Generate(long timestamp, Size maxSize)
        {
            //return Generate(timestamp);
            
            Size ratioStretchedSize = GetRatioStretchedSize(maxSize);

            try
            {
                if (!hasGenerated || currentBitmap == null || currentBitmap.Size != ratioStretchedSize)
                    Render(ratioStretchedSize);
            }
            catch (Exception)
            {
                log.ErrorFormat("Error while generating Bitmap image.");
            }

            return currentBitmap ?? errorBitmap;
        }

        public void DisposePrevious(System.Drawing.Bitmap previous) 
        {
            // We do not dispose anything here since we only have a single copy of the image that we constantly return.
        }

        public void SetRotation(ImageRotation rotation)
        {
            // Re-initialize with different rotation.
            // This resets originalBitmap and discards currentBitmap.
            Open(filename, rotation);
        }

        /// <summary>
        /// Create a new copy of the internal bitmap at the specified size.
        /// The passed size should already be at the correct aspect ratio.
        /// </summary>
        private void Render(Size newSize)
        {
            try
            {
                hasGenerated = false;
                if (currentBitmap != null)
                    currentBitmap.Dispose();

                stopwatch.Restart();
                Size maxCustomDecodingSize = new SizeF(referenceSize.Width * maxDecodingFactor, referenceSize.Height * maxDecodingFactor).ToSize();

                if (newSize.Width >= maxCustomDecodingSize.Width || newSize.Height >= maxCustomDecodingSize.Height)
                    newSize = new Size(maxCustomDecodingSize.Width, maxCustomDecodingSize.Height);

                // Create a new copy of the original image at a different size.
                if (newSize.Width == referenceSize.Width && newSize.Height == referenceSize.Height)
                    currentBitmap = BitmapHelper.Copy(originalBitmap);
                else
                    currentBitmap = new System.Drawing.Bitmap(originalBitmap, newSize);

                currentBitmap.SetResolution(96, 96);
                hasGenerated = true;
                log.DebugFormat("Bitmap resize copy: {0} -> {1}: {2} ms.", originalBitmap.Size.ToString(), currentBitmap.Size.ToString(), stopwatch.ElapsedMilliseconds);

            }
            catch (Exception)
            {
                currentBitmap = null;
            }
        }

        private Size GetRatioStretchedSize(Size maxSize)
        {
            float ratioWidth = (float)referenceSize.Width / maxSize.Width;
            float ratioHeight = (float)referenceSize.Height / maxSize.Height;
            float ratio = Math.Max(ratioWidth, ratioHeight);
            Size size = new Size((int)(referenceSize.Width / ratio), (int)(referenceSize.Height / ratio));

            return size;
        }

        private OpenVideoResult Open(string filename, ImageRotation rotation)
        {
            this.filename = filename;
            OpenVideoResult res = OpenVideoResult.NotSupported;

            if (originalBitmap != null)
                originalBitmap.Dispose();

            hasGenerated = false;
            //if (currentBitmap != null)
            //  currentBitmap.Dispose();

            try
            {
                originalBitmap = new System.Drawing.Bitmap(filename);
                if (originalBitmap != null)
                {
                    res = OpenVideoResult.Success;
                    originalSize = originalBitmap.Size;

                    // Force input to be 96 dpi like GDI+ so we don't have any surprises when calling Graphics.DrawImageUnscaled().
                    originalBitmap.SetResolution(96, 96);
                
                    switch (rotation)
                    {
                        case ImageRotation.Rotate90:
                            originalBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case ImageRotation.Rotate180:
                            originalBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case ImageRotation.Rotate270:
                            originalBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        default:
                            break;
                    }

                    referenceSize = originalBitmap.Size;
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
