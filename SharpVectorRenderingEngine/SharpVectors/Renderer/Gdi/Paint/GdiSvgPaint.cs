using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Css;

namespace SharpVectors.Renderer.Gdi
{
	public class GdiSvgPaint : SvgPaint
	{
		private SvgStyleableElement _element;
		private PaintServer ps;

		public GdiSvgPaint(SvgStyleableElement elm, string propName) : base(elm.GetComputedStyle("").GetPropertyValue(propName))
		{
			_element = elm;
		}

		#region Private methods
		private int getOpacity(string fillOrStroke)
		{
			double alpha = 255;
			string opacity;

			opacity = _element.GetPropertyValue(fillOrStroke + "-opacity");
			if(opacity.Length > 0) alpha *= SvgNumber.ParseToFloat(opacity);

			opacity = _element.GetPropertyValue("opacity");
			if(opacity.Length > 0) alpha *= SvgNumber.ParseToFloat(opacity);

			alpha = Math.Min(alpha, 255);
			alpha = Math.Max(alpha, 0);

            return Convert.ToInt32(alpha);
		}

		private LineCap getLineCap()
		{
			switch(_element.GetPropertyValue("stroke-linecap"))
			{
				case "round":
					return LineCap.Round;
				case "square":
					return LineCap.Square;
				default:
					return LineCap.Flat;
			}
		}

		private LineJoin getLineJoin()
		{
			switch(_element.GetPropertyValue("stroke-linejoin"))
			{
                case "round":
					return LineJoin.Round;
				case "bevel":
					return LineJoin.Bevel;
				default:
					return LineJoin.Miter;
			}
		}

		private float getStrokeWidth()
		{
			string strokeWidth = _element.GetPropertyValue("stroke-width");
			if(strokeWidth.Length == 0) strokeWidth = "1px";

			SvgLength strokeWidthLength = new SvgLength(_element, "stroke-width", SvgLengthDirection.Viewport, strokeWidth);
			return (float)strokeWidthLength.Value;
		}

		private float getMiterLimit()
		{
			string miterLimitStr = _element.GetPropertyValue("stroke-miterlimit");
			if(miterLimitStr.Length == 0) miterLimitStr = "4";
			
			float miterLimit = SvgNumber.ParseToFloat(miterLimitStr);
			if(miterLimit<1) throw new SvgException(SvgExceptionType.SvgInvalidValueErr, "stroke-miterlimit can not be less then 1");
			
			return miterLimit;
		}

		private float[] getDashArray(float strokeWidth)
		{
			string dashArray = _element.GetPropertyValue("stroke-dasharray");

			if(dashArray.Length == 0 || dashArray == "none")
			{
				return null;
			}
			else
			{
				SvgNumberList list = new SvgNumberList(dashArray);
				
				uint len = list.NumberOfItems;
				float[] fDashArray = new float[len];

				for(uint i = 0; i<len; i++)
				{
					//divide by strokeWidth to take care of the difference between Svg and GDI+
					fDashArray[i] = list.GetItem(i).Value / strokeWidth;
				}

				if(len % 2 == 1)
				{
					//odd number of values, duplicate
					float[] tmpArray = new float[len*2];
					fDashArray.CopyTo(tmpArray, 0);
					fDashArray.CopyTo(tmpArray, (int)len);
					
					fDashArray = tmpArray;
				}

				return fDashArray;
			}
		}

		private float getDashOffset(float strokeWidth)
		{
			string dashOffset = _element.GetPropertyValue("stroke-dashoffset");
			if(dashOffset.Length > 0)
			{
				//divide by strokeWidth to take care of the difference between Svg and GDI+
				SvgLength dashOffsetLength = new SvgLength(_element, "stroke-dashoffset", SvgLengthDirection.Viewport, dashOffset);
				return (float)dashOffsetLength.Value;
			}
			else
			{
                return 0;
			}
		}
		private PaintServer getPaintServer(string uri)
		{
			string absoluteUri = _element.ResolveUri(uri);
			return PaintServer.CreatePaintServer(_element.OwnerDocument, absoluteUri);
		}
		#endregion

