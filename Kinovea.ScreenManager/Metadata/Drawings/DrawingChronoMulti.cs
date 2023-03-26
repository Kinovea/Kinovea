#region License
/*
Copyright � Joan Charmant 2008-2009.
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
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A Chronometer tool with multiple time sections.
    /// 
    /// Note on overlap and unclosed sections:
    /// The way the menus are set up allows for overlapping sections and open-ended sections (no end point).
    /// We don't do any special treatment for these.
    /// The sections are always ordered based on their starting point.
    /// 
    /// The boundary frames are part of the sections.
    /// </summary>
    [XmlType ("ChronoMulti")]
    public class DrawingChronoMulti : AbstractDrawing, IDecorable, IKvaSerializable, ITimeable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get {  return "Advanced stopwatch"; }
        }
        public override int ContentHash
        {
            get
            {
                int iHash = visibleTimestamp.GetHashCode();
                iHash ^= invisibleTimestamp.GetHashCode();
                iHash ^= sections.GetHashCode();
                iHash ^= styleHelper.ContentHash;
                iHash ^= showLabel.GetHashCode();
                iHash ^= locked.GetHashCode();

                return iHash;
            }
        }
        public DrawingStyle DrawingStyle
        {
            get { return style;}
        }
        public Color Color
        {
            get { return styleHelper.GetBackgroundColor(255); }
        }
        public override InfosFading  InfosFading
        {
            // Fading is not modifiable from outside for chrono.
            // The chrono visibility uses its own mechanism.
            get { return null; }
            set { }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.CopyPaste; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get
            {
                // This drawing needs to know the current time to produce the right menus.
                throw new InvalidProgramException();
            }
        }
        public List<VideoSection> VideoSections
        {
            get { return sections; }
        }

        public List<string> SectionNames
        {
            get { return sectionNames; }
        }


        #endregion

        #region Members
        // Core

        private long visibleTimestamp;               	// chrono becomes visible.
        private long invisibleTimestamp;             	// chrono stops being visible.
        private List<VideoSection> sections = new List<VideoSection>(); 
        private long contextTimestamp;                  // timestamp for context-menu operations.
        private bool showLabel;
        private string text;
        private List<string> sectionNames = new List<string>();
        private bool locked;
        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private InfosFading infosFading;
        private static readonly int allowedFramesOver = 12;  // Number of frames the chrono stays visible after the 'Hiding' point.
        private RoundedRectangle mainBackground = new RoundedRectangle();
        private RoundedRectangle lblBackground = new RoundedRectangle();
        private int backgroundOpacity = 225;

        #region Menu
        private ToolStripMenuItem mnuVisibility = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHideBefore = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowBefore = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHideAfter = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowAfter = new ToolStripMenuItem();
        
        private ToolStripMenuItem mnuAction = new ToolStripMenuItem();
        private ToolStripMenuItem mnuStart = new ToolStripMenuItem();
        private ToolStripMenuItem mnuStop = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSplit = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRenameSections = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMoveCurrentStart = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMoveCurrentEnd = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMovePreviousEnd = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMoveNextStart = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMovePreviousSplit = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMoveNextSplit = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteSection = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteTimes = new ToolStripMenuItem();
        
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowLabel = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLocked = new ToolStripMenuItem();
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingChronoMulti(PointF p, long start, long averageTimeStampsPerFrame, DrawingStyle preset = null)
        {
            // Core
            visibleTimestamp = 0;
            invisibleTimestamp = long.MaxValue;
            mainBackground.Rectangle = new RectangleF(p, SizeF.Empty);
            lblBackground.Rectangle = RectangleF.Empty;

            text = "error";

            styleHelper.Bicolor = new Bicolor(Color.Black);
            styleHelper.Font = new Font("Arial", 16, FontStyle.Bold);
            styleHelper.Clock = false;
            if (preset == null)
                preset = ToolManager.GetStylePreset("ChronoMulti");

            style = preset.Clone();
            BindStyle();

            // We use the InfosFading utility to fade the chrono away.
            // The refererence frame will be the frame at which fading start.
            // Must be updated on "Hide" menu.
            infosFading = new InfosFading(invisibleTimestamp, averageTimeStampsPerFrame);
            infosFading.FadingFrames = allowedFramesOver;
            infosFading.UseDefault = false;

            InitializeMenus();
        }

        public DrawingChronoMulti(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata metadata)
            : this(PointF.Empty, 0, 1, null)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }

        private void InitializeMenus()
        {
            // Visibility menus.
            mnuShowBefore.Image = Properties.Drawings.showbefore;
            mnuShowAfter.Image = Properties.Drawings.showafter;
            mnuHideBefore.Image = Properties.Drawings.hidebefore;
            mnuHideAfter.Image = Properties.Drawings.hideafter;
            mnuShowBefore.Click += MnuShowBefore_Click;
            mnuShowAfter.Click += MnuShowAfter_Click;
            mnuHideBefore.Click += MnuHideBefore_Click;
            mnuHideAfter.Click += MnuHideAfter_Click;
            mnuVisibility.Image = Properties.Drawings.persistence;
            mnuVisibility.DropDownItems.AddRange(new ToolStripItem[] { 
                mnuShowBefore, 
                mnuShowAfter, 
                new ToolStripSeparator(), 
                mnuHideBefore, 
                mnuHideAfter });

            // Action
            mnuAction.Image = Properties.Resources.action;
            mnuStart.Image = Properties.Drawings.chronostart;
            mnuStop.Image = Properties.Drawings.chronostop;
            mnuSplit.Image = Properties.Drawings.chrono_split;
            mnuRenameSections.Image = Properties.Resources.rename;
            mnuMoveCurrentStart.Image = Properties.Resources.chronosectionstart;
            mnuMoveCurrentEnd.Image = Properties.Resources.chronosectionend;
            mnuMovePreviousEnd.Image = Properties.Resources.chronosectionend;
            mnuMoveNextStart.Image = Properties.Resources.chronosectionstart;
            mnuMovePreviousSplit.Image = Properties.Resources.chronosectionstart;
            mnuMoveNextSplit.Image = Properties.Resources.chronosectionend;
            mnuDeleteSection.Image = Properties.Resources.bin_empty;
            mnuDeleteTimes.Image = Properties.Resources.bin_empty;
            mnuStart.Click += mnuStart_Click;
            mnuStop.Click += mnuStop_Click;
            mnuSplit.Click += mnuSplit_Click;
            mnuRenameSections.Click += mnuRenameSections_Click;
            mnuMoveCurrentStart.Click += mnuMoveCurrentStart_Click;
            mnuMoveCurrentEnd.Click += mnuMoveCurrentEnd_Click;
            mnuMovePreviousEnd.Click += mnuMovePreviousEnd_Click;
            mnuMoveNextStart.Click += mnuMoveNextStart_Click;
            mnuMovePreviousSplit.Click += mnuMovePreviousSplit_Click;
            mnuMoveNextSplit.Click += mnuMoveNextSplit_Click;
            mnuDeleteSection.Click += mnuDeleteSection_Click;
            mnuDeleteTimes.Click += mnuDeleteTimes_Click;


            // Options
            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowLabel.Image = Properties.Drawings.label;
            mnuLocked.Image = Properties.Drawings.padlock2;
            mnuShowLabel.Click += mnuShowLabel_Click;
            mnuLocked.Click += mnuLock_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowLabel,
                mnuLocked,
            });

        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            if (currentTimestamp < visibleTimestamp)
                return;

            double opacityFactor = 1.0;
            if (currentTimestamp > invisibleTimestamp)
                opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);

            if (opacityFactor <= 0)
                return;

            bool onFirst = sections.Count == 1 || (sections.Count > 1 && currentTimestamp < sections[1].Start);
            List<List<string>> entries = GetTimecodes(currentTimestamp);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < entries.Count; i++)
            {   
                // If we are before or on the first section, only show the time.
                if (entries[i][0] == null || onFirst)
                {
                    sb.AppendLine(string.Format("{0}", entries[i][1]));
                }
                else
                {
                    string line = string.Format("{0}: {1} | {2}", entries[i][0], entries[i][1], entries[i][2]);
                    if (sections[i].Contains(currentTimestamp))
                        line += " ◀";

                    sb.AppendLine(line);
                }
            }
            
            text = sb.ToString();

            using (SolidBrush brushBack = styleHelper.GetBackgroundBrush((int)(opacityFactor * backgroundOpacity)))
            using (SolidBrush brushText = styleHelper.GetForegroundBrush((int)(opacityFactor * 255)))
            using (Font fontText = styleHelper.GetFont((float)transformer.Scale))
            {
                SizeF textSize = canvas.MeasureString(text, fontText);

                Point bgLocation = transformer.Transform(mainBackground.Rectangle.Location);
                Size bgSize = new Size((int)textSize.Width, (int)textSize.Height);

                SizeF untransformed = transformer.Untransform(textSize);

                // Backup the main rectangle for hit-testing.
                mainBackground.Rectangle = new RectangleF(mainBackground.Rectangle.Location, untransformed);

                // Background rounded rectangle.
                Rectangle rect = new Rectangle(bgLocation, bgSize);
                int roundingRadius = fontText.Height / 4;
                RoundedRectangle.Draw(canvas, rect, brushBack, roundingRadius, false, false, null);

                // Main text.
                canvas.DrawString(text, fontText, brushText, rect.Location);

                // Drawing name.
                if (showLabel && name.Length > 0)
                {
                    using (Font fontLabel = styleHelper.GetFont((float)transformer.Scale * 0.5f))
                    {
                        // Note: the alignment here assumes fixed margins of the rounded rectangle class.

                        string text = (locked ? "■ " : "") + name;
                        SizeF lblTextSize = canvas.MeasureString(text, fontLabel);
                        int labelRoundingRadius = fontLabel.Height / 3;
                        int top = rect.Location.Y - (int)lblTextSize.Height - roundingRadius - labelRoundingRadius;
                        Rectangle lblRect = new Rectangle(rect.Location.X, top, (int)lblTextSize.Width, (int)lblTextSize.Height);
                        
                        RoundedRectangle.Draw(canvas, lblRect, brushBack, labelRoundingRadius, true, false, null);
                        canvas.DrawString(text, fontLabel, brushText, lblRect.Location);

                        // Update the rectangle for hit testing.
                        lblBackground.Rectangle = transformer.Untransform(lblRect);
                    }
                }
            }
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            long maxHitTimeStamps = invisibleTimestamp;
            if (maxHitTimeStamps != long.MaxValue)
                maxHitTimeStamps += (allowedFramesOver * parentMetadata.AverageTimeStampsPerFrame);

            if (currentTimestamp >= visibleTimestamp && currentTimestamp <= maxHitTimeStamps)
            {
                using (Font fontText = styleHelper.GetFont(1.0f))
                {
                    int roundingRadius = fontText.Height / 4;
                    result = mainBackground.HitTest(point, true, (int)(roundingRadius * 1.8f), transformer);
                }

                if(result < 0)
                    result = lblBackground.HitTest(point, false, 0, transformer);
            }

            return result;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            // Invisible handler to change font size.
            int targetHeight = (int)(point.Y - mainBackground.Rectangle.Location.Y);
            StyleElementFontSize elem = style.Elements["font size"] as StyleElementFontSize;
            elem.ForceSize(targetHeight, text.TrimEnd(), styleHelper.Font);
            UpdateLabelRectangle();
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            mainBackground.Move(dx, dy);
            lblBackground.Move(dx, dy);
        }
        public override PointF GetCopyPoint()
        {
            return mainBackground.Rectangle.Center();
        }
        #endregion

        #region KVA Serialization
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("Position", XmlHelper.WritePointF(mainBackground.Rectangle.Location));

                w.WriteStartElement("Values");

                w.WriteElementString("Visible", (visibleTimestamp == long.MaxValue) ? "-1" : visibleTimestamp.ToString());
                w.WriteElementString("Invisible", (invisibleTimestamp == long.MaxValue) ? "-1" : invisibleTimestamp.ToString());
                
                if (sections.Count > 0)
                {
                    w.WriteStartElement("Sections");

                    for (int i = 0; i < sections.Count; i++)
                    {
                        VideoSection section = sections[i];
                        string name = sectionNames[i];
                        w.WriteStartElement("Section");
                        if (!string.IsNullOrEmpty(name))
                            w.WriteAttributeString("name", name);
                        w.WriteString(XmlHelper.WriteVideoSection(section));
                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                }

                w.WriteElementString("Locked", locked.ToString().ToLower());
                
                // </values>
                w.WriteEndElement();
            }

            if (ShouldSerializeStyle(filter))
            {
                // Label
                w.WriteStartElement("Label");
                w.WriteElementString("Show", showLabel.ToString().ToLower());
                w.WriteEndElement();

                w.WriteStartElement("DrawingStyle");
                style.WriteXml(w);
                w.WriteEndElement();
            }
        }

        public List<MeasuredDataTime> CollectMeasuredData()
        {
            List<MeasuredDataTime> mdtList = new List<MeasuredDataTime>();

            long cumulTimestamps = 0;
            for (int i = 0; i < sections.Count; i++)
            {
                if (sections[i].End == long.MaxValue)
                    continue;

                MeasuredDataTime mdt = new MeasuredDataTime();
                
                string sectionName = string.IsNullOrEmpty(sectionNames[i]) ? (i + 1).ToString() : sectionNames[i];
                mdt.Name = string.Format("{0} > {1}", this.Name, sectionName);
                var section = sections[i];
                mdt.Start = parentMetadata.GetNumericalTime(section.Start, TimeType.UserOrigin);
                mdt.Stop = parentMetadata.GetNumericalTime(section.End, TimeType.UserOrigin);

                long elapsedTimestamps = section.End - section.Start;
                cumulTimestamps += elapsedTimestamps;

                mdt.Duration = parentMetadata.GetNumericalTime(elapsedTimestamps, TimeType.Absolute);
                mdt.Cumul = parentMetadata.GetNumericalTime(cumulTimestamps, TimeType.Absolute);

                mdtList.Add(mdt);
            }

            return mdtList;
        }


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
                    case "Position":
                        PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        mainBackground.Rectangle = new RectangleF(p.Scale(scale.X, scale.Y), SizeF.Empty);
                        break;
                    case "Values":
                        ParseWorkingValues(xmlReader, timestampMapper);
                        break;
                    case "DrawingStyle":
                        style = new DrawingStyle(xmlReader);
                        BindStyle();
                        break;
                    case "Label":
                        ParseLabel(xmlReader);
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            xmlReader.ReadEndElement();
            SanityCheckValues();
        }
        private void ParseWorkingValues(XmlReader xmlReader, TimestampMapper timestampMapper)
        {
            if(timestampMapper == null)
            {
                xmlReader.ReadOuterXml();
                return;
            }

            xmlReader.ReadStartElement();

            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "Visible":
                        visibleTimestamp = timestampMapper(xmlReader.ReadElementContentAsLong());
                        break;
                    case "Invisible":
                        long hide = xmlReader.ReadElementContentAsLong();
                        invisibleTimestamp = (hide == -1) ? long.MaxValue : timestampMapper(hide);
                        break;
                    case "Sections":
                        ParseSections(xmlReader, timestampMapper);
                        break;
                    case "Locked":
                        locked = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            xmlReader.ReadEndElement();
        }

        private void ParseSections(XmlReader xmlReader, TimestampMapper timestampMapper)
        {
            sections.Clear();
            sectionNames.Clear();

            if (timestampMapper == null)
            {
                xmlReader.ReadOuterXml();
                return;
            }

            xmlReader.ReadStartElement();

            while (xmlReader.NodeType == XmlNodeType.Element)
            {
                switch (xmlReader.Name)
                {
                    case "Section":
                        
                        string sectionName = "";
                        if (xmlReader.MoveToAttribute("name"))
                            sectionName = xmlReader.ReadContentAsString();

                        xmlReader.ReadStartElement();
                        
                        VideoSection section = XmlHelper.ParseVideoSection(xmlReader.ReadContentAsString());
                        section = new VideoSection(timestampMapper(section.Start), timestampMapper(section.End));
                        InsertSection(section, sectionName);

                        xmlReader.ReadEndElement();
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            xmlReader.ReadEndElement();
        }
        private void SanityCheckValues()
        {
            visibleTimestamp = Math.Max(visibleTimestamp, 0);
            invisibleTimestamp = Math.Max(invisibleTimestamp, 0);
        }
        private void ParseLabel(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement();

            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "Show":
                        showLabel = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
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

        #region Tool-specific context menu

        /// <summary>
        /// Get the context menu according to the current time and locale.
        /// </summary>
        public List<ToolStripItem> GetContextMenu(long timestamp)
        {
            List<ToolStripItem> contextMenu = new List<ToolStripItem>();
            ReloadMenusCulture();

            // Backup the time globally for use in the event handlers callbacks.
            contextTimestamp = timestamp;

            // The context menu depends on whether we are on a live or dead section.
            mnuAction.DropDownItems.Clear();
            int sectionIndex = GetSectionIndex(contextTimestamp);

            if (sectionIndex >= 0)
            {
                // Live section.
                // If the next event is a split point we move the split as a whole (current end and next start).
                // Another option would be to allow moving the next start when we are on the boundary frame, this requires two actions to 
                // move a split but it is the lowest level and allow detaching the end points making up the split.
                //
                // Rationale: the most common scenario for adjusting existing end points will be to respect the "type" (split vs disconnected).
                // If the user really wants to disconnect a split they can always delete one of the sections and redo.
                bool isPrevSplit = sectionIndex > 0 && sections[sectionIndex - 1].End == sections[sectionIndex].Start;
                bool isNextSplit = sectionIndex < sections.Count - 1 && sections[sectionIndex + 1].Start == sections[sectionIndex].End;

                mnuAction.DropDownItems.AddRange(new ToolStripItem[] {
                    mnuStop,
                    mnuSplit,
                    new ToolStripSeparator(),
                    isPrevSplit ? mnuMovePreviousSplit : mnuMoveCurrentStart,
                    isNextSplit ? mnuMoveNextSplit : mnuMoveCurrentEnd,
                    new ToolStripSeparator(),
                    mnuRenameSections,
                    mnuDeleteSection,
                    mnuDeleteTimes,
                });
            }
            else
            {
                // Dead section.
                // If we are on a dead section we already know the previous and next events aren't split points.
                mnuAction.DropDownItems.AddRange(new ToolStripItem[] {
                    mnuStart,
                    new ToolStripSeparator(),
                    mnuMovePreviousEnd,
                    mnuMoveNextStart,
                    new ToolStripSeparator(),
                    mnuRenameSections,
                    mnuDeleteTimes,
                });
            }

            // Corner-case dead sections.
            mnuMovePreviousEnd.Enabled = !IsBeforeFirstSection(sectionIndex);
            mnuMoveNextStart.Enabled = !IsAfterLastSection(sectionIndex);
            mnuRenameSections.Enabled = sections.Count > 0;
            mnuDeleteTimes.Enabled = sections.Count > 0;

            mnuShowLabel.Checked = showLabel;
            mnuLocked.Checked = locked;

            contextMenu.AddRange(new ToolStripItem[] {
                mnuVisibility,
                mnuAction,
                mnuOptions,
            });

            return contextMenu;
        }

        private void ReloadMenusCulture()
        {
            // Visibility
            mnuVisibility.Text = ScreenManagerLang.Generic_Visibility;
            mnuHideBefore.Text = ScreenManagerLang.mnuHideBefore;
            mnuShowBefore.Text = ScreenManagerLang.mnuShowBefore;
            mnuHideAfter.Text = ScreenManagerLang.mnuHideAfter;
            mnuShowAfter.Text = ScreenManagerLang.mnuShowAfter;

            // Action
            mnuAction.Text = "Action";

            // When we are on a live section.
            mnuStop.Text = "Stop: end the current time section on this frame";
            mnuSplit.Text = "Split: end the current time section on this frame and start a new one";
            mnuRenameSections.Text = "Rename time sections";
            mnuMoveCurrentStart.Text = "Move the start of the current time section to this frame";
            mnuMoveCurrentEnd.Text = "Move the end of the current time section to this frame";
            mnuMovePreviousSplit.Text = "Move the previous split point to this frame";
            mnuMoveNextSplit.Text = "Move the next split point to this frame";
            mnuDeleteSection.Text = "Delete the current time section";

            // When we are on a dead section.
            mnuStart.Text = "Start a new time section on this frame";
            mnuMovePreviousEnd.Text = "Move the end of the previous section to this frame";
            mnuMoveNextStart.Text = "Move the start of the next section to this frame";
            mnuDeleteTimes.Text = "Delete all times";

            // Options.
            mnuOptions.Text = "Options";
            mnuShowLabel.Text = ScreenManagerLang.mnuShowLabel;
            mnuLocked.Text = "Locked";
        }

        #region Visibility
        private void MnuShowBefore_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            visibleTimestamp = 0;
            InvalidateFromMenu(sender);
        }

        private void MnuShowAfter_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            invisibleTimestamp = long.MaxValue;
            infosFading.ReferenceTimestamp = invisibleTimestamp;
            InvalidateFromMenu(sender);
        }

        private void MnuHideBefore_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            visibleTimestamp = CurrentTimestampFromMenu(sender);
            InvalidateFromMenu(sender);
        }

        private void MnuHideAfter_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            invisibleTimestamp = CurrentTimestampFromMenu(sender);
            infosFading.ReferenceTimestamp = invisibleTimestamp;
            InvalidateFromMenu(sender);
        }
        #endregion

        private void mnuStart_Click(object sender, EventArgs e)
        {
            // Start a new section here.
            CaptureMemento(SerializationFilter.Core);
            
            InsertSection(new VideoSection(contextTimestamp, long.MaxValue));
            
            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuStop_Click(object sender, EventArgs e)
        {
            // Stop the current section here.
            int sectionIndex = GetSectionIndex(contextTimestamp);
            if (sectionIndex < 0)
                return;

            CaptureMemento(SerializationFilter.Core);

            StopSection(sectionIndex, contextTimestamp);

            if (contextTimestamp > invisibleTimestamp)
                invisibleTimestamp = contextTimestamp;

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuSplit_Click(object sender, EventArgs e)
        {
            // Stop the current section here and start a new one.
            int sectionIndex = GetSectionIndex(contextTimestamp);
            if (sectionIndex < 0)
                return;

            CaptureMemento(SerializationFilter.Core);
            
            StopSection(sectionIndex, contextTimestamp);
            InsertSection(new VideoSection(contextTimestamp, long.MaxValue));

            if (contextTimestamp > invisibleTimestamp)
                invisibleTimestamp = contextTimestamp;

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }


        private void mnuRenameSections_Click(object sender, EventArgs e)
        {
            // The dialog is responsible for backing up and restoring the state in case of cancellation.
            // When we exit the dialog the drawing has been modified or reverted to its original state,
            // or the original state pushed to the history stack in case of validation.
            if (sections.Count == 0)
                return;

            int sectionIndex = GetSectionIndex(contextTimestamp);

            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            IDrawingHostView host = tsmi.Tag as IDrawingHostView;
            
            FormTimeSections fts = new FormTimeSections(this, sectionIndex, host);
            FormsHelper.Locate(fts);
            fts.ShowDialog();

            InvalidateFromMenu(sender);
        }

        private void mnuMoveCurrentStart_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(contextTimestamp);
            if (sectionIndex < 0)
                return;

            CaptureMemento(SerializationFilter.Core);

            sections[sectionIndex] = new VideoSection(contextTimestamp, sections[sectionIndex].End);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuMoveCurrentEnd_Click(object sender, EventArgs e)
        {
            // Technically "Move current end" is the same as "Stop", but we keep it for symmetry purposes.
            int sectionIndex = GetSectionIndex(contextTimestamp);
            if (sectionIndex < 0)
                return;

            CaptureMemento(SerializationFilter.Core);

            sections[sectionIndex] = new VideoSection(sections[sectionIndex].Start, contextTimestamp);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuMovePreviousSplit_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(contextTimestamp);
            if (sectionIndex < 1)
                return;

            CaptureMemento(SerializationFilter.Core);

            int prevIndex = sectionIndex - 1;
            sections[prevIndex] = new VideoSection(sections[prevIndex].Start, contextTimestamp);
            sections[sectionIndex] = new VideoSection(contextTimestamp, sections[sectionIndex].End);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuMoveNextSplit_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(contextTimestamp);
            if (sectionIndex < 0 || sectionIndex > sections.Count - 2)
                return;

            CaptureMemento(SerializationFilter.Core);

            int nextIndex = sectionIndex + 1;
            sections[sectionIndex] = new VideoSection(sections[sectionIndex].Start, contextTimestamp);
            sections[nextIndex] = new VideoSection(contextTimestamp, sections[nextIndex].End);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuMovePreviousEnd_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(contextTimestamp);
            if (sectionIndex >= 0 || IsBeforeFirstSection(sectionIndex))
                return;

            CaptureMemento(SerializationFilter.Core);

            int prevIndex = -(sectionIndex + 2);
            sections[prevIndex] = new VideoSection(sections[prevIndex].Start, contextTimestamp);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuMoveNextStart_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(contextTimestamp);
            if (sectionIndex >= 0 || IsAfterLastSection(sectionIndex))
                return;

            CaptureMemento(SerializationFilter.Core);

            int nextIndex = -(sectionIndex + 1);
            sections[nextIndex] = new VideoSection(contextTimestamp, sections[nextIndex].End);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuDeleteSection_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(contextTimestamp);
            if (sectionIndex < 0)
                return;

            CaptureMemento(SerializationFilter.Core);

            RemoveSection(sectionIndex);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuDeleteTimes_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);

            sections.Clear();
            sectionNames.Clear();

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuShowLabel_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Style);
            showLabel = !mnuShowLabel.Checked;
            InvalidateFromMenu(sender);
        }
        
        private void mnuLock_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            locked = !mnuLocked.Checked;
            InvalidateFromMenu(sender);
        }
        #endregion

        #region ITimeable
        public void StartStop(long timestamp)
        {
            if (locked || timestamp < visibleTimestamp || timestamp > invisibleTimestamp)
                return;

            // The commands are mapped to the start/stop menus, not the move menus.
            // That is, we only support the creation of a new section or setting the end of the last section.
            // No overwrite of existing data.
            // Overwriting existing data is always ambiguous with a combo start/stop command.

            // Determine if we are on a live or dead section.
            int sectionIndex = GetSectionIndex(timestamp);
            if (sectionIndex < 0)
            {
                // Dead section.
                if (IsAfterLastSection(sectionIndex))
                {
                    // Create a new section.
                    CaptureMemento(SerializationFilter.Core);
                    InsertSection(new VideoSection(timestamp, long.MaxValue));
                }
                else
                {
                    // There is already another section in the future.
                    return;
                }
            }
            else
            {
                // Live section.
                if (sections[sectionIndex].End == long.MaxValue)
                {
                    // Open-ended section.
                    CaptureMemento(SerializationFilter.Core);
                    StopSection(sectionIndex, timestamp);
                }
                else
                {
                    // This section already has an ending.
                    return;
                }
            }

        }

        public void Split(long timestamp)
        {
            if (locked || timestamp < visibleTimestamp || timestamp > invisibleTimestamp)
                return;

            // Determine if we are on a live or dead section.
            int sectionIndex = GetSectionIndex(timestamp);
            if (sectionIndex < 0)
                return;

            // Live section.
            if (sections[sectionIndex].End == long.MaxValue)
            {
                // Open-ended section.
                CaptureMemento(SerializationFilter.Core);
                StopSection(sectionIndex, timestamp);
                InsertSection(new VideoSection(timestamp, long.MaxValue));
            }
        }

        #endregion

        #region Lower level helpers
        private void BindStyle()
        {
            DrawingStyle.SanityCheck(style, ToolManager.GetStylePreset("ChronoMulti"));
            style.Bind(styleHelper, "Bicolor", "color");
            style.Bind(styleHelper, "Font", "font size");
        }
        private void UpdateLabelRectangle()
        {
            using (Font f = styleHelper.GetFont(0.5f))
            {
                SizeF size = TextHelper.MeasureString(name, f);
                lblBackground.Rectangle = new RectangleF(
                    mainBackground.X, 
                    mainBackground.Y - lblBackground.Rectangle.Height, 
                    size.Width + 11, 
                    size.Height);
            }
        }

        /// <summary>
        /// Insert a new section into the list.
        /// </summary>
        private void InsertSection(VideoSection section, string name = "")
        {
            // Find insertion point and insert the new section there.
            bool found = false;
            int i = 0;
            for (i = 0; i < sections.Count; i++)
            {
                if (sections[i].Start < section.Start)
                    continue;

                found = true;
                break;
            }

            if (!found)
            {
                sections.Add(section);
                sectionNames.Add(name);
            }
            else
            {
                sections.Insert(i, section);
                sectionNames.Insert(i, name);
            }
        }

        private void RemoveSection(int index)
        {
            sections.RemoveAt(index);
            sectionNames.RemoveAt(index);
        }

        /// <summary>
        /// Update the end time of a specific section.
        /// </summary>
        private void StopSection(int index, long timestamp)
        {
            sections[index] = new VideoSection(sections[index].Start, timestamp);
        }

        /// <summary>
        /// Returns the section index that timestamp is in. 
        /// Otherwise returns a negative number based on the next section:
        /// -1 if we are before the first live zone, 
        /// -2 if we are after the first and before the second, 
        /// -n if we are before the n-th section.
        /// -(n+1) if we are after the last section.
        /// 
        /// In case of overlapping sections, returns the section with the earliest starting point.
        /// An open-ended section contains all the timestamps after its start.
        /// </summary>
        private int GetSectionIndex(long timestamp)
        {
            int result = -1;
            for (int i = 0; i < sections.Count; i++)
            {
                // Before the start of this section. 
                if (timestamp < sections[i].Start)
                    break;

                // Between start and end of this section.
                // The end of the section is part of the section.
                if (timestamp <= sections[i].End)
                {
                    result = i;
                    break;
                }

                // After that section.
                result--;
            }

            return result;
        }

        /// <summary>
        /// Returns true if this dead-zone index is before the first section.
        /// </summary>
        private bool IsBeforeFirstSection(int index)
        {
            return index == -1;
        }

        /// <summary>
        /// Returns true if this dead-zone index is after the last section.
        /// </summary>
        private bool IsAfterLastSection(int index)
        {
            return index == -(sections.Count + 1);
        }

        /// <summary>
        /// Returns all the sections text, based on the current timestamp.
        /// Each entry in the returned list contains the section name, elapsed time and cumulative time.
        /// - The elapsed time is the time since the start of the section.
        /// - The cumulative time is the total live time since the start of the first section.
        /// - The cumulative time of each section stops when the section ends.
        /// 
        /// The produced text is always contextualized to the current time.
        /// Sections that are not yet started are not returned (exception when before first).
        /// No special treatment for open-ended sections.
        /// Overlapping sections are counted twice.
        /// </summary>
        private List<List<string>> GetTimecodes(long currentTimestamp)
        {
            List<List<string>> entries = new List<List<string>>();
            
            int sectionIndex = GetSectionIndex(currentTimestamp);
            if (IsBeforeFirstSection(sectionIndex))
            {
                string elapsed = parentMetadata.TimeCodeBuilder(0, TimeType.Absolute, TimecodeFormat.Unknown, true);
                entries.Add(new List<string>() { null, elapsed, null });
                return entries;
            }

            long cumulTimestamps = 0;
            for (int i = 0; i < sections.Count; i++)
            {
                if (currentTimestamp < sections[i].Start)
                    break;

                string name = string.IsNullOrEmpty(sectionNames[i]) ? (i + 1).ToString() : sectionNames[i];

                long elapsedTimestamps = 0;
                if (currentTimestamp <= sections[i].End)
                    elapsedTimestamps = currentTimestamp - sections[i].Start;
                else
                    elapsedTimestamps = sections[i].End - sections[i].Start;

                cumulTimestamps += elapsedTimestamps;

                string elapsed = parentMetadata.TimeCodeBuilder(elapsedTimestamps, TimeType.Absolute, TimecodeFormat.Unknown, true);
                string cumul = parentMetadata.TimeCodeBuilder(cumulTimestamps, TimeType.Absolute, TimecodeFormat.Unknown, true);

                entries.Add(new List<string>() { name, elapsed, cumul });
            }

            return entries;
        }

        /// <summary>
        /// Capture the current state and push it to the undo/redo stack.
        /// </summary>
        private void CaptureMemento(SerializationFilter filter)
        {
            var memento = new HistoryMementoModifyDrawing(parentMetadata, parentMetadata.ChronoManager.Id, this.Id, this.Name, filter);
            parentMetadata.HistoryStack.PushNewCommand(memento);
        }
        #endregion
    }
}
