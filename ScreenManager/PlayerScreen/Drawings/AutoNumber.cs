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
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// AutoNumber. (MultiDrawingItem of AutoNumberManager)
	/// Describe and draw a single autonumber.
	/// </summary>
	public class AutoNumber : IKvaSerializable
	{
	    #region Properties
	    public int Value {
	        get { return m_Value;}
	    }
	    #endregion

		#region Members
		private long m_iPosition;
		private RoundedRectangle m_Background = new RoundedRectangle();   // <-- Also used as a simple ellipsis-defining rectangle when value < 10.
		private InfosFading m_InfosFading;
		private int m_Value = 1;
		private double m_LastScaleFactor = 1.0;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public AutoNumber(long _iPosition, long _iAverageTimeStampsPerFrame, Point _location, int _value)
		{
			m_iPosition = _iPosition;
			m_Background.Rectangle = new Rectangle(_location, Size.Empty);
			m_Value = _value;
			
			m_InfosFading = new InfosFading(_iPosition, _iAverageTimeStampsPerFrame);
			m_InfosFading.UseDefault = false;
			m_InfosFading.FadingFrames = 25;
			
			SetText(m_Value.ToString());
		}
		public AutoNumber(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback, long _iAverageTimeStampsPerFrame)
            : this(0, 0, Point.Empty, 0)
        {
		     ReadXml(_xmlReader, _scale, _remapTimestampCallback);
		     
		     m_InfosFading = new InfosFading(m_iPosition, _iAverageTimeStampsPerFrame);
			 m_InfosFading.UseDefault = false;
			 m_InfosFading.FadingFrames = 25;
			 
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
			
			m_LastScaleFactor = _transformer.Scale;
			
			using(SolidBrush brushBack = new SolidBrush(backColor))
			using(SolidBrush brushFront = new SolidBrush(frontColor))
			using(Pen penContour = new Pen(frontColor, 2))
			using(Font f = new Font("Arial", 16, FontStyle.Bold))
			{
			    string text = m_Value.ToString();
                
			    SizeF textSize = _canvas.MeasureString(text, f);
                Point location = _transformer.Transform(m_Background.Rectangle.Location);
                Rectangle rect = new Rectangle(location, m_Background.Rectangle.Size);
                
			    if(m_Value < 10)
			    {
			        _canvas.FillEllipse(brushBack, rect);
			        _canvas.DrawEllipse(penContour, rect);
			    }
                else
                {
                    RoundedRectangle.Draw(_canvas, rect, brushBack, f.Height/4, false, true, penContour);    
                }
                
                Point textLocation = new Point(location.X + (int)((rect.Width - textSize.Width)/2), location.Y + 2);
                _canvas.DrawString(text, f, brushFront, textLocation);
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
			    // Special case. We double-unscale the drawing (but not its location).
			    Rectangle rect = new Rectangle(m_Background.Rectangle.Location, new Size((int)(m_Background.Rectangle.Width / m_LastScaleFactor), (int)(m_Background.Rectangle.Height / m_LastScaleFactor)));
			    if(rect.Contains(_point))
			        return 0;
			}

			return iHitResult;
		}
		public void MouseMove(int _deltaX, int _deltaY)
		{
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
		
		#region KVA Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback)
        {
            if(_remapTimestampCallback == null)
            {
                _xmlReader.ReadOuterXml();
                return;                
            }
            
            _xmlReader.ReadStartElement();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "Time":
				        m_iPosition = _remapTimestampCallback(_xmlReader.ReadElementContentAsLong(), false);
                        break;
					case "Location":
                        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        m_Background.Rectangle = new Rectangle(p.Scale(_scale.X, _scale.Y), Size.Empty);
                        break;
					case "Value":
                        m_Value = _xmlReader.ReadElementContentAsInt();
						break;
				    default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			_xmlReader.ReadEndElement();
        }
        public void WriteXml(XmlWriter _xmlWriter)
		{
            _xmlWriter.WriteElementString("Time", m_iPosition.ToString());
            _xmlWriter.WriteElementString("Location", string.Format("{0};{1}", m_Background.X, m_Background.Y));
            _xmlWriter.WriteElementString("Value", m_Value.ToString());
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
                
                int width = m_Value < 10 ? (int)textSize.Height : (int)textSize.Width;
                int height = (int)textSize.Height;
                m_Background.Rectangle = new Rectangle(m_Background.Rectangle.Location, new Size(width, height));
            }
		}
		#endregion
	}
}
