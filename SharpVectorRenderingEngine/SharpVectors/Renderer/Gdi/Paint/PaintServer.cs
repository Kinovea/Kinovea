using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Xml;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Css;

namespace SharpVectors.Renderer.Gdi
{
	public abstract class PaintServer
	{
		public static PaintServer CreatePaintServer(SvgDocument document, string absoluteUri)
		{
			XmlNode node = document.GetNodeByUri(absoluteUri);

			if(node is SvgGradientElement)
			{
				return new GradientPaintServer((SvgGradientElement)node);
			}
			else if(node is SvgPatternElement)
			{
				return new PatternPaintServer((SvgPatternElement)node);
			}
			else
			{
				return null;
			}
		}

		public PaintServer(){}

		public abstract Brush GetBrush(RectangleF bounds);
	}
}
