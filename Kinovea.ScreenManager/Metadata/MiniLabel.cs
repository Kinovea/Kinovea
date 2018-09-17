#region License
/*
Copyright © Joan Charmant 2008-2011.
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A class to encapsulate a mini label.
    /// Mainly used for Keyframe labels / line measure / track speed.
    /// 
    /// The object is comprised of an attach point and the mini label itself.
    /// The label can be moved relatively to the attach point from the container drawing tool.
    /// 
    /// The mini label position is expressed in absolute coordinates. (previously was relative to the attach).
    /// 
    /// The text to display is actually reset just before we need to draw it.
    /// </summary>
    public class MiniLabel
    {
        #region Properties
        public long Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }
        public int AttachIndex
        {
            get { return attachIndex; }
            set { attachIndex = value; }
        }
        public Color BackColor
        {
            get { return styleHelper.Bicolor.Background; }
            set { styleHelper.Bicolor = new Bicolor(value); }
        }
        #endregion

        #region Members
        private string text = "Label";
        private RoundedRectangle background = new RoundedRectangle();
        private long timestamp; // Absolute time.
        private int attachIndex; // The index of the reference point in the track points list.
        private PointF attachLocation; // The point we are attached to (image coordinates).
        private StyleHelper styleHelper = new StyleHelper();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction
        public MiniLabel() : this(PointF.Empty, Color.Black, null){}
        public MiniLabel(PointF attachPoint, Color color, IImageToViewportTransformer transformer)
        {
            this.attachLocation = attachPoint;
            int tx = -20;
            int ty = -50;
            if (transformer != null)
            {
                tx = transformer.Untransform(-20);
                ty = transformer.Untransform(-50);
            }

            background.Rectangle = new Rectangle(attachPoint.Translate(tx, ty).ToPoint(), Size.Empty);
            styleHelper.Font = new Font("Arial", 8, FontStyle.Bold);
            styleHelper.Bicolor = new Bicolor(Color.FromArgb(160, color));
        }
        public MiniLabel(XmlReader xmlReader, PointF scale)
            : this(PointF.Empty, Color.Black, null)
        {
            ReadXml(xmlReader, scale);
        }
        #endregion

        #region Public methods
        public bool HitTest(PointF point, IImageToViewportTransformer transformer)
        {
            return (background.HitTest(point, false, 0, transformer) > -1);
        }
        public override int GetHashCode()
        {
            int hash = 0;
            hash ^= background.Rectangle.Location.GetHashCode();
            hash ^= styleHelper.ContentHash;
            return hash;
        }
        public void Draw(Graphics canvas, IImageToViewportTransformer transformer, double opacityFactor)
        {
            using(SolidBrush fillBrush = styleHelper.GetBackgroundBrush((int)(opacityFactor*255)))
            using(Pen p = styleHelper.GetBackgroundPen((int)(opacityFactor*64)))
            using(Font f = styleHelper.GetFont((float)transformer.Scale))
            using(SolidBrush fontBrush = styleHelper.GetForegroundBrush((int)(opacityFactor*255)))
            {
                SizeF textSize = canvas.MeasureString(text, f);
                Point location = transformer.Transform(background.Rectangle.Location);
                Size size = new Size((int)textSize.Width, (int)textSize.Height);

                SizeF untransformed = transformer.Untransform(textSize);
                background.Rectangle = new RectangleF(background.Rectangle.Location, untransformed);
                
                Point attch = transformer.Transform(attachLocation);
                Point center = transformer.Transform(background.Center);
                canvas.FillEllipse(fillBrush, attch.Box(2));
                canvas.DrawLine(p, attch, center);
                
                Rectangle rect = new Rectangle(location, size);
                RoundedRectangle.Draw(canvas, rect, fillBrush, f.Height/4, false, false, null);
                canvas.DrawString(text, f, fontBrush, rect.Location);
            }
        }    
        public void SetAttach(PointF p, bool moveLabel)
        {
            float dx = p.X - attachLocation.X;
            float dy = p.Y - attachLocation.Y;
            
            attachLocation = p;
            
            if(moveLabel)
                background.Move(dx, dy);
        }
        public void SetLabel(PointF point)
        {
            background.CenterOn(point);
        }
        public void MoveLabel(float dx, float dy)
        {
            background.Move(dx, dy);
        }
        public void SetText(string text)
        {
            this.text = text;
            
            using(Font f = styleHelper.GetFont(1F))
            {
                SizeF textSize = TextHelper.MeasureString(text, f);
                background.Rectangle = new RectangleF(background.Rectangle.Location, textSize);
            }
        }
        public void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteElementString("SpacePosition", String.Format(CultureInfo.InvariantCulture, "{0};{1}", background.X, background.Y));
            xmlWriter.WriteElementString("TimePosition", timestamp.ToString());
        }
        public void ReadXml(XmlReader xmlReader, PointF scale)
        {
            xmlReader.ReadStartElement();
             
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "SpacePosition":
                        PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        PointF location = p.Scale(scale.X, scale.Y);
                        background.Rectangle = new RectangleF(location, SizeF.Empty);
                        break;
                    case "TimePosition":
                        timestamp = xmlReader.ReadElementContentAsLong();
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
             
            xmlReader.ReadEndElement();
        }
        #endregion
    }
}
