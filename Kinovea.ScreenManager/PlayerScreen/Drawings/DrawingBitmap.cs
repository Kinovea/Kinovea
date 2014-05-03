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
            get { return infosFading; }
            set { infosFading = value; }
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
        private Bitmap bitmap;
        private BoundingBox boundingBox = new BoundingBox();
        private float initialScale = 1.0f;			            // The scale we apply upon loading to make sure the image fits the screen.
        private int originalWidth;
        private int originalHeight;
        private Size videoSize;
        private static readonly int snapMargin = 0;
        // Decoration
        private InfosFading infosFading;
        private ColorMatrix fadingColorMatrix = new ColorMatrix();
        private ImageAttributes fadingImgAttr = new ImageAttributes();
        private Pen penBoundingBox;
        private SolidBrush brushBoundingBox;
        #endregion

        #region Constructors
        public DrawingBitmap(int width, int height, long timestamp, long averageTimeStampsPerFrame, string filename)
        {
            bitmap = new Bitmap(filename);

            if(bitmap != null)
                Initialize(width, height, timestamp, averageTimeStampsPerFrame);
        }
        public DrawingBitmap(int width, int height, long timestamp, long averageTimeStampsPerFrame, Bitmap bmp)
        {
            bitmap = AForge.Imaging.Image.Clone(bmp);

            if(bitmap != null)
                Initialize(width, height, timestamp, averageTimeStampsPerFrame);
        }
        private void Initialize(int width, int height, long timestamp, long averageTimeStampsPerFrame)
        {
            videoSize = new Size(width, height);
            
            originalWidth = bitmap.Width;
            originalHeight  = bitmap.Height;
            
            // Set the initial scale so that the drawing is some part of the image height, to make sure it fits well.
            // For bitmap drawing, we only do this if no upsizing is involved.
            initialScale = (float) (((float)height * 0.75) / originalHeight);
            if(initialScale < 1.0)
            {
                originalWidth = (int) ((float)originalWidth * initialScale);
                originalHeight = (int) ((float)originalHeight * initialScale);
            }
            
            boundingBox.Rectangle = new Rectangle((width - originalWidth) / 2, (height - originalHeight) / 2, originalWidth, originalHeight);
            
            // Fading
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            infosFading.UseDefault = false;
            infosFading.AlwaysVisible = true;            
            
            // This is used to set the opacity factor.
            fadingColorMatrix.Matrix00 = 1.0f;
            fadingColorMatrix.Matrix11 = 1.0f;
            fadingColorMatrix.Matrix22 = 1.0f;
            fadingColorMatrix.Matrix33 = 1.0f;	// Change alpha value here for fading. (i.e: 0.5f).
            fadingColorMatrix.Matrix44 = 1.0f;
            fadingImgAttr.SetColorMatrix(fadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            
            penBoundingBox = new Pen(Color.White, 1);
            penBoundingBox.DashStyle = DashStyle.Dash;
            brushBoundingBox = new SolidBrush(penBoundingBox.Color);        	
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacityFactor <= 0)
                return;

            Rectangle rect = transformer.Transform(boundingBox.Rectangle);

            if (bitmap == null)
                return;
            
            fadingColorMatrix.Matrix33 = (float)opacityFactor;
            fadingImgAttr.SetColorMatrix(fadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            canvas.DrawImage(bitmap, rect, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, fadingImgAttr);

            if (selected)
                boundingBox.Draw(canvas, rect, penBoundingBox, brushBoundingBox, 4);
        }
        public override int HitTest(Point point, long currentTimestamp, IImageToViewportTransformer transformer, bool zooming)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity > 0)
                result = boundingBox.HitTest(point, transformer);
            
            return result;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            boundingBox.MoveHandle(point.ToPoint(), handleNumber, new Size(originalWidth, originalHeight), true);
        }
        public override void MoveDrawing(float dx, float dy, Keys _ModifierKeys, bool zooming)
        {
            boundingBox.MoveAndSnap((int)dx, (int)dy, videoSize, snapMargin);
        }
        #endregion
    }
}