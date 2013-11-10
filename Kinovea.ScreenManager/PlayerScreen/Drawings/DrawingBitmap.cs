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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class DrawingBitmap : AbstractDrawing
    {
        #region Properties
        public override string DisplayName
        {
            get {  return "Bitmap Drawing"; }
        }
        public override int ContentHash
        {
            get { return 0; }
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
        private Bitmap m_Bitmap;
        private BoundingBox m_BoundingBox = new BoundingBox();
        private float m_fInitialScale = 1.0f;			            // The scale we apply upon loading to make sure the image fits the screen.
        private int m_iOriginalWidth;
        private int m_iOriginalHeight;
        private Size m_videoSize;
        private static readonly int m_snapMargin = 0;
        // Decoration
        private InfosFading m_InfosFading;
        private ColorMatrix m_FadingColorMatrix = new ColorMatrix();
        private ImageAttributes m_FadingImgAttr = new ImageAttributes();
        private Pen m_PenBoundingBox;
        private SolidBrush m_BrushBoundingBox;
        #endregion

        #region Constructors
        public DrawingBitmap(int _iWidth, int _iHeight, long _iTimestamp, long _iAverageTimeStampsPerFrame, string _filename)
        {
            m_Bitmap = new Bitmap(_filename);

            if(m_Bitmap != null)
            {
                Initialize(_iWidth, _iHeight, _iTimestamp, _iAverageTimeStampsPerFrame);
            }
        }
        public DrawingBitmap(int _iWidth, int _iHeight, long _iTimestamp, long _iAverageTimeStampsPerFrame, Bitmap _bmp)
        {
            m_Bitmap = AForge.Imaging.Image.Clone(_bmp);

            if(m_Bitmap != null)
            {
                Initialize(_iWidth, _iHeight, _iTimestamp, _iAverageTimeStampsPerFrame);
            }
        }
        private void Initialize(int _iWidth, int _iHeight, long _iTimestamp, long _iAverageTimeStampsPerFrame)
        {
            m_videoSize = new Size(_iWidth, _iHeight);
            
            m_iOriginalWidth = m_Bitmap.Width;
            m_iOriginalHeight  = m_Bitmap.Height;
            
            // Set the initial scale so that the drawing is some part of the image height, to make sure it fits well.
            // For bitmap drawing, we only do this if no upsizing is involved.
            m_fInitialScale = (float) (((float)_iHeight * 0.75) / m_iOriginalHeight);
            if(m_fInitialScale < 1.0)
            {
                m_iOriginalWidth = (int) ((float)m_iOriginalWidth * m_fInitialScale);
                m_iOriginalHeight = (int) ((float)m_iOriginalHeight * m_fInitialScale);
            }
            
            m_BoundingBox.Rectangle = new Rectangle((_iWidth - m_iOriginalWidth) / 2, (_iHeight - m_iOriginalHeight) / 2, m_iOriginalWidth, m_iOriginalHeight);
            
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
            if (fOpacityFactor <= 0)
                return;

            Rectangle rect = _transformer.Transform(m_BoundingBox.Rectangle);
            
            if (m_Bitmap != null)
            {
                m_FadingColorMatrix.Matrix33 = (float)fOpacityFactor;
                m_FadingImgAttr.SetColorMatrix(m_FadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                _canvas.DrawImage(m_Bitmap, rect, 0, 0, m_Bitmap.Width, m_Bitmap.Height, GraphicsUnit.Pixel, m_FadingImgAttr);

                if (_bSelected)
                {
                    m_BoundingBox.Draw(_canvas, rect, m_PenBoundingBox, m_BrushBoundingBox, 4);
                }
                    
            }
        }
        public override int HitTest(Point point, long currentTimestamp, IImageToViewportTransformer transformer, bool zooming)
        {
            // Convention: miss = -1, object = 0, handle = n.
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
    }
}