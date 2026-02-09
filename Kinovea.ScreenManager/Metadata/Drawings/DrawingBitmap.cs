#region License
/*
Copyright © Joan Charmant 2011.
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
using Kinovea.ScreenManager.Properties;

namespace Kinovea.ScreenManager
{
    [XmlType("Bitmap")]
    public class DrawingBitmap : AbstractDrawing, IScalable, IKvaSerializable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get {  return "Bitmap Image"; }
        }
        public override int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= boundingBox.ContentHash;
                hash ^= infosFading.ContentHash;
                return hash;
            }
        } 
        public override InfosFading InfosFading
        {
            get { return infosFading; }
            set { infosFading = value; }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.Fading | DrawingCapabilities.CopyPaste; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get { return null; }
        }
        public override bool  IsValid
        {
            get { return valid; }
        }
        public bool IsSticker
        {
            get { return isSticker; }
        }
        #endregion

        #region Members
        private bool valid;
        private string filename;
        private Bitmap bitmap;
        private BoundingBox boundingBox = new BoundingBox();
        private int originalWidth;
        private int originalHeight;
        private Size videoSize;
        private InfosFading infosFading;
        private ColorMatrix fadingColorMatrix = new ColorMatrix();
        private ImageAttributes fadingImgAttr = new ImageAttributes();
        private Pen penBoundingBox;
        private SolidBrush brushBoundingBox;
        private bool isSticker;
        private string stickerRef = "_1f600";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingBitmap(long timestamp, double averageTimeStampsPerFrame, string filename)
        {
            if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                this.filename = filename;
                bitmap = new Bitmap(filename);
            }

            valid = bitmap != null;
            
            Initialize(timestamp, averageTimeStampsPerFrame);
        }

        public DrawingBitmap(long timestamp, double averageTimeStampsPerFrame, Bitmap bmp)
        {
            if (bmp != null)
                bitmap = BitmapHelper.Copy(bmp);

            valid = bitmap != null;
            Initialize(timestamp, averageTimeStampsPerFrame);
        }

        /// <summary>
        /// Standard drawing tool constructor, used for the Sticker variant of the tool.
        /// </summary>
        public DrawingBitmap(PointF origin, long timestamp, double averageTimeStampsPerFrame, StyleElements preset = null, IImageToViewportTransformer transformer = null)
        {
            isSticker = true;
            bitmap = Stickers.ResourceManager.GetObject(stickerRef) as Bitmap;
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
        public override void Draw(Graphics canvas, DistortionHelper distorter, CameraTransformer cameraTransformer, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            if (!valid || bitmap == null)
                return;
            
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacityFactor <= 0)
                return;

            Rectangle rect = transformer.Transform(boundingBox.Rectangle);

            fadingColorMatrix.Matrix33 = (float)opacityFactor;
            fadingImgAttr.SetColorMatrix(fadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            canvas.DrawImage(bitmap, rect, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, fadingImgAttr);
            
            if (selected)
            {
                boundingBox.Draw(canvas, rect, penBoundingBox, brushBoundingBox, 4);
            }
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            if (!valid)
                return -1;

            if (infosFading.GetOpacityFactor(currentTimestamp) <= 0)
                return -1;
            
            return boundingBox.HitTest(point, transformer);
        }
        public override void MoveDrawing(float dx, float dy, Keys _ModifierKeys)
        {
            boundingBox.Move(dx, dy);
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            boundingBox.MoveHandle(point, handleNumber, new Size(originalWidth, originalHeight), true);
        }
        
        public override PointF GetCopyPoint()
        {
            return boundingBox.Rectangle.Center();
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

            // Abort if the drawing is coming from XML.
            if (!boundingBox.Rectangle.IsEmpty)
                return;

            // Set the initial scale so that the drawing is some part of the image height, to make sure it fits well.
            // For bitmap drawing, we only do this if no upsizing is involved.
            float initialScale = (float)(((float)videoSize.Height * 0.75) / originalHeight);
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

            if (xmlReader.MoveToAttribute("name"))
                name = xmlReader.ReadContentAsString();

            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "File":
                        filename = xmlReader.ReadElementContentAsString();
                        break;
                    case "BoundingBox":
                        RectangleF rect = XmlHelper.ParseRectangleF(xmlReader.ReadElementContentAsString());
                        boundingBox.Rectangle = rect.ToRectangle();
                        break;
                    case "Sticker":
                        isSticker = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "StickerReference":
                        stickerRef = xmlReader.ReadElementContentAsString();
                        bitmap = Stickers.ResourceManager.GetObject(stickerRef) as Bitmap;
                        break;
                    case "Bitmap":
                        bitmap = XmlHelper.ParseImageFromBase64(xmlReader.ReadElementContentAsString());
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

            if (bitmap != null)
                filename = null;

            if (bitmap == null && !string.IsNullOrEmpty(filename) && File.Exists(filename))
                bitmap = new Bitmap(filename);

            valid = bitmap != null;
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("File", filename);
                w.WriteElementString("BoundingBox", XmlHelper.WriteRectangleF(boundingBox.Rectangle));
                w.WriteElementString("Sticker", XmlHelper.WriteBoolean(isSticker));

                if (isSticker)
                {
                    w.WriteElementString("StickerReference", stickerRef);
                }
                else
                {
                    w.WriteElementString("Bitmap", XmlHelper.WriteBitmap(bitmap));
                }
            }

            if (ShouldSerializeFading(filter))
            {
                w.WriteStartElement("InfosFading");
                infosFading.WriteXml(w);
                w.WriteEndElement();
            }
        }
        #endregion
        
        /// <summary>
        /// Show sticker selection dialog and update the sticker reference.
        /// </summary>
        public bool SelectSticker()
        {
            if (!isSticker)
                throw new InvalidProgramException();

            bool changedSticker = false;
            
            FormStickerPicker fsp = new FormStickerPicker();
            FormsHelper.Locate(fsp);
            if (fsp.ShowDialog() == DialogResult.OK)
            {
                if (stickerRef != fsp.PickedStickerRef)
                {
                    stickerRef = fsp.PickedStickerRef;
                    bitmap = Stickers.ResourceManager.GetObject(stickerRef) as Bitmap;
                    changedSticker = true;
                }

            }

            fsp.Dispose();
            return changedSticker;
        }

        private void Initialize(long timestamp, double averageTimeStampsPerFrame)
        {
            // Fading
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            infosFading.UseDefault = true;
            infosFading.AlwaysVisible = false;

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