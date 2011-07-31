/*
Copyright © Joan Charmant 2009.
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
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager.Obsolete
{
	/// <summary>
	/// Class that holds the style information for lines, including size, shape and color.
	/// Implemented in a Pen object.
	/// </summary>
	// fixme: turn into a value type ?
	// only has data, not behavior and returns internal data as copies, not references.
	public class LineStyle
    {
        #region Properties
        public static LineStyle DefaultValue
        {
        	get { return new LineStyle(5, LineShape.Simple, Color.White); }
        }
        public int Size
        {
        	get { return m_iSize;}
        }
        public Color Color
        {
        	get { return m_Color;}
        }
        public LineShape Shape
        {
        	get { return m_LineShape;}
        }
        #endregion
        
        #region Members
        private Pen m_Pen;
        private int m_iSize;
        private LineShape m_LineShape;
        private Color m_Color;
        #endregion

        #region Constructor
        public LineStyle() 
        	: this(1, LineShape.Simple, Color.Black)
        {
        }
        public LineStyle(int _iSize, LineShape _lineShape, Color _color)
        {
        	m_iSize = _iSize;
        	m_LineShape = _lineShape;
        	m_Color = _color;
        	m_Pen = new Pen(_color, _iSize);
        	SetPen();
        }
        #endregion
        
        private void SetPen()
        {
        	m_Pen.Width = m_iSize;
        	m_Pen.Color = m_Color;
        	
        	switch(m_LineShape)
        	{
        		case LineShape.Simple:
        			m_Pen.StartCap = LineCap.Round;
        			m_Pen.EndCap = LineCap.Round;
        			m_Pen.DashStyle = DashStyle.Solid;
        			break;
        		case LineShape.EndArrow:
        			m_Pen.StartCap = LineCap.Round;
        			m_Pen.EndCap = LineCap.ArrowAnchor;
        			m_Pen.DashStyle = DashStyle.Solid;
        			break;
        		case LineShape.DoubleArrow:
        			m_Pen.StartCap = LineCap.ArrowAnchor;
        			m_Pen.EndCap = LineCap.ArrowAnchor;
        			m_Pen.DashStyle = DashStyle.Solid;
        			break;
        		case LineShape.Dash:
        			m_Pen.StartCap = LineCap.Round;
        			m_Pen.EndCap = LineCap.Round;
        			m_Pen.DashStyle = DashStyle.Dash;
        			break;
        		case LineShape.DashDot:
        			m_Pen.StartCap = LineCap.Round;
        			m_Pen.EndCap = LineCap.Round;
        			m_Pen.DashStyle = DashStyle.DashDot;
        			break;
        		case LineShape.Track:
        			m_Pen.StartCap = LineCap.Round;
        			m_Pen.EndCap = LineCap.Round;
        			m_Pen.DashStyle = DashStyle.Solid;
        			m_Pen.CompoundArray = new float[] { 0.0F, 0.40F, 0.60F, 1.0F };
        			break;
        	}
        	
        	m_Pen.LineJoin = LineJoin.Round;
        	
        }
        
        public override int GetHashCode()
        {
        	return m_iSize.GetHashCode() ^ m_LineShape.GetHashCode() ^ m_Color.GetHashCode();
        }
        public void Draw(Graphics _canvas, bool _bCircle, Color _color)
        {
        	// The style draws itself on a canvas.
        	
        	// The internal pen object includes color information,
        	// however this function is only used for style picking buttons, 
        	// The internal color may be overridden by the caller, 
        	// we will not persist it to the actual m_Color member.
        	// (Usually we'll want this to be drawn in black.)
        	
        	_canvas.SmoothingMode = SmoothingMode.AntiAlias;
        	
        	if(_bCircle)
        	{
        		// Show the style as circles (e.g. for pencil tool styles).
        		int left = ((int)_canvas.VisibleClipBounds.Width - m_iSize) / 2;
                int top = ((int)_canvas.VisibleClipBounds.Height - m_iSize) / 2;
                
                SolidBrush b = new SolidBrush(_color);
                _canvas.FillEllipse(b, left, top, m_iSize, m_iSize);
                b.Dispose();
        	}
        	else
        	{
        		// Show the styles as lines (e.g. for line2D or Tracks).
        		Pen p = (Pen)m_Pen.Clone();
        		p.Color = _color;
	        	_canvas.DrawLine(p, 2, _canvas.VisibleClipBounds.Height / 2, _canvas.VisibleClipBounds.Width - 4, _canvas.VisibleClipBounds.Height / 2);
	        	p.Dispose();
        	}
        }	
        public void ToXml(XmlTextWriter _xmlWriter)
        {
        	_xmlWriter.WriteStartElement("LineStyle");
        	_xmlWriter.WriteStartElement("Size");
            _xmlWriter.WriteString(m_iSize.ToString());
            _xmlWriter.WriteEndElement();
            _xmlWriter.WriteStartElement("LineShape");
            _xmlWriter.WriteString(m_LineShape.ToString());
            _xmlWriter.WriteEndElement();
			_xmlWriter.WriteStartElement("ColorRGB");
			_xmlWriter.WriteString(m_Color.R.ToString() + ";" + m_Color.G.ToString() + ";" + m_Color.B.ToString());
            _xmlWriter.WriteEndElement();
            _xmlWriter.WriteEndElement();
        }
        public static LineStyle FromXml(XmlReader _xmlReader)
        {
            // Read all tags between <LineStyle> and </LineStyle> and fills up an object.
            LineStyle lineStyle;
            int iSize = 1;
            LineShape lineShape = LineShape.Simple; 
            Color col = Color.Black;

            try
            {
	            while (_xmlReader.Read())
	            {
	                if (_xmlReader.IsStartElement())
	                {
	                    if (_xmlReader.Name == "Size")
	                    {
	                        iSize = int.Parse(_xmlReader.ReadString());
	                    }
	                    else if (_xmlReader.Name == "LineShape")
	                    {
	                    	lineShape = (LineShape)LineShape.Parse(lineShape.GetType(), _xmlReader.ReadString());
	                    }
	                    else if (_xmlReader.Name == "ColorRGB")
	                    {
	                    	col = XmlHelper.ParseColor(_xmlReader.ReadString());	
	                    }
	                    else
	                    {
	                        // forward compatibility : ignore new fields. 
	                    }
	                }
	                else if (_xmlReader.Name == "LineStyle")
	                {
	                    break;
	                }
	                else
	                {
	                    // Fermeture d'un tag interne.
	                }
	            }  
	            
	            lineStyle = new LineStyle(iSize, lineShape, col);
	            
            }
            catch(Exception)
            {
            	lineStyle = DefaultValue;	
            }
            
            return lineStyle;
        }
        public LineStyle Clone()
        {
        	return new LineStyle(m_iSize, m_LineShape, m_Color);	
        }
        public void Update(LineStyle _style, bool _bUpdateColor, bool _bUpdateSize, bool _bUpdateLineShape)
        {
        	// Update the LineStyle for specified variables.
        	// this is most commonly used to update size and shape without touching color.
        	
        	if(_bUpdateColor) m_Color = _style.Color;        	
        	if(_bUpdateSize) m_iSize = _style.Size;        	
        	if(_bUpdateLineShape) m_LineShape = _style.Shape;

        	SetPen();
        }
        public void Update(Color _color)
        {
        	// Update the LineStyle for color only. 
        	m_Color = _color;        	
        	SetPen();
        }
        public Pen GetInternalPen(int _iAlpha, float _fPenWidth)
        {
        	// This is used just before a drawing operation to get the internal state.
        	Pen p = (Pen)m_Pen.Clone();
        	
        	if(_iAlpha < 255 && _iAlpha >= 0)
	        	p.Color = Color.FromArgb(_iAlpha, m_Color);
    
        	p.Width = _fPenWidth;
        	
        	return p;
        }
        public Pen GetInternalPen(int _iAlpha)
        {
        	return GetInternalPen(_iAlpha, (float)m_iSize);
        }
	}
	
	/// <summary>
	/// Enum to restrict the possible styles for lines.
	/// </summary>
	public enum LineShape
    {
        Simple,						// Solid,	No Arrows,				Simple. 	
		EndArrow,					// Solid, 	End Arrow,				Simple.
        DoubleArrow,				// Solid, 	Start and End Arrows,	Simple.
        Dash,						// Dash,	No Arrows,				Simple. 	
        DashDot,					// DashDot, No Arrows, 				Simple.
        Track						// Solid,	No Arrows,				Compound.
    }
}
