using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using SharpVectors.Collections;
using SharpVectors.Dom.Events;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;

namespace SharpVectors.Renderer.Gdi
{
    /// <summary>
    /// Renders a Svg image to GDI+
    /// </summary>
    public class GdiRenderer : ISvgRenderer, IDisposable
    {
        #region Private Fields

        /// <summary>
        /// A counter that tracks the next hit color.
        /// </summary>
        private int counter = 0;

        /// <summary>
        /// Maps a 'hit color' to a graphics node.
        /// </summary>
        /// <remarks>
        /// The 'hit color' is an integer identifier that identifies the
        /// graphics node that drew it.  When 'hit colors' are drawn onto
        /// a bitmap (ie. <see cref="idMapRaster">idMapRaster</see> the 'hit color'
        /// of each pixel with the help of <see cref="graphicsNodes"
        /// >graphicsNodes</see> can identify for a given x, y coordinate the
        /// relevant graphics node a mouse event should be dispatched to.
        /// </remarks>
        private Hashtable graphicsNodes = new Hashtable();

        /// <summary>
        /// The bitmap containing the rendered Svg image.
        /// </summary>
        private Bitmap rasterImage;

        /// <summary>
        /// A secondary back-buffer used for invalidation repaints. The invalidRect will
        /// be bitblt to the rasterImage front buffer
        /// </summary>
        private Bitmap invalidatedRasterImage;

        /// <summary>
        /// A bitmap image that consists of 'hit color' instead of visual
        /// color.  A 'hit color' is an integer identifier that identifies
        /// the graphics node that drew it.  A 'hit color' can therefore
        /// identify the graphics node that corresponds an x-y coordinates.
        /// </summary>
        private Bitmap idMapRaster;

        /// <summary>
        /// Maintains a mapping from namespace:tagname pair to a type.  This
        /// object is used facilitate instantiate objects of the type
        /// corresponding a given namespace:tagname pair.
        /// </summary>
        private TypeDictionary nodeByTagName;

        /// <summary>
        /// The renderer's <see cref="GraphicsWrapper">GraphicsWrapper</see>
        /// object.
        /// </summary>
        private GraphicsWrapper graphics;

        /// <summary>
        /// The renderer's back color.
        /// </summary>
        private Color backColor;

        /// <summary>
        /// The renderer's <see cref="SvgWindow">SvgWindow</see> object.
        /// </summary>
        private ISvgWindow window;

