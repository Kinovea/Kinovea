#region License
/*
Copyright © Joan Charmant 2011.
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
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using SharpVectors.Renderer.Gdi;

namespace Kinovea.Video.SVG
{
    public class FrameGeneratorSVG : IFrameGenerator
    {
        #region Properties
        public Size Size {
            get { return initialized ? originalSize : Size.Empty; }
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

        public OpenVideoResult Initialize(string filepath)
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
            if(currentBitmap == null || currentBitmap.Size != ratioStretchedSize)
                Render(ratioStretchedSize);

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
            currentBitmap = renderer.Render(svgWindow.Document as SvgDocument);

            /*m_fDrawingScale = (float)m_BoundingBox.Rectangle.Width / (float)m_iOriginalWidth;
            m_fDrawingRenderingScale = (float)(_fScreenScaling * m_fDrawingScale * m_fInitialScale);

            if (m_svgRendered == null || m_fDrawingRenderingScale != m_SvgWindow.Document.RootElement.CurrentScale)
            {
                // In the case of percentage, CurrentScale is always 100%. But since there is a cache for the transformation matrix,
                // we need to set it anyway to clear the cache.
                m_SvgWindow.Document.RootElement.CurrentScale = m_bSizeInPercentage ? 1.0f : (float)m_fDrawingRenderingScale;

                m_SvgWindow.InnerWidth = _size.Width;
                m_SvgWindow.InnerHeight = _size.Height;

                m_svgRendered = m_Renderer.Render(m_SvgWindow.Document as SvgDocument);

                log.Debug(String.Format("Rendering SVG ({0};{1}), Initial scaling to fit video: {2:0.00}. User scaling: {3:0.00}. Video image scaling: {4:0.00}, Final transformation: {5:0.00}.",
                                        m_iOriginalWidth, m_iOriginalHeight, m_fInitialScale, m_fDrawingScale, _fScreenScaling, m_fDrawingRenderingScale));
            }*/
        }

        private Size GetRatioStretchedSize(Size maxSize)
        {
            float ratioWidth = (float)originalSize.Width / maxSize.Width;
            float ratioHeight = (float)originalSize.Height / maxSize.Height;
            float ratio = Math.Max(ratioWidth, ratioHeight);

            int width = (int)(originalSize.Width / ratio);
            int height = (int)(originalSize.Height / ratio);

            return new Size(width, height);
        }
    }
}
