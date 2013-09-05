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
        public int Value 
        {
            get { return value;}
        }
        #endregion

        #region Members
        private long position;
        private RoundedRectangle background = new RoundedRectangle();   // <-- Also used as a simple ellipsis-defining rectangle when value < 10.
        private InfosFading infosFading;
        private int value = 1;
        private StyleHelper styleHelper;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public AutoNumber(long _iPosition, long _iAverageTimeStampsPerFrame, Point _location, int _value, StyleHelper _styleHelper)
        {
            position = _iPosition;
            background.Rectangle = new Rectangle(_location, Size.Empty);
            value = _value;

            infosFading = new InfosFading(_iPosition, _iAverageTimeStampsPerFrame);
            infosFading.UseDefault = false;
            infosFading.FadingFrames = 25;

            styleHelper = _styleHelper;
            
            SetText(value.ToString());
        }
        public AutoNumber(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback, long _iAverageTimeStampsPerFrame, StyleHelper _styleHelper)
            : this(0, 0, Point.Empty, 0, _styleHelper)
        {
             ReadXml(_xmlReader, _scale, _remapTimestampCallback);
             
             infosFading = new InfosFading(position, _iAverageTimeStampsPerFrame);
             infosFading.UseDefault = false;
             infosFading.FadingFrames = 25;
             
             SetText(value.ToString());
        }
        #endregion
        
        #region Public methods
        public void Draw(Graphics _canvas, IImageToViewportTransformer _transformer, long _timestamp)
        {
            double fOpacityFactor = infosFading.GetOpacityFactor(_timestamp);
            if(fOpacityFactor <= 0)
                return;
        
            int alpha = (int)(255 * fOpacityFactor);
            
            using(SolidBrush brushBack = styleHelper.GetBackgroundBrush((int)(fOpacityFactor * 255)))
            using(SolidBrush brushFront = styleHelper.GetForegroundBrush((int)(fOpacityFactor * 255)))
            using(Pen penContour = styleHelper.GetForegroundPen((int)(fOpacityFactor * 255)))
            using(Font f = styleHelper.GetFont((float)_transformer.Scale))
            {
                // Note: recompute the background size each time in case font floored.
                string text = value.ToString();
                penContour.Width = 2;
                SizeF textSize = _canvas.MeasureString(text, f);
                Point bgLocation = _transformer.Transform(background.Rectangle.Location);              
                
                Size bgSize;
                
                if(value < 10)
                {
                    bgSize = new Size((int)textSize.Height, (int)textSize.Height);
                    Rectangle rect = new Rectangle(bgLocation, bgSize);
                    _canvas.FillEllipse(brushBack, rect);
                    _canvas.DrawEllipse(penContour, rect);
                }
                else
                {
                    bgSize = new Size((int)textSize.Width, (int)textSize.Height);
                    Rectangle rect = new Rectangle(bgLocation, bgSize);
                    RoundedRectangle.Draw(_canvas, rect, brushBack, f.Height/4, false, true, penContour);    
                }
                
                int verticalShift = (int)(textSize.Height / 10);
                Point textLocation = new Point(bgLocation.X + (int)((bgSize.Width - textSize.Width)/2), bgLocation.Y + verticalShift);
                _canvas.DrawString(text, f, brushFront, textLocation);
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
        public void MouseMove(int _deltaX, int _deltaY)
        {
            background.Move(_deltaX, _deltaY);
        }
        public bool IsVisible(long _timestamp)
        {
            return infosFading.GetOpacityFactor(_timestamp) > 0;
        }
        #endregion
        
        #region KVA Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback)
        {
            if(_remapTimestampCallback == null)
            {
                _xmlReader.ReadOuterXml();
                return;                
            }
            
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(_xmlReader.Name)
                {
                    case "Time":
                        position = _remapTimestampCallback(_xmlReader.ReadElementContentAsLong(), false);
                        break;
                    case "Location":
                        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        background.Rectangle = new Rectangle(p.Scale(_scale.X, _scale.Y), Size.Empty);
                        break;
                    case "Value":
                        value = _xmlReader.ReadElementContentAsInt();
                        break;
                    default:
                        string unparsed = _xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            _xmlReader.ReadEndElement();
        }
        public void WriteXml(XmlWriter _xmlWriter)
        {
            _xmlWriter.WriteElementString("Time", position.ToString());
            _xmlWriter.WriteElementString("Location", string.Format("{0};{1}", background.X, background.Y));
            _xmlWriter.WriteElementString("Value", value.ToString());
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
                
                int width = value < 10 ? (int)textSize.Height : (int)textSize.Width;
                int height = (int)textSize.Height;
                background.Rectangle = new Rectangle(background.Rectangle.Location, new Size(width, height));
            }
        }
        #endregion
    }
}
