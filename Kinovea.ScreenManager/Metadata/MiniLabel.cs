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
    /// Mainly used for Keyframe labels on the trajectory and the measure labels on measurable objects.
    /// 
    /// The object comprises an attach point and the mini label itself.
    /// The label can be moved relatively to the attach point from the container drawing tool.
    /// 
    /// The mini label position is expressed in absolute coordinates. (previously was relative to the attach).
    /// 
    /// The text to display is actually reset just before we need to draw it.
    /// Order of operations: 1. SetText, 2. Draw, 3. HitTest.
    /// </summary>
    public class MiniLabel
    {
        #region Properties
        
        /// <summary>
        /// The timestamp associated with the label. This is used to compute fading and recognize known attach points.
        /// </summary>
        public long Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }

        /// <summary>
        /// An index into a list of points owned by the drawing.
        /// This is used to contextualize the displayed information based on the reference point.
        /// Used when there are multiple mini labels like for trajectory or kinogram.
        /// </summary>
        public int AttachIndex
        {
            get { return attachIndex; }
            set { attachIndex = value; }
        }

        /// <summary>
        /// The Font used by the mini label.
        /// </summary>
        public int FontSize
        {
            get { return (int)styleData.Font.Size; }
            set { styleData.Font = new Font("Arial", value, FontStyle.Bold); }
        }

        /// <summary>
        /// The color of the background of the label.
        /// </summary>
        public Color BackColor
        {
            get { return styleData.GetBackgroundColor(); }
            set { styleData.BackgroundColor = value; }
        }

        /// <summary>
        /// The name to use when the MeasureLabelType is set to Name.
        /// For other measure options the text is computed on the fly.
        /// </summary>
        public string Name
        {
            get; set;
        }

        public bool ShowConnector
        {
            get { return showConnector; }
            set { showConnector = value; }
        }
        #endregion

        #region Members
        private string text = "Label";
        private RoundedRectangle background = new RoundedRectangle();
        private long timestamp; // Absolute time.
        private int attachIndex; // The index of the reference point in the track points list.
        private PointF attachLocation; // The point we are attached to (image coordinates).
        private bool showConnector = true; // Whether to draw the connection between the label and the attach point.
        private StyleData styleData = new StyleData();
        private IImageToViewportTransformer transformer;
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
                this.transformer = transformer;
                tx = transformer.Untransform(-20);
                ty = transformer.Untransform(-50);
            }

            background.Rectangle = new Rectangle(attachPoint.Translate(tx, ty).ToPoint(), Size.Empty);
            styleData.BackgroundColor = Color.FromArgb(160, color);
            styleData.Font = new Font("Arial", 8, FontStyle.Bold);
        }
        public MiniLabel(XmlReader xmlReader, PointF scale)
            : this(PointF.Empty, Color.Black, null)
        {
            ReadXml(xmlReader, scale);
        }
        #endregion

        #region Public methods
        public bool HitTest(PointF point)
        {
            return (background.HitTest(point, false, 0, transformer) > -1);
        }
        public override int GetHashCode()
        {
            int hash = 0;
            hash ^= background.Rectangle.Location.GetHashCode();
            hash ^= styleData.ContentHash;
            return hash;
        }
        public void Draw(Graphics canvas, IImageToViewportTransformer transformer, double opacity)
        {
            this.transformer = transformer;

            using(SolidBrush fillBrush = styleData.GetBackgroundBrush((int)(opacity*255)))
            using(Pen p = styleData.GetBackgroundPen((int)(opacity*64)))
            using(Font f = styleData.GetFont((float)transformer.Scale))
            using(SolidBrush fontBrush = styleData.GetForegroundBrush((int)(opacity*255)))
            {
                SizeF textSize = canvas.MeasureString(text, f);
                Point location = transformer.Transform(background.Rectangle.Location);
                Size size = new Size((int)textSize.Width, (int)textSize.Height);

                SizeF untransformed = transformer.Untransform(textSize);
                background.Rectangle = new RectangleF(background.Rectangle.Location, untransformed);
                
                // Connector
                if (showConnector)
                {
                    Point attch = transformer.Transform(attachLocation);
                    Point center = transformer.Transform(background.Center);
                    canvas.FillEllipse(fillBrush, attch.Box(2));
                    canvas.DrawLine(p, attch, center);
                }
                
                // Background
                Rectangle rect = new Rectangle(location, size);
                RoundedRectangle.Draw(canvas, rect, fillBrush, f.Height/4, false, false, null);

                // Text
                canvas.DrawString(text, f, fontBrush, rect.Location);
            }
        }    

        /// <summary>
        /// Set the attach point to the passed point.
        /// If moveLabel is true we also move the label itself so the 
        /// relative position of the label is preserved.
        /// </summary>
        public void SetAttach(PointF p, bool moveLabel)
        {
            float dx = p.X - attachLocation.X;
            float dy = p.Y - attachLocation.Y;
            
            attachLocation = p;
            
            if(moveLabel)
                background.Move(dx, dy);
        }
        public void SetCenter(PointF point)
        {
            background.CenterOn(point);
        }
        public void MoveLabel(float dx, float dy)
        {
            background.Move(dx, dy);
        }

        /// <summary>
        /// Change the text of the label and update the size of the background rectangle.
        /// </summary>
        public void SetText(string text, IImageToViewportTransformer transformer = null)
        {
            this.text = text;
            this.transformer = transformer;
            
            using(Font f = styleData.GetFont(1F))
            {
                SizeF textSize = TextHelper.MeasureString(text, f);
                if (transformer != null)
                    textSize = transformer.Untransform(textSize);
                
                background.Rectangle = new RectangleF(background.Rectangle.Location, textSize);
            }
        }

        public void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteElementString("SpacePosition", String.Format(CultureInfo.InvariantCulture, "{0};{1}", background.X, background.Y));
            xmlWriter.WriteElementString("TimePosition", timestamp.ToString());
        }

        /// <summary>
        /// Write the mini label to KVA.
        /// referenceAttach is the attach point at the reference timestamp of the parent drawing.
        /// The mini label should be stored based on this point.
        /// </summary>
        public void WriteXml(XmlWriter xmlWriter, PointF referenceAttach)
        {
            // Move the label based on the reference attach point before saving the data.
            float dx = referenceAttach.X - attachLocation.X;
            float dy = referenceAttach.Y - attachLocation.Y;
            background.Move(dx, dy);

            WriteXml(xmlWriter);
            
            // Restore the location for the current frame.
            background.Move(-dx, -dy);
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
