using System;
using System.Drawing;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Css;

namespace SharpVectors.Renderer.Gdi
{
	public class GdiSvgColor : SvgColor
	{
		private SvgStyleableElement _element;
		private string _propertyName;

		public GdiSvgColor(SvgStyleableElement elm, string propertyName) : base(elm.GetComputedStyle("").GetPropertyValue(propertyName))
		{
			_element = elm;
			_propertyName = propertyName;
		}

		public int getOpacity()
		{
			string propName;
			if(_propertyName.Equals("stop-color"))
			{
				propName = "stop-opacity";
			}
			else if(_propertyName.Equals("flood-color"))
			{
				propName = "flood-opacity";
			}
			else
			{
				return 255;
			}

			double alpha = 255;
			string opacity;

			opacity = _element.GetPropertyValue(propName);
			if(opacity.Length > 0) alpha *= SvgNumber.ParseToFloat(opacity);

			alpha = Math.Min(alpha, 255);
			alpha = Math.Max(alpha, 0);

			return Convert.ToInt32(alpha);
		}

		public Color Color
		{
			get
			{
				SvgColor colorToUse;
				if(ColorType == SvgColorType.CurrentColor)
				{
					string sCurColor = _element.GetComputedStyle("").GetPropertyValue("color");
					colorToUse = new SvgColor(sCurColor);
				}
				else if(ColorType == SvgColorType.Unknown)
				{
					colorToUse = new SvgColor("black");
				}
				else
				{
					colorToUse = this;
				}
				
				int red = Convert.ToInt32(colorToUse.RgbColor.Red.GetFloatValue(CssPrimitiveType.Number));
				int green = Convert.ToInt32(colorToUse.RgbColor.Green.GetFloatValue(CssPrimitiveType.Number));
				int blue = Convert.ToInt32(colorToUse.RgbColor.Blue.GetFloatValue(CssPrimitiveType.Number));
				return Color.FromArgb(getOpacity(), red, green, blue);
			}
		}
	}
}
