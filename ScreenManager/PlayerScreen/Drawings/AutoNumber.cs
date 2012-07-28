#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// AutoNumber. (MultiDrawingItem of AutoNumberManager)
	/// Describe and draw a single autonumber.
	/// </summary>
	public class AutoNumber
	{
	    #region Properties
	    public int Value {
	        get { return m_Value;}
	    }
	    #endregion

		#region Members
		private long m_iPosition;
		private RoundedRectangle m_Background = new RoundedRectangle();
		private InfosFading m_InfosFading;
		private int m_Value = 1;
		#endregion
		
		#region Constructor
		public AutoNumber(long _iPosition, long _iAverageTimeStampsPerFrame, Point _center, int _value)
		{
			m_iPosition = _iPosition;
			m_Background.Rectangle = new Rectangle(_center, Size.Empty);
			m_InfosFading = new InfosFading(_iPosition, _iAverageTimeStampsPerFrame);
			m_InfosFading.UseDefault = false;
			m_InfosFading.FadingFrames = 25;
			m_Value = _value;
			SetText(m_Value.ToString());
		}
		#endregion
		
		#region Public methods
		public void Draw(Graphics _canvas, CoordinateSystem _transformer, long _timestamp)
        {
			double fOpacityFactor = m_InfosFading.GetOpacityFactor(_timestamp);
			if(fOpacityFactor <= 0)
			    return;
		
			int alpha = (int)(255 * fOpacityFactor);
			
			Color backColor = Color.FromArgb(alpha, Color.Black);
			Color frontColor = Color.FromArgb(alpha, Color.White);
			
			using(SolidBrush brushBack = new SolidBrush(backColor))
			using(SolidBrush brushFront = new SolidBrush(frontColor))
			using(Pen penContour = new Pen(frontColor, 2))
			using(Font f = new Font("Arial", 16, FontStyle.Bold))
			{
			    string text = m_Value.ToString();
                
			    SizeF textSize = _canvas.MeasureString(text, f);
                Point location = _transformer.Transform(m_Background.Rectangle.Location);
                
			    if(m_Value < 10)
			    {
			        int side = (int)textSize.Height;
			        Rectangle rect = new Rectangle(new Point(location.X + (int)((textSize.Width - side)/2), location.Y), new Size(side, side));
			        _canvas.FillEllipse(brushBack, rect);
			        _canvas.DrawEllipse(penContour, rect);
                    _canvas.DrawString(text, f, brushFront, location.Translate(0, 2));
			    }
                else
                {
                    Size size = new Size((int)textSize.Width, (int)textSize.Height);
                    Rectangle rect = new Rectangle(location, size);
                    RoundedRectangle.Draw(_canvas, rect, brushBack, f.Height/4, false, true, penContour);    
                    _canvas.DrawString(text, f, brushFront, rect.Location.Translate(0, 2));
                }
			}
		}
		public int HitTest(Point _point, long _iCurrentTimeStamp)
		{
			// Note: Coordinates are already descaled.
            // Hit Result: -1: miss, 0: on object, 1 on handle.
			int iHitResult = -1;
			double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimeStamp);
			if(fOpacityFactor > 0)
			{
			    return m_Background.HitTest(_point, false);
			}
			return iHitResult;
		}
		public void MouseMove(int _deltaX, int _deltaY)
		{
			//m_Center.X += _deltaX;
            //m_Center.Y += _deltaY;
            m_Background.Move(_deltaX, _deltaY);
		}
		public void MoveHandleTo(Point point)
        {
            // Not implemented.
        }
		public bool IsVisible(long _timestamp)
		{
			return m_InfosFading.GetOpacityFactor(_timestamp) > 0;
		}
		#endregion
		
		#region Private methods
		private void SetText(string text)
		{
            using(Button but = new Button())
            using(Graphics g = but.CreateGraphics())
            using(Font f = new Font("Arial", 16, FontStyle.Bold))
            {
                SizeF textSize = g.MeasureString(text, f);
                m_Background.Rectangle = new Rectangle(m_Background.Rectangle.Location, new Size((int)textSize.Width, (int)textSize.Height));
            }
		}
		#endregion
	}
}
