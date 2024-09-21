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
                hash ^= showArrow.GetHashCode();
                hash ^= showCircle.GetHashCode();
                hash ^= hasBackground.GetHashCode();
                hash ^= arrowEnd.GetHashCode();
                hash ^= styleData.ContentHash;
                hash ^= infosFading.ContentHash;
                return hash;
            }
        } 
        public StyleElements StyleElements
        {
            get { return styleElements;}
        }
        public override InfosFading InfosFading
        {
            get { return infosFading; }
            set { infosFading = value; }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading | DrawingCapabilities.CopyPaste; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get
            {
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                ReloadMenusCulture();

                contextMenu.AddRange(new ToolStripItem[] {
                    mnuOptions,
                });

                mnuShowArrow.Checked = showArrow;
                mnuShowCircle.Checked = showCircle;
                mnuHasBackground.Checked = hasBackground;
                return contextMenu;
            }
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
        private PointF arrowEnd;
        private StyleElements styleElements = new StyleElements();
        private StyleData styleData = new StyleData();
        private InfosFading infosFading;
        private bool editing;
        private Font fontText;
        private IImageToViewportTransformer imageToViewportTransformer;

        #region Menus
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowArrow = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowCircle = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHasBackground = new ToolStripMenuItem();
        #endregion

        // Options
        private bool showArrow;
        private bool showCircle;
        private bool hasBackground;

        private RoundedRectangle background = new RoundedRectangle();
        private TextBox textBox;
        private Control host;
        
        private const int defaultFontSize = 14;    		// will also be used for the text box.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingText(PointF p, long timestamp, long averageTimeStampsPerFrame, StyleElements preset = null)
        {
            text = "";
            background.Rectangle = new RectangleF(p, SizeF.Empty);
            arrowEnd = p.Translate(-50, -50);
            showArrow = false;
            showCircle = false;
            hasBackground = true;

            SetupStyle(preset);
            
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            editing = false;

            fontText = styleData.GetFontDefaultSize(defaultFontSize);

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

            InitializeMenus();
        }
        public DrawingText(PointF p, long timestamp, long averageTimeStampsPerFrame, string text)
            : this(p, timestamp, averageTimeStampsPerFrame, ToolManager.GetDefaultStyleElements("Label"))
        {
            this.text = TextHelper.FixMissingCarriageReturns(text);
            UpdateLabelRectangle();
        }
        public DrawingText(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0, ToolManager.GetDefaultStyleElements("Label"))
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        private void InitializeMenus()
        {
            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowArrow.Image = Properties.Drawings.arrow;
            mnuShowCircle.Image = Properties.Drawings.circle;
            mnuHasBackground.Image = Properties.Drawings.filled;
            mnuShowArrow.Click += mnuShowArrow_Click;
            mnuShowCircle.Click += mnuShowCircle_Click;
            mnuHasBackground.Click += mnuHasBackground_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowArrow,
                mnuShowCircle,
                mnuHasBackground,
            });
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, CameraTransformer cameraTransformer, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity <= 0)
                return;

            //int backgroundOpacity = editing ? 255 : 192;
            int backgroundOpacity = 255;
            using (SolidBrush brushBack = styleData.GetBackgroundBrush((int)(opacity * backgroundOpacity)))
            using (SolidBrush brushText = styleData.GetForegroundBrush((int)(opacity * 255)))
            using (Font fontText = styleData.GetFont((float)transformer.Scale))
            {
                SizeF textSize = canvas.MeasureString(text, fontText);
                Point bgLocation = transformer.Transform(background.Rectangle.Location);
                Size bgSize = new Size((int)textSize.Width, (int)textSize.Height);

                SizeF untransformed = transformer.Untransform(textSize);
                background.Rectangle = new RectangleF(background.Rectangle.Location, untransformed);

                Rectangle rect = new Rectangle(bgLocation, bgSize);
                int roundingRadius = fontText.Height / 4;

                if (showArrow)
                {
                    PointF end = transformer.Transform(arrowEnd);
                    PointF start = GeometryHelper.IntersectionRectangleCenter(rect, end);
                    DrawArrow(canvas, transformer, backgroundOpacity, brushBack.Color, start, end);
                }
                else if (showCircle)
                {
                    PointF end = transformer.Transform(arrowEnd);
                    PointF start = GeometryHelper.IntersectionRectangleCenter(rect, end);
                    DrawCircle(canvas, transformer, backgroundOpacity, brushBack.Color, start, end);
                }

                if (editing)
                {
                    // Only draw the background. The text is provided by the textbox control.
                    RoundedRectangle.Draw(canvas, rect, brushBack, roundingRadius, false, false, null);
                }
                else if (hasBackground)
                {
                    RoundedRectangle.Draw(canvas, rect, brushBack, roundingRadius, false, false, null);
                    canvas.DrawString(text, fontText, brushText, rect.Location);
                }
                else
                {
                    canvas.DrawString(text, fontText, brushBack, rect.Location);
                }
            }
        }
        private void DrawArrow(Graphics canvas, IImageToViewportTransformer transformer, float opacity, Color color, PointF start, PointF end)
        {
            
            using (Pen pen = styleData.GetPen(opacity, transformer.Scale))
            {
                pen.Color = color;
                pen.StartCap = LineCap.Round;

                bool canDrawArrow = ArrowHelper.UpdateStartEnd(pen.Width, ref start, ref end, false, true);
                if (!canDrawArrow)
                    return;
                
                canvas.DrawLine(pen, start, end);
                ArrowHelper.Draw(canvas, pen, end, start);
            }
        }

        private void DrawCircle(Graphics canvas, IImageToViewportTransformer transformer, float opacity, Color color, PointF start, PointF end)
        {
            using (Pen pen = styleData.GetPen(opacity, transformer.Scale))
            {
                pen.Color = color;
                pen.StartCap = LineCap.Round;

                PointF center = end;
                bool canDrawArrow = ArrowHelper.UpdateStartEnd(pen.Width, ref start, ref end, false, true);
                if (!canDrawArrow)
                    return;

                canvas.DrawLine(pen, start, end);

                // The center of the circle is 3 pen width away from the end of the line.
                // This is based on the arrow drawing routine with the tip of the arrow at the original end point.
                canvas.DrawEllipse(pen, center.Box((int)pen.Width*3));
            }
        }

        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity <= 0)
                return -1;

            // Background label: 0, hidden resizer: 1, arrow end: 2.
            if (showArrow || showCircle)
            {
                // Is point on the disc around the arrow end.
                using (GraphicsPath areaPath = new GraphicsPath())
                {
                    // The circle radius is 3 line size.
                    areaPath.AddEllipse(arrowEnd.Box(styleData.LineSize*3));
                    if (HitTester.HitPath(point, areaPath, 0, true, transformer))
                        return 2;
                }

                if (IsPointOnSegment(point, background.Rectangle.Center(), arrowEnd, transformer))
                    return 0;
            }

            // Compute the size of the hidden handle zone based on the font size.
            using (Font fontText = styleData.GetFont(1.0f))
            {
                int roundingRadius = fontText.Height / 4;
                return background.HitTest(point, true, (int)(roundingRadius * 1.8f), transformer);
            }
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            if (handleNumber == 2)
            {
                arrowEnd = point;
            }
            else if (handleNumber == 1)
            {
                // Invisible handler to change font size.
                int targetHeight = (int)(point.Y - background.Rectangle.Location.Y);
                StyleElementFontSize elem = styleElements.Elements["font size"] as StyleElementFontSize;
                elem.ForceSize(targetHeight, text, styleData.Font);
                UpdateLabelRectangle();
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            background.Move(dx, dy);

            // The default behavior is to move the entire drawing as it is the standard
            // thing to do for most calls of MoveDrawing.
            // To allow for moving only the text label while keeping the arrow end, we listen to the CTRL key.
            if ((modifierKeys & Keys.Control) != Keys.Control)
                arrowEnd = arrowEnd.Translate(dx, dy);

            RelocateEditbox();
        }
        public override PointF GetCopyPoint()
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
                    case "BackgroundVisible":
                        hasBackground = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "ArrowVisible":
                        showArrow = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "CircleVisible":
                        showCircle = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "ArrowEnd":
                        arrowEnd = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        break;
                    case "DrawingStyle":
                        styleElements.ImportXML(xmlReader);
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
                w.WriteElementString("BackgroundVisible", hasBackground.ToString().ToLower());
                w.WriteElementString("ArrowVisible", showArrow.ToString().ToLower());
                w.WriteElementString("CircleVisible", showCircle.ToString().ToLower());
                w.WriteElementString("ArrowEnd", XmlHelper.WritePointF(arrowEnd));
            }

            if (ShouldSerializeStyle(filter))
            {
                w.WriteStartElement("DrawingStyle");
                styleElements.WriteXml(w);
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

        #region Context menu
        private void mnuShowArrow_Click(object sender, EventArgs e)
        {
            showArrow = !showArrow;
            if (showArrow)
                showCircle = false;
                
            InvalidateFromMenu(sender);
        }

        private void mnuShowCircle_Click(object sender, EventArgs e)
        {
            showCircle = !showCircle;
            if (showCircle)
                showArrow = false;

            InvalidateFromMenu(sender);
        }

        private void mnuHasBackground_Click(object sender, EventArgs e)
        {
            hasBackground = !hasBackground;
            InvalidateFromMenu(sender);
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
        public void SetEditMode(bool editing, PointF point, IImageToViewportTransformer transformer)
        {
            this.editing = editing;

            if(imageToViewportTransformer == null)
               imageToViewportTransformer = transformer;

            if (editing)
            {
                RelocateEditbox(); // This is needed because the container top-left corner may have changed 
                textBox.BackColor = styleData.GetBackgroundColor();
                textBox.ForeColor = styleData.GetForegroundColor();
                
                try
                {
                    Font oldFont = textBox.Font;

                    fontText = styleData.GetFont((float)transformer.Scale);
                    textBox.Font = fontText;
                    textBox.Text = text;

                    UpdateLabelRectangle();
                    
                    textBox.Visible = true;
                    textBox.Focus();

                    // If this is done earlier it causes an exception in System.Drawing.Font.ToLogFont().
                    oldFont.Dispose();

                    // It is hard to find the correct character because we don't know the location of the text box in image space. 
                    // We only have its location in the host panel.
                    // See index = textBox.GetCharIndexFromPosition(textBoxPoint);
                    textBox.Select(0, 0);
                    textBox.ScrollToCaret();
                }
                catch (Exception e)
                {
                    log.ErrorFormat(e.Message);
                }
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
        private void SetupStyle(StyleElements preset)
        {
            styleData.BackgroundColor = Color.Black;
            styleData.Font = new Font("Arial", defaultFontSize, FontStyle.Bold);
            styleData.LineSize = 2;
            if (preset == null)
                preset = ToolManager.GetDefaultStyleElements("Label");

            styleElements = preset.Clone();
            BindStyle();
        }
        private void BindStyle()
        {
            StyleElements.SanityCheck(styleElements, ToolManager.GetDefaultStyleElements("Label"));
            styleElements.Bind(styleData, "Bicolor", "back color");
            styleElements.Bind(styleData, "Font", "font size");
            styleElements.Bind(styleData, "LineSize", "LineSize");
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
            using(Font f = styleData.GetFont(1F))
            {
                SizeF textSize = TextHelper.MeasureString(text, f);
                background.Rectangle = new RectangleF(background.Rectangle.Location, textSize);

                // Note that the edit box uses the stretched font size, taking into account zoom.
                // The character spacing isn't exactly the same as during drawing, and there is a weird
                // behavior with multiline strings.
                SizeF boxSize = TextHelper.MeasureString(text, fontText);
                textBox.Size = new Size((int)boxSize.Width + 1, (int)boxSize.Height + 1);
            }
        }
        private bool IsPointOnSegment(PointF point, PointF a, PointF b, IImageToViewportTransformer transformer)
        {
            using (GraphicsPath areaPath = new GraphicsPath())
            {
                if (a.NearlyCoincideWith(b))
                    areaPath.AddLine(a.X, a.Y, a.X + 2, a.Y);
                else
                    areaPath.AddLine(a, b);

                return HitTester.HitPath(point, areaPath, styleData.LineSize, false, transformer);
            }
        }

        private void ReloadMenusCulture()
        {
            // Options
            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuShowArrow.Text = ScreenManagerLang.mnuShowArrow;
            mnuShowCircle.Text = "Show circle";
            mnuHasBackground.Text = ScreenManagerLang.DrawingText_Background;
        }
        #endregion
    }
}
