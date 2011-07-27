#region License
/*
Copyright © Joan Charmant 2011.
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
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{	
	/// <summary>
	/// A class to encapsulate the various styling primitive a drawing may need for rendering, 
	/// and provide some utility functions to get a Pen, Brush, Font or Color object according to client opacity or zoom.
	/// Typical drawing would use just two or three of the primitive for its decoration and leave the others undefined.
	/// 
	/// The primitives can be bound to a style element (editable in the UI) through the Bind() method on the 
	/// style element, passing the name of the primitive. The binding will be effective only if types are compatible.
	/// todo: example.
	/// </summary>
	/// <remarks>
	/// This class should merge and replace "LineStyle" and "InfoTextDecoration" classes.
	/// </remarks>
	public class StyleHelper
	{
		#region Exposed function delegates
		public DelegateBindWrite BindWrite;
		public DelegateBindRead BindRead;
		
		/// <summary>
		/// Event raised when the value is changed dynamically through binding.
		/// This may be useful if the Drawing has several StyleHelper that must be linked somehow.
		/// An example use is when we change the main color of the track, we need to propagate the change
		/// to the small label attached (for the Label following mode).
		/// </summary>
		/// <remarks>The event is not raised when the value is changed manually</remarks>
		public event DelegateValueChanged ValueChanged;
		
		#endregion
		
		#region Properties
		public Color Color
		{
			get { return m_Color; }
			set { m_Color = value; }
		}
		public int LineSize
		{
			get { return m_iLineSize; }
			set { m_iLineSize = value;}
		}
		public LineEnding LineEnding
		{
			get { return m_LineEnding; }
			set { m_LineEnding = value;}
		}
		public Font Font
		{
			get { return m_Font; }
			set 
			{ 
				if(value != null)
				{
					// We make temp copies of the variables because we call .Dispose() but 
					// it's possible that input value was pointing to the same reference.
					string fontName = value.Name;
            		FontStyle fontStyle = value.Style;
            		float fontSize = value.Size;
            		m_Font.Dispose();
            		m_Font = new Font(fontName, fontSize, fontStyle);
				}
				else
				{
					m_Font.Dispose();
					m_Font = null;
				}
			}
		}
		public Bicolor Bicolor
		{
			get { return m_Bicolor; }
			set { m_Bicolor = value; }
		}
		public TrackShape TrackShape
		{
			get { return m_TrackShape; }
			set { m_TrackShape = value;}
		}
		#endregion
		
		#region Members
		private Color m_Color;
		private int m_iLineSize;
		private Font m_Font = new Font("Arial", 12, FontStyle.Regular);
		private Bicolor m_Bicolor;
		private LineEnding m_LineEnding = LineEnding.None;
		private TrackShape m_TrackShape = TrackShape.Solid;
		
		
		// Internal only
		private static readonly int[] m_AllowedFontSizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36 };
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public StyleHelper()
		{
			BindWrite = DoBindWrite;
			BindRead = DoBindRead;
		}
		#endregion
		
		#region Public Methods
		
		#region Color and LineSize properties
		/// <summary>
		/// Returns a Pen object suitable to draw a background or color only contour.
		/// The pen object will only integrate the color property and be of width 1.
		/// </summary>
		/// <param name="_iAlpha">Alpha value to multiply the color with</param>
		/// <returns>Pen object initialized with the current value of color and width = 1.0</returns>
		public Pen GetPen(int _iAlpha)
		{
			Color c = (_iAlpha >= 0 && _iAlpha < 255) ? Color.FromArgb(_iAlpha, m_Color) : m_Color;			
			
        	return NormalPen(new Pen(c, 1.0f));
		}

		/// <summary>
		/// Returns a Pen object suitable to draw a line or contour.
		/// The pen object will integrate the color, line size, line shape, and line endings properties.
		/// </summary>
		/// <param name="_iAlpha">Alpha value to multiply the color with</param>
		/// <param name="_fStretchFactor">zoom value to multiply the line size with</param>
		/// <returns>Pen object initialized with the current value of color and line size properties</returns>
		public Pen GetPen(int _iAlpha, double _fStretchFactor)
		{
			Color c = (_iAlpha >= 0 && _iAlpha < 255) ? Color.FromArgb(_iAlpha, m_Color) : m_Color;
			float fPenWidth = (float)((double)m_iLineSize * _fStretchFactor);
			if (fPenWidth < 1) fPenWidth = 1;
			
			Pen p = new Pen(c, fPenWidth);
			p.LineJoin = LineJoin.Round;
			
			// Line endings
			p.StartCap = m_LineEnding.StartCap;
        	p.EndCap = m_LineEnding.EndCap;
        	
			// Line shape
			p.DashStyle = m_TrackShape.DashStyle;
			
			return p;
		}
		
		/// <summary>
		/// Returns a Brush object suitable to draw a background or colored area.
		/// Only use the color property.
		/// </summary>
		/// <param name="_iAlpha">Alpha value to multiply the color with</param>
		/// <returns>Brush object initialized with the current value of color property</returns>
		public SolidBrush GetBrush(int _iAlpha)
		{
			Color c = (_iAlpha >= 0 && _iAlpha < 255) ? Color.FromArgb(_iAlpha, m_Color) : m_Color;
			return new SolidBrush(c);
		}
		#endregion
		
		#region Font property
		public Font GetFont(float _fStretchFactor)
		{
			float fFontSize = GetRescaledFontSize(_fStretchFactor);			
			return new Font(m_Font.Name, fFontSize, m_Font.Style);
		}
		public Font GetFontDefaultSize(int _fontSize)
		{
			return new Font(m_Font.Name, _fontSize, m_Font.Style);
		}
		public void ForceFontSize(int _wantedHeight, String _text)
        {
        	// Compute the optimal font size from a given background rectangle.
        	// This is used when the user drag the bottom right corner to resize the text.
        	// _wantedHeight is unscaled.
        	Button but = new Button();
            Graphics g = but.CreateGraphics();

            // We must loop through all allowed font size and compute the output rectangle to find the best match.
            // We only compare with wanted height for simplicity.
            int iSmallestDiff = int.MaxValue;
            int iBestCandidate = m_AllowedFontSizes[0];
            
            foreach(int size in m_AllowedFontSizes)
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
            
            // Push to internal value.
            string fontName = m_Font.Name;
            FontStyle fontStyle = m_Font.Style;
            m_Font.Dispose();
            m_Font = new Font(fontName, iBestCandidate, fontStyle);
        }
		#endregion
		
		#region Bicolor property
		public Color GetForegroundColor(int _iAlpha)
		{
			Color c = (_iAlpha >= 0 && _iAlpha < 255) ? Color.FromArgb(_iAlpha, m_Bicolor.Foreground) : m_Bicolor.Foreground;
			return c;
		}
		public SolidBrush GetForegroundBrush(int _iAlpha)
		{
			Color c = GetForegroundColor(_iAlpha);
			return new SolidBrush(c);
		}
		public Pen GetForegroundPen(int _iAlpha)
		{
			Color c = GetForegroundColor(_iAlpha);
			return NormalPen(new Pen(c, 1.0f));
		}
		public Color GetBackgroundColor(int _iAlpha)
		{
			Color c = (_iAlpha >= 0 && _iAlpha < 255) ? Color.FromArgb(_iAlpha, m_Bicolor.Background) : m_Bicolor.Background;
			return c;
		}
		public SolidBrush GetBackgroundBrush(int _iAlpha)
		{
			Color c = GetBackgroundColor(_iAlpha);
			return new SolidBrush(c);
		}
		public Pen GetBackgroundPen(int _iAlpha)
		{
			Color c = GetBackgroundColor(_iAlpha);
			return NormalPen(new Pen(c, 1.0f));
		}
		#endregion
		
		public override int GetHashCode()
		{
			int iHash = 0;
            
			iHash ^= m_Color.GetHashCode();
            iHash ^= m_iLineSize.GetHashCode();
			iHash ^= m_Font.GetHashCode();
            iHash ^= m_Bicolor.GetHashCode();
			iHash ^= m_LineEnding.GetHashCode();
			iHash ^= m_TrackShape.GetHashCode();
			
			return iHash;
		}
		
		#endregion
		
		#region Private Methods
		private void DoBindWrite(string _targetProperty, object _value)
		{
			// Check type and import value if compatible with the target prop.
			bool imported = false;
			switch(_targetProperty)
			{
				case "Color":
					{
						if(_value is Color)
						{
							m_Color = (Color)_value;
							imported = true;
						}
						break;	
					}
				case "LineSize":
					{
						if(_value is int)
						{
							m_iLineSize = (int)_value;
							imported = true;
						}
						
						break;
					}
				case "LineEnding":
					{
						if(_value is LineEnding)
						{
							m_LineEnding = (LineEnding)_value;
							imported = true;
						}
						
						break;
					}
				case "TrackShape":
					{
						if(_value is TrackShape)
						{
							m_TrackShape = (TrackShape)_value;
							imported = true;
						}
						
						break;
					}
				case "Font":
					{
						if(_value is int)
						{
							// Recreate the font changing just the size.
							string fontName = m_Font.Name;
							FontStyle fontStyle = m_Font.Style;
	            			m_Font.Dispose();
	            			m_Font = new Font(fontName, (int)_value, fontStyle);
	            			imported = true;
						}
						break;
					}
				case "Bicolor":
					{
						if(_value is Color)
						{
							m_Bicolor.Background = (Color)_value;
							imported = true;
						}
						break;	
					}
				default:
					{
						log.DebugFormat("Unknown target property \"{0}\"." , _targetProperty);
						break;
					}
			}
			
			if(imported)
			{
				if(ValueChanged != null) ValueChanged();
			}
			else
			{
				log.DebugFormat("Could not import value \"{0}\" to property \"{1}\"." , _value.ToString(), _targetProperty);
			}
			
		}
		private void DoBindRead(string _sourceProperty, ref object _targetValue)
		{
			// Take the local property and extract something of the required type (the type of _targetValue).
			// This function is used by style element to stay up to date in case the bound property has been modified externally.
			// The style element might be of an entirely different type than the property.
			bool converted = false;
			switch(_sourceProperty)
			{
				case "Color":
					{
						if(_targetValue is Color)
						{
							_targetValue = m_Color;
							converted = true;
						}
						break;	
					}
				case "LineSize":
					{
						if(_targetValue is int)
						{
							_targetValue = m_iLineSize;
							converted = true;
						}
						break;
					}
				case "LineEnding":
					{
						if(_targetValue is LineEnding)
						{
							_targetValue = m_LineEnding;
							converted = true;
						}
						break;
					}
				case "TrackShape":
					{
						if(_targetValue is TrackShape)
						{
							_targetValue = m_TrackShape;
							converted = true;
						}
						break;
					}
				case "Font":
					{
						if(_targetValue is int)
						{
							_targetValue = (int)m_Font.Size;
							converted = true;
						}
						break;
					}
				case "Bicolor":
					{
						if(_targetValue is Color)
						{
							_targetValue = m_Bicolor.Background;
							converted = true;
						}
						break;	
					}
				default:
					{
						log.DebugFormat("Unknown source property \"{0}\"." , _sourceProperty);
						break;
					}	
			}
			
			if(!converted)
			{
				log.DebugFormat("Could not convert property \"{0}\" to update value \"{1}\"." , _sourceProperty, _targetValue);
			}
		}
		private float GetRescaledFontSize(float _fStretchFactor)
		{
			// Get the strecthed font size.
			// The final font size returned here may not be part of the allowed font sizes
			// and may exeed the max allowed font size, because it's just for rendering purposes.
			float fFontSize = (float)(m_Font.Size * _fStretchFactor);
			if(fFontSize < 8) fFontSize = 8;
			return fFontSize;
		}
		private Pen NormalPen(Pen _p)
		{
			_p.StartCap = LineCap.Round;
        	_p.EndCap = LineCap.Round;
        	_p.LineJoin = LineJoin.Round;
        	return _p;
		}
		#endregion
	}

	/// <summary>
	/// A simple wrapper around two color values.
	/// When setting the background color, the foreground color is automatically adjusted 
	/// to black or white depending on the luminosity of the background color.
	/// </summary>
	public struct Bicolor
	{
		public Color Foreground
		{
			get { return m_Foreground;}
		}
		public Color Background
		{
			get { return m_Background;}
			set 
			{ 
				m_Background = value;
				m_Foreground = value.GetBrightness() >= 0.5  ? Color.Black : Color.White;
			}
		}
		
		private Color m_Foreground;
		private Color m_Background;
		
		public Bicolor(Color _backColor)
		{
			m_Background = _backColor;
			m_Foreground = _backColor.GetBrightness() >= 0.5  ? Color.Black : Color.White;
		}
	}
	
	/// <summary>
	/// A simple wrapper around two LineCap values.
	/// Used to describe arrow endings and possibly other endings.
	/// </summary>
	[TypeConverter(typeof(LineEndingConverter))]
	public struct LineEnding
	{
		public readonly LineCap StartCap;
		public readonly LineCap EndCap;
		
		public LineEnding(LineCap _start, LineCap _end)
		{
			StartCap = _start;
			EndCap = _end;
		}
		
		#region Predefined static values
		public static LineEnding None
		{
			get { return new LineEnding(LineCap.Round, LineCap.Round);}
		}
		public static LineEnding StartArrow
		{
			get { return new LineEnding(LineCap.ArrowAnchor, LineCap.Round);}
		}
		public static LineEnding EndArrow
		{
			get { return new LineEnding(LineCap.Round, LineCap.ArrowAnchor);}
		}
		public static LineEnding DoubleArrow
		{
			get { return new LineEnding(LineCap.ArrowAnchor, LineCap.ArrowAnchor);}
		}
		#endregion
	}
	
	public class LineEndingConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}	
			else
			{
				return base.CanConvertFrom(context, sourceType);
			}
		}
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if(destinationType == typeof(string))
			{
				return true;
			}
			else
			{
				return base.CanConvertTo(context, destinationType);
			}
		}	
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if(value is string)
			{
				string stringValue = value as string;
				
				if (stringValue.Length == 0)
					return LineEnding.None;
				
				string[] split = stringValue.Split(new Char[] { ';' });
				
				if(split.Length != 2)
					return LineEnding.None;
				
				TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(LineCap));
				LineCap start = (LineCap)enumConverter.ConvertFromString(context, culture, split[0]);
				LineCap end = (LineCap)enumConverter.ConvertFromString(context, culture, split[1]);
				
				return new LineEnding(start, end);
			}
			else
			{
				return base.ConvertFrom(context, culture, value);
			}
		}
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				LineEnding lineEnding = (LineEnding)value;
				TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(LineCap));
				string result = String.Format("{0};{1}", 
				                              enumConverter.ConvertToString(context, culture, (LineCap)lineEnding.StartCap), 
				                              enumConverter.ConvertToString(context, culture, (LineCap)lineEnding.EndCap));
				return result;
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
	
	/// <summary>
	/// A simple wrapper around a dash style and the presence of time ticks.
	/// Used to describe line shape for tracks.
	/// </summary>
	public struct TrackShape
	{
		public readonly DashStyle DashStyle;
		public readonly bool ShowSteps;
		
		public TrackShape(DashStyle _style, bool _steps)
		{
			DashStyle = _style;
			ShowSteps = _steps;
		}
		
		#region Predefined static values
		public static TrackShape Solid
		{
			get { return new TrackShape(DashStyle.Solid, false);}
		}
		public static TrackShape Dash
		{
			get { return new TrackShape(DashStyle.Dash, false);}
		}
		public static TrackShape SolidSteps
		{
			get { return new TrackShape(DashStyle.Solid, true);}
		}
		public static TrackShape DashSteps
		{
			get { return new TrackShape(DashStyle.Dash, true);}
		}
		#endregion
	}
		
}
