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
    /// Counter tool.
    /// </summary>
    [XmlType ("Counter")]
    public class DrawingCounter : AbstractDrawing, IDecorable, IKvaSerializable, ITimeable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get {  return "Counter"; }
        }
        public override int ContentHash
        {
            get
            {
                int hash = visibleTimestamp.GetHashCode();
                hash ^= invisibleTimestamp.GetHashCode();
                hash ^= beats.GetHashCode();
                hash ^= styleHelper.ContentHash;
                hash ^= showLabel.GetHashCode();
                hash ^= locked.GetHashCode();
                hash ^= zeroBased.GetHashCode();
                hash ^= halfCadence.GetHashCode();
                hash ^= doubleCadence.GetHashCode();

                return hash;
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
        #endregion

        #region Members
        // Core

        private long visibleTimestamp;               	// chrono becomes visible.
        private long invisibleTimestamp;             	// chrono stops being visible.
        private List<long> beats = new List<long>();
        private long contextTimestamp;                  // timestamp for context-menu operations.
        private string text;
        private bool measureInitialized;
        private MeasureLabelType measureLabelType = MeasureLabelType.Count;
        
        // We can't just show "0" when there is no value because zero is a valid value
        // when using zero-based numbering.
        private const string strNoValue = "*";

        // Options
        private bool showLabel;
        private bool locked;
        private bool zeroBased;
        private bool halfCadence;
        private bool doubleCadence;
        
        // Decoration
        private StyleMaster styleHelper = new StyleMaster();
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
        private ToolStripMenuItem mnuAddBeat = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteBeat = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteBeats = new ToolStripMenuItem();
        
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowLabel = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLocked = new ToolStripMenuItem();
        private ToolStripMenuItem mnuZeroBased = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHalfCadence = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDoubleCadence = new ToolStripMenuItem();

        private ToolStripMenuItem mnuMeasurement = new ToolStripMenuItem();
        private Dictionary<MeasureLabelType, ToolStripMenuItem> mnuMeasureLabelTypes = new Dictionary<MeasureLabelType, ToolStripMenuItem>();
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingCounter(PointF p, long start, long averageTimeStampsPerFrame, DrawingStyle preset = null)
        {
            // Core
            visibleTimestamp = 0;
            invisibleTimestamp = long.MaxValue;
            mainBackground.Rectangle = new RectangleF(p, SizeF.Empty);
            lblBackground.Rectangle = RectangleF.Empty;

            text = strNoValue;

            styleHelper.Bicolor = new Bicolor(Color.Black);
            styleHelper.Font = new Font("Consolas", 16, FontStyle.Bold);
            styleHelper.Clock = false;
            if (preset == null)
                preset = ToolManager.GetStylePreset("Counter");

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

        public DrawingCounter(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata metadata)
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
            mnuShowBefore.Click += mnuShowBefore_Click;
            mnuShowAfter.Click += mnuShowAfter_Click;
            mnuHideBefore.Click += mnuHideBefore_Click;
            mnuHideAfter.Click += mnuHideAfter_Click;
            mnuVisibility.Image = Properties.Drawings.persistence;
            mnuVisibility.DropDownItems.AddRange(new ToolStripItem[] { 
                mnuShowBefore, 
                mnuShowAfter, 
                new ToolStripSeparator(), 
                mnuHideBefore, 
                mnuHideAfter });

            // Action
            mnuAction.Image = Properties.Resources.action;
            mnuAddBeat.Image = Properties.Drawings.monitor;
            mnuDeleteBeat.Image = Properties.Resources.bin_empty;
            mnuDeleteBeats.Image = Properties.Resources.bin_empty;
            mnuAddBeat.Click += mnuAddBeat_Click;
            mnuDeleteBeat.Click += mnuDeleteBeat_Click;
            mnuDeleteBeats.Click += mnuDeleteBeats_Click;
            mnuAction.DropDownItems.AddRange(new ToolStripItem[] {
                mnuAddBeat,
                new ToolStripSeparator(),
                mnuDeleteBeat,
                mnuDeleteBeats,
            });

            // Options
            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowLabel.Image = Properties.Drawings.label;
            mnuLocked.Image = Properties.Drawings.padlock2;
            mnuZeroBased.Image = Properties.Drawings.notification_counter;
            //mnuHalfCadence.Image = Properties.Drawings.notification_counter;
            //mnuDoubleCadence.Image = Properties.Drawings.notification_counter;
            mnuShowLabel.Click += mnuShowLabel_Click;
            mnuLocked.Click += mnuLock_Click;
            mnuZeroBased.Click += mnuZeroBased_Click;
            mnuHalfCadence.Click += mnuHalfCadence_Click;
            mnuDoubleCadence.Click += mnuDoubleCadence_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowLabel,
                mnuLocked,
                mnuZeroBased,
                mnuHalfCadence,
                mnuDoubleCadence
            });

            // Measurement menus.
            mnuMeasurement.Image = Properties.Drawings.label;
            mnuMeasurement.DropDownItems.Clear();
            mnuMeasurement.DropDownItems.AddRange(new ToolStripItem[] {
                CreateMeasureLabelTypeMenu(MeasureLabelType.Count),
                CreateMeasureLabelTypeMenu(MeasureLabelType.CountReverse),
                new ToolStripSeparator(),
                CreateMeasureLabelTypeMenu(MeasureLabelType.CadenceInstant),
                CreateMeasureLabelTypeMenu(MeasureLabelType.CadenceAverage),
                new ToolStripSeparator(),
                CreateMeasureLabelTypeMenu(MeasureLabelType.PeriodInstant),
                CreateMeasureLabelTypeMenu(MeasureLabelType.PeriodAverage),
            });
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

            text = BuildText(currentTimestamp);

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
                
                if (beats.Count > 0)
                {
                    w.WriteStartElement("Beats");

                    for (int i = 0; i < beats.Count; i++)
                    {
                        w.WriteStartElement("Beat");
                        w.WriteString(XmlHelper.WriteTimestamp(beats[i]));
                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                }

                // Options
                w.WriteElementString("Locked", locked.ToString().ToLower());
                w.WriteElementString("ZeroBased", zeroBased.ToString().ToLower());
                w.WriteElementString("HalfCadence", halfCadence.ToString().ToLower());
                w.WriteElementString("DoubleCadence", doubleCadence.ToString().ToLower());

                TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(MeasureLabelType));
                string xmlMeasureLabelType = enumConverter.ConvertToString(measureLabelType);
                w.WriteElementString("ExtraData", xmlMeasureLabelType);

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

        //public MeasuredDataTime CollectMeasuredData()
        //{
        //    MeasuredDataTime mdt = new MeasuredDataTime();
        //    mdt.Name = this.Name;
        //    mdt.VisibleColumns = this.visibleColumns;

        //    long cumulTimestamps = 0;
        //    for (int i = 0; i < sections.Count; i++)
        //    {
        //        if (sections[i].Section.End == long.MaxValue)
        //            continue;

        //        MeasuredDataTimeSection mdts = new MeasuredDataTimeSection();    
        //        mdts.Name = string.IsNullOrEmpty(sections[i].Name) ? (i + 1).ToString() : sections[i].Name;
        //        mdts.Tag = sections[i].Tag;

        //        if (!string.IsNullOrEmpty(mdts.Tag))
        //            mdt.HasTags = true;

        //        var section = sections[i];
        //        mdts.Start = parentMetadata.GetNumericalTime(section.Section.Start, TimeType.UserOrigin);
        //        mdts.Stop = parentMetadata.GetNumericalTime(section.Section.End, TimeType.UserOrigin);

        //        long elapsedTimestamps = section.Section.End - section.Section.Start;
        //        cumulTimestamps += elapsedTimestamps;

        //        mdts.Duration = parentMetadata.GetNumericalTime(elapsedTimestamps, TimeType.Absolute);
        //        mdts.Cumul = parentMetadata.GetNumericalTime(cumulTimestamps, TimeType.Absolute);

        //        mdt.Sections.Add(mdts);
        //    }

        //    return mdt;
        //}


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
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            measureInitialized = true;
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
                    case "Beats":
                        ParseBeats(xmlReader, timestampMapper);
                        break;
                    case "Locked":
                        locked = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "ZeroBased":
                        zeroBased = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "HalfCadence":
                        halfCadence = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "DoubleCadence":
                        doubleCadence = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "ExtraData":
                        measureLabelType = XmlHelper.ParseEnum<MeasureLabelType>(xmlReader.ReadElementContentAsString(), MeasureLabelType.None);
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            xmlReader.ReadEndElement();
        }

        private void ParseBeats(XmlReader xmlReader, TimestampMapper timestampMapper)
        {
            beats.Clear();

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
                    case "Beat":
                        xmlReader.ReadStartElement();
                        long beat = XmlHelper.ParseTimestamp(xmlReader.ReadContentAsString());
                        beats.Add(beat);
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

            // Enable or disable the Add/Delete based on whether we are currently on a beat or not.
            int beatIndex = GetBeatIndexExact(timestamp);
            mnuAddBeat.Enabled = (beatIndex == -1);
            bool isBeforeFirst = IsBeforeFirstBeat(timestamp);
            mnuDeleteBeat.Enabled = !isBeforeFirst;
            
            // Options
            mnuShowLabel.Checked = showLabel;
            mnuLocked.Checked = locked;
            mnuZeroBased.Checked = zeroBased;
            mnuHalfCadence.Checked = halfCadence;
            mnuDoubleCadence.Checked = doubleCadence;

            contextMenu.AddRange(new ToolStripItem[] {
                mnuVisibility,
                mnuAction,
                mnuOptions,
                mnuMeasurement,
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

            mnuAddBeat.Text = ScreenManagerLang.DrawingCounter_AddBeat;
            mnuDeleteBeat.Text = ScreenManagerLang.DrawingCounter_DeleteBeat;
            mnuDeleteBeats.Text = ScreenManagerLang.DrawingCounter_DeleteAllBeats;

            // Options.
            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuShowLabel.Text = ScreenManagerLang.mnuShowLabel;
            mnuLocked.Text = ScreenManagerLang.mnuOptions_Chrono_Locked;
            mnuZeroBased.Text = ScreenManagerLang.DrawingCounter_ZeroBasedNumbering;
            mnuHalfCadence.Text = ScreenManagerLang.DrawingCounter_Half;
            mnuDoubleCadence.Text = ScreenManagerLang.DrawingCounter_Double;

            // Measurement
            mnuMeasurement.Text = ScreenManagerLang.mnuMeasure_Label_Menu;
            foreach (var pair in mnuMeasureLabelTypes)
            {
                ToolStripMenuItem tsmi = pair.Value;
                MeasureLabelType measureLabelType = pair.Key;
                tsmi.Text = GetMeasureLabelOptionText(measureLabelType);
                tsmi.Checked = this.measureLabelType == measureLabelType;
            }
        }

        #region Visibility
        private void mnuShowBefore_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            visibleTimestamp = 0;
            InvalidateFromMenu(sender);
        }

        private void mnuShowAfter_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            invisibleTimestamp = long.MaxValue;
            infosFading.ReferenceTimestamp = invisibleTimestamp;
            InvalidateFromMenu(sender);
        }

        private void mnuHideBefore_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            visibleTimestamp = CurrentTimestampFromMenu(sender);
            InvalidateFromMenu(sender);
        }

        private void mnuHideAfter_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            invisibleTimestamp = CurrentTimestampFromMenu(sender);
            infosFading.ReferenceTimestamp = invisibleTimestamp;
            InvalidateFromMenu(sender);
        }
        #endregion

        private void mnuAddBeat_Click(object sender, EventArgs e)
        {
            // Start a new section here.
            CaptureMemento(SerializationFilter.Core);
            
            InsertBeat(contextTimestamp);
            
            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuDeleteBeat_Click(object sender, EventArgs e)
        {
            // Note: this is not equivalent to hitting F7 on an existing beat.
            // This function will also delete the previous beat if it exists.
            int beatIndex = GetBeatIndexInterval(contextTimestamp);
            if (beatIndex < 0)
                return;

            CaptureMemento(SerializationFilter.Core);

            RemoveBeat(beatIndex);

            InvalidateFromMenu(sender);
            UpdateFramesMarkersFromMenu(sender);
        }

        private void mnuDeleteBeats_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);

            beats.Clear();
            
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

        private void mnuZeroBased_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            zeroBased = !mnuZeroBased.Checked;
            InvalidateFromMenu(sender);
        }

        private void mnuHalfCadence_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            halfCadence = !mnuHalfCadence.Checked;
            if (halfCadence)
            {
                doubleCadence = false;
                zeroBased = true;
            }
            InvalidateFromMenu(sender);
        }

        private void mnuDoubleCadence_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            doubleCadence = !mnuDoubleCadence.Checked;
            if (doubleCadence)
            {
                halfCadence = false;
                zeroBased = true;
            }
            InvalidateFromMenu(sender);
        }
        #endregion

        #region ITimeable
        public void StartStop(long timestamp)
        {
            // Start/Stop of a time section is not supported in this tool.
        }

        public void Split(long timestamp)
        {
            // Splitting of a time section is not supported in this tool.
        }


        public void Beat(long timestamp)
        {
            if (locked || timestamp < visibleTimestamp || timestamp > invisibleTimestamp)
                return;

            // Insert a new beat at the passed time, 
            // or remove it if there is already one.
            CaptureMemento(SerializationFilter.Core);
            int beatIndex = GetBeatIndexExact(timestamp);
            if (beatIndex >= 0)
                RemoveBeat(beatIndex);
            else
                InsertBeat(timestamp);
        }

        #endregion

        public void InitializeMeasurableData(MeasureLabelType measureLabelType)
        {
            if (measureInitialized)
                return;

            measureInitialized = true;

            List<MeasureLabelType> supported = new List<MeasureLabelType>()
            {
                MeasureLabelType.Count,
                MeasureLabelType.CountReverse,
                MeasureLabelType.CadenceInstant,
                MeasureLabelType.CadenceAverage,
                MeasureLabelType.CadenceVariation,
                MeasureLabelType.PeriodInstant,
                MeasureLabelType.PeriodAverage,
            };

            MeasureLabelType defaultMeasureLabelType = MeasureLabelType.Count;
            this.measureLabelType = supported.Contains(measureLabelType) ? measureLabelType : defaultMeasureLabelType;
        }

        #region Lower level helpers
        private void BindStyle()
        {
            DrawingStyle.SanityCheck(style, ToolManager.GetStylePreset("Counter"));
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
        /// Insert a new beat into the list.
        /// </summary>
        private void InsertBeat(long timestamp)
        {
            // Index of inter-beat section we are in.
            int index = GetBeatIndexInterval(timestamp);
            if (index == -1)
            {
                beats.Insert(0, timestamp);
                return;
            }

            if (beats[index] == timestamp)
                return;

            beats.Insert(index + 1, timestamp);
        }

        private void RemoveBeat(int index)
        {
            beats.RemoveAt(index);
        }

        /// <summary>
        /// Capture the current state and push it to the undo/redo stack.
        /// </summary>
        private void CaptureMemento(SerializationFilter filter)
        {
            var memento = new HistoryMementoModifyDrawing(parentMetadata, parentMetadata.ChronoManager.Id, this.Id, this.Name, filter);
            parentMetadata.HistoryStack.PushNewCommand(memento);
        }

        /// <summary>
        /// Returns the text to use for the main object display.
        /// </summary>
        private string BuildText(long timestamp)
        {
            //----------------------------------------------------
            // Beats and Cycles
            // - Count and reverse count are based on raw beats.
            // - This is where zero-based numbering may be optional.
            // - If we are only counting repetetions we don't want zero-based.
            // - Cadence and Period are always based on cycles so zero-based numbering is implicit.
            // - Reverse counting is also cycle based, we want the take-off to be zero.
            // 
            // Half and Double
            // - This is interesting when annotating by full cycles (e.g: same leg/arm going down),
            // but we are actually interested in the half-cycle metric. (or vice-versa).
            // - It can be used for both beats and cycles but it only really makes sense for cycles,
            // so when one of these is on, zero-based numbering should also be on. This is done 
            // automatically in the event handler of the option.
            // 
            // Instantaneous metrics
            // - When calculating instantaneous metrics we use the cycle the current frame is in.
            // - That is, the instantaneous frequency is: 1 / current cycle duration in seconds.
            // - If we are exactly on a beat, use the interval leading to it.
            // 
            // Average metrics
            // - For average we always use the full series, not just the cycles up to where we are.
            //----------------------------------------------------

            // Index of inter-beat section we are in.
            int index = GetBeatIndexInterval(timestamp);

            // Bail out if we are before the first beat.
            if (index == -1)
                return strNoValue;

            switch (measureLabelType)
            {
                case MeasureLabelType.Count:
                    {
                        // Number of beats. 
                        // If zero-based is enabled this is the number of cycles.
                        float beatCount = zeroBased ? index : index + 1;
                        if (halfCadence)
                            beatCount /= 2.0f;
                        else if (doubleCadence)
                            beatCount *= 2.0f;

                        return string.Format("{0}", beatCount);
                    }
                case MeasureLabelType.CountReverse:
                    {
                        // Number of beats in reverse so that the last beat is 0.
                        // This is typically used for approach run.
                        float beatCount = beats.Count - 1 - index;
                        if (halfCadence)
                            beatCount /= 2.0f;
                        else if (doubleCadence)
                            beatCount *= 2.0f;

                        return string.Format("{0}", beatCount);
                    }
                case MeasureLabelType.CadenceInstant:
                case MeasureLabelType.PeriodInstant:
                    {
                        // For instant cadence/period, use the current inter-beat interval.
                        long start = 0;
                        long end = 0;

                        if (timestamp == beats[index])
                        {
                            // We are exactly on a beat, use the interval leading to it. 

                            // Bail out if we are on the first beat.
                            if (index == 0)
                                return strNoValue;

                            start = beats[index - 1];
                            end = timestamp;
                        }
                        else
                        {
                            // We are between two beats or after the last.

                            // Bail out if we are after the last one.
                            if (index == beats.Count - 1)
                                return strNoValue;

                            start = beats[index];
                            end = beats[index + 1];
                        }

                        long duration = end - start;
                        if (measureLabelType == MeasureLabelType.CadenceInstant)
                        {
                            float cycles = 1.0f;
                            if (halfCadence)
                                cycles = 0.5f;
                            else if (doubleCadence)
                                cycles = 2.0f;

                            string cadence = parentMetadata.GetCadence(cycles, duration);
                            return cadence;
                        }
                        else if (measureLabelType == MeasureLabelType.PeriodInstant)
                        {
                            if (halfCadence)
                                duration *= 2;
                            else if (doubleCadence)
                                duration /= 2;

                            string period = parentMetadata.TimeCodeBuilder(duration, TimeType.Absolute, TimecodeFormat.Unknown, true);
                            return period;
                        }
                        else
                        {
                            return strNoValue;
                        }
                    }
                case MeasureLabelType.CadenceAverage:
                case MeasureLabelType.PeriodAverage:
                    {
                        // For average cadence/period we always consider the whole list.
                        
                        // Bail out if there is only one beat.
                        if (beats.Count == 1)
                            return strNoValue;

                        long duration = beats[beats.Count - 1] - beats[0];
                        float cycles = beats.Count - 1;
                        if (halfCadence)
                            cycles /= 2.0f;
                        else if (doubleCadence)
                            cycles *= 2.0f;

                        if (measureLabelType == MeasureLabelType.CadenceAverage)
                        {
                            string cadence = parentMetadata.GetCadence(cycles, duration);
                            return cadence;
                        }
                        else
                        {
                            long avgDuration = (long)(duration / cycles);
                            string average = parentMetadata.TimeCodeBuilder(avgDuration, TimeType.Absolute, TimecodeFormat.Unknown, true);
                            return average;
                        }
                    }
                default:
                    {
                        return strNoValue;
                    }
            }
        }

        private bool IsBeforeFirstBeat(long timestamp)
        {
            return (beats.Count == 0 || timestamp < beats[0]);
        }

        /// <summary>
        /// Returns the exact beat index we are on.
        /// If we are not exactly on a beat returns -1.
        /// </summary>
        private int GetBeatIndexExact(long timestamp)
        {
            for (int i = 0; i < beats.Count; i++)
            {
                if (timestamp == beats[i])
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Returns the beat index we are on or the previous one if between two.
        /// This can also be used to get the index of the interval we are in.
        /// If we are before the first one it returns -1.
        /// </summary>
        private int GetBeatIndexInterval(long timestamp)
        {
            int i;
            for (i = 0; i < beats.Count; i++)
            {
                if (timestamp < beats[i])
                    break;
            }

            return i - 1;
        }

        private ToolStripMenuItem CreateMeasureLabelTypeMenu(MeasureLabelType measureLabelType)
        {
            ToolStripMenuItem mnu = new ToolStripMenuItem();
            mnu.Click += mnuMeasureLabelType_Click;
            mnuMeasureLabelTypes.Add(measureLabelType, mnu);
            return mnu;
        }

        private void mnuMeasureLabelType_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            MeasureLabelType measureLabelType = MeasureLabelType.None;
            foreach (var pair in mnuMeasureLabelTypes)
            {
                if (pair.Value == tsmi)
                {
                    measureLabelType = pair.Key;
                    break;
                }
            }

            this.measureLabelType = measureLabelType;
            InvalidateFromMenu(tsmi);
        }
        private string GetMeasureLabelOptionText(MeasureLabelType data)
        {
            switch (data)
            {
                case MeasureLabelType.Count: return "Count";
                case MeasureLabelType.CountReverse: return "Reverse count";
                case MeasureLabelType.CadenceInstant: return "Cadence (instantaneous)";
                case MeasureLabelType.CadenceAverage: return "Cadence (average)";
                case MeasureLabelType.CadenceVariation: return "Cadence variation";
                case MeasureLabelType.PeriodInstant: return "Period (instantaneous)";
                case MeasureLabelType.PeriodAverage: return "Period (average)";
            }

            return "";
        }
        #endregion


    }
}
