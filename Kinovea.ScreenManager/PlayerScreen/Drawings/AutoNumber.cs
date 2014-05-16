#region License
/*
Copyright © Joan Charmant 2012.
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
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// AutoNumber. (MultiDrawingItem of AutoNumberManager)
    /// Describe and draw a single autonumber.
    /// </summary>
    public class AutoNumber : IKvaSerializable
    {
        #region Properties
        public Guid Id
        {
            get { return id; }
        }
        public int Value 
        {
            get { return value;}
        }
        #endregion

        #region Members
        private long position;
        private Guid id = Guid.NewGuid();
        private RoundedRectangle background = new RoundedRectangle();   // <-- Also used as a simple ellipsis-defining rectangle when value < 10.
        private InfosFading infosFading;
        private int value = 1;
        private StyleHelper styleHelper;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public AutoNumber(long position, long averageTimeStampsPerFrame, Point location, int value, StyleHelper styleHelper)
        {
            this.position = position;
            background.Rectangle = new Rectangle(location, Size.Empty);
            this.value = value;

            infosFading = new InfosFading(position, averageTimeStampsPerFrame);
            infosFading.UseDefault = false;
            infosFading.FadingFrames = 25;

            this.styleHelper = styleHelper;
            
            SetText(value.ToString());
        }
        public AutoNumber(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, long averageTimeStampsPerFrame, StyleHelper styleHelper)
            : this(0, 0, Point.Empty, 0, styleHelper)
        {
             ReadXml(xmlReader, scale, timestampMapper);
             
             infosFading = new InfosFading(position, averageTimeStampsPerFrame);
             infosFading.UseDefault = false;
             infosFading.FadingFrames = 25;
             
             SetText(value.ToString());
        }
        #endregion
        
        #region Public methods
        public void Draw(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            double fOpacityFactor = infosFading.GetOpacityFactor(timestamp);
            if(fOpacityFactor <= 0)
                return;
        
            int alpha = (int)(255 * fOpacityFactor);
            
            using(SolidBrush brushBack = styleHelper.GetBackgroundBrush((int)(fOpacityFactor * 255)))
            using(SolidBrush brushFront = styleHelper.GetForegroundBrush((int)(fOpacityFactor * 255)))
            using(Pen penContour = styleHelper.GetForegroundPen((int)(fOpacityFactor * 255)))
            using(Font f = styleHelper.GetFont((float)transformer.Scale))
            {
                // Note: recompute the background size each time in case font floored.
                string text = value.ToString();
                penContour.Width = 2;
                SizeF textSize = canvas.MeasureString(text, f);
                Point bgLocation = transformer.Transform(background.Rectangle.Location);
                SizeF untransformed = transformer.Untransform(textSize);
                background.Rectangle = new RectangleF(background.Rectangle.Location, untransformed);
                
                Size bgSize;
                if(value < 10)
                {
                    bgSize = new Size((int)textSize.Height, (int)textSize.Height);
                    Rectangle rect = new Rectangle(bgLocation, bgSize);
                    canvas.FillEllipse(brushBack, rect);
                    canvas.DrawEllipse(penContour, rect);
                }
                else
                {
                    bgSize = new Size((int)textSize.Width, (int)textSize.Height);
                    Rectangle rect = new Rectangle(bgLocation, bgSize);
                    RoundedRectangle.Draw(canvas, rect, brushBack, f.Height/4, false, true, penContour);    
                }
                
                int verticalShift = (int)(textSize.Height / 10);
                Point textLocation = new Point(bgLocation.X + (int)((bgSize.Width - textSize.Width)/2), bgLocation.Y + verticalShift);
                canvas.DrawString(text, f, brushFront, textLocation);
            }
        }
        public int HitTest(Point point, long currentTimeStamp, IImageToViewportTransformer transformer)
        {
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimeStamp);
            if(opacity > 0)
                return background.HitTest(point, false, transformer);

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
        private void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            if(timestampMapper == null)
            {
                xmlReader.ReadOuterXml();
                return;                
            }

            if (xmlReader.MoveToAttribute("id"))
                id = new Guid(xmlReader.ReadContentAsString());

            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "Time":
                        position = timestampMapper(xmlReader.ReadElementContentAsLong(), false);
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
        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("Time", position.ToString());
            w.WriteElementString("Location", XmlHelper.WritePointF(background.Rectangle.Location));
            w.WriteElementString("Value", value.ToString());
        }
        #endregion
        
        #region Private methods
        private void SetText(string text)
        {
            using(Button but = new Button())
            using(Graphics g = but.CreateGraphics())
            using(Font f = styleHelper.GetFont(1.0F))
            {
                SizeF textSize = g.MeasureString(text, f);
                
                float width = value < 10 ? textSize.Height : textSize.Width;
                float height = textSize.Height;
                background.Rectangle = new RectangleF(background.Rectangle.Location, new SizeF(width, height));
            }
        }
        #endregion
    }
}
