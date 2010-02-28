using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;

namespace SharpVectors.Renderer.Gdi
{
	/// <summary>
	/// Summary description for SvgElementGraphicsNode.
	/// </summary>
	public class SvgElementGraphicsNode : GraphicsNode
	{
        #region Constructor
		public SvgElementGraphicsNode(SvgElement element) : base(element)
		{
		}
        #endregion

        #region Public Methods
		public override void Render(ISvgRenderer renderer)
		{
            GraphicsWrapper graphics = ((GdiRenderer) renderer).GraphicsWrapper;

			SvgSvgElement svgElm = (SvgSvgElement) element;

			float x = (float)svgElm.X.AnimVal.Value;
			float y = (float)svgElm.Y.AnimVal.Value;
			float width = (float)svgElm.Width.AnimVal.Value;
			float height = (float)svgElm.Height.AnimVal.Value;

			RectangleF elmRect = new RectangleF(x, y, width, height);

			if ( element.ParentNode is SvgElement )
			{
                // TODO: should it be moved with x and y?
			}

			fitToViewbox(graphics, elmRect);
		}
        #endregion
	}
}
