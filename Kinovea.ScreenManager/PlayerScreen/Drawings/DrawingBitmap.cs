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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Kinovea.Services;
using System.Xml.Serialization;
using System.Xml;
using System;
using System.IO;

namespace Kinovea.ScreenManager
{
    [XmlType("Bitmap")]
    public class DrawingBitmap : AbstractDrawing, IScalable, IKvaSerializable
    {
        #region Properties
        public override string DisplayName
        {
            get {  return "Bitmap Image"; }
        }
        public override int ContentHash
        {
            get { return 0; }
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
        public override bool  IsValid
        {
            get { return valid; }
        }
        #endregion

        #region Members
        private bool valid;
        private string filename;
        private Bitmap bitmap;
        private BoundingBox boundingBox = new BoundingBox();
        private float initialScale = 1.0f;			            // The scale we apply upon loading to make sure the image fits the screen.
        private int originalWidth;
        private int originalHeight;
        private Size videoSize;
        private static readonly int snapMargin = 0;
        private InfosFading infosFading;
        private ColorMatrix fadingColorMatrix = new ColorMatrix();
        private ImageAttributes fadingImgAttr = new ImageAttributes();
        private Pen penBoundingBox;
        private SolidBrush brushBoundingBox;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingBitmap(long timestamp, long averageTimeStampsPerFrame, string filename)
        {
            if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                this.filename = filename;
                bitmap = new Bitmap(filename);
            }

            valid = bitmap != null;
            
            Initialize(timestamp, averageTimeStampsPerFrame);
        }
        public DrawingBitmap(long timestamp, long averageTimeStampsPerFrame, Bitmap bmp)
        {
            if (bmp != null)
                bitmap = AForge.Imaging.Image.Clone(bmp);

            valid = bitmap != null;
            
            Initialize(timestamp, averageTimeStampsPerFrame);
        }
        public DrawingBitmap(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(0, 0, "")
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            if (!valid)
                return;
            
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacityFactor <= 0)
                return;

            Rectangle rect = transformer.Transform(boundingBox.Rectangle);

            fadingColorMatrix.Matrix33 = (float)opacityFactor;
            fadingImgAttr.SetColorMatrix(fadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            canvas.DrawImage(bitmap, rect, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, fadingImgAttr);

            if (selected)
                boundingBox.Draw(canvas, rect, penBoundingBox, brushBoundingBox, 4);
        }
        public override int HitTest(Point point, long currentTimestamp, IImageToViewportTransformer transformer, bool zooming)
        {
            if (!valid)
                return -1;
            
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity > 0)
                result = boundingBox.HitTest(point, transformer);
            
            return result;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            boundingBox.MoveHandle(point.ToPoint(), handleNumber, new Size(originalWidth, originalHeight), true);
        }
        public override void MoveDrawing(float dx, float dy, Keys _ModifierKeys, bool zooming)
        {
            boundingBox.MoveAndSnap((int)dx, (int)dy, videoSize, snapMargin);
        }
        #endregion

        #region IScalable
        public void Scale(Size size)
        {
            if (bitmap == null)
                return;

            videoSize = size;
            originalWidth = bitmap.Width;
            originalHeight = bitmap.Height;

            // Set the initial scale so that the drawing is some part of the image height, to make sure it fits well.
            // For bitmap drawing, we only do this if no upsizing is involved.
            initialScale = (float)(((float)videoSize.Height * 0.75) / originalHeight);
            
            if (initialScale < 1.0)
            {
                originalWidth = (int)((float)originalWidth * initialScale);
                originalHeight = (int)((float)originalHeight * initialScale);
            }

            boundingBox.Rectangle = new Rectangle((videoSize.Width - originalWidth) / 2, (videoSize.Height - originalHeight) / 2, originalWidth, originalHeight);
        }
        #endregion

        #region KVA Serialization
        public void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            if (xmlReader.MoveToAttribute("id"))
                identifier = new Guid(xmlReader.ReadContentAsString());

            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
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

            if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
                bitmap = new Bitmap(filename);

            valid = bitmap != null;
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
    }
}