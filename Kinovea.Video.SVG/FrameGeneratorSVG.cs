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
using System;
using System.Drawing;
using Kinovea.Services;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using SharpVectors.Renderer.Gdi;

namespace Kinovea.Video.SVG
{
    public class FrameGeneratorSVG : IFrameGenerator
    {
        #region Properties
        public Size OriginalSize 
        {
            get { return initialized ? originalSize : Size.Empty; }
        }

        public Size ReferenceSize 
        {
            get { return OriginalSize; }
        }

        public ImageRotation ImageRotation
        {
            get { return ImageRotation.Rotate0; }
        }
        #endregion

        #region Members
        private GdiRenderer renderer = new GdiRenderer();
        private SvgWindow svgWindow;
        private bool sizeInPercentage;
        private Size originalSize;
        private float ratio = 1.0f;
        private bool initialized;

        private Bitmap currentBitmap;
        private Bitmap errorBitmap;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FrameGeneratorSVG()
        {
            errorBitmap = new Bitmap(640, 480);
            currentBitmap = new Bitmap(640, 480);

            renderer.BackColor = Color.Transparent;
            svgWindow = new SvgWindow(100, 100, renderer);
        }

        public OpenVideoResult Open(string filepath)
        {
            OpenVideoResult res = OpenVideoResult.NotSupported;

            try
            {
                svgWindow.Src = filepath;
                InitSizeInfo();
                res = OpenVideoResult.Success;
            }
            catch(Exception e)
            {
                log.ErrorFormat("An error occured while trying to open {0}", filepath);
                log.Error(e);
            }

            return res;
        }

        public Bitmap Generate(long timestamp)
        {
            if (currentBitmap == null || currentBitmap.Width != originalSize.Width)
                Render(originalSize);

            return (currentBitmap != null) ? currentBitmap : errorBitmap;
        }

        public Bitmap Generate(long timestamp, Size maxSize)
        {
            if (!initialized)
                return errorBitmap;

            Size ratioStretchedSize = GetRatioStretchedSize(maxSize);

            try
            {
                if (currentBitmap == null || currentBitmap.Size != ratioStretchedSize)
                    Render(ratioStretchedSize);
            }
            catch (Exception)
            {
                log.ErrorFormat("Error while generating SVG image.");
            }

            return (currentBitmap != null) ? currentBitmap : errorBitmap;
        }
        
        public void DisposePrevious(Bitmap previous) { }
        
        public void Close()
        {
            if(currentBitmap != null)
                currentBitmap.Dispose();
            
            if(errorBitmap != null)
                errorBitmap.Dispose();
        }

        private void InitSizeInfo()
        {
            ISvgSvgElement root = svgWindow.Document.RootElement;
            sizeInPercentage = root.Width.BaseVal.UnitType == SvgLengthType.Percentage;
            if (sizeInPercentage)
            {
                int width = (int)(root.ViewBox.BaseVal.Width * (root.Width.BaseVal.Value / 100));
                int height = (int)(root.ViewBox.BaseVal.Height * (root.Height.BaseVal.Value / 100));
                originalSize = new Size(width, height);
            }
            else
            {
                originalSize = new Size((int)root.Width.BaseVal.Value, (int)root.Height.BaseVal.Value);
            }

            ratio = (float)originalSize.Width / originalSize.Height;
            initialized = true;
        }

        private void Render(Size size)
        {
            //int height = (int)(width / ratio);
            float scale = (float)size.Width / originalSize.Width;

            svgWindow.Document.RootElement.CurrentScale = sizeInPercentage ? 1.0f : scale;
            svgWindow.InnerWidth = size.Width;
            svgWindow.InnerHeight = size.Height;
            try
            {
                currentBitmap = renderer.Render(svgWindow.Document as SvgDocument);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while rendering SVG. {0}", e.ToString());
                currentBitmap = null;
            }

            //currentBitmap.Save(string.Format("{0}.png", Guid.NewGuid().ToString()));
        }

        private Size GetRatioStretchedSize(Size maxSize)
        {
            float ratioWidth = (float)originalSize.Width / maxSize.Width;
            float ratioHeight = (float)originalSize.Height / maxSize.Height;
            float ratio = Math.Max(ratioWidth, ratioHeight);
            Size size = new Size((int)(originalSize.Width / ratio), (int)(originalSize.Height / ratio));

            return size;
        }

        public void SetRotation(ImageRotation rotation)
        {
            throw new NotImplementedException();
        }
    }
}
