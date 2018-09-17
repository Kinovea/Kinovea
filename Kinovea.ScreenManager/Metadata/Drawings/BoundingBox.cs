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
            get { return rectangle; }
            set { rectangle = value; }
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
        private Rectangle rectangle;
        private Size minimalSize = new Size(50,50);
        #endregion

        public BoundingBox(){}
        public BoundingBox(int side)
        {
            minimalSize = new Size(side, side);
        }

        public void Draw(Graphics canvas, Rectangle rect, Pen pen, SolidBrush brush, int widen)
        {
            canvas.DrawRectangle(pen, rect);
            canvas.FillEllipse(brush, rect.Left - widen, rect.Top - widen, widen * 2, widen * 2);
            canvas.FillEllipse(brush, rect.Left - widen, rect.Bottom - widen, widen * 2, widen * 2);
            canvas.FillEllipse(brush, rect.Right - widen, rect.Top - widen, widen * 2, widen * 2);
            canvas.FillEllipse(brush, rect.Right - widen, rect.Bottom - widen, widen * 2, widen * 2);
        }
        public int HitTest(PointF point, IImageToViewportTransformer transformer)
        {
            int result = -1;
            
            PointF topLeft = new PointF(rectangle.Left, rectangle.Top);
            PointF topRight = new PointF(rectangle.Right, rectangle.Top);
            PointF botRight = new PointF(rectangle.Right, rectangle.Bottom);
            PointF botLeft = new PointF(rectangle.Left, rectangle.Bottom);

            if (HitTester.HitTest(topLeft, point, transformer))
                result = 1;
            else if (HitTester.HitTest(topRight, point, transformer))
                result = 2;
            else if (HitTester.HitTest(botRight, point, transformer))
                result = 3;
            else if (HitTester.HitTest(botLeft, point, transformer))
                result = 4;
            else if (rectangle.Contains(point.ToPoint()))
                result = 0;

            return result;
        }

        public void MoveHandle(PointF point, int handleNumber, Size originalSize, bool keepAspectRatio)
        {
            if (keepAspectRatio)
                MoveHandleKeepAspectRatio(point.ToPoint(), handleNumber, originalSize);
            else
                MoveHandleFree(point.ToPoint(), handleNumber);
        }
        
        public void Move(float dx, float dy)
        {
            rectangle = new Rectangle((int)(rectangle.X + dx), (int)(rectangle.Y + dy), rectangle.Width, rectangle.Height);
        }
        public void MoveAndSnap(int deltaX, int deltaY, Size containerSize, int snapMargin)
        {
            int dx = deltaX;
            int dy = deltaY;

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
        private void MoveHandleKeepAspectRatio(Point point, int handleNumber, Size originalSize)
        {
            // TODO: refactor/simplify.
            
            switch (handleNumber)
            {
                case 1:
                    {
                        // Top left handler.
                        int dx = point.X - rectangle.Left;
                        int newWidth = rectangle.Width - dx;

                        if (newWidth > minimalSize.Width)
                        {
                            double qRatio = (double)newWidth / (double)originalSize.Width;
                            int newHeight = (int)((double)originalSize.Height * qRatio); 	// Only if square.

                            int newY = rectangle.Top + rectangle.Height - newHeight;

                            rectangle = new Rectangle(point.X, newY, newWidth, newHeight);
                        }
                        break;
                    }
                case 2:
                    {

                        // Top right handler.
                        int dx = rectangle.Right - point.X;
                        int newWidth = rectangle.Width - dx;

                        if (newWidth > minimalSize.Width)
                        {
                            double qRatio = (double)newWidth / (double)originalSize.Width;
                            int newHeight = (int)((double)originalSize.Height * qRatio); 	// Only if square.

                            int newY = rectangle.Top + rectangle.Height - newHeight;
                            int newX = point.X - newWidth;

                            rectangle = new Rectangle(newX, newY, newWidth, newHeight);
                        }
                        break;
                    }
                case 3:
                    {
                        // Bottom right handler.
                        int dx = rectangle.Right - point.X;
                        int newWidth = rectangle.Width - dx;

                        if (newWidth > minimalSize.Width)
                        {
                            double qRatio = (double)newWidth / (double)originalSize.Width;
                            int newHeight = (int)((double)originalSize.Height * qRatio); 	// Only if square.

                            int newY = rectangle.Y;
                            int newX = point.X - newWidth;

                            rectangle = new Rectangle(newX, newY, newWidth, newHeight);
                        }
                        break;
                    }
                case 4:
                    {
                        // Bottom left handler.
                        int dx = point.X - rectangle.Left;
                        int newWidth = rectangle.Width - dx;

                        if (newWidth > minimalSize.Width)
                        {
                            double qRatio = (double)newWidth / (double)originalSize.Width;
                            int newHeight = (int)((double)originalSize.Height * qRatio); 	// Only if square.

                            int newY = rectangle.Y;

                            rectangle = new Rectangle(point.X, newY, newWidth, newHeight);
                        }
                        break;
                    }
                default:
                    break;
            }
        }
        public void MoveHandleKeepSymmetry(Point point, int handleNumber, PointF center)
        {
            Rectangle target = Rectangle.Empty;
            Vector shift = new Vector(point, center);

            switch (handleNumber)
            {
                case 1:
                    target = new Rectangle(point.X, point.Y, (int)(shift.X * 2), (int)(shift.Y * 2));
                    break;
                case 2:
                    target = new Rectangle(point.X + (int)(-shift.X * 2), point.Y, (int)(-shift.X * 2), (int)(shift.Y * 2));
                    break;
                case 3:
                    target = new Rectangle(point.X + (int)(-shift.X * 2), point.Y + (int)(-shift.Y * 2), (int)(-shift.X * 2), (int)(-shift.Y * 2));
                    break;
                case 4:
                    target = new Rectangle(point.X, point.Y + (int)(shift.X * 2), (int)(shift.X * 2), (int)(-shift.Y * 2));
                    break;
            }
            
            ApplyWithConstraints(target);
        }
        private void MoveHandleFree(Point point, int handleNumber)
        {
            Rectangle target = Rectangle.Empty;
            
            switch (handleNumber)
            {
                case 1:
                    target = new Rectangle(point.X, point.Y, rectangle.Right - point.X, rectangle.Bottom - point.Y);
                    break;
                case 2:
                    target = new Rectangle(rectangle.Left, point.Y, point.X - rectangle.Left, rectangle.Bottom - point.Y);
                    break;
                case 3:
                    target = new Rectangle(rectangle.Left, rectangle.Top, point.X - rectangle.Left, point.Y - rectangle.Top);
                    break;
                case 4:
                    target = new Rectangle(point.X, rectangle.Top, rectangle.Right - point.X, point.Y - rectangle.Top);
                    break;
            }
            
            ApplyWithConstraints(target);
        }
        private void ApplyWithConstraints(Rectangle target)
        {
            if(target.Width < minimalSize.Width)
                target = new Rectangle(rectangle.Left, target.Top, minimalSize.Width, target.Height);
            
            if(target.Height < minimalSize.Height)
                target = new Rectangle(target.Left, rectangle.Top, target.Width, minimalSize.Height);
            
            rectangle = target;
        }
    }
}
