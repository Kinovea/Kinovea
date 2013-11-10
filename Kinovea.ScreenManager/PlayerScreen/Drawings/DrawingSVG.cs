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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using SharpVectors.Renderer.Gdi;

namespace Kinovea.ScreenManager
{
    public class DrawingSVG : AbstractDrawing
    {
        #region Properties
        public override string DisplayName
        {
            get {  return "SVG Drawing"; }
        }
        public override int ContentHash
        {
            get { return 0;}
        }
        public override InfosFading InfosFading
        {
            get { return m_InfosFading; }
            set { m_InfosFading = value; }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.Opacity; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get { return null; }
        }
        #endregion

        #region Members
        // SVG
        private GdiRenderer m_Renderer  = new GdiRenderer();
        private SvgWindow m_SvgWindow;
        private bool m_bLoaded;
        private Bitmap m_svgRendered;
        // Position
        // The drawing scale is used to keep track of the user transform on the drawing, outside of the image transform context.
        // Drawing original dimensions are used to compute the drawing scale.
        private float m_fDrawingScale = 1.0f;			// The current scale of the drawing if it were rendered on the original sized image.
        private float m_fInitialScale = 1.0f;			// The scale we apply upon loading to make sure the image fits the screen.
        private float m_fDrawingRenderingScale = 1.0f;  // The scale of the drawing taking drawing transform AND image transform into account.
        private int m_iOriginalWidth;					// After initial scaling.
        private int m_iOriginalHeight;
        private BoundingBox m_BoundingBox = new BoundingBox();
        private bool m_bSizeInPercentage;               // A property of some SVG files.
        private bool m_bFinishedResizing;
        private Size m_videoSize;
        private static readonly int m_snapMargin = 0;
        // Decoration
        private InfosFading m_InfosFading;
        private ColorMatrix m_FadingColorMatrix = new ColorMatrix();
        private ImageAttributes m_FadingImgAttr = new ImageAttributes();
        private Pen m_PenBoundingBox;
        private SolidBrush m_BrushBoundingBox;
        // Instru
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingSVG(int _iWidth, int _iHeight, long _iTimestamp, long _iAverageTimeStampsPerFrame, string _filename)
        {
            m_videoSize = new Size(_iWidth, _iHeight);
            
            // Init and import an SVG.
            m_Renderer.BackColor = Color.Transparent;
            
            // Rendering window. The width and height will be updated later.
            m_SvgWindow = new SvgWindow(100, 100, m_Renderer);
            
            // FIXME: some files have external DTD that will be attempted to be loaded.
            // See files created from Amaya for example.
            m_SvgWindow.Src = _filename;
            m_bLoaded = true;
            
            if(m_SvgWindow.Document.RootElement.Width.BaseVal.UnitType == SvgLengthType.Percentage)
            {
                m_bSizeInPercentage = true;
                m_iOriginalWidth = (int)(m_SvgWindow.Document.RootElement.ViewBox.BaseVal.Width * (m_SvgWindow.Document.RootElement.Width.BaseVal.Value/100));
                m_iOriginalHeight = (int)(m_SvgWindow.Document.RootElement.ViewBox.BaseVal.Height * (m_SvgWindow.Document.RootElement.Height.BaseVal.Value/100));	
            }
            else
            {
                m_bSizeInPercentage = false;
                m_iOriginalWidth = (int)m_SvgWindow.Document.RootElement.Width.BaseVal.Value;
                m_iOriginalHeight  = (int)m_SvgWindow.Document.RootElement.Height.BaseVal.Value;		        
            }
            
            // Set the initial scale so that the drawing is some part of the image height, to make sure it fits well.
            m_fInitialScale = (float) (((float)_iHeight * 0.75) / m_iOriginalHeight);
            m_iOriginalWidth = (int) ((float)m_iOriginalWidth * m_fInitialScale);
            m_iOriginalHeight = (int) ((float)m_iOriginalHeight * m_fInitialScale);
            
            m_BoundingBox.Rectangle = new Rectangle((_iWidth - m_iOriginalWidth)/2, (_iHeight - m_iOriginalHeight)/2, m_iOriginalWidth, m_iOriginalHeight);

            // Render on first draw call.
            m_bFinishedResizing = true;
            
            // Fading
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            m_InfosFading.UseDefault = false;
            m_InfosFading.AlwaysVisible = true;            
            
            // This is used to set the opacity factor.
            m_FadingColorMatrix.Matrix00 = 1.0f;
            m_FadingColorMatrix.Matrix11 = 1.0f;
            m_FadingColorMatrix.Matrix22 = 1.0f;
            m_FadingColorMatrix.Matrix33 = 1.0f;	// Change alpha value here for fading. (i.e: 0.5f).
            m_FadingColorMatrix.Matrix44 = 1.0f;
            m_FadingImgAttr.SetColorMatrix(m_FadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            
            m_PenBoundingBox = new Pen(Color.White, 1);
            m_PenBoundingBox.DashStyle = DashStyle.Dash;
            m_BrushBoundingBox = new SolidBrush(m_PenBoundingBox.Color);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, IImageToViewportTransformer _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor <= 0 || !m_bLoaded)
                return;

            Rectangle rect = _transformer.Transform(m_BoundingBox.Rectangle);
            
            if(m_bFinishedResizing)
            {
                m_bFinishedResizing = false;
                RenderAtNewScale(rect.Size, _transformer.Scale);
            }
            
            if (m_svgRendered != null)
            {
                m_FadingColorMatrix.Matrix33 = (float)fOpacityFactor;
                m_FadingImgAttr.SetColorMatrix(m_FadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                _canvas.DrawImage(m_svgRendered, rect, 0, 0, m_svgRendered.Width, m_svgRendered.Height, GraphicsUnit.Pixel, m_FadingImgAttr);

                if (_bSelected)
                    m_BoundingBox.Draw(_canvas, rect, m_PenBoundingBox, m_BrushBoundingBox, 4);
            }
        }
        public override int HitTest(Point point, long currentTimestamp, IImageToViewportTransformer transformer, bool zooming)
        {
            int result = -1;
            double opacity = m_InfosFading.GetOpacityFactor(currentTimestamp);
            if (opacity > 0)
                result = m_BoundingBox.HitTest(point, transformer);
            
            return result;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            m_BoundingBox.MoveHandle(point.ToPoint(), handleNumber, new Size(m_iOriginalWidth, m_iOriginalHeight), true);
        }
        public override void MoveDrawing(float dx, float dy, Keys _ModifierKeys, bool zooming)
        {
            m_BoundingBox.MoveAndSnap((int)dx, (int)dy, m_videoSize, m_snapMargin);
        }
        #endregion
       
        public void ResizeFinished()
        {
            // While the user was resizing the drawing or the image, we didn't update / render the SVG image.
            // Now that he is done, we can stop using the low quality interpolation and resort to SVG scalability.
            
            // However we do not know the final scale until we get back in Draw(),
            // So we just switch a flag on and we'll call the rendering from there.
            m_bFinishedResizing = true;
        }
        
        #region Lower level helpers
        private void RenderAtNewScale(Size _size, double _fScreenScaling)
        {
            // Depending on the complexity of the SVG, this can be a costly operation.
            // We should only do that when mouse move is over,
            // and use the interpolated version during the change.
            
            // Compute the final drawing sizes,
            // taking both the drawing transformation and the image scaling into account.
            m_fDrawingScale = (float)m_BoundingBox.Rectangle.Width / (float)m_iOriginalWidth;
            m_fDrawingRenderingScale = (float)(_fScreenScaling * m_fDrawingScale * m_fInitialScale);
            
            if(m_svgRendered == null || m_fDrawingRenderingScale != m_SvgWindow.Document.RootElement.CurrentScale)
            {
                // In the case of percentage, CurrentScale is always 100%. But since there is a cache for the transformation matrix,
                // we need to set it anyway to clear the cache.
                m_SvgWindow.Document.RootElement.CurrentScale = m_bSizeInPercentage ? 1.0f : (float)m_fDrawingRenderingScale;

                m_SvgWindow.InnerWidth = _size.Width;
                m_SvgWindow.InnerHeight = _size.Height;
                
                m_svgRendered = m_Renderer.Render(m_SvgWindow.Document as SvgDocument);
                
                log.Debug(String.Format("Rendering SVG ({0};{1}), Initial scaling to fit video: {2:0.00}. User scaling: {3:0.00}. Video image scaling: {4:0.00}, Final transformation: {5:0.00}.",
                                        m_iOriginalWidth, m_iOriginalHeight, m_fInitialScale, m_fDrawingScale, _fScreenScaling, m_fDrawingRenderingScale));
            }
        }
        #endregion
    }
}

