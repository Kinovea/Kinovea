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
using System.Xml;

namespace Videa.Services
{
	/// <summary>
	/// A class to encapsulate all text decoration informations
	/// and provide serialization routines.
	/// </summary>
	// fixme: maybe we could turn this into a value type and an immutable type.
	public class InfosTextDecoration
	{
		#region Properties
		public static InfosTextDecoration DefaultValue
		{
			get { return new InfosTextDecoration();}
		} 
		public int FontSize
		{
			get { return (int)m_Font.Size;}
		}
		public Color BackColor
		{
			get { return m_BackColor;}
		}
		#endregion
		
		#region Members
		private Font m_Font;
		private Color m_ForeColor;
		private Color m_BackColor;
		#endregion
		
		#region Construction
		public InfosTextDecoration() 
			: this("Arial", 8, FontStyle.Bold, Color.White, Color.FromArgb(160, Color.Black))
		{
		}
		public InfosTextDecoration(int _fontSize, Color _backColor)
			: this("Arial", _fontSize, FontStyle.Bold, Color.White, Color.FromArgb(160, _backColor))
		{
		}
		public InfosTextDecoration(string _fontName, int _fontSize, FontStyle _fontStyle, Color _foreColor, Color _backColor)
		{
			m_Font = new Font(_fontName, _fontSize, _fontStyle);
			m_ForeColor = _foreColor;
			m_BackColor = _backColor;
			FixForeColor();
		}
		#endregion
		
		public void Update(Color _color)
		{
			// Update the TextDecoration for color only.
			m_BackColor = _color;
			FixForeColor();
		}
		public void Update(int _iFontSize)
		{
			// Update the TextDecoration for color only.
			m_Font = new Font(m_Font.Name, _iFontSize, m_Font.Style);
		}
		
		#region XML conversion
		public void ToXml(XmlTextWriter _xmlWriter)
        {
			_xmlWriter.WriteStartElement("TextDecoration");
			
			_xmlWriter.WriteStartElement("FontName");
            _xmlWriter.WriteString(m_Font.Name);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("FontSize");
            _xmlWriter.WriteString(m_Font.Size.ToString());
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("ForeColor");
            _xmlWriter.WriteString(m_ForeColor.A + ";" + m_ForeColor.R + ";" + m_ForeColor.G + ";" + m_ForeColor.B);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("BackColor");
            _xmlWriter.WriteString(m_BackColor.A + ";" + m_BackColor.R + ";" + m_BackColor.G + ";" + m_BackColor.B);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteEndElement();	
		}
		public static InfosTextDecoration FromXml(XmlReader _xmlReader)
		{
			// When we land in this method we MUST already be at the "TextDecoration" node.
			
			InfosTextDecoration result = new InfosTextDecoration();
			string fontName = "Arial";
			int fontSize = 8;
			
			while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "FontName")
                    {
                        fontName = _xmlReader.ReadString();
                    }
                    else if (_xmlReader.Name == "FontSize")
                    {
                    	fontSize = int.Parse(_xmlReader.ReadString());
                    }
                    else if (_xmlReader.Name == "ForeColor")
                    {
                        result.m_ForeColor = XmlHelper.ColorParse(_xmlReader.ReadString(), ';');
                    }
                    else if (_xmlReader.Name == "BackColor")
                    {
                        result.m_BackColor = XmlHelper.ColorParse(_xmlReader.ReadString(), ';');
                    }
                }
                else if (_xmlReader.Name == "TextDecoration")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
			
			#region old format
			/*private static void ParseBackgroundBrush(XmlTextReader _xmlReader, KeyframeLabel _kfl)
	        {
	            while (_xmlReader.Read())
	            {
	                if (_xmlReader.IsStartElement())
	                {
	                    if (_xmlReader.Name == "Alpha")
	                    {
	                        _kfl.m_iBackgroundAlpha = int.Parse(_xmlReader.ReadString());
	                    }
	                    else if (_xmlReader.Name == "ColorRGB")
	                    {
	                        _kfl.BackgroundColor = XmlHelper.ColorParse(_xmlReader.ReadString(), ';');
	                    }
	                }
	                else if (_xmlReader.Name == "BackgroundBrush")
	                {
	                    break;
	                }
	                else
	                {
	                    // Fermeture d'un tag interne.
	                }
	            }
	
	        }*/
	        /*private static void ParseFont(XmlTextReader _xmlReader, KeyframeLabel _kfl)
	        {
	            while (_xmlReader.Read())
	            {
	                if (_xmlReader.IsStartElement())
	                {
	                    if (_xmlReader.Name == "Size")
	                    {
	                        _kfl.m_iFontSize = int.Parse(_xmlReader.ReadString());
	                    }
	                    else if (_xmlReader.Name == "Name")
	                    {
	                        _kfl.m_FontName = _xmlReader.ReadString();
	                    }
	                    else if (_xmlReader.Name == "ColorRGB")
	                    {
	                        _kfl.m_FontColor = XmlHelper.ColorParse(_xmlReader.ReadString(), ';');
	                    }
	                }
	                else if (_xmlReader.Name == "Font")
	                {
	                    break;
	                }
	                else
	                {
	                    // Fermeture d'un tag interne.
	                }
	            }
	        }*/
			#endregion
			
			result.m_Font = new Font(fontName, fontSize, FontStyle.Bold);
			result.FixForeColor();
			return result;
		}
		#endregion
		
		public Color GetFadingBackColor(double _fFadingFactor)
		{
			return GetFadingColor(m_BackColor, _fFadingFactor);
		}
		public Color GetFadingForeColor(double _fFadingFactor)
		{
			return GetFadingColor(m_ForeColor, _fFadingFactor);
		}
		private Color GetFadingColor(Color _color, double _fFadingFactor)
		{
			return Color.FromArgb((int)((double)_color.A * _fFadingFactor), _color.R, _color.G, _color.B);
		}
		public Font GetInternalFont(float _fStretchFactor)
		{
			// Returns the internal font with a different size.
			// used for labels on chrono for exemple or to get the strecthed font.
			
			float fFontSize = m_Font.Size * _fStretchFactor;
			if(fFontSize < 8) fFontSize = 8;
			
			return new Font(m_Font.Name, fFontSize, m_Font.Style);
		}
		public Font GetInternalFont()
		{
			return GetInternalFont(1f);
		}
		#region Utilities
		private void FixForeColor()
        {
            // If bright background, write in black and vice versa.
            m_ForeColor = m_BackColor.GetBrightness() >= 0.5  ? Color.Black : Color.White;
        }
		public override int GetHashCode()
        {
			int iHash = 0;
            
			iHash ^= m_Font.GetHashCode();
            iHash ^= m_ForeColor.GetHashCode();
			iHash ^= m_BackColor.GetHashCode();
			
            return iHash;
		}
		public InfosTextDecoration Clone()
		{
			return new InfosTextDecoration(m_Font.Name, (int)m_Font.Size, m_Font.Style, m_ForeColor, m_BackColor);
		}
		#endregion
	}
}
