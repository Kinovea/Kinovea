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
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A helper class for drawings using a bounding box.
    /// Basically a wrapper around a rectangle, with added method for moving, hit testing, and drawing.
    /// By convention, the handles id of the bounding box will always be 1 to 4.
    /// When the drawing has other handles, they should start at id 5.
    /// </summary>
    public class BoundingBox
    {
        #region Properties
        public Rectangle Rectangle
        {
            get { return rectangle.ToRectangle(); }
            set { rectangle = value; }
        }

        public float X
        {
            get { return rectangle.X; }
        }

        public float Y
        {
            get { return rectangle.Y; }
        }

        public int ContentHash
        {
            get
            {
                int hash = rectangle.GetHashCode();
                return hash;
            }
        }
        #endregion

        #region Members
        private RectangleF rectangle;
        private SizeF minimalSize = new SizeF(50,50);
        #endregion

        public BoundingBox(){}
        public BoundingBox(int minSide)
        {
            minimalSize = new Size(minSide, minSide);
        }

        public void Draw(Graphics canvas, Rectangle rect, Pen pen, SolidBrush brush, int widen)
        {
            canvas.DrawRectangle(pen, rect);
            
            int size = widen * 2;
            canvas.FillEllipse(brush, rect.Left - widen, rect.Top - widen, size, size);
            canvas.FillEllipse(brush, rect.Left - widen, rect.Bottom - widen, size, size);
            canvas.FillEllipse(brush, rect.Right - widen, rect.Top - widen, size, size);
            canvas.FillEllipse(brush, rect.Right - widen, rect.Bottom - widen, size, size);
        }

        /// <summary>
        /// Mapping: -1: not hit, 0: in the rectangle, 1-4: corners. 
        /// </summary>
        public int HitTest(PointF point, IImageToViewportTransformer transformer)
        {
            int result = -1;
            
            PointF topLeft = new PointF(rectangle.Left, rectangle.Top);
            PointF topRight = new PointF(rectangle.Right, rectangle.Top);
            PointF botRight = new PointF(rectangle.Right, rectangle.Bottom);
            PointF botLeft = new PointF(rectangle.Left, rectangle.Bottom);

            if (HitTester.HitPoint(point, topLeft, transformer))
                result = 1;
            else if (HitTester.HitPoint(point, topRight, transformer))
                result = 2;
            else if (HitTester.HitPoint(point, botRight, transformer))
                result = 3;
            else if (HitTester.HitPoint(point, botLeft, transformer))
                result = 4;
            else if (rectangle.Contains(point.ToPoint()))
                result = 0;

            return result;
        }

        public void MoveHandle(PointF point, int handleNumber, Size originalSize, bool keepAspectRatio)
        {
            if (keepAspectRatio)
                MoveHandleKeepAspectRatio(point, handleNumber, originalSize);
            else
                MoveHandleFree(point, handleNumber);
        }
        
        public void Move(float dx, float dy)
        {
            rectangle = rectangle.Translate(dx, dy);
        }
        public void MoveAndSnap(float dx, float dy, Size containerSize, int snapMargin)
        {
            if (containerSize == Size.Empty)
            {
                Move(dx, dy);
                return;
            }

            if(rectangle.Left + dx < snapMargin)
                dx = - rectangle.Left;
            
            if(rectangle.Right + dx > containerSize.Width - snapMargin)
                dx = containerSize.Width - rectangle.Right;
            
            if(rectangle.Top + dy < snapMargin)
                dy = - rectangle.Top;
            
            if(rectangle.Bottom + dy > containerSize.Height - snapMargin)
                dy = containerSize.Height - rectangle.Bottom;
            
            Move(dx, dy);
        }
        private void MoveHandleKeepAspectRatio(PointF point, int handleNumber, Size originalSize)
        {
            // TODO: refactor/simplify.
            
            switch (handleNumber)
            {
                case 1:
                    {
                        // Top left handler.
                        float dx = point.X - rectangle.Left;
                        float newWidth = rectangle.Width - dx;

                        if (newWidth > minimalSize.Width)
                        {
                            float scale = newWidth / originalSize.Width;
                            float newHeight = originalSize.Height * scale;
                            float newY = rectangle.Top + rectangle.Height - newHeight;
                            rectangle = new RectangleF(point.X, newY, newWidth, newHeight);
                        }
                        break;
                    }
                case 2:
                    {

                        // Top right handler.
                        float dx = rectangle.Right - point.X;
                        float newWidth = rectangle.Width - dx;

                        if (newWidth > minimalSize.Width)
                        {
                            float scale = newWidth / originalSize.Width;
                            float newHeight = originalSize.Height * scale;
                            float newY = rectangle.Top + rectangle.Height - newHeight;
                            float newX = point.X - newWidth;
                            rectangle = new RectangleF(newX, newY, newWidth, newHeight);
                        }
                        break;
                    }
                case 3:
                    {
                        // Bottom right handler.
                        float dx = rectangle.Right - point.X;
                        float newWidth = rectangle.Width - dx;

                        if (newWidth > minimalSize.Width)
                        {
                            float scale = newWidth / originalSize.Width;
                            float newHeight = originalSize.Height * scale;
                            float newY = rectangle.Y;
                            float newX = point.X - newWidth;
                            rectangle = new RectangleF(newX, newY, newWidth, newHeight);
                        }
                        break;
                    }
                case 4:
                    {
                        // Bottom left handler.
                        float dx = point.X - rectangle.Left;
                        float newWidth = rectangle.Width - dx;
                        
                        if (newWidth > minimalSize.Width)
                        {
                            float scale = newWidth / originalSize.Width;
                            float newHeight = originalSize.Height * scale;
                            float newY = rectangle.Y;
                            rectangle = new RectangleF(point.X, newY, newWidth, newHeight);
                        }
                        break;
                    }
                default:
                    break;
            }
        }
        
        
        /// <summary>
        /// Move a corner handle to a new location.
        /// Automatically moves the other corners to keep the bounding box centered.
        /// </summary>
        public void MoveHandleKeepSymmetry(Point point, int handleNumber, PointF center, bool square)
        {
            float dx = Math.Abs(center.X - point.X);
            float dy = Math.Abs(center.Y - point.Y);
            
            if (square)
            {
                float d = Math.Max(dx, dy);
                dx = dy = d;
            }

            Rectangle target = new RectangleF(center.X - dx, center.Y - dy, dx * 2, dy * 2).ToRectangle();
            ApplyWithConstraints(target);
        }
        private void MoveHandleFree(PointF point, int handleNumber)
        {
            RectangleF target = RectangleF.Empty;
            
            switch (handleNumber)
            {
                case 1:
                    target = new RectangleF(point.X, point.Y, rectangle.Right - point.X, rectangle.Bottom - point.Y);
                    break;
                case 2:
                    target = new RectangleF(rectangle.Left, point.Y, point.X - rectangle.Left, rectangle.Bottom - point.Y);
                    break;
                case 3:
                    target = new RectangleF(rectangle.Left, rectangle.Top, point.X - rectangle.Left, point.Y - rectangle.Top);
                    break;
                case 4:
                    target = new RectangleF(point.X, rectangle.Top, rectangle.Right - point.X, point.Y - rectangle.Top);
                    break;
            }
            
            ApplyWithConstraints(target);
        }
        private void ApplyWithConstraints(RectangleF target)
        {
            if(target.Width < minimalSize.Width)
                target = new RectangleF(rectangle.Left, target.Top, minimalSize.Width, target.Height);
            
            if(target.Height < minimalSize.Height)
                target = new RectangleF(target.Left, rectangle.Top, target.Width, minimalSize.Height);
            
            rectangle = target;
        }
    }
}
