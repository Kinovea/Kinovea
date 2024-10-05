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
using System.Linq;
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
                iHash ^= styleData.ContentHash;
                iHash ^= showLabel.GetHashCode();
                iHash ^= locked.GetHashCode();

                return iHash;
            }
        }
        public StyleElements StyleElements
        {
            get { return style;}
        }
        public Color Color
        {
            get { return styleData.GetBackgroundColor(); }
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
        public List<ChronoSection> Sections
        {
            get { return sections; }
        }
        #endregion

        #region Members
        // Core

        private long visibleTimestamp;               	// chrono becomes visible.
        private long invisibleTimestamp;             	// chrono stops being visible.
        private List<ChronoSection> sections = new List<ChronoSection>(); 
        private long contextTimestamp;                  // timestamp for context-menu operations.
        private string text;
        private HashSet<ChronoColumns> visibleColumns = new HashSet<ChronoColumns>();

        // Options
        private bool showLabel;
        private bool locked;

        // Decoration
        private StyleData styleData = new StyleData();
        private StyleElements style;
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
        
        private ToolStripMenuItem mnuColumns = new ToolStripMenuItem();
        private ToolStripMenuItem mnuColumnName = new ToolStripMenuItem();
        private ToolStripMenuItem mnuColumnCumul = new ToolStripMenuItem();
        private ToolStripMenuItem mnuColumnTag = new ToolStripMenuItem();
        
        private ToolStripMenuItem mnuConfigureSections = new ToolStripMenuItem();
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingChronoMulti(PointF p, long start, long averageTimeStampsPerFrame, StyleElements preset = null)
        {
            // Core
            visibleTimestamp = 0;
            invisibleTimestamp = long.MaxValue;
            mainBackground.Rectangle = new RectangleF(p, SizeF.Empty);
            lblBackground.Rectangle = RectangleF.Empty;

            text = "error";

            styleData.BackgroundColor = Color.Black;
            styleData.Font = new Font("Consolas", 16, FontStyle.Bold);
            styleData.Clock = false;
            if (preset == null)
                preset = ToolManager.GetDefaultStyleElements("ChronoMulti");

            style = preset.Clone();
            BindStyle();

            // We use the InfosFading utility to fade the chrono away.
            // The refererence frame will be the frame at which fading start.
            // Must be updated on "Hide" menu.
            infosFading = new InfosFading(invisibleTimestamp, averageTimeStampsPerFrame);
            infosFading.FadingFrames = allowedFramesOver;
            infosFading.UseDefault = false;

            InitializeMenus();

            visibleColumns.Clear();
            visibleColumns.Add(ChronoColumns.Name);
            visibleColumns.Add(ChronoColumns.Duration);
            visibleColumns.Add(ChronoColumns.Cumul);
            visibleColumns.Add(ChronoColumns.Tag);
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

            // Column and section management
            mnuColumns.Image = Properties.Drawings.label;
            mnuColumnName.Click += (s, e) => mnuColumn_Click(s, ChronoColumns.Name);
            mnuColumnCumul.Click += (s, e) => mnuColumn_Click(s, ChronoColumns.Cumul);
            mnuColumnTag.Click += (s, e) => mnuColumn_Click(s, ChronoColumns.Tag);
            mnuColumns.DropDownItems.AddRange(new ToolStripItem[] {
                mnuColumnName,
                mnuColumnCumul,
                mnuColumnTag 
            });

            mnuConfigureSections.Image = Properties.Resources.timetable;
            mnuConfigureSections.Click += mnuConfigureSections_Click;
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, CameraTransformer camTransformer, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            if (currentTimestamp < visibleTimestamp)
                return;

            infosFading.MasterFactor = locked ? 0.75f : 1.0f;
            double opacityFactor = infosFading.MasterFactor;
            if (currentTimestamp > invisibleTimestamp)
                opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);

            if (opacityFactor <= 0)
                return;

            ChronoStringBuilder csb = new ChronoStringBuilder(sections, parentMetadata);
            text = csb.Build(currentTimestamp, visibleColumns);

            using (SolidBrush brushBack = styleData.GetBackgroundBrush((int)(opacityFactor * backgroundOpacity)))
            using (SolidBrush brushText = styleData.GetForegroundBrush((int)(opacityFactor * 255)))
            using (Font fontText = styleData.GetFont((float)transformer.Scale))
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
                    using (Font fontLabel = styleData.GetFont((float)transformer.Scale * 0.5f))
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
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            long maxHitTimeStamps = invisibleTimestamp;
            if (maxHitTimeStamps != long.MaxValue)
                maxHitTimeStamps += (allowedFramesOver * parentMetadata.AverageTimeStampsPerFrame);

            if (currentTimestamp >= visibleTimestamp && currentTimestamp <= maxHitTimeStamps)
            {
                using (Font fontText = styleData.GetFont(1.0f))
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
            elem.ForceSize(targetHeight, text.TrimEnd(), styleData.Font);
            UpdateLabelRectangle();
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys)
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
                        VideoSection section = sections[i].Section;
                        string name = sections[i].Name;
                        string tag = sections[i].Tag;
                        w.WriteStartElement("Section");
                        if (!string.IsNullOrEmpty(name))
                            w.WriteAttributeString("name", name);
                        if (!string.IsNullOrEmpty(tag))
                            w.WriteAttributeString("tag", tag);
                        w.WriteString(XmlHelper.WriteVideoSection(section));
                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                }

                w.WriteElementString("Locked", locked.ToString().ToLower());

                string strVisibleColumns = string.Join(";", visibleColumns.ToArray());
                w.WriteElementString("VisibleColumns", strVisibleColumns);

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

        public MeasuredDataTime CollectMeasuredData()
        {
            MeasuredDataTime mdt = new MeasuredDataTime();
            mdt.Name = this.Name;
            mdt.VisibleColumns = this.visibleColumns;

            long cumulTimestamps = 0;
            for (int i = 0; i < sections.Count; i++)
            {
                if (sections[i].Section.End == long.MaxValue)
                    continue;

                MeasuredDataTimeSection mdts = new MeasuredDataTimeSection();    
                mdts.Name = string.IsNullOrEmpty(sections[i].Name) ? (i + 1).ToString() : sections[i].Name;
                mdts.Tag = sections[i].Tag;

                if (!string.IsNullOrEmpty(mdts.Tag))
                    mdt.HasTags = true;

                var section = sections[i];
                mdts.Start = parentMetadata.GetNumericalTime(section.Section.Start, TimeType.UserOrigin);
                mdts.Stop = parentMetadata.GetNumericalTime(section.Section.End, TimeType.UserOrigin);

                long elapsedTimestamps = section.Section.End - section.Section.Start;
                cumulTimestamps += elapsedTimestamps;

                mdts.Duration = parentMetadata.GetNumericalTime(elapsedTimestamps, TimeType.Absolute);
                mdts.Cumul = parentMetadata.GetNumericalTime(cumulTimestamps, TimeType.Absolute);

                mdt.Sections.Add(mdts);
            }

            return mdt;
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
                        style.ImportXML(xmlReader);
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
                    case "VisibleColumns":
                        visibleColumns.Clear();
                        string str = xmlReader.ReadElementContentAsString();
                        string[] a = str.Split(new char[] { ';' });
                        foreach(var strCol in a)
                        {
                            ChronoColumns col;
                            bool parsed = Enum.TryParse<ChronoColumns>(strCol, out col);
                            if (parsed)
                                visibleColumns.Add(col);
                        }

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
                        string sectionTag = "";
                        if (xmlReader.MoveToAttribute("name"))
                            sectionName = xmlReader.ReadContentAsString();
                        
                        if (xmlReader.MoveToAttribute("tag"))
                            sectionTag = xmlReader.ReadContentAsString();

                        xmlReader.ReadStartElement();
                        
                        VideoSection section = XmlHelper.ParseVideoSection(xmlReader.ReadContentAsString());
                        section = new VideoSection(timestampMapper(section.Start), timestampMapper(section.End));
                        InsertSection(section, sectionName, sectionTag);

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
            int sectionIndex = GetSectionIndex(sections, contextTimestamp);

            if (sectionIndex >= 0)
            {
                // Live section.
                // If the next event is a split point we move the split as a whole (current end and next start).
                // Another option would be to allow moving the next start when we are on the boundary frame, this requires two actions to 
                // move a split but it is the lowest level and allow detaching the end points making up the split.
                //
                // Rationale: the most common scenario for adjusting existing end points will be to respect the "type" (split vs disconnected).
                // If the user really wants to disconnect a split they can always delete one of the sections and redo.
                bool isPrevSplit = sectionIndex > 0 && sections[sectionIndex - 1].Section.End == sections[sectionIndex].Section.Start;
                bool isNextSplit = sectionIndex < sections.Count - 1 && sections[sectionIndex + 1].Section.Start == sections[sectionIndex].Section.End;

                mnuAction.DropDownItems.AddRange(new ToolStripItem[] {
                    mnuStop,
                    mnuSplit,
                    new ToolStripSeparator(),
                    isPrevSplit ? mnuMovePreviousSplit : mnuMoveCurrentStart,
                    isNextSplit ? mnuMoveNextSplit : mnuMoveCurrentEnd,
                    new ToolStripSeparator(),
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
                    mnuDeleteTimes,
                });
            }

            // Corner-case dead sections.
            mnuMovePreviousEnd.Enabled = !IsBeforeFirstSection(sectionIndex);
            mnuMoveNextStart.Enabled = !IsAfterLastSection(sectionIndex);
            mnuConfigureSections.Enabled = sections.Count > 0;
            mnuDeleteTimes.Enabled = sections.Count > 0;

            // Options
            mnuShowLabel.Checked = showLabel;
            mnuLocked.Checked = locked;

            // Columns and section management
            mnuColumnName.Checked = visibleColumns.Contains(ChronoColumns.Name);
            mnuColumnCumul.Checked = visibleColumns.Contains(ChronoColumns.Cumul);
            mnuColumnTag.Checked = visibleColumns.Contains(ChronoColumns.Tag);

            contextMenu.AddRange(new ToolStripItem[] {
                mnuVisibility,
                mnuAction,
                mnuOptions,
                mnuColumns,
                mnuConfigureSections,
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
            mnuAction.Text = ScreenManagerLang.mnuAction;

            // When we are on a live section.
            mnuStop.Text = ScreenManagerLang.mnuAction_Chrono_Stop;
            mnuSplit.Text = ScreenManagerLang.mnuAction_Chrono_Split;
            mnuMoveCurrentStart.Text = ScreenManagerLang.mnuAction_Chrono_MoveCurrentStart;
            mnuMoveCurrentEnd.Text = ScreenManagerLang.mnuAction_Chrono_MoveCurrentEnd;
            mnuMovePreviousSplit.Text = ScreenManagerLang.mnuAction_Chrono_MovePrevSplit;
            mnuMoveNextSplit.Text = ScreenManagerLang.mnuAction_Chrono_MoveNextSplit;
            mnuDeleteSection.Text = ScreenManagerLang.mnuAction_Chrono_DeleteSection;

            // When we are on a dead section.
            mnuStart.Text = ScreenManagerLang.mnuAction_Chrono_Start;
            mnuMovePreviousEnd.Text = ScreenManagerLang.mnuAction_Chrono_MovePrevEnd;
            mnuMoveNextStart.Text = ScreenManagerLang.mnuAction_Chrono_MoveNextStart;
            mnuDeleteTimes.Text = ScreenManagerLang.mnuAction_Chrono_DeleteTimes;

            // Options.
            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuShowLabel.Text = ScreenManagerLang.mnuShowLabel;
            mnuLocked.Text = ScreenManagerLang.mnuOptions_Chrono_Locked;

            // Columns.
            mnuColumns.Text = ScreenManagerLang.mnuMeasure_Chrono_Menu;
            mnuColumnName.Text = ScreenManagerLang.mnuMeasure_Name;
            mnuColumnCumul.Text = ScreenManagerLang.mnuMeasure_Chrono_Cumul;
            mnuColumnTag.Text = ScreenManagerLang.mnuMeasure_Chrono_Tag;

            // Sections.
            mnuConfigureSections.Text = ScreenManagerLang.mnuTimeSections;
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
            int sectionIndex = GetSectionIndex(sections, contextTimestamp);
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
            int sectionIndex = GetSectionIndex(sections, contextTimestamp);
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
        
        private void mnuMoveCurrentStart_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(sections, contextTimestamp);
            if (sectionIndex < 0)
                return;

            CaptureMemento(SerializationFilter.Core);

            sections[sectionIndex].Section = new VideoSection(contextTimestamp, sections[sectionIndex].Section.End);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuMoveCurrentEnd_Click(object sender, EventArgs e)
        {
            // Technically "Move current end" is the same as "Stop", but we keep it for symmetry purposes.
            int sectionIndex = GetSectionIndex(sections, contextTimestamp);
            if (sectionIndex < 0)
                return;

            CaptureMemento(SerializationFilter.Core);

            sections[sectionIndex].Section = new VideoSection(sections[sectionIndex].Section.Start, contextTimestamp);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuMovePreviousSplit_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(sections, contextTimestamp);
            if (sectionIndex < 1)
                return;

            CaptureMemento(SerializationFilter.Core);

            int prevIndex = sectionIndex - 1;
            sections[prevIndex].Section = new VideoSection(sections[prevIndex].Section.Start, contextTimestamp);
            sections[sectionIndex].Section = new VideoSection(contextTimestamp, sections[sectionIndex].Section.End);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuMoveNextSplit_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(sections, contextTimestamp);
            if (sectionIndex < 0 || sectionIndex > sections.Count - 2)
                return;

            CaptureMemento(SerializationFilter.Core);

            int nextIndex = sectionIndex + 1;
            sections[sectionIndex].Section = new VideoSection(sections[sectionIndex].Section.Start, contextTimestamp);
            sections[nextIndex].Section = new VideoSection(contextTimestamp, sections[nextIndex].Section.End);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuMovePreviousEnd_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(sections, contextTimestamp);
            if (sectionIndex >= 0 || IsBeforeFirstSection(sectionIndex))
                return;

            CaptureMemento(SerializationFilter.Core);

            int prevIndex = -(sectionIndex + 2);
            sections[prevIndex].Section = new VideoSection(sections[prevIndex].Section.Start, contextTimestamp);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuMoveNextStart_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(sections, contextTimestamp);
            if (sectionIndex >= 0 || IsAfterLastSection(sectionIndex))
                return;

            CaptureMemento(SerializationFilter.Core);

            int nextIndex = -(sectionIndex + 1);
            sections[nextIndex].Section = new VideoSection(contextTimestamp, sections[nextIndex].Section.End);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuDeleteSection_Click(object sender, EventArgs e)
        {
            int sectionIndex = GetSectionIndex(sections, contextTimestamp);
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

        private void mnuColumn_Click(object sender, ChronoColumns column)
        {
            if (visibleColumns.Contains(column))
                visibleColumns.Remove(column);
            else
                visibleColumns.Add(column);

            InvalidateFromMenu(sender);
        }

        private void mnuConfigureSections_Click(object sender, EventArgs e)
        {
            // The dialog is responsible for backing up and restoring the state in case of cancellation.
            // When we exit the dialog the drawing has been modified or reverted to its original state,
            // and in case of validation, the original state pushed to the history stack.
            if (sections.Count == 0)
                return;

            int sectionIndex = GetSectionIndex(sections, contextTimestamp);

            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            IDrawingHostView host = tsmi.Tag as IDrawingHostView;

            try
            {
                FormTimeSections fts = new FormTimeSections(this, sectionIndex, host);
                FormsHelper.Locate(fts);
                fts.ShowDialog();
            }
            catch
            {
            }

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
            int sectionIndex = GetSectionIndex(sections, timestamp);
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
                if (sections[sectionIndex].Section.End == long.MaxValue)
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
            int sectionIndex = GetSectionIndex(sections, timestamp);
            if (sectionIndex < 0)
                return;

            // Live section.
            if (sections[sectionIndex].Section.End == long.MaxValue)
            {
                // Open-ended section.
                CaptureMemento(SerializationFilter.Core);
                StopSection(sectionIndex, timestamp);
                InsertSection(new VideoSection(timestamp, long.MaxValue));
            }
        }

        public void Beat(long timestamp)
        {
            // This tool does not respond to the beat command.
        }

        #endregion

        #region Lower level helpers
        private void BindStyle()
        {
            StyleElements.SanityCheck(style, ToolManager.GetDefaultStyleElements("ChronoMulti"));
            style.Bind(styleData, "Bicolor", "color");
            style.Bind(styleData, "Font", "font size");
        }
        private void UpdateLabelRectangle()
        {
            using (Font f = styleData.GetFont(0.5f))
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
        private void InsertSection(VideoSection section, string name = "", string tag = "")
        {
            // Find insertion point and insert the new section there.
            ChronoSection chronoSection = new ChronoSection(section, name, tag);

            bool found = false;
            int i = 0;
            for (i = 0; i < sections.Count; i++)
            {
                if (sections[i].Section.Start < section.Start)
                    continue;

                found = true;
                break;
            }

            if (!found)
            {
                sections.Add(chronoSection);
            }
            else
            {
                sections.Insert(i, chronoSection);
            }
        }

        private void RemoveSection(int index)
        {
            sections.RemoveAt(index);
        }

        /// <summary>
        /// Update the end time of a specific section.
        /// </summary>
        private void StopSection(int index, long timestamp)
        {
            sections[index].Section = new VideoSection(sections[index].Section.Start, timestamp);
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
        /// Capture the current state and push it to the undo/redo stack.
        /// </summary>
        private void CaptureMemento(SerializationFilter filter)
        {
            var memento = new HistoryMementoModifyDrawing(parentMetadata, parentMetadata.ChronoManager.Id, this.Id, this.Name, filter);
            parentMetadata.HistoryStack.PushNewCommand(memento);
        }
        #endregion

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
        public static int GetSectionIndex(List<ChronoSection> sections, long timestamp)
        {
            int result = -1;
            for (int i = 0; i < sections.Count; i++)
            {
                // Before the start of this section. 
                if (timestamp < sections[i].Section.Start)
                    break;

                // Between start and end of this section.
                // The end of the section is part of the section.
                if (timestamp <= sections[i].Section.End)
                {
                    result = i;
                    break;
                }

                // After that section.
                result--;
            }

            return result;
        }
    }
}
