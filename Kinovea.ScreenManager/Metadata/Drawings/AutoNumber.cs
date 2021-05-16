#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;
using System.Xml.Serialization;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// AutoNumber. (MultiDrawingItem of AutoNumberManager)
    /// Describe and draw a single autonumber.
    /// </summary>
    [XmlType("AutoNumber")]
    public class AutoNumber : AbstractMultiDrawingItem, IKvaSerializable
    {
        #region Properties
        public string Name
        {
            get { return "AutoNumber"; }
        }
        public int Value 
        {
            get { return value;}
        }
        public override int ContentHash
        {
            get { return value.GetHashCode() ^ background.GetHashCode(); }
        }
        #endregion

        #region Members
        private long position;
        private RoundedRectangle background = new RoundedRectangle();   // <-- Also used as a simple ellipsis-defining rectangle when value < 10.
        private InfosFading infosFading;
        private int value = 1;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public AutoNumber(long position, long averageTimeStampsPerFrame, PointF location, int value)
        {
            this.position = position;
            background.Rectangle = new RectangleF(location, SizeF.Empty);
            this.value = value;

            infosFading = new InfosFading(position, averageTimeStampsPerFrame);
            infosFading.UseDefault = false;
            infosFading.FadingFrames = 25;
        }

        public AutoNumber(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata metadata)
            : this(0, 0, Point.Empty, 0)
        {
             ReadXml(xmlReader, scale, timestampMapper);

             infosFading = new InfosFading(position, metadata.AverageTimeStampsPerFrame);
             infosFading.UseDefault = false;
             infosFading.FadingFrames = 25;
        }
        #endregion
        
        #region Public methods
        public void Draw(Graphics canvas, IImageToViewportTransformer transformer, long timestamp, StyleHelper styleHelper)
        {
            double opacityFactor = infosFading.GetOpacityFactor(timestamp);
            if(opacityFactor <= 0)
                return;
        
            using(SolidBrush brushBack = styleHelper.GetBackgroundBrush((int)(opacityFactor * 255)))
            using(SolidBrush brushText = styleHelper.GetForegroundBrush((int)(opacityFactor * 255)))
            using(Font fontText = styleHelper.GetFont((float)transformer.Scale))
            using(Pen penContour = styleHelper.GetForegroundPen((int)(opacityFactor * 255)))
            {
                // Note: recompute background size in case the font floored.
                string text = value.ToString();
                SizeF textSize = canvas.MeasureString(text, fontText);

                Point bgLocation = transformer.Transform(background.Rectangle.Location);
                Size bgSize = new Size((int)textSize.Width, (int)textSize.Height);

                SizeF untransformed = transformer.Untransform(textSize);
                background.Rectangle = new RectangleF(background.Rectangle.Location, untransformed);
                
                penContour.Width = 2;
                Rectangle rect = new Rectangle(bgLocation, bgSize);
                RoundedRectangle.Draw(canvas, rect, brushBack, fontText.Height/4, false, true, penContour);
                canvas.DrawString(text, fontText, brushText, rect.Location);
            }
        }
        public int HitTest(PointF point, long currentTimeStamp, IImageToViewportTransformer transformer)
        {
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimeStamp);
            if(opacity > 0)
                return background.HitTest(point, false, 0, transformer);

            return result;
        }
        public void MouseMove(float dx, float dy)
        {
            background.Move(dx, dy);
        }
        public bool IsVisible(long timestamp)
        {
            return infosFading.GetOpacityFactor(timestamp) > 0;
        }
        #endregion
        
        #region KVA Serialization
        public void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            if(timestampMapper == null)
            {
                xmlReader.ReadOuterXml();
                return;                
            }

            if (xmlReader.MoveToAttribute("id"))
                identifier = new Guid(xmlReader.ReadContentAsString());

            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "Time":
                        position = timestampMapper(xmlReader.ReadElementContentAsLong());
                        break;
                    case "Location":
                        PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        background.Rectangle = new RectangleF(p.Scale(scale.X, scale.Y), SizeF.Empty);
                        break;
                    case "Value":
                        value = xmlReader.ReadElementContentAsInt();
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            xmlReader.ReadEndElement();
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if ((filter & SerializationFilter.Core) == SerializationFilter.Core)
            {
                w.WriteElementString("Time", position.ToString());
                w.WriteElementString("Location", XmlHelper.WritePointF(background.Rectangle.Location));
                w.WriteElementString("Value", value.ToString());
            }
        }
        #endregion
        
        #region Private methods
        private void SetText(StyleHelper styleHelper)
        {
            string text = value.ToString();
            
            using(Font f = styleHelper.GetFont(1.0F))
            {
                SizeF textSize = TextHelper.MeasureString(text, f);
                
                float width = value < 10 ? textSize.Height : textSize.Width;
                float height = textSize.Height;
                background.Rectangle = new RectangleF(background.Rectangle.Location, new SizeF(width, height));
            }
        }
        #endregion
    }
}
