#region License
/*
Copyright © Joan Charmant 2008-2011.
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType ("Label")]
    public class DrawingText : AbstractDrawing, IKvaSerializable, IDecorable
    {
        #region Properties
        public override string DisplayName
        {
            get {  return ScreenManagerLang.ToolTip_DrawingToolText; }
        }
        public override int ContentHash
        {
            get 
            { 
                int hash = text.GetHashCode();
                hash ^= background.Rectangle.Location.GetHashCode();
                hash ^= styleHelper.ContentHash;
                hash ^= infosFading.ContentHash;
                return hash;
            }
        } 
        public DrawingStyle DrawingStyle
        {
            get { return style;}
        }
        public override InfosFading InfosFading
        {
            get { return infosFading; }
            set { infosFading = value; }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get { return null; }
        }
        public TextBox EditBox
        {
            get { return textBox; }
            set { textBox = value;}
        }
        public Control ContainerScreen
        {
            get { return host; }
            set { host = value;}
        }
        public bool Editing
        {
            get { return editing; }
        }
        #endregion

        #region Members
        private string text;
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private InfosFading infosFading;
        private bool editing;
        private IImageToViewportTransformer imageToViewportTransformer;
        
        private RoundedRectangle background = new RoundedRectangle();
        private TextBox textBox;
        private Control host;
        
        private const int defaultFontSize = 16;    		// will also be used for the text box.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingText(Point p, long timestamp, long averageTimeStampsPerFrame, DrawingStyle stylePreset)
        {
            text = " ";
            background.Rectangle = new Rectangle(p, Size.Empty);
            
            // Decoration & binding with editors
            styleHelper.Bicolor = new Bicolor(Color.Black);
            styleHelper.Font = new Font("Arial", defaultFontSize, FontStyle.Bold);
            if(stylePreset != null)
            {
                style = stylePreset.Clone();
                BindStyle();
            }
            
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            editing = false;

            textBox = new TextBox() { 
                Visible = false, 
                BackColor = Color.White, 
                BorderStyle = BorderStyle.None, 
                Multiline = true,
                Text = text,
                Font = styleHelper.GetFontDefaultSize(defaultFontSize)
            };
            
            textBox.TextChanged += TextBox_TextChanged;
            UpdateLabelRectangle();
        }
        public DrawingText(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(Point.Empty,0,0, ToolManager.Label.StylePreset.Clone())
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacityFactor <= 0 || editing)
                return;
                
            using (SolidBrush brushBack = styleHelper.GetBackgroundBrush((int)(opacityFactor * 128)))
            using (SolidBrush brushText = styleHelper.GetForegroundBrush((int)(opacityFactor * 255)))
            using (Font fontText = styleHelper.GetFont((float)transformer.Scale))
            {
                // Note: recompute background size in case the font floored.
                SizeF textSize = canvas.MeasureString(text, fontText);
                Point bgLocation = transformer.Transform(background.Rectangle.Location);
                Size bgSize = new Size((int)textSize.Width, (int)textSize.Height);

                SizeF untransformed = transformer.Untransform(textSize);
                background.Rectangle = new RectangleF(background.Rectangle.Location, untransformed);
                
                Rectangle rect = new Rectangle(bgLocation, bgSize);
                RoundedRectangle.Draw(canvas, rect, brushBack, fontText.Height/4, false, false, null);
                canvas.DrawString(text, fontText, brushText, rect.Location);
            }
        }
        public override int HitTest(Point point, long currentTimestamp, IImageToViewportTransformer transformer, bool zooming)
        {
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity > 0)
                result = background.HitTest(point, true, transformer);

            return result;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {	
            // Invisible handler to change font size.
            int wantedHeight = (int)(point.Y - background.Rectangle.Location.Y);
            styleHelper.ForceFontSize(wantedHeight, text);
            style.ReadValue();
            UpdateLabelRectangle();
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            background.Move(dx, dy);
            RelocateEditbox();
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
                    case "Text":
                        text = xmlReader.ReadElementContentAsString();
                        break;
                    case "Position":
                        PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        background.Rectangle = new RectangleF(p.Scale(scale.X, scale.Y), SizeF.Empty);
                        break;
                    case "DrawingStyle":
                        style = new DrawingStyle(xmlReader);
                        BindStyle();
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
            UpdateLabelRectangle();
        }
        public void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteElementString("Text", text);
            xmlWriter.WriteElementString("Position", XmlHelper.WritePointF(background.Rectangle.Location));
            
            xmlWriter.WriteStartElement("DrawingStyle");
            style.WriteXml(xmlWriter);
            xmlWriter.WriteEndElement();
            
            xmlWriter.WriteStartElement("InfosFading");
            infosFading.WriteXml(xmlWriter);
            xmlWriter.WriteEndElement(); 
        }
        #endregion
        
        public void SetEditMode(bool editing, IImageToViewportTransformer transformer)
        {
            this.editing = editing;

            if(imageToViewportTransformer == null)
               imageToViewportTransformer = transformer;

            if (editing)
            {
                RelocateEditbox(); // This is needed because the container top-left corner may have changed 
                textBox.Text = text;
            }
            
            textBox.Visible = editing;
        }
        public void RelocateEditbox()
        {
            if(imageToViewportTransformer != null && host != null)
            {
                Rectangle rect =  imageToViewportTransformer.Transform(background.Rectangle);
                textBox.Location = rect.Location.Translate(host.Left, host.Top);
            }
        }

        #region Lower level helpers
        private void BindStyle()
        {
            style.Bind(styleHelper, "Bicolor", "back color");
            style.Bind(styleHelper, "Font", "font size");
        }
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            text = textBox.Text;
            UpdateLabelRectangle();
        }
        private void UpdateLabelRectangle()
        {
            // Text changed or font size changed.
            using(Button but = new Button())
            using(Graphics g = but.CreateGraphics())
            using(Font f = styleHelper.GetFont(1F))
            {
                SizeF textSize = g.MeasureString(text, f);
                background.Rectangle = new RectangleF(background.Rectangle.Location, textSize);
                
                // Also update the edit box size. (Use a fixed font though).
                // The extra space is to account for blank new lines.
                SizeF boxSize = g.MeasureString(text + " ", textBox.Font);
                textBox.Size = new Size((int)boxSize.Width + 10, (int)boxSize.Height);
            }
        }
        #endregion
    }
}
