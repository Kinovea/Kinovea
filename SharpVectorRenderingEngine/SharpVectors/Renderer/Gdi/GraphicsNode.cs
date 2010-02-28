using System;
using System.Xml;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SharpVectors.Dom.Css;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;

namespace SharpVectors.Renderer.Gdi
{
  public class GraphicsNode : RenderingNode
  {
    #region Constructor
    public GraphicsNode(SvgElement element) : base(element)
    {
    }
    #endregion

    #region Fields
    protected Color _uniqueColor = Color.Empty;
    public Color UniqueColor
    {
      get{return _uniqueColor;}
    }

    protected GraphicsContainerWrapper graphicsContainer;
    public Matrix TransformMatrix = null;
    #endregion
		
    #region Protected Methods
    protected void Clip(GraphicsWrapper gr)
    {
      // todo: should we correct the clipping to adjust to the off-one-pixel drawing?
      gr.TranslateClip(1,1);
			
      #region Clip with clip
      // see http://www.w3.org/TR/SVG/masking.html#OverflowAndClipProperties 
      if(element is ISvgSvgElement ||
        element is ISvgMarkerElement ||
        element is ISvgSymbolElement ||
        element is ISvgPatternElement)
      {
        // check overflow property
        CssValue overflow =  ((SvgElement)element).GetComputedCssValue("overflow", String.Empty) as CssValue;
        // TODO: clip can have "rect(10 10 auto 10)"
        CssPrimitiveValue clip =  ((SvgElement)element).GetComputedCssValue("clip", String.Empty) as CssPrimitiveValue;

        string sOverflow = null;

        if(overflow != null || overflow.CssText == "")
        {
          sOverflow = overflow.CssText;
        } 
        else 
        {
          if (this is ISvgSvgElement)
            sOverflow = "hidden";
        }
					
        if (sOverflow != null) {
          // "If the 'overflow' property has a value other than hidden or scroll, the property has no effect (i.e., a clipping rectangle is not created)."
          if(sOverflow == "hidden" || sOverflow == "scroll")
          {
            RectangleF clipRect = RectangleF.Empty;
            if(clip != null && clip.PrimitiveType == CssPrimitiveType.Rect)
            {
              if(element is ISvgSvgElement)
              {
                ISvgSvgElement svgElement = (ISvgSvgElement)element;
                SvgRect viewPort = svgElement.Viewport as SvgRect;
                clipRect = viewPort.ToRectangleF();
                IRect clipShape = (Rect)clip.GetRectValue();
                if (clipShape.Top.PrimitiveType != CssPrimitiveType.Ident)
                  clipRect.Y += (float)clipShape.Top.GetFloatValue(CssPrimitiveType.Number);
                if (clipShape.Left.PrimitiveType != CssPrimitiveType.Ident)
                  clipRect.X += (float)clipShape.Left.GetFloatValue(CssPrimitiveType.Number);
                if (clipShape.Right.PrimitiveType != CssPrimitiveType.Ident)
                  clipRect.Width = (clipRect.Right-clipRect.X)-(float)clipShape.Right.GetFloatValue(CssPrimitiveType.Number);
                if (clipShape.Bottom.PrimitiveType != CssPrimitiveType.Ident)
                  clipRect.Height = (clipRect.Bottom-clipRect.Y)-(float)clipShape.Bottom.GetFloatValue(CssPrimitiveType.Number);
              }
            }
            else if (clip == null || (clip.PrimitiveType == CssPrimitiveType.Ident && clip.GetStringValue() == "auto"))
            {
              if(element is ISvgSvgElement)
              {
                ISvgSvgElement svgElement = (ISvgSvgElement)element;
                SvgRect viewPort = svgElement.Viewport as SvgRect;
                clipRect = viewPort.ToRectangleF();
              }
              else if(element is ISvgMarkerElement ||
                element is ISvgSymbolElement ||
                element is ISvgPatternElement)
              {
                // TODO: what to do here?
              }
            }
            if(clipRect != RectangleF.Empty)
            {
              gr.SetClip(clipRect);
            }
          }
        }
      }
      #endregion

      #region Clip with clip-path
      // see: http://www.w3.org/TR/SVG/masking.html#EstablishingANewClippingPath
      if ( element is IGraphicsElement ||
        element is IContainerElement)
      {
        CssPrimitiveValue clipPath = ((SvgElement)element).GetComputedCssValue("clip-path", String.Empty) as CssPrimitiveValue;

        if(clipPath != null && clipPath.PrimitiveType == CssPrimitiveType.Uri)
        {
          string absoluteUri = ((SvgElement)element).ResolveUri(clipPath.GetStringValue());
					
          SvgClipPathElement eClipPath = ((SvgDocument)element.OwnerDocument).GetNodeByUri(absoluteUri) as SvgClipPathElement;

          if ( eClipPath != null )
          {
            GraphicsPath gpClip = eClipPath.GetGraphicsPath();

            SvgUnitType pathUnits = (SvgUnitType)eClipPath.ClipPathUnits.AnimVal;

            if ( pathUnits == SvgUnitType.ObjectBoundingBox )
            {
              SvgTransformableElement transElement = element as SvgTransformableElement;

              if(transElement != null)
              {
                ISvgRect bbox = transElement.GetBBox();
						
                // scale clipping path
                Matrix matrix = new Matrix();
                matrix.Scale((float)bbox.Width, (float)bbox.Height);
                gpClip.Transform(matrix);
                gr.SetClip(gpClip);									
									
                // offset clip
                gr.TranslateClip( (float)bbox.X, (float)bbox.Y );
              }
              else
              {
                throw new NotImplementedException("clip-path with SvgUnitType.ObjectBoundingBox "
                  + "not supported for this type of element: " + element.GetType());
              }
            }
            else
            {
              gr.SetClip(gpClip);									
            }
          }
        }
      }
      #endregion
    }

