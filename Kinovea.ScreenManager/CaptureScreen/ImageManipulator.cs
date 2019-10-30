#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Helper for image panning and resizing.
    /// </summary>
    public class ImageManipulator
    {
        #region Properties
        public bool Started
        {
            get { return started;}
        }
        /*public Point ImageLocation
        {
            get { return imageLocation;}
        }
        public Size ImageSize
        {
            get { return ImageSize;}
        }*/
        public Rectangle DisplayRectangle
        {
            get { return displayRectangle;}
        }
        #endregion
    
        #region Members
        private bool started;
        private ManipulationType manipulationType = ManipulationType.None;
        private Point mouseStart;
        private Point mousePrevious;
        private Rectangle refDisplayRectangle;
        private Rectangle displayRectangle;
        private int handle;
        private bool expanded = false;
        private int stickyMargin = 17;
        private Cursor cursorClosedHand;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public ImageManipulator()
        {
            Bitmap bmp = Properties.Drawings.handclose24b;
            cursorClosedHand = new Cursor(bmp.GetHicon());
        }
        
        #region Public methods
        public void Start(Point mouse, int hit, Rectangle displayRectangle)
        {
            if(hit < 0)
            {
                manipulationType = ManipulationType.None;
                return;
            }
            
            manipulationType = hit == 0 ? ManipulationType.Move : ManipulationType.Resize;
            mouseStart = mouse;
            mousePrevious = mouse;
            handle = hit;
            refDisplayRectangle = displayRectangle;
            this.displayRectangle = displayRectangle;
            started = true;
        }
        
        public void Move(Point mouse, bool sticky, Size containerSize, Size referenceSize)
        {
            if(!started)
                return;
                
            Point deltaStart = mouse.Subtract(mouseStart);
            Point delta = mouse.Subtract(mousePrevious);
            mousePrevious = mouse;
            
            if(manipulationType == ManipulationType.Move)
                DoMove(deltaStart.X, deltaStart.Y, sticky, containerSize);
            else if(manipulationType == ManipulationType.Resize)
                DoResize(handle, deltaStart, sticky, referenceSize);
        }
        
        public void End()
        {
            manipulationType = ManipulationType.None;
            started = false;
        }
        
        public void Expand(Size imageSize, Rectangle displayRectangle, Size containerSize)
        {
            if(!displayRectangle.Size.FitsIn(containerSize))
            {
                Maximize(displayRectangle, containerSize);
            }
            else if(!imageSize.FitsIn(displayRectangle.Size))
            {
                Reset(imageSize, containerSize);
            }
            else
            {
                if(expanded)
                    Reset(imageSize, containerSize);
                else
                    Maximize(displayRectangle, containerSize);
            }
        }
        
        public Cursor GetCursorClosedHand()
        {
            // temporary hack.
            // Ultimately the hand tool would be responsible for image manipulation.
            return cursorClosedHand;
        }
        #endregion
       
        private void Maximize(Rectangle displayRectangle, Size containerSize)
        {
            this.displayRectangle = UIHelper.RatioStretch(displayRectangle.Size, containerSize);
            expanded = true;
        }
        
        private void Reset(Size size, Size containerSize)
        {
            Point location = new Point((containerSize.Width - size.Width)/ 2, (containerSize.Height - size.Height) / 2);
            this.displayRectangle = new Rectangle(location, size);
            expanded = false;
        }
        
        private void DoMove(int dx, int dy, bool sticky, Size containerSize)
        {
            displayRectangle = refDisplayRectangle.Translate(dx, dy);
            if(sticky)
                displayRectangle.Location = StickToBorders(displayRectangle, containerSize);
        }
        
        private void DoResize(int handle, Point delta, bool sticky, Size referenceSize)
        {
            int left = refDisplayRectangle.Left;
            int top = refDisplayRectangle.Top;
            int width = refDisplayRectangle.Width;
            int height = refDisplayRectangle.Height;
            
            float dx = (float)delta.X / refDisplayRectangle.Width;
            float dy = (float)delta.Y / refDisplayRectangle.Height;
            float d = Extremum(dx, dy);

            int dxRef = referenceSize.Width - refDisplayRectangle.Width;
            int dyRef = referenceSize.Height - refDisplayRectangle.Height;
            int stickyPixels = 30;
            
            int minWidth = referenceSize.Width / 10;
            int minHeight = referenceSize.Height / 10;

            switch(handle)
            {
                case 1:
                {
                    int x = (int)(d * refDisplayRectangle.Width);
                    int y = (int)(d * refDisplayRectangle.Height);
                    
                    if(sticky)
                    {
                        if(Math.Abs(dxRef + x) < stickyPixels || Math.Abs(dyRef + y) < stickyPixels)
                        {
                            x = -dxRef;
                            y = -dyRef;
                        }
                    }
                    
                    if(refDisplayRectangle.Width - x < minWidth)
                        x = refDisplayRectangle.Width - minWidth;
                        
                    if(refDisplayRectangle.Height - y < minHeight)
                        y = refDisplayRectangle.Height - minHeight;
                    
                    left = refDisplayRectangle.Left + x;
                    top = refDisplayRectangle.Top + y;
                    width = refDisplayRectangle.Right - left;
                    height = refDisplayRectangle.Bottom - top;
                    break;
                }
                case 2:
                {
                    int x = d == dx ? (int)(d * refDisplayRectangle.Width) : (int)(-d * refDisplayRectangle.Width);
                    int y = d == dy ? (int)(d * refDisplayRectangle.Height) : (int)(-d * refDisplayRectangle.Height);
                    
                    if(sticky)
                    {
                        if(Math.Abs(dxRef - x) < stickyPixels || Math.Abs(dyRef + y) < stickyPixels)
                        {
                            x = dxRef;
                            y = -dyRef;
                        }
                    }
                    
                    if(refDisplayRectangle.Right + x - refDisplayRectangle.Left < minWidth)
                        x = minWidth + refDisplayRectangle.Left - refDisplayRectangle.Right;
                    
                    if(refDisplayRectangle.Height - y < minHeight)
                        y = refDisplayRectangle.Height - minHeight;
                    
                    left = refDisplayRectangle.Left;
                    top = refDisplayRectangle.Top + y;
                    width = refDisplayRectangle.Width + x;
                    height = refDisplayRectangle.Bottom - top;
                    break;
                }
                case 3:
                {
                    int x = (int)(d * refDisplayRectangle.Width);
                    int y = (int)(d * refDisplayRectangle.Height);
                    
                    if(sticky)
                    {
                        if(Math.Abs(dxRef - x) < stickyPixels || Math.Abs(dyRef - y) < stickyPixels)
                        {
                            x = dxRef;
                            y = dyRef;
                        }
                    }
                    
                    if(refDisplayRectangle.Right + x - refDisplayRectangle.Left < minWidth)
                        x = minWidth + refDisplayRectangle.Left - refDisplayRectangle.Right;
                        
                    if(refDisplayRectangle.Bottom + y - refDisplayRectangle.Top < minHeight)
                        y = minHeight + refDisplayRectangle.Top - refDisplayRectangle.Bottom;
                    
                    left = refDisplayRectangle.Left;
                    top = refDisplayRectangle.Top;
                    width = refDisplayRectangle.Width + x;
                    height = refDisplayRectangle.Height + y;
                    break;
                }
                case 4:
                {
                    int x = d == dx ? (int)(d * refDisplayRectangle.Width) : (int)(-d * refDisplayRectangle.Width);
                    int y = d == dy ? (int)(d * refDisplayRectangle.Height) : (int)(-d * refDisplayRectangle.Height);
                    
                    if(sticky)
                    {
                        if(Math.Abs(dxRef + x) < stickyPixels || Math.Abs(dyRef - y) < stickyPixels)
                        {
                            x = -dxRef;
                            y = dyRef;
                        }
                    }
                    
                    if(refDisplayRectangle.Width - x < minWidth)
                        x = refDisplayRectangle.Width - minWidth;
                    
                    if(refDisplayRectangle.Bottom + y - refDisplayRectangle.Top < minHeight)
                        y = minHeight + refDisplayRectangle.Top - refDisplayRectangle.Bottom;
                    
                    left = refDisplayRectangle.Left + x;
                    top = refDisplayRectangle.Top;
                    width = refDisplayRectangle.Right - left;
                    height = refDisplayRectangle.Height + y;
                    break;
                }
            }
            
            displayRectangle = new Rectangle(left, top, width, height);
            
            //log.DebugFormat("Delta:{0}, Rectangle ref:{1}, New:{2}", delta, refDisplayRectangle, displayRectangle);
        }
        
        private int Extremum(int a, int b)
        {
            if(Math.Abs(a) > Math.Abs(b))
                return a;
            else
                return b;
        }
        
        private float Extremum(float a, float b)
        {
            if(Math.Abs(a) > Math.Abs(b))
                return a;
            else
                return b;
        }
        
        private Point StickToBorders(Rectangle rect, Size container)
        {
            Point result = rect.Location;
            
            if(rect.X > -stickyMargin && rect.X < stickyMargin)
                result.X = 0;
            
            if(rect.Y > -stickyMargin && rect.Y < stickyMargin)
                result.Y = 0;
            
            if(rect.X + rect.Width > container.Width - stickyMargin && 
               rect.X + rect.Width < container.Width + stickyMargin)
                result.X = container.Width - rect.Width;

            if(rect.Y + rect.Height > container.Height - stickyMargin && 
               rect.Y + rect.Height < container.Height + stickyMargin)
                result.Y = container.Height - rect.Height;

            return result;
        }
    }
}
