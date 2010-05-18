using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Diagnostics;

using SharpVectors.Dom.Css;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;


namespace SharpVectors.Renderer.Gdi
{
	public class GDIPathGraphicsNode : GraphicsNode
	{
        #region Constructor
		public GDIPathGraphicsNode(SvgElement element) : base(element)
		{
		}
        #endregion
		
		#region Marker code
		private string extractMarkerUrl(string propValue)
		{
			Regex reUrl = new Regex(@"^url\((?<uri>.+)\)$");

			Match match = reUrl.Match(propValue);
			if(match.Success)
			{
				return match.Groups["uri"].Value;
			}
			else
			{
				return String.Empty;
			}
		}

		private void PaintMarkers(GdiRenderer renderer, SvgStyleableElement styleElm, GraphicsWrapper gr)
		{		
      // OPTIMIZE

			if ( styleElm is ISharpMarkerHost )
			{
				string markerStartUrl = extractMarkerUrl(styleElm.GetPropertyValue("marker-start", "marker"));
				string markerMiddleUrl = extractMarkerUrl(styleElm.GetPropertyValue("marker-mid", "marker"));
				string markerEndUrl = extractMarkerUrl(styleElm.GetPropertyValue("marker-end", "marker"));

				RenderingNode grNode;
				if ( markerStartUrl.Length > 0 )
				{
					grNode = renderer.GetGraphicsNodeByUri(styleElm.BaseURI, markerStartUrl);
					if (grNode is SvgMarkerGraphicsNode)
					{
						((SvgMarkerGraphicsNode) grNode).PaintMarker(renderer, gr, SvgMarkerPosition.Start, styleElm);
					}
				}

				if ( markerMiddleUrl.Length > 0 )
				{
					// TODO markerMiddleUrl != markerStartUrl
					grNode = renderer.GetGraphicsNodeByUri(styleElm.BaseURI, markerMiddleUrl);
					if ( grNode is SvgMarkerGraphicsNode )
					{
						((SvgMarkerGraphicsNode) grNode).PaintMarker(renderer, gr, SvgMarkerPosition.Mid, styleElm);
					}
				}

				if ( markerEndUrl.Length > 0 )
				{
					// TODO: markerEndUrl != markerMiddleUrl
					grNode = renderer.GetGraphicsNodeByUri(styleElm.BaseURI, markerEndUrl);

					if(grNode is SvgMarkerGraphicsNode)
					{
						((SvgMarkerGraphicsNode) grNode).PaintMarker(renderer, gr, SvgMarkerPosition.End, styleElm);
					}
				}
			}
		}
		#endregion

		#region Private methods
		private Brush GetBrush(GraphicsPath gp)
		{
			GdiSvgPaint paint = new GdiSvgPaint(element as SvgStyleableElement, "fill");
			return paint.GetBrush(gp);
		}

		private Pen GetPen(GraphicsPath gp)
		{
			GdiSvgPaint paint = new GdiSvgPaint(element as SvgStyleableElement, "stroke");
			return paint.GetPen(gp);
		}
		#endregion

		#region Public methods

        public override void BeforeRender(ISvgRenderer renderer)
        {
            if (_uniqueColor.IsEmpty)
        		  _uniqueColor = ((GdiRenderer)renderer)._getNextColor(this);

            GraphicsWrapper graphics = ((GdiRenderer) renderer).GraphicsWrapper;

            graphicsContainer = graphics.BeginContainer();
            SetQuality(graphics);
            Transform(graphics);
        }

        public override void Render(ISvgRenderer renderer)
        {
			GdiRenderer gdiRenderer = renderer as GdiRenderer;
            GraphicsWrapper graphics = gdiRenderer.GraphicsWrapper;

			if ( !(element is SvgClipPathElement) && !(element.ParentNode is SvgClipPathElement) )
			{
				SvgStyleableElement styleElm = element as SvgStyleableElement;
				if ( styleElm != null )
				{
					string sVisibility = styleElm.GetPropertyValue("visibility");
					string sDisplay = styleElm.GetPropertyValue("display");

					if (element is ISharpGDIPath && sVisibility != "hidden" && sDisplay != "none")
					{
						GraphicsPath gp = ((ISharpGDIPath)element).GetGraphicsPath();

						if ( gp != null )
						{
							Clip(graphics);

							GdiSvgPaint fillPaint = new GdiSvgPaint(styleElm, "fill");
							Brush brush = fillPaint.GetBrush(gp);

							GdiSvgPaint strokePaint = new GdiSvgPaint(styleElm, "stroke");
							Pen pen = strokePaint.GetPen(gp);

							if ( brush != null ) 
							{
								if ( brush is PathGradientBrush ) 
								{
									GradientPaintServer gps = fillPaint.PaintServer as GradientPaintServer;
									//GraphicsContainer container = graphics.BeginContainer();
									
									graphics.SetClip(gps.GetRadialGradientRegion(gp.GetBounds()), CombineMode.Exclude);

									SolidBrush tempBrush = new SolidBrush(((PathGradientBrush)brush).InterpolationColors.Colors[0]);
									graphics.FillPath(this, tempBrush,gp);
									tempBrush.Dispose();
									graphics.ResetClip();

									//graphics.EndContainer(container);
								}
 
								graphics.FillPath(this, brush, gp);
								brush.Dispose();
							}

							if ( pen != null ) 
							{
								if ( pen.Brush is PathGradientBrush ) 
								{
									GradientPaintServer gps = strokePaint.PaintServer as GradientPaintServer;
									GraphicsContainerWrapper container = graphics.BeginContainer();

									graphics.SetClip(gps.GetRadialGradientRegion(gp.GetBounds()), CombineMode.Exclude);

									SolidBrush tempBrush = new SolidBrush(((PathGradientBrush)pen.Brush).InterpolationColors.Colors[0]);
									Pen tempPen = new Pen(tempBrush, pen.Width);
									graphics.DrawPath(this, tempPen,gp);
									tempPen.Dispose();
									tempBrush.Dispose();

									graphics.EndContainer(container);
								}
 
								graphics.DrawPath(this, pen, gp);
								pen.Dispose();
							}
						}
					}
					PaintMarkers(gdiRenderer, styleElm, graphics);
				}
			}
		}
		#endregion

	}
}
