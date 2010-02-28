using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using SharpVectors.Net;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;

namespace SharpVectors.Renderer.Gdi
{
	/// <summary>
	/// Summary description for SvgImageGraphicsNode.
	/// </summary>
	public class SvgImageElementGraphicsNode : GraphicsNode
	{
        #region Constructor
		public SvgImageElementGraphicsNode(SvgElement element) : base(element)
		{
		}
        #endregion

		private GdiRenderer gdiRenderer = new GdiRenderer();

		private SvgWindow getSvgWindow()
		{
			SvgImageElement iElm = Element as SvgImageElement;
			SvgWindow wnd = iElm.SvgWindow;
			wnd.Renderer = gdiRenderer;
			gdiRenderer.Window = wnd;
			return wnd;
		}

        #region Public Methods
		public override void Render(ISvgRenderer renderer)
		{
            GraphicsWrapper graphics = ((GdiRenderer) renderer).GraphicsWrapper;
			SvgImageElement iElement = (SvgImageElement) element;
			//HttpResource resource = iElement.ReferencedResource;

			/*if (resource != null )
			{*/
				ImageAttributes imageAttributes = new ImageAttributes();

				string sOpacity = iElement.GetPropertyValue("opacity");
				if ( sOpacity.Length > 0 )
				{
					double opacity = SvgNumber.ParseToFloat(sOpacity);
					ColorMatrix myColorMatrix = new ColorMatrix();
					myColorMatrix.Matrix00 = 1.00f; // Red
					myColorMatrix.Matrix11 = 1.00f; // Green
					myColorMatrix.Matrix22 = 1.00f; // Blue
					myColorMatrix.Matrix33 = (float)opacity; // alpha
					myColorMatrix.Matrix44 = 1.00f; // w

					imageAttributes.SetColorMatrix(myColorMatrix,ColorMatrixFlag.Default,ColorAdjustType.Bitmap);
				}

                float width = (float)iElement.Width.AnimVal.Value;
				float height = (float)iElement.Height.AnimVal.Value;

				Rectangle destRect = new Rectangle();
				destRect.X = Convert.ToInt32(iElement.X.AnimVal.Value);
				destRect.Y = Convert.ToInt32(iElement.Y.AnimVal.Value);
				destRect.Width = Convert.ToInt32(width);
				destRect.Height = Convert.ToInt32(height);

				Image image;
				if ( iElement.IsSvgImage )
				{
					SvgWindow wnd = getSvgWindow();
                    gdiRenderer.BackColor = Color.Empty;
					gdiRenderer.Render(wnd.Document as SvgDocument);

                    //wnd.Render();
					image = gdiRenderer.RasterImage;
					image.Save(@"c:\inlinesvg.png", ImageFormat.Png);
				}
				else
				{
					image = iElement.Bitmap;
				}

				if(image != null)
				{
					graphics.DrawImage(this, image, destRect, 0f,0f,image.Width,image.Height, GraphicsUnit.Pixel, imageAttributes);
				}
			//}
		}
        #endregion
	}
}