        /// <summary>
        /// 
        /// </summary>
        private IEventTarget currentTarget = null;
        private IEventTarget currentDownTarget = null;
        private float currentDownX = 0;
        private float currentDownY = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the GdiRenderer class.
        /// </summary>
        public GdiRenderer()
        {
            nodeByTagName = new TypeDictionary();
            string ns = SvgDocument.SvgNamespace;

            SetTagNameNodeType(ns, "svg", typeof(SvgElementGraphicsNode));
            SetTagNameNodeType(ns, "image", typeof(SvgImageElementGraphicsNode));
            SetTagNameNodeType(ns, "marker", typeof(SvgMarkerGraphicsNode));

            backColor = Color.White;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a bitmap image of the a rendered Svg document.
        /// </summary>
        public Bitmap RasterImage
        {
            get
            {
                return rasterImage;
            }
        }

        /// <summary>
        /// Gets the image map of the rendered Svg document. This
        /// is a picture of how the renderer will map the (x,y) positions
        /// of mouse events to objects. You can display this raster
        /// to help in debugging of hit testing.
        /// </summary>
        public Bitmap IdMapRaster
        {
            get
            {
                return idMapRaster;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Window">Window</see> of the
        /// renderer.
        /// </summary>
        /// <value>
        /// The <see cref="Window">Window</see> of the renderer.
        /// </value>
        public ISvgWindow Window
        {
            get
            {
                return window;
            }
            set
            {
                window = value;
            }
        }

        /// <summary>
        /// Gets or sets the back color of the renderer.
        /// </summary>
        /// <value>
        /// The back color of the renderer.
        /// </value>
        public Color BackColor
        {
            get
            {
                return backColor;
            }
            set
            {
                backColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="GraphicsWrapper">GraphicsWrapper</see>
        /// object associated with this renderer.
        /// </summary>
        /// <value>
        /// The <see cref="GraphicsWrapper">GraphicsWrapper</see> object
        /// associated with this renderer.
        /// </value>
        public GraphicsWrapper GraphicsWrapper
        {
            get
            {
                return graphics;
            }
            set
            {
                graphics = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Graphics">Graphics</see> object
        /// associated with this renderer.
        /// </summary>
        /// <value>
        /// The <see cref="Graphics">Graphics</see> object associated
        /// with this renderer.
        /// </value>
        public Graphics Graphics
        {
            get
            {
                return graphics.Graphics;
            }
            set
            {
                graphics.Graphics = value;
            }
        }

        #endregion

        #region Public methods

        
        public bool HitTest(int x, int y)
        {
        	bool hit = false;
        	
        	Color pixel = idMapRaster.GetPixel(x, y);
            GraphicsNode grNode = _getGraphicsNodeFromColor(pixel);
            if (grNode != null)
            {
            	hit = true;
            }
            
            return hit;
        }
        
        
        /// <summary>
        /// Creates a new <see cref="ISvgRenderer">ISvgRenderer</see> object.
        /// TODO: I don't know what this method is for apart from implementing
        /// an interface method that isn't used at all.
        /// </summary>
        /// <returns>
        /// A new <see cref="ISvgRenderer">ISvgRenderer</see> object.
        /// </returns>
        public ISvgRenderer GetRenderer()
        {
            return new GdiRenderer();
        }

        /// <summary>
        /// Adds an entry into the nodeByTagName mapping.
        /// </summary>
        /// <param name="namespaceURI">
        /// The namespaceURI of the mapping key.
        /// </param>
        /// <param name="localName">
        /// The localName of the mapping key.
        /// </param>
        /// <param name="type">
        /// The Xml element type corresponding to the mapping key.
        /// </param>
        public void SetTagNameNodeType(
            string namespaceURI,
            string localName,
            Type type)
        {
            nodeByTagName[namespaceURI + ":" + localName] = type;
        }

        /// <summary>
        /// Generates a new <see cref="RenderingNode">RenderingNode</see> that
        /// corresponds to the given <see cref="XmlElement">XmlElement</see>.
        /// </summary>
        /// <param name="node">
        /// The <see cref="XmlElement">XmlElement</see> node for which to generate
        /// a new <see cref="RenderingNode">RenderingNode</see> object.
        /// </param>
        /// <returns>
        /// The generated <see cref="RenderingNode">RenderingNode</see> that
        /// corresponds to the given <see cref="XmlElement">XmlElement</see>.
        /// </returns>
        public RenderingNode GetRenderingNode(
            ISvgElement node)
        {
            SvgElement svgNode = (SvgElement)node;
            string name = svgNode.NamespaceURI + ":" + svgNode.LocalName;
            RenderingNode result;

            if (nodeByTagName.ContainsKey(name))
            {
                object[] args = new object[] { svgNode };
                result = (RenderingNode)nodeByTagName.CreateInstance(name, args);
            }
            else if (node is ISharpGDIPath)
            {
                result = new GDIPathGraphicsNode(svgNode);
            }
            else
            {
                result = new GraphicsNode(svgNode);
            }

            return result;
        }

        /// <summary>
        /// Generates a new <see cref="RenderingNode">RenderingNode</see> that
        /// corresponds to the given Uri.
        /// </summary>
        /// <param name="baseUri">
        /// The base Uri.
        /// </param>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The generated <see cref="RenderingNode">RenderingNode</see> that
        /// corresponds to the given Uri.
        /// </returns>
        public RenderingNode GetGraphicsNodeByUri(
            string baseUri,
            string url)
        {
            if (url.StartsWith("#"))
            {
                // do nothing
            }
            else if (baseUri != "")
            {
                Uri absoluteUri = new Uri(new Uri(baseUri), url);
                url = absoluteUri.AbsoluteUri;
            }
            else
            {
                // TODO: Handle xml:base here?        
                // Right now just skip this... it can't be resolved, must assume it is absolute
            }
            ISvgElement elm = ((SvgDocument)Window.Document).GetNodeByUri(url) as ISvgElement;

            if (elm != null)
            {
                return GetRenderingNode(elm);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// BeforeRender - Make sure we have a Graphics object to render to.
        /// If we don't have one, then create one to matche the SvgWindow's
        /// physical dimensions.
        /// </summary>
        internal void RendererBeforeRender()
        {
            // Testing for null here allows "advanced" developers to create their own Graphics object for rendering
            if (graphics == null)
            {
                // Get the current SVGWindow's width and height
                int innerWidth = (int)window.InnerWidth;
                int innerHeight = (int)window.InnerHeight;

                // Make sure we have an actual area to render to
                if (innerWidth > 0 && innerHeight > 0)
                {
                    // See if we already have a rasterImage that matches the current SVGWindow dimensions
                    if (rasterImage == null || rasterImage.Width != innerWidth || rasterImage.Height != innerHeight)
                    {
                        // Nope, so create one
                        if (rasterImage != null)
                        {
                            rasterImage.Dispose();
                            rasterImage = null;
                        }
                        rasterImage = new Bitmap(innerWidth, innerHeight);
                    }

                    // Maybe we are only repainting an invalidated section
                    if (invalidRect != RectangleF.Empty)
                    {
                        // TODO: Worry about pan...
                        if (invalidRect.X < 0)
                            invalidRect.X = 0;
                        if (invalidRect.Y < 0)
                            invalidRect.Y = 0;
                        if (invalidRect.Right > innerWidth)
                            invalidRect.Width = innerWidth - invalidRect.X;
                        if (invalidRect.Bottom > innerHeight)
                            invalidRect.Height = innerHeight - invalidRect.Y;

                        if (invalidatedRasterImage == null || invalidatedRasterImage.Width < invalidRect.Right ||
                            invalidatedRasterImage.Height < invalidRect.Bottom)
                        {
                            // Nope, so create one
                            if (invalidatedRasterImage != null)
                            {
                                invalidatedRasterImage.Dispose();
                                invalidatedRasterImage = null;
                            }
                            invalidatedRasterImage = new Bitmap((int)invalidRect.Right, (int)invalidRect.Bottom);
                        }
                        // Make a GraphicsWrapper object from the regionRasterImage and clear it to the background color
                        graphics = GraphicsWrapper.FromImage(invalidatedRasterImage, false);
                        graphics.Clear(backColor);
                    }
                    else
                    {
                        // Make a GraphicsWrapper object from the rasterImage and clear it to the background color
                        graphics = GraphicsWrapper.FromImage(rasterImage, false);
                        graphics.Clear(backColor);
                    }
                }
            }
        }

        /// <summary>
        /// AfterRender - Dispose of Graphics object created for rendering.
        /// </summary>
        private void RendererAfterRender()
        {
            if (graphics != null)
            {
                // Check if we only invalidated a rect
                if (invalidRect != RectangleF.Empty
                   && invalidatedRasterImage != null)
                {
                    // We actually drew everything on invalidatedRasterImage and now we
                    // need to copy that to rasterImage
                    Graphics tempGraphics = Graphics.FromImage(rasterImage);
                    tempGraphics.DrawImage(invalidatedRasterImage, invalidRect.X, invalidRect.Y,
                      invalidRect, GraphicsUnit.Pixel);
                    tempGraphics.Dispose();
                    tempGraphics = null;

                    // If we currently have an idMapRaster here, then we need to create
                    // a temporary graphics object to draw the invalidated portion from
                    // our main graphics window onto it.
                    if (idMapRaster != null)
                    {
                        tempGraphics = Graphics.FromImage(idMapRaster);
                        tempGraphics.DrawImage(graphics.IdMapRaster, invalidRect.X, invalidRect.Y,
                          invalidRect, GraphicsUnit.Pixel);
                        tempGraphics.Dispose();
                        tempGraphics = null;
                    }
                    else
                    {
                        idMapRaster = graphics.IdMapRaster;
                    }
                    // We have updated the invalid region
                    invalidRect = RectangleF.Empty;
                }
                else
                {
                    if (idMapRaster != null && idMapRaster != graphics.IdMapRaster)
                        idMapRaster.Dispose();
                    idMapRaster = graphics.IdMapRaster;
                }

                graphics.Dispose();
                graphics = null;
            }
        }

        public void InvalidateRect(RectangleF rect)
        {
            if (invalidRect == RectangleF.Empty)
                invalidRect = rect;
            else
                invalidRect.Intersect(rect);
        }

        /// <summary>
        /// Renders the <see cref="SvgElement">SvgElement</see>.
        /// </summary>
        /// <param name="node">
        /// The <see cref="SvgElement">SvgElement</see> node to be
        /// rendered
        /// </param>
        /// <returns>
        /// The bitmap on which the rendering was performed.
        /// </returns>
        public Bitmap Render(
            ISvgElement node)
        {
            RectangleF updatedRect;
            if (invalidRect != RectangleF.Empty)
                updatedRect = new RectangleF(invalidRect.X, invalidRect.Y, invalidRect.Width, invalidRect.Height);
            else
                updatedRect = RectangleF.Empty;
            RendererBeforeRender();
            ((SvgElement)node).Render(this);
            RendererAfterRender();
            if (onRender != null)
                OnRender(updatedRect);
            return rasterImage;
        }

        /// <summary>
        /// Renders the <see cref="SvgDocument">SvgDocument</see>.
        /// </summary>
        /// <param name="node">
        /// The <see cref="SvgDocument">SvgDocument</see> node to be
        /// rendered
        /// </param>
        /// <returns>
        /// The bitmap on which the rendering was performed.
        /// </returns>
        public Bitmap Render(
            ISvgDocument node)
        {
            RectangleF updatedRect;
            if (invalidRect != RectangleF.Empty)
                updatedRect = new RectangleF(invalidRect.X, invalidRect.Y, invalidRect.Width, invalidRect.Height);
            else
                updatedRect = RectangleF.Empty;
            RendererBeforeRender();
            ((SvgDocument)node).Render(this);
            RendererAfterRender();
            if (onRender != null)
                OnRender(updatedRect);
            return rasterImage;
        }

        public void ClearMap()
        {
            graphicsNodes.Clear();
        }


        private RectangleF invalidRect = RectangleF.Empty;
        /// <summary>
        /// The invalidated region
        /// </summary>   
        public RectangleF InvalidRect
        {
            get
            {
                return invalidRect;
            }
            set
            {
                invalidRect = value;
            }
        }
        #endregion

        #region Event handlers
        private RenderEvent onRender;
        public RenderEvent OnRender
        {
            get
            {
                return onRender;
            }
            set
            {
                onRender = value;
            }

        }

        /// <summary>
        /// Processes mouse events.
        /// </summary>
        /// <param name="type">
        /// A string describing the type of mouse event that occured.
        /// </param>
        /// <param name="e">
        /// The <see cref="MouseEventArgs">MouseEventArgs</see> that contains
        /// the event data.
        /// </param>
        public void OnMouseEvent(
            string type,
            MouseEventArgs e)
        {
            if (idMapRaster != null)
            {
                try
                {
                    Color pixel = idMapRaster.GetPixel(e.X, e.Y);
                    GraphicsNode grNode = _getGraphicsNodeFromColor(pixel);
                    if (grNode != null)
                    {
                        SvgElement grElement = (SvgElement)grNode.Element;
                        IEventTarget target;
                        if (grElement.ElementInstance != null)
                            target = grElement.ElementInstance as IEventTarget;
                        else
                            target = grElement as IEventTarget;

                        if (target != null)
                        {
                            switch (type)
                            {
                            case "mousemove":
                                {
                                    if (currentTarget == target)
                                    {
                                        target.DispatchEvent(new MouseEvent(
                                            EventType.MouseMove, true, false,
                                            null, // todo: put view here
                                            0, // todo: put detail here
                                            e.X, e.Y, e.X, e.Y,
                                            false, false, false, false,
                                            0, null, false));
                                    }
                                    else
                                    {
                                        if (currentTarget != null)
                                        {
                                            currentTarget.DispatchEvent(new MouseEvent(
                                                EventType.MouseOut, true, false,
                                                null, // todo: put view here
                                                0, // todo: put detail here
                                                e.X, e.Y, e.X, e.Y,
                                                false, false, false, false,
                                                0, null, false));
                                        }

                                        target.DispatchEvent(new MouseEvent(
                                            EventType.MouseOver, true, false,
                                            null, // todo: put view here
                                            0, // todo: put detail here
                                            e.X, e.Y, e.X, e.Y,
                                            false, false, false, false,
                                            0, null, false));
                                    }
                                    break;
                                }
                            case "mousedown":
                                target.DispatchEvent(new MouseEvent(
                                    EventType.MouseDown, true, false,
                                    null, // todo: put view here
                                    0, // todo: put detail here
                                    e.X, e.Y, e.X, e.Y,
                                    false, false, false, false,
                                    0, null, false));
                                currentDownTarget = target;
                                currentDownX = e.X;
                                currentDownY = e.Y;
                                break;
                            case "mouseup":
                                target.DispatchEvent(new MouseEvent(
                                    EventType.MouseUp, true, false,
                                    null, // todo: put view here
                                    0, // todo: put detail here
                                    e.X, e.Y, e.X, e.Y,
                                    false, false, false, false,
                                    0, null, false));
                                if (/*currentDownTarget == target &&*/ Math.Abs(currentDownX - e.X) < 5 && Math.Abs(currentDownY - e.Y) < 5)
                                {
                                    target.DispatchEvent(new MouseEvent(
                                      EventType.Click, true, false,
                                      null, // todo: put view here
                                      0, // todo: put detail here
                                      e.X, e.Y, e.X, e.Y,
                                      false, false, false, false,
                                      0, null, false));
                                }
                                currentDownTarget = null;
                                currentDownX = 0;
                                currentDownY = 0;
                                break;
                            }
                            currentTarget = target;
                        }
                        else
                        {

                            // jr patch
                            if (currentTarget != null && type == "mousemove")
                            {
                                currentTarget.DispatchEvent(new MouseEvent(
                                  EventType.MouseOut, true, false,
                                  null, // todo: put view here
                                  0, // todo: put detail here
                                  e.X, e.Y, e.X, e.Y,
                                  false, false, false, false,
                                  0, null, false));
                            }
                            currentTarget = null;
                        }
                    }
                    else
                    {
                        // jr patch
                        if (currentTarget != null && type == "mousemove")
                        {
                            currentTarget.DispatchEvent(new MouseEvent(
                              EventType.MouseOut, true, false,
                              null, // todo: put view here
                              0, // todo: put detail here
                              e.X, e.Y, e.X, e.Y,
                              false, false, false, false,
                              0, null, false));
                        }
                        currentTarget = null;
                    }
                }
                catch
                {
                }
            }
        }

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Allocate a hit color for the specified graphics node.
        /// </summary>
        /// <param name="grNode">
        /// The <see cref="GraphicsNode">GraphicsNode</see> object for which to
        /// allocate a new hit color.
        /// </param>
        /// <returns>
        /// The hit color for the <see cref="GraphicsNode">GraphicsNode</see>
        /// object.
        /// </returns>
        internal Color _getNextColor(
            GraphicsNode grNode)
        {
            //	TODO: [newhoggy] It looks like there is a potential memory leak here.
            //	We only ever add to the graphicsNodes map, never remove
            //	from it, so it will grow every time this function is called.

            // The counter is used to generate IDs in the range [0,2^24-1]
            // The 24 bits of the counter are interpreted as follows:
            // [red 7 bits | green 7 bits | blue 7 bits |shuffle term 3 bits]
            // The shuffle term is used to define how the remaining high
            // bit is set on each color. The colors are generated in the
            // range [0,127] (7 bits) instead of [0,255]. Then the shuffle term
            // is used to adjust them into the range [0,255].
            // This algorithm has the feature that consecutive ids generate
            // visually distinct colors.
            int id = counter++; // Zero should be the first color.
            int shuffleTerm = id & 7;
            int r = 0x7f & (id >> 17);
            int g = 0x7f & (id >> 10);
            int b = 0x7f & (id >> 3);

            switch (shuffleTerm)
            {
            case 0: break;
            case 1: b |= 0x80; break;
            case 2: g |= 0x80; break;
            case 3: g |= 0x80; b |= 0x80; break;
            case 4: r |= 0x80; break;
            case 5: r |= 0x80; b |= 0x80; break;
            case 6: r |= 0x80; g |= 0x80; break;
            case 7: r |= 0x80; g |= 0x80; b |= 0x80; break;
            }

            Color color = Color.FromArgb(r, g, b);

            graphicsNodes.Add(color, grNode);

            return color;
        }

        internal void _removeColor(Color color,
          GraphicsNode grNode)
        {
            if (!color.IsEmpty)
            {
                graphicsNodes[color] = null;
                graphicsNodes.Remove(color);
            }
        }

        /// <summary>
        /// Gets the <see cref="GraphicsNode">GraphicsNode</see> object that
        /// corresponds to the given hit color.
        /// </summary>
        /// <param name="color">
        /// The hit color for which to get the corresponding
        /// <see cref="GraphicsNode">GraphicsNode</see> object.
        /// </param>
        /// <remarks>
        /// Returns <c>null</c> if a corresponding
        /// <see cref="GraphicsNode">GraphicsNode</see> object cannot be
        /// found for the given hit color.
        /// </remarks>
        /// <returns>
        /// The <see cref="GraphicsNode">GraphicsNode</see> object that
        /// corresponds to the given hit color
        /// </returns>
        private GraphicsNode _getGraphicsNodeFromColor(
            Color color)
        {
            if (color.A == 0)
            {
                return null;
            }
            else
            {
                return (GraphicsNode)graphicsNodes[color];
            }
        }

        /// <summary>
        /// TODO: This method is not used.
        /// </summary>
        /// <param name="color">
        /// </param>
        /// <returns>
        /// </returns>
        private static int ColorToId(
            Color color)
        {
            int r = color.R;
            int g = color.G;
            int b = color.B;
            int shuffleTerm = 0;

            if (0 != (r & 0x80))
            {
                shuffleTerm |= 4;
                r &= 0x7f;
            }

            if (0 != (g & 0x80))
            {
                shuffleTerm |= 2;
                g &= 0x7f;
            }

            if (0 != (b & 0x80))
            {
                shuffleTerm |= 1;
                b &= 0x7f;
            }

            return (r << 17) + (g << 10) + (b << 3) + shuffleTerm;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (idMapRaster != null)
                idMapRaster.Dispose();
            if (invalidatedRasterImage != null)
                invalidatedRasterImage.Dispose();
            if (rasterImage != null)
                rasterImage.Dispose();
            if (graphics != null)
                graphics.Dispose();

        }

        #endregion
    }
}