    protected void SetQuality(GraphicsWrapper gr)
    {
      Graphics graphics = gr.Graphics;

      string colorRendering = ((SvgElement)element).GetComputedStringValue("color-rendering", String.Empty);
      switch(colorRendering)
      {
        case "optimizeSpeed":
          graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
          break;
        case "optimizeQuality":
          graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
          break;
        default:
          // "auto"
          // todo: could use AssumeLinear for slightly better
          graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
          break;
      }

      if (element is SvgTextContentElement) 
      {
        // Unfortunately the text rendering hints are not applied because the
        // text path is recorded and painted to the Graphics object as a path
        // not as text.
        string textRendering = ((SvgElement)element).GetComputedStringValue("text-rendering", String.Empty);
        switch(textRendering)
        {
          case "optimizeSpeed":
            graphics.SmoothingMode = SmoothingMode.HighSpeed; 
            //graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            break;
          case "optimizeLegibility":
            graphics.SmoothingMode = SmoothingMode.HighQuality; 
            //graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            break;
          case "geometricPrecision":
            graphics.SmoothingMode = SmoothingMode.AntiAlias; 
            //graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            break;
          default:
            // "auto"
            graphics.SmoothingMode = SmoothingMode.AntiAlias; 
            //graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            break;
        }
      } 
      else 
      {
        string shapeRendering = ((SvgElement)element).GetComputedStringValue("shape-rendering", String.Empty);
        switch(shapeRendering)
        {              
          case "optimizeSpeed":
            graphics.SmoothingMode = SmoothingMode.HighSpeed;
            break;
          case "crispEdges":
            graphics.SmoothingMode = SmoothingMode.None;
            break;
          case "geometricPrecision":
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            break;
          default:
            // "auto"
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            break;
        }
      }            			
    }

    protected void Transform(GraphicsWrapper gr)
    {
      if ( element is ISvgTransformable )
      {
        if ( TransformMatrix == null )
        {
          ISvgTransformable transElm = (ISvgTransformable) element;
          SvgTransformList svgTList = (SvgTransformList)transElm.Transform.AnimVal;
          //SvgTransform svgTransform = (SvgTransform)svgTList.Consolidate();
          SvgMatrix svgMatrix = ((SvgTransformList)transElm.Transform.AnimVal).TotalMatrix;

          TransformMatrix = new Matrix(
            (float) svgMatrix.A,
            (float) svgMatrix.B,
            (float) svgMatrix.C,
            (float) svgMatrix.D,
            (float) svgMatrix.E,
            (float) svgMatrix.F);
        }
        gr.Transform = TransformMatrix;
      }
    }

    protected void fitToViewbox(GraphicsWrapper graphics, RectangleF elmRect)
    {
      if ( element is ISvgFitToViewBox )
      {
        ISvgFitToViewBox fitToVBElm = (ISvgFitToViewBox) element;
        SvgPreserveAspectRatio spar = (SvgPreserveAspectRatio)fitToVBElm.PreserveAspectRatio.AnimVal;

        float[] translateAndScale = spar.FitToViewBox(
          (SvgRect)fitToVBElm.ViewBox.AnimVal,
          new SvgRect(elmRect.X, elmRect.Y, elmRect.Width, elmRect.Height)
          );
        graphics.TranslateTransform(translateAndScale[0], translateAndScale[1]);
        graphics.ScaleTransform(translateAndScale[2], translateAndScale[3]);
      }
    }
    #endregion

    #region Public Methods
    public override void BeforeRender(ISvgRenderer renderer)     {
      if (_uniqueColor.IsEmpty)
        _uniqueColor = ((GdiRenderer)renderer)._getNextColor(this);

      GraphicsWrapper graphics = ((GdiRenderer) renderer).GraphicsWrapper;
			
      graphicsContainer = graphics.BeginContainer();
      SetQuality(graphics);
      Transform(graphics);
      Clip(graphics);
    }

    public override void AfterRender(ISvgRenderer renderer)
    {
      GraphicsWrapper graphics = ((GdiRenderer)renderer).GraphicsWrapper;

      graphics.EndContainer(graphicsContainer);
    }
    #endregion

  }
}