		#region Public methods
		public Brush GetBrush(GraphicsPath gp)
		{
			return GetBrush(gp, "fill");

		}

		private Brush GetBrush(GraphicsPath gp, string propPrefix)
		{
			SvgPaint fill;
			if(PaintType == SvgPaintType.None)
			{
				return null;
			}
			else if(PaintType == SvgPaintType.CurrentColor)
			{
                fill = new GdiSvgPaint(_element, "color");
			}
			else
			{
				fill = this;
			}

			if(fill.PaintType == SvgPaintType.Uri || 
				fill.PaintType == SvgPaintType.UriCurrentColor ||
				fill.PaintType == SvgPaintType.UriNone ||
				fill.PaintType == SvgPaintType.UriRgbColor ||
				fill.PaintType == SvgPaintType.UriRgbColorIccColor)
			{
				ps = getPaintServer(fill.Uri);
				if(ps != null)
				{
					Brush br = ps.GetBrush(gp.GetBounds());
          if (br is LinearGradientBrush) 
          {
            LinearGradientBrush lgb = (LinearGradientBrush)br;
            int opacityl = getOpacity(propPrefix);
            for (int i = 0; i < lgb.InterpolationColors.Colors.Length; i++) 
            {
              lgb.InterpolationColors.Colors[i] = Color.FromArgb(opacityl, lgb.InterpolationColors.Colors[i]);
            }
            for (int i = 0; i < lgb.LinearColors.Length; i++) 
            {
              lgb.LinearColors[i] = Color.FromArgb(opacityl, lgb.LinearColors[i]);
            }
          } else if (br is PathGradientBrush) 
          {
            PathGradientBrush pgb = (PathGradientBrush)br;
            int opacityl = getOpacity(propPrefix);
            for (int i = 0; i < pgb.InterpolationColors.Colors.Length; i++) 
            {
              pgb.InterpolationColors.Colors[i] = Color.FromArgb(opacityl, pgb.InterpolationColors.Colors[i]);
            }
            for (int i = 0; i < pgb.SurroundColors.Length; i++) 
            {
              pgb.SurroundColors[i] = Color.FromArgb(opacityl, pgb.SurroundColors[i]);
            }
          }
          return br;
				}
				else
				{
					if(PaintType == SvgPaintType.UriNone ||
						PaintType == SvgPaintType.Uri)
					{
						return null;
					}
					else if(PaintType == SvgPaintType.UriCurrentColor)
					{
						fill = new GdiSvgPaint(_element, "color");
					}
					else
					{
						fill = this;
					}
				}
			}

			SolidBrush brush = new SolidBrush( ((RgbColor)fill.RgbColor).GdiColor );
			int opacity = getOpacity(propPrefix);
			brush.Color = Color.FromArgb(opacity, brush.Color);
			return brush;
		}

		public Pen GetPen(GraphicsPath gp)
		{
			float strokeWidth = getStrokeWidth();
			if(strokeWidth == 0) return null;
			
			GdiSvgPaint stroke;
			if(PaintType == SvgPaintType.None)
			{
				return null;
			}
			else if(PaintType == SvgPaintType.CurrentColor)
			{
				stroke = new GdiSvgPaint(_element, "color");
			}
			else
			{
				stroke = this;
			}

			Pen pen = new Pen(stroke.GetBrush(gp, "stroke"), strokeWidth);
			
			pen.StartCap = pen.EndCap = getLineCap();
			pen.LineJoin = getLineJoin();
			pen.MiterLimit = getMiterLimit();
				
			float[] fDashArray = getDashArray(strokeWidth);
			if(fDashArray != null)
			{
				// Do not draw if dash array had a zero value in it

				for(int i=0;i<fDashArray.Length;i++) 
				{
					if(fDashArray[i] == 0)
						return null;
				}

				pen.DashPattern = fDashArray;
			}

			pen.DashOffset = getDashOffset(strokeWidth);

			return pen;
		}

		#endregion

		#region Public properties
		public PaintServer PaintServer
		{
			get
			{
				return ps;
			}
		}

		#endregion
	}
}
