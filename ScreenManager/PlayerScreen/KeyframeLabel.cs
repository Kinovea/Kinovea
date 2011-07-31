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
		public string Text
		{
			get { return m_Text; }
			set { m_Text = value; }
		}
		public Point AttachLocation
        {
			get { return m_AttachLocation;}
			set 
			{
				// Note: to also trigger the update of the mini label, use MoveTo().
				m_AttachLocation = value;
			}
		}
		public Point TopLeft
		{
			get { return m_TopLeft;}
			set { m_TopLeft = value;}
		}
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
        private string m_Text;
        
        private Point m_TopLeft;                         			// absolute position of label (in image coords).
        private double m_fStretchFactor;
		
        private LabelBackground m_LabelBackground = new LabelBackground();
        private SizeF m_BackgroundSize;								// Size of the area taken by the text. (without margins) (scaled).
        
        private long m_iTimestamp;                 					// Absolute time.
        
        private int m_iAttachIndex;									// The index of the reference point in the track points list.
        private Point m_AttachLocation = new Point(0,0);			// The point we are attached to (image coordinates).
        private Point m_AttachLocationRescaled = new Point(0,0);	// The point we are attached to (scaled coordinates).
        
        private StyleHelper m_StyleHelper = new StyleHelper();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction
        public KeyframeLabel() : this(Point.Empty, Color.Black){}
        public KeyframeLabel(Point _attachPoint, Color _color)
        {
        	m_AttachLocation = _attachPoint;
        	m_TopLeft = new Point(_attachPoint.X - 20, _attachPoint.Y - 50);
        	
        	m_Text = "Label";
        	m_StyleHelper.Font = new Font("Arial", 8, FontStyle.Bold);
        	m_StyleHelper.Bicolor = new Bicolor(Color.FromArgb(160, _color));
        	m_fStretchFactor = 1.0;
        }
        public KeyframeLabel(XmlReader _xmlReader, PointF _scale)
            : this(Point.Empty, Color.Black)
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion

        #region Public methods
        public bool HitTest(Point _point)
        {
            // _point is mouse coordinates already descaled.
            Size descaledSize = new Size((int)((m_BackgroundSize.Width + m_LabelBackground.MarginWidth) / m_fStretchFactor), (int)((m_BackgroundSize.Height + m_LabelBackground.MarginHeight) / m_fStretchFactor));

            GraphicsPath areaPath = new GraphicsPath();
            areaPath.AddRectangle(new Rectangle(m_TopLeft.X, m_TopLeft.Y, descaledSize.Width, descaledSize.Height));

            // Create region from the path
            Region areaRegion = new Region(areaPath);

            return areaRegion.IsVisible(_point);
        }
        public override int GetHashCode()
        {
            int iHash = 0;
            
            iHash ^= m_StyleHelper.GetHashCode();
            iHash ^= m_TopLeft.GetHashCode();
            
            return iHash;
        }
        public void Draw(Graphics _canvas, double _fStretchFactor, Point _DirectZoomTopLeft, double _fOpacityFactor)
        {
        	m_fStretchFactor = _fStretchFactor;
        	
            // Draw background rounded rectangle.
            // all sizes are based on font size.
            Font f = m_StyleHelper.GetFont((float)m_fStretchFactor);
            m_BackgroundSize = _canvas.MeasureString( " " + m_Text + " ", f);
            int radius = (int)(f.Size / 2);
            
            RescaleCoordinates(m_fStretchFactor, _DirectZoomTopLeft);
        	
        	// Small dot and connector.        	
        	SolidBrush fillBrush = m_StyleHelper.GetBackgroundBrush((int)(_fOpacityFactor*255));
        	Pen p = m_StyleHelper.GetBackgroundPen((int)(_fOpacityFactor*64));
            
            _canvas.FillEllipse(fillBrush, m_AttachLocationRescaled.X - 2, m_AttachLocationRescaled.Y - 2, 4, 4);
            _canvas.DrawLine(p, m_AttachLocationRescaled.X, m_AttachLocationRescaled.Y, m_LabelBackground.Location.X + m_BackgroundSize.Width / 2, m_LabelBackground.Location.Y + m_BackgroundSize.Height / 2);
            p.Dispose();
            fillBrush.Dispose();
            
            // Background
        	m_LabelBackground.Draw(_canvas, _fOpacityFactor, radius, (int)m_BackgroundSize.Width, (int)m_BackgroundSize.Height, m_StyleHelper.Bicolor.Background);
        	
        	// Text
        	SolidBrush fontBrush = m_StyleHelper.GetForegroundBrush((int)(_fOpacityFactor*255));
        	_canvas.DrawString(" " + m_Text, f, fontBrush, m_LabelBackground.TextLocation);
        	fontBrush.Dispose();
        	f.Dispose();
        }    
        public void MoveTo(Point _attachPoint)
        {
        	// This method update the attach point AND report the same ammount of displacement on the mini label.
        	int dx = _attachPoint.X - m_AttachLocation.X;
			int dy = _attachPoint.Y - m_AttachLocation.Y;
				
			m_AttachLocation = _attachPoint;
			m_TopLeft = new Point(m_TopLeft.X + dx, m_TopLeft.Y + dy);
        }
        public void MoveLabel(Point _point)
        {
        	// _point is mouse coordinates already descaled.
        	// Move the center of the mini label there.
        	m_TopLeft = new Point((int)(_point.X - (m_BackgroundSize.Width/2)), (int)(_point.Y - (m_BackgroundSize.Height/2)));
        }
        public void MoveLabel(int dx, int dy)
        {
        	m_TopLeft = new Point(m_TopLeft.X + dx, m_TopLeft.Y + dy);
        }
        public void WriteXml(XmlWriter _xmlWriter)
        {
            _xmlWriter.WriteElementString("SpacePosition", String.Format("{0};{1}", m_TopLeft.X, m_TopLeft.Y));
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
				        m_TopLeft = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
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
        
        #region Private methods
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
        	m_LabelBackground.Location = new Point((int)((double)(m_TopLeft.X-_DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_TopLeft.Y-_DirectZoomTopLeft.Y) * _fStretchFactor));
        	m_AttachLocationRescaled = new Point((int)((double)(m_AttachLocation.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_AttachLocation.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));            
        }
		#endregion            
    }
}
