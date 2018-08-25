#region License
/*
Copyright © Joan Charmant 2008-2009.
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    [XmlType ("Chrono")]
    public class DrawingChrono : AbstractDrawing, IDecorable, IKvaSerializable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get {  return ScreenManagerLang.ToolTip_DrawingToolChrono; }
        }
        public override int ContentHash
        {
            get 
            { 
                int iHash = startCountingTimestamp.GetHashCode();
                iHash ^= stopCountingTimestamp.GetHashCode();
                iHash ^= visibleTimestamp.GetHashCode();
                iHash ^= invisibleTimestamp.GetHashCode();
                iHash ^= countdown.GetHashCode();
                iHash ^= styleHelper.ContentHash;
                iHash ^= label.GetHashCode();
                iHash ^= showLabel.GetHashCode();
    
                return iHash;
            }
        } 
        public DrawingStyle DrawingStyle
        {
            get { return style;}
        }
        public override InfosFading  InfosFading
        {
            // Fading is not modifiable from outside for chrono.
            get { return null; }
            set { }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.ConfigureColorSize; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get { return null; }
        }
        public Metadata ParentMetadata
        {
            get { return parentMetadata; }    // unused.
            set { parentMetadata = value; }
        }
        public long TimeStart
        {
            get { return startCountingTimestamp; }
        }
        public long TimeStop
        {
            get { return stopCountingTimestamp; }
        }
        public long TimeVisible
        {
            get { return visibleTimestamp; }
        }
        public long TimeInvisible
        {
            get { return invisibleTimestamp; }
        }
        public bool CountDown
        {
            get { return countdown;}
            set 
            {
                // We should only toggle to countdown if we do have a stop value.
                countdown = value;
            }
        }
        public bool HasTimeStop
        {
            // This is used to know if we can toggle to countdown or not.
            get{ return (stopCountingTimestamp != long.MaxValue);}
        }
        
        // The following properties are used from the formConfigureChrono.
        public string Label
        {
            get { return label; }
            set 
            { 
                label = value;
                Name = value;
                UpdateLabelRectangle();
            }
        }
        public bool ShowLabel
        {
            get { return showLabel; }
            set { showLabel = value; }
        }
        #endregion

        #region Members
        // Core
        private long startCountingTimestamp;         	// chrono starts counting.
        private long stopCountingTimestamp;          	// chrono stops counting. 
        private long visibleTimestamp;               	// chrono becomes visible.
        private long invisibleTimestamp;             	// chrono stops being visible.
        private bool countdown;							// chrono works backwards. (Must have a stop)
        private string timecode;
        private string label;
        private bool showLabel;
        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private InfosFading infosFading;
        private static readonly int allowedFramesOver = 12;  // Number of frames the chrono stays visible after the 'Hiding' point.
        private RoundedRectangle mainBackground = new RoundedRectangle();
        private RoundedRectangle lblBackground = new RoundedRectangle();
        
        private Metadata parentMetadata;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingChrono(PointF p, long start, long averageTimeStampsPerFrame, DrawingStyle preset = null)
        {
            // Core
            visibleTimestamp = start;
            startCountingTimestamp = long.MaxValue;
            stopCountingTimestamp = long.MaxValue;
            invisibleTimestamp = long.MaxValue;
            countdown = false;
            mainBackground.Rectangle = new RectangleF(p, SizeF.Empty);

            timecode = "error";

            styleHelper.Bicolor = new Bicolor(Color.Black);
            styleHelper.Font = new Font("Arial", 16, FontStyle.Bold);

            if (preset == null)
                preset = ToolManager.GetStylePreset("Chrono");
            
            style = preset.Clone();
            BindStyle();
            
            label = "";
            showLabel = true;
            
            // We use the InfosFading utility to fade the chrono away.
            // The refererence frame will be the frame at which fading start.
            // Must be updated on "Hide" menu.
            infosFading = new InfosFading(invisibleTimestamp, averageTimeStampsPerFrame);
            infosFading.FadingFrames = allowedFramesOver;
            infosFading.UseDefault = false;
        }
        public DrawingChrono(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata metadata)
            : this(PointF.Empty, 0, 1, null)
        {
            ReadXml(xmlReader, scale, timestampMapper);
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

            timecode = GetTimecode(currentTimestamp);
            string text = timecode;

            using (SolidBrush brushBack = styleHelper.GetBackgroundBrush((int)(opacityFactor * 128)))
            using (SolidBrush brushText = styleHelper.GetForegroundBrush((int)(opacityFactor * 255)))
            using (Font fontText = styleHelper.GetFont((float)transformer.Scale))
            {
                SizeF textSize = canvas.MeasureString(text, fontText);
                Point bgLocation = transformer.Transform(mainBackground.Rectangle.Location);
                Size bgSize = new Size((int)textSize.Width, (int)textSize.Height);

                SizeF untransformed = transformer.Untransform(textSize);
                mainBackground.Rectangle = new RectangleF(mainBackground.Rectangle.Location, untransformed);

                Rectangle rect = new Rectangle(bgLocation, bgSize);
                RoundedRectangle.Draw(canvas, rect, brushBack, fontText.Height / 4, false, false, null);
                canvas.DrawString(text, fontText, brushText, rect.Location);

                if (showLabel && label.Length > 0)
                {
                    using (Font fontLabel = styleHelper.GetFont((float)transformer.Scale * 0.5f))
                    {
                        SizeF lblTextSize = canvas.MeasureString(label, fontLabel); 
                        Rectangle lblRect = new Rectangle(rect.Location.X, rect.Location.Y - (int)lblTextSize.Height, (int)lblTextSize.Width, (int)lblTextSize.Height);
                        RoundedRectangle.Draw(canvas, lblRect, brushBack, fontLabel.Height/3, true, false, null);
                        canvas.DrawString(label, fontLabel, brushText, lblRect.Location);
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
                result = mainBackground.HitTest(point, true, transformer);
                if(result < 0) 
                    result = lblBackground.HitTest(point, false, transformer);
            }

            return result;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            // Invisible handler to change font size.
            int wantedHeight = (int)(point.Y - mainBackground.Rectangle.Location.Y);
            styleHelper.ForceFontSize(wantedHeight, timecode);
            style.ReadValue();
            UpdateLabelRectangle();
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            mainBackground.Move(dx, dy);
            lblBackground.Move(dx, dy);
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
                w.WriteElementString("StartCounting", (startCountingTimestamp == long.MaxValue) ? "-1" : startCountingTimestamp.ToString());
                w.WriteElementString("StopCounting", (stopCountingTimestamp == long.MaxValue) ? "-1" : stopCountingTimestamp.ToString());
                w.WriteElementString("Invisible", (invisibleTimestamp == long.MaxValue) ? "-1" : invisibleTimestamp.ToString());
                w.WriteElementString("Countdown", countdown.ToString().ToLower());

                if (ShouldSerializeAll(filter))
                {
                    // Spreadsheet support
                    string userDuration = "0";
                    if (startCountingTimestamp != long.MaxValue && stopCountingTimestamp != long.MaxValue)
                        userDuration = parentMetadata.TimeCodeBuilder(stopCountingTimestamp - startCountingTimestamp, TimeType.Duration, TimecodeFormat.Unknown, false);

                    w.WriteElementString("UserDuration", userDuration);
                }

                // </values>
                w.WriteEndElement();
            }

            if (ShouldSerializeStyle(filter))
            {
                // Label
                w.WriteStartElement("Label");
                w.WriteElementString("Text", label);
                w.WriteElementString("Show", showLabel.ToString().ToLower());
                w.WriteEndElement();

                w.WriteStartElement("DrawingStyle");
                style.WriteXml(w);
                w.WriteEndElement();
            }
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
                        visibleTimestamp = timestampMapper(xmlReader.ReadElementContentAsLong(), false);
                        break;
                    case "StartCounting":
                        long start = xmlReader.ReadElementContentAsLong(); 
                        startCountingTimestamp = (start == -1) ? long.MaxValue : timestampMapper(start, false);
                        break;
                    case "StopCounting":
                        long stop = xmlReader.ReadElementContentAsLong();
                        stopCountingTimestamp = (stop == -1) ? long.MaxValue : timestampMapper(stop, false);
                        break;
                    case "Invisible":
                        long hide = xmlReader.ReadElementContentAsLong();
                        invisibleTimestamp = (hide == -1) ? long.MaxValue : timestampMapper(hide, false);                        
                        break;
                    case "Countdown":
                        countdown = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "UserDuration":
                        xmlReader.ReadOuterXml();
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
        private void SanityCheckValues()
        {
            startCountingTimestamp = Math.Max(startCountingTimestamp, 0);
            stopCountingTimestamp = Math.Max(stopCountingTimestamp, 0);
            invisibleTimestamp = Math.Max(invisibleTimestamp, 0);
            
            visibleTimestamp = Math.Min(Math.Max(visibleTimestamp, 0), startCountingTimestamp);
            
            if (stopCountingTimestamp < startCountingTimestamp)
                stopCountingTimestamp = long.MaxValue;

            if (invisibleTimestamp < stopCountingTimestamp)
                invisibleTimestamp = long.MaxValue;
        }
        private void ParseLabel(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "Text":
                        label = xmlReader.ReadElementContentAsString();
                        break;
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

        #region PopMenu commands Implementation that change internal values.
        public void Start(long currentTimestamp)
        {
            startCountingTimestamp = currentTimestamp;

            if (stopCountingTimestamp < startCountingTimestamp)
                stopCountingTimestamp = long.MaxValue;
        }
        public void Stop(long currentTimestamp)
        {
            stopCountingTimestamp = currentTimestamp;

            if (stopCountingTimestamp <= startCountingTimestamp)
                startCountingTimestamp = stopCountingTimestamp;

            if (stopCountingTimestamp > invisibleTimestamp)
                invisibleTimestamp = stopCountingTimestamp;
        }
        public void Hide(long currentTimestamp)
        {
            invisibleTimestamp = currentTimestamp;

            // Update fading conf.
            infosFading.ReferenceTimestamp = invisibleTimestamp;
            
            // Avoid counting when fading.
            if (invisibleTimestamp >= stopCountingTimestamp)
                return;
            
            stopCountingTimestamp = invisibleTimestamp;
            if (stopCountingTimestamp < startCountingTimestamp)
                startCountingTimestamp = stopCountingTimestamp;
        }
        #endregion

        #region Lower level helpers
        private void BindStyle()
        {
            style.Bind(styleHelper, "Bicolor", "color");
            style.Bind(styleHelper, "Font", "font size");    
        }
        private void UpdateLabelRectangle()
        {
            using(Font f = styleHelper.GetFont(0.5F))
            using(Button but = new Button())
            using(Graphics g = but.CreateGraphics())
            {
                SizeF size = g.MeasureString(label, f);
                lblBackground.Rectangle = new RectangleF(
                    mainBackground.X, mainBackground.Y - lblBackground.Rectangle.Height, size.Width + 11, size.Height);
            }
        }
        private string GetTimecode(long timestamp)
        {
            long timestamps;

            // Compute Text value depending on where we are.
            if (timestamp > startCountingTimestamp)
            {
                if (timestamp <= stopCountingTimestamp)
                {
                    // After start and before stop.
                    if(countdown)
                        timestamps = stopCountingTimestamp - timestamp;
                    else
                        timestamps = timestamp - startCountingTimestamp;                		
                }
                else
                {
                    // After stop. Keep max value.
                    timestamps = countdown ? 0 : stopCountingTimestamp - startCountingTimestamp;
                }
            }
            else
            {
                // Before start. Keep min value.
                timestamps = countdown ? stopCountingTimestamp - startCountingTimestamp : 0;
            }

            return parentMetadata.TimeCodeBuilder(timestamps, TimeType.Duration, TimecodeFormat.Unknown, false);
        }
        #endregion
    }

    /// <summary>
    /// Enum used in CommandModifyChrono to know what value we are touching.
    /// </summary>
    public enum ChronoModificationType
    {
        TimeStart,
        TimeStop,
        TimeHide,
        Countdown
    }
}
