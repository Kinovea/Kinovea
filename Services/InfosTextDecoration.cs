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
using System.Windows.Forms;
using System.Xml;

namespace Kinovea.Services
{
	/// <summary>
	/// A class to encapsulate all text decoration informations
	/// and provide serialization routines.
	/// </summary>
	// fixme: maybe we could turn this into a value type and an immutable type.
	public class InfosTextDecoration
	{
		public static readonly int[] AllowedFontSizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36 };
		public static readonly int MinFontSize = AllowedFontSizes[0];
		public static readonly int MaxFontSize = AllowedFontSizes[AllowedFontSizes.Length - 1];
      	
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
			// Update the TextDecoration for font size only.
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
			// The final font size returned here may not be part of the allowed font sizes
			// and may exeed the max allowed font size, because it's just for rendering purposes.
			Font f;
			if(_fStretchFactor == 1.0f)
			{
				f = m_Font;
			}
			else
			{
				float fFontSize = m_Font.Size * _fStretchFactor;
				if(fFontSize < 8) fFontSize = 8;
			
				f = new Font(m_Font.Name, fFontSize, m_Font.Style);
			}
			
			return f;
		}
		public Font GetInternalFont()
		{
			return GetInternalFont(1.0f);
		}
		public int ReverseFontSize(int _wantedHeight, String _text)
        {
        	// Compute the optimal font size from a given background rectangle.
        	// This is used when the user drag the bottom right corner to resize the text.
        	// _wantedHeight is unscaled.
        	Button but = new Button();
            Graphics g = but.CreateGraphics();

            // We must loop through all allowed font size and compute the output rectangle to find the best match.
            // We only compare with wanted height for simplicity.
            int iSmallestDiff = int.MaxValue;
            int iBestCandidate = MinFontSize;
            
            foreach(int size in AllowedFontSizes)
            {
            	Font testFont = new Font(m_Font.Name, size, m_Font.Style);
            	SizeF bgSize = g.MeasureString(_text + " ", testFont);
            	testFont.Dispose();
            	
            	int diff = (int)Math.Abs(_wantedHeight - (int)bgSize.Height);
            	
            	if(diff < iSmallestDiff)
            	{
            		iSmallestDiff = diff;
            		iBestCandidate = size;
            	}	
            }
            
            g.Dispose();
            return iBestCandidate;
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
