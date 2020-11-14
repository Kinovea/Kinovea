#region License
/*
Copyright © Joan Charmant 2015.
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType("TestGrid")]
    public class DrawingTestGrid : AbstractDrawing, IScalable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get { return ScreenManagerLang.DrawingName_TestGrid; }
        }
        public override int ContentHash
        {
            get { return 0; }
        }
        public override InfosFading InfosFading
        {
            get { return null; }
            set {  }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get 
            {
                return null;
            }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.None; }
        }
        public bool Visible { get; set; }
        #endregion

        #region Members
        private SizeF imageSize;
        private float halfWidth;
        private float halfHeight;
        private ToolStripMenuItem menuHide = new ToolStripMenuItem();
        private const int divisions = 10;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingTestGrid()
        {
            menuHide.Click += menuHide_Click;
            menuHide.Image = Properties.Drawings.hide;
        }
        #endregion

        #region AbstractDrawing implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            if (!Visible || imageSize == SizeF.Empty)
                return;

            DrawGrid(canvas, transformer);
        }
        private void DrawGrid(Graphics canvas, IImageToViewportTransformer transformer)
        {
            // Drawing is done in normalized space [-1.0, +1.0] for simplicity.

            float aspectRatio = imageSize.Width / imageSize.Height;
            float factorX = aspectRatio > 1.0f ? (1.0f/aspectRatio) : 1.0f;
            float factorY = aspectRatio < 1.0f ? (1.0f/aspectRatio) : 1.0f;
            float normalizedStepX = 2.0f * factorX / divisions;
            float normalizedStepY = 2.0f * factorY / divisions;
                
            Pen plines = new Pen(Color.Firebrick, 1);

            // Horizontals
            float top = 0;

            // Upward
            while (true)
            {
                top = top - normalizedStepY;

                if (top < -1)
                    break;

                DrawLine(canvas, transformer, plines, new PointF(-1, top), new PointF(1, top));
            }

            // Downward
            top = 0;
            while (true)
            {
                top = top + normalizedStepY;

                if (top > 1)
                    break;

                DrawLine(canvas, transformer, plines, new PointF(-1, top), new PointF(1, top));
            }

            // Verticals

            float left = 0;
            // Leftward
            while (true)
            {
                left = left - normalizedStepX;

                if (left < -1)
                    break;

                DrawLine(canvas, transformer, plines, new PointF(left, -1), new PointF(left, 1));
            }

            // Rigthward
            left = 0;
            while (true)
            {
                left = left + normalizedStepX;

                if (left > 1)
                    break;

                DrawLine(canvas, transformer, plines, new PointF(left, -1), new PointF(left, 1));
            }

            // Axes
            Pen paxes = new Pen(Color.Firebrick, 1);
            DrawLine(canvas, transformer, paxes, new PointF(-1, 0), new PointF(1, 0));
            DrawLine(canvas, transformer, paxes, new PointF(0, -1), new PointF(0, 1));

            // Diagonals
            DrawLine(canvas, transformer, plines, new PointF(-1, 1), new PointF(1, -1));
            DrawLine(canvas, transformer, plines, new PointF(-1, -1), new PointF(1, 1));

            // Circle
            float radiusX = aspectRatio > 1.0f ? factorX : 1.0f;
            float radiusY = aspectRatio < 1.0f ? aspectRatio : 1.0f;
            DrawCenteredCircle(canvas, transformer, plines, radiusX, radiusY);
            
            plines.Dispose();
            paxes.Dispose();
        }
        private void DrawLine(Graphics canvas, IImageToViewportTransformer transformer, Pen pen, PointF a, PointF b)
        {
            canvas.DrawLine(pen, transformer.Transform(Map(a)), transformer.Transform(Map(b)));
        }

        private void DrawCenteredCircle(Graphics canvas, IImageToViewportTransformer transformer, Pen pen, float radiusX, float radiusY)
        {
            PointF a = Map(new PointF(-radiusX, radiusY));
            PointF b = Map(new PointF(radiusX, radiusY));
            PointF c = Map(new PointF(radiusX, -radiusY));
            RectangleF rect = new RectangleF(a.X, a.Y, b.X - a.X, c.Y - a.Y);

            canvas.DrawEllipse(pen, transformer.Transform(rect));
        }

        /// <summary>
        /// Go from normalized coordinates in [-1, +1] space to image coordinates.
        /// </summary>
        private PointF Map(PointF p)
        {
            return new PointF(halfWidth + (p.X * halfWidth), halfHeight + (p.Y * halfHeight));
        }
        
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            return -1;
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
        }
        public override PointF GetCopyPoint()
        {
            return new PointF(imageSize.Width / 2, imageSize.Height / 2);
        }
        
        #endregion

        #region IScalable implementation
        public void Scale(Size imageSize)
        {
            this.imageSize = new SizeF(imageSize.Width, imageSize.Height);
            this.halfWidth = imageSize.Width / 2.0f;
            this.halfHeight = imageSize.Height / 2.0f;
        }
        #endregion

        #region Private methods
        private void menuHide_Click(object sender, EventArgs e)
        {
            Visible = false;
            InvalidateFromMenu(sender);
        }
        #endregion
    }
}
