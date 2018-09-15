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
        public override string ToolDisplayName
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
        private Font fontText;
        private IImageToViewportTransformer imageToViewportTransformer;
        
        private RoundedRectangle background = new RoundedRectangle();
        private TextBox textBox;
        private Control host;
        
        private const int defaultFontSize = 14;    		// will also be used for the text box.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingText(PointF p, long timestamp, long averageTimeStampsPerFrame, DrawingStyle stylePreset)
        {
            text = "";
            background.Rectangle = new RectangleF(p, SizeF.Empty);
            
            styleHelper.Bicolor = new Bicolor(Color.Black);
            styleHelper.Font = new Font("Arial", defaultFontSize, FontStyle.Bold);

            if (stylePreset != null)
            {
                style = stylePreset.Clone();
                BindStyle();
            }
            
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            editing = false;

            fontText = styleHelper.GetFontDefaultSize(defaultFontSize);

            textBox = new TextBox() {
                Visible = false,
                BackColor = Color.White, 
                BorderStyle = BorderStyle.None,
                Multiline = true,
                Text = text,
                Font = fontText
            };

            textBox.Margin = new Padding(0, 0, 0, 0);
            textBox.TextAlign = HorizontalAlignment.Left;
            textBox.WordWrap = false;

            textBox.TextChanged += TextBox_TextChanged;
            UpdateLabelRectangle();
        }
        public DrawingText(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0, ToolManager.GetStylePreset("Label"))
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacityFactor <= 0)
                return;

            int backgroundOpacity = editing ? 255 : 192;
            using (SolidBrush brushBack = styleHelper.GetBackgroundBrush((int)(opacityFactor * backgroundOpacity)))
            using (SolidBrush brushText = styleHelper.GetForegroundBrush((int)(opacityFactor * 255)))
            using (Font fontText = styleHelper.GetFont((float)transformer.Scale))
            {
                SizeF textSize = canvas.MeasureString(text, fontText);
                Point bgLocation = transformer.Transform(background.Rectangle.Location);
                Size bgSize = new Size((int)textSize.Width, (int)textSize.Height);

                SizeF untransformed = transformer.Untransform(textSize);
                background.Rectangle = new RectangleF(background.Rectangle.Location, untransformed);
                
                Rectangle rect = new Rectangle(bgLocation, bgSize);
                int roundingRadius = fontText.Height / 4;
                RoundedRectangle.Draw(canvas, rect, brushBack, roundingRadius, false, false, null);
                
                if (!editing)
                    canvas.DrawString(text, fontText, brushText, rect.Location);
            }
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity <= 0)
                return -1;
            
            // Compute the size of the hidden handle zone based on the font size.
            using (Font fontText = styleHelper.GetFont(1.0f))
            {
                int roundingRadius = fontText.Height / 4;
                result = background.HitTest(point, true, (int)(roundingRadius * 1.8f), transformer);
            }
            
            return result;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            // Invisible handler to change font size.
            int targetHeight = (int)(point.Y - background.Rectangle.Location.Y);
            StyleElementFontSize elem = style.Elements["font size"] as StyleElementFontSize;
            elem.ForceSize(targetHeight, text, styleHelper.Font);
            UpdateLabelRectangle();
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            background.Move(dx, dy);
            RelocateEditbox();
        }
        public override PointF GetPosition()
        {
            return background.Rectangle.Center();
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
                    case "Text":
                        text = xmlReader.ReadElementContentAsString();
                        text = TextHelper.FixMissingCarriageReturns(text);
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
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("Text", text);
                w.WriteElementString("Position", XmlHelper.WritePointF(background.Rectangle.Location));
            }

            if (ShouldSerializeStyle(filter))
            {
                w.WriteStartElement("DrawingStyle");
                style.WriteXml(w);
                w.WriteEndElement();
            }

            if (ShouldSerializeFading(filter))
            {
                w.WriteStartElement("InfosFading");
                infosFading.WriteXml(w);
                w.WriteEndElement();
            }
        }
        #endregion
        
        public void InitializeText()
        {
            if (string.IsNullOrEmpty(text))
            {
                text = name;
                UpdateLabelRectangle();
            }
        }
        public void SetEditMode(bool editing, IImageToViewportTransformer transformer)
        {
            this.editing = editing;

            if(imageToViewportTransformer == null)
               imageToViewportTransformer = transformer;

            if (editing)
            {
                RelocateEditbox(); // This is needed because the container top-left corner may have changed 
                textBox.BackColor = styleHelper.Bicolor.Background;
                textBox.ForeColor = styleHelper.Bicolor.Foreground;

                fontText = styleHelper.GetFont((float)transformer.Scale);
                textBox.Font.Dispose();
                textBox.Font = new Font(fontText.Name, fontText.Size, fontText.Style);
                textBox.Text = text;

                UpdateLabelRectangle();

                textBox.Visible = true;
                textBox.Select(0, 1);
                textBox.ScrollToCaret();
            }
            else
            {
                textBox.Visible = false;
            }
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
            if (string.IsNullOrEmpty(text))
                text = " ";

            UpdateLabelRectangle();
            InvalidateFromTextbox(sender);
        }
        private void UpdateLabelRectangle()
        {
            // Text changed or font size changed.
            using(Font f = styleHelper.GetFont(1F))
            {
                SizeF textSize = TextHelper.MeasureString(text, f);
                background.Rectangle = new RectangleF(background.Rectangle.Location, textSize);

                // Note that the edit box uses the stretched font size, taking into account zoom.
                // The character spacing isn't exactly the same as during drawing, and there is a weird
                // behavior with multiline strings.
                SizeF boxSize = TextHelper.MeasureString(text, fontText);
                textBox.Size = new Size((int)boxSize.Width, (int)boxSize.Height);
            }
        }
        #endregion
    }
}
