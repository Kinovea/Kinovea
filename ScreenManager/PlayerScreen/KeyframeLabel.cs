#region License
/*
Copyright © Joan Charmant 2008-2011.
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// A class to encapsulate a mini label.
	/// Mainly used for Keyframe labels / line measure / track speed.
	/// 
	/// The object is comprised of an attach point and the mini label itself.
	/// The label can be moved relatively to the attach point from the container drawing tool.
	/// 
	/// The mini label position is expressed in absolute coordinates. (previously was relative to the attach).
	/// 
	/// The text to display is actually reset just before we need to draw it.
	/// </summary>
    public class KeyframeLabel
    {
        #region Properties
		public long Timestamp
        {
        	get { return m_iTimestamp; }
			set { m_iTimestamp = value; }	
        }
		public int AttachIndex
		{
			get { return m_iAttachIndex; }
			set { m_iAttachIndex = value; }
		}
		public Color BackColor
		{
			get { return m_StyleHelper.Bicolor.Background; }
			set { m_StyleHelper.Bicolor = new Bicolor(value); }
		}
        #endregion

        #region Members
        private string m_Text = "Label";
        private RoundedRectangle m_Background = new RoundedRectangle();
        private long m_iTimestamp;                 					// Absolute time.
        private int m_iAttachIndex;									// The index of the reference point in the track points list.
        private Point m_AttachLocation;			                    // The point we are attached to (image coordinates).
        private StyleHelper m_StyleHelper = new StyleHelper();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction
        public KeyframeLabel() : this(Point.Empty, Color.Black){}
        public KeyframeLabel(Point _attachPoint, Color _color)
        {
        	m_AttachLocation = _attachPoint;
        	m_Background.Rectangle = new Rectangle(_attachPoint.Translate(-20, -50), Size.Empty);
        	m_StyleHelper.Font = new Font("Arial", 8, FontStyle.Bold);
        	m_StyleHelper.Bicolor = new Bicolor(Color.FromArgb(160, _color));
        }
        public KeyframeLabel(XmlReader _xmlReader, PointF _scale)
            : this(Point.Empty, Color.Black)
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion

        #region Public methods
        public bool HitTest(Point _point, CoordinateSystem transformer)
        {
            return (m_Background.HitTest(_point, false, transformer) > -1);
        }
        public override int GetHashCode()
        {
            int iHash = 0;
            iHash ^= m_Background.Rectangle.Location.GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();
            return iHash;
        }
        public void Draw(Graphics _canvas, CoordinateSystem _transformer, double _fOpacityFactor)
        {
        	using(SolidBrush fillBrush = m_StyleHelper.GetBackgroundBrush((int)(_fOpacityFactor*255)))
        	using(Pen p = m_StyleHelper.GetBackgroundPen((int)(_fOpacityFactor*64)))
            using(Font f = m_StyleHelper.GetFont((float)_transformer.Scale))
            using(SolidBrush fontBrush = m_StyleHelper.GetForegroundBrush((int)(_fOpacityFactor*255)))
        	{
                
                // Small dot and connector. 
                Point attch = _transformer.Transform(m_AttachLocation);
                Point center = _transformer.Transform(m_Background.Center);
                _canvas.FillEllipse(fillBrush, attch.Box(2));
                _canvas.DrawLine(p, attch, center);
                
                // Background and text.
                SizeF textSize = _canvas.MeasureString(m_Text, f);
                Point location = _transformer.Transform(m_Background.Rectangle.Location);
                Size size = new Size((int)textSize.Width, (int)textSize.Height);
                Rectangle rect = new Rectangle(location, size);
                RoundedRectangle.Draw(_canvas, rect, fillBrush, f.Height/4, false, false, null);
                _canvas.DrawString(m_Text, f, fontBrush, rect.Location);
        	}
        }    
        public void SetAttach(Point _point, bool _moveLabel)
        {
            int dx = _point.X - m_AttachLocation.X;
			int dy = _point.Y - m_AttachLocation.Y;
            
            m_AttachLocation = _point;
            
            if(_moveLabel)
                m_Background.Move(dx, dy);
        }
        public void SetLabel(Point _point)
        {
            m_Background.CenterOn(_point);
        }
        public void MoveLabel(int dx, int dy)
        {
            m_Background.Move(dx, dy);
        }
        public void SetText(string _text)
        {
            m_Text = _text;

            using(Button but = new Button())
            using(Graphics g = but.CreateGraphics())
            using(Font f = m_StyleHelper.GetFont(1F))
            {
                SizeF textSize = g.MeasureString(m_Text, f);
                m_Background.Rectangle = new Rectangle(m_Background.Rectangle.Location, new Size((int)textSize.Width, (int)textSize.Height));
            }
        }
        public void WriteXml(XmlWriter _xmlWriter)
        {
            _xmlWriter.WriteElementString("SpacePosition", String.Format("{0};{1}", m_Background.X, m_Background.Y));
            _xmlWriter.WriteElementString("TimePosition", m_iTimestamp.ToString());
        }
        public void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
             _xmlReader.ReadStartElement();
             
             while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "SpacePosition":
				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
				        Point location = new Point((int)(_scale.X * p.X), (int)(_scale.Y * p.Y));
				        m_Background.Rectangle = new Rectangle(location, Size.Empty);
                        break;
					case "TimePosition":
                        m_iTimestamp = _xmlReader.ReadElementContentAsLong();
                        break;
					default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
             
            _xmlReader.ReadEndElement();
        }
        #endregion
    }
}
