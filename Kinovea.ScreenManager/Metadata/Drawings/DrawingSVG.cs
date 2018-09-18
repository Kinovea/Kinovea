#region License
/*
Copyright © Joan Charmant 2010.
jcharmant@gmail.com 
 
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using SharpVectors.Renderer.Gdi;
using System.Xml.Serialization;
using System.IO;

namespace Kinovea.ScreenManager
{
    [XmlType("SVG")]
    public class DrawingSVG : AbstractDrawing, IScalable, IKvaSerializable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get {  return "SVG Image"; }
        }
        public override int ContentHash
        {
            get { return 0;}
        }
        public override InfosFading InfosFading
        {
            get { return infosFading; }
            set { infosFading = value; }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.Opacity; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get { return null; }
        }
        public override bool IsValid
        {
            get { return valid; }
        }
        #endregion

        #region Members
        private bool valid;
        private string filename;
        // SVG
        private GdiRenderer renderer  = new GdiRenderer();
        private SvgWindow svgWindow;
        private bool loaded;
        private Bitmap svgRendered;
        // Position
        // The drawing scale is used to keep track of the user transform on the drawing, outside of the image transform context.
        // Drawing original dimensions are used to compute the drawing scale.
        private float drawingScale = 1.0f;			// The current scale of the drawing if it were rendered on the original sized image.
        private float initialScale = 1.0f;			// The scale we apply upon loading to make sure the image fits the screen.
        private float drawingRenderingScale = 1.0f;  // The scale of the drawing taking drawing transform AND image transform into account.
        private int originalWidth;					// After initial scaling.
        private int originalHeight;
        private BoundingBox boundingBox = new BoundingBox();
        private bool sizeInPercentage;               // A property of some SVG files.
        private bool finishedResizing;
        private Size videoSize;
        private static readonly int snapMargin = 0;
        // Decoration
        private InfosFading infosFading;
        private ColorMatrix fadingColorMatrix = new ColorMatrix();
        private ImageAttributes fadingImgAttr = new ImageAttributes();
        private Pen penBoundingBox;
        private SolidBrush brushBoundingBox;
        // Instru
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingSVG(long timestamp, long averageTimeStampsPerFrame, string filename)
        {
            this.filename = filename;
            renderer.BackColor = Color.Transparent;
            svgWindow = new SvgWindow(100, 100, renderer);
            
            if (!string.IsNullOrEmpty(filename))
                LoadSVG(filename);
            
            Initialize(timestamp, averageTimeStampsPerFrame);
        }
        public DrawingSVG(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(0, 0, "")
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacityFactor <= 0 || !loaded)
                return;

            Rectangle rect = transformer.Transform(boundingBox.Rectangle);
            
            if(finishedResizing)
            {
                finishedResizing = false;
                RenderAtNewScale(rect.Size, transformer.Scale);
            }

            if (svgRendered == null)
                return;

            fadingColorMatrix.Matrix33 = (float)opacityFactor;
            fadingImgAttr.SetColorMatrix(fadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            canvas.DrawImage(svgRendered, rect, 0, 0, svgRendered.Width, svgRendered.Height, GraphicsUnit.Pixel, fadingImgAttr);

            if (selected)
                boundingBox.Draw(canvas, rect, penBoundingBox, brushBoundingBox, 4);
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity > 0)
                result = boundingBox.HitTest(point, transformer);
            
            return result;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            boundingBox.MoveHandle(point, handleNumber, new Size(originalWidth, originalHeight), true);
        }
        public override void MoveDrawing(float dx, float dy, Keys _ModifierKeys, bool zooming)
        {
            boundingBox.MoveAndSnap((int)dx, (int)dy, videoSize, snapMargin);
        }
        public override PointF GetCopyPoint()
        {
            return boundingBox.Rectangle.Center();
        }
        #endregion

        #region IScalable
        public void Scale(Size size)
        {
            videoSize = size;

            // Set the initial scale so that the drawing is some part of the image height, to make sure it fits well.
            initialScale = (float)(((float)videoSize.Height * 0.75) / originalHeight);
            originalWidth = (int)((float)originalWidth * initialScale);
            originalHeight = (int)((float)originalHeight * initialScale);

            boundingBox.Rectangle = new Rectangle((videoSize.Width - originalWidth) / 2, (videoSize.Height - originalHeight) / 2, originalWidth, originalHeight);

            // Render on first draw call.
            finishedResizing = true;
        }
        #endregion

        #region KVA Serialization
        public void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            if (xmlReader.MoveToAttribute("id"))
                identifier = new Guid(xmlReader.ReadContentAsString());

            if (xmlReader.MoveToAttribute("name"))
                name = xmlReader.ReadContentAsString();

            xmlReader.ReadStartElement();

            while (xmlReader.NodeType == XmlNodeType.Element)
            {
                switch (xmlReader.Name)
                {
                    case "File":
                        filename = xmlReader.ReadElementContentAsString();
                        break;
                    case "InfosFading":
                        infosFading.ReadXml(xmlReader);
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            xmlReader.ReadEndElement();

            LoadSVG(filename);
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
                w.WriteElementString("File", filename);

            if (ShouldSerializeFading(filter))
            {
                w.WriteStartElement("InfosFading");
                infosFading.WriteXml(w);
                w.WriteEndElement();
            }
            
            // TODO: opacity value
            // TODO: bounding box.
        }
        #endregion

        public void ResizeFinished()
        {
            // While the user was resizing the drawing or the image, we didn't update / render the SVG image.
            // Now that he is done, we can stop using the low quality interpolation and resort to SVG scalability.
            
            // However we do not know the final scale until we get back in Draw(),
            // So we just switch a flag on and we'll call the rendering from there.
            finishedResizing = true;
        }
        
        #region Lower level helpers
        private void LoadSVG(string filename)
        {
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
            {
                log.ErrorFormat("Error while loading SVG file. File not found.");
                return;
            }

            try
            {
                svgWindow.Src = filename;
                loaded = true;
                valid = true;

                if (svgWindow.Document.RootElement.Width.BaseVal.UnitType == SvgLengthType.Percentage)
                {
                    sizeInPercentage = true;
                    originalWidth = (int)(svgWindow.Document.RootElement.ViewBox.BaseVal.Width * (svgWindow.Document.RootElement.Width.BaseVal.Value / 100));
                    originalHeight = (int)(svgWindow.Document.RootElement.ViewBox.BaseVal.Height * (svgWindow.Document.RootElement.Height.BaseVal.Value / 100));
                }
                else
                {
                    sizeInPercentage = false;
                    originalWidth = (int)svgWindow.Document.RootElement.Width.BaseVal.Value;
                    originalHeight = (int)svgWindow.Document.RootElement.Height.BaseVal.Value;
                }
            }
            catch(Exception e)
            {
                // FIXME: some files have external DTD that will be attempted to be loaded.
                // See files created from Amaya for example.
                // FIXME: we could also have an error placeholder image as a way to inform the user.
                log.ErrorFormat("Error while loading SVG file. {0}", e.Message);
            }
        }
        private void Initialize(long timestamp, long averageTimeStampsPerFrame)
        {
            // Fading
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            infosFading.UseDefault = false;
            infosFading.AlwaysVisible = true;

            // This is used to set the opacity factor.
            fadingColorMatrix.Matrix00 = 1.0f;
            fadingColorMatrix.Matrix11 = 1.0f;
            fadingColorMatrix.Matrix22 = 1.0f;
            fadingColorMatrix.Matrix33 = 1.0f;	// Change alpha value here for fading. (i.e: 0.5f).
            fadingColorMatrix.Matrix44 = 1.0f;
            fadingImgAttr.SetColorMatrix(fadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            penBoundingBox = new Pen(Color.White, 1);
            penBoundingBox.DashStyle = DashStyle.Dash;
            brushBoundingBox = new SolidBrush(penBoundingBox.Color);
        }
        private void RenderAtNewScale(Size size, double screenScaling)
        {
            // Depending on the complexity of the SVG, this can be a costly operation.
            // We should only do that when mouse move is finished,
            // and use an interpolated version during the change.
            
            // Compute the final drawing sizes, taking both the drawing transformation and the image scaling into account.
            drawingScale = (float)boundingBox.Rectangle.Width / (float)originalWidth;
            drawingRenderingScale = (float)(screenScaling * drawingScale * initialScale);
            
            if(svgRendered == null || drawingRenderingScale != svgWindow.Document.RootElement.CurrentScale)
            {
                // In the case of percentage, CurrentScale is always 100%. 
                // But since there is a cache for the transformation matrix, we need to set it anyway to clear the cache.
                svgWindow.Document.RootElement.CurrentScale = sizeInPercentage ? 1.0f : (float)drawingRenderingScale;

                svgWindow.InnerWidth = size.Width;
                svgWindow.InnerHeight = size.Height;
                
                svgRendered = renderer.Render(svgWindow.Document as SvgDocument);
                
                log.Debug(String.Format("Rendering SVG ({0};{1}), Initial scaling to fit video: {2:0.00}. User scaling: {3:0.00}. Video image scaling: {4:0.00}, Final transformation: {5:0.00}.",
                                        originalWidth, originalHeight, initialScale, drawingScale, screenScaling, drawingRenderingScale));
            }
        }
        #endregion
    }
}

