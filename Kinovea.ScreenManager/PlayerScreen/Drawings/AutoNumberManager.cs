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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Auto Numbers.
    /// This is the proxy object dispatching all individual numbers requests. (draw, hit testing, etc.)
    /// </summary>
    public class AutoNumberManager : AbstractMultiDrawing, IDecorable
    {
        #region Properties
        public override string DisplayName
        {
            get { return ScreenManagerLang.ToolTip_DrawingToolAutonumbers;}
        }
        public override int ContentHash
        {
            get { return 0; }
        } 
        public DrawingStyle DrawingStyle
        {
            get { return style;}
        }
        public override object SelectedItem {
            get 
            {
                if(selected >= 0 && selected < autoNumbers.Count)
                    return autoNumbers[selected];
                else
                    return null;
            }
        }
        public override int Count {
            get { return autoNumbers.Count; }
        }
        
        // Fading is not currently modifiable from outside.
        public override InfosFading  InfosFading
        {
            get { return null;}
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
        #endregion
        
        #region Members
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private const int defaultFontSize = 16;
        private List<AutoNumber> autoNumbers = new List<AutoNumber>();
        private int selected = -1;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public AutoNumberManager(DrawingStyle preset)
        {
            styleHelper.Bicolor = new Bicolor(Color.Black);
            styleHelper.Font = new Font("Arial", defaultFontSize, FontStyle.Bold);
            if(preset != null)
            {
                style = preset.Clone();
                BindStyle();
            }
        }
        public AutoNumberManager(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, long averageTimeStampsPerFrame)
            : this(ToolManager.AutoNumbers.StylePreset.Clone())
        {
            ReadXml(xmlReader, scale, timestampMapper, averageTimeStampsPerFrame);
        }
        
        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            foreach(AutoNumber number in autoNumbers)
                number.Draw(canvas, transformer, currentTimestamp);
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            if(selected >= 0 && selected < autoNumbers.Count)
                autoNumbers[selected].MouseMove(dx, dy);
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
        }
        public override int HitTest(Point point, long currentTimestamp, IImageToViewportTransformer transformer, bool zooming)
        {
            int currentNumber = 0;
            int handle = -1;
            foreach(AutoNumber number in autoNumbers)
            {
                handle = number.HitTest(point, currentTimestamp, transformer);
                if(handle >= 0)
                {
                    selected = currentNumber;
                    break;
                }
                currentNumber++;
            }
            
            return handle;
        }
        #endregion
        
        #region AbstractMultiDrawing Implementation
        public override void Add(object item)
        {
            // Used in the context of redo.
            AutoNumber number = item as AutoNumber;
            if(number == null)
                return;
            
            autoNumbers.Add(number);
            selected = autoNumbers.Count - 1;
        }
        public override void Remove(object item)
        {
            AutoNumber number = item as AutoNumber;
            if(number == null)
                return;
            
            autoNumbers.Remove(number);
            selected = -1;
        }
        public override void Clear()
        {
            autoNumbers.Clear();
            selected = -1;
        }
        #endregion
        
        #region Public methods
        public void Add(Point point, long position, long averageTimeStampsPerFrame)
        {
            // Equivalent to GetNewDrawing() for regular drawing tools.
            int nextValue = NextValue(position);
            selected = InsertSorted(new AutoNumber(position, averageTimeStampsPerFrame, point, nextValue, styleHelper));
        }
        public void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, long averageTimeStampsPerFrame)
        {
            Clear();

            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "DrawingStyle":
                        style = new DrawingStyle(xmlReader);
                        BindStyle();
                        break;
                    case "AutoNumber":
                        AutoNumber number = new AutoNumber(xmlReader, scale, timestampMapper, averageTimeStampsPerFrame, styleHelper);
                        InsertSorted(number);
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }	
            
            xmlReader.ReadEndElement();
        }
        public void WriteXml(XmlWriter w)
        {
            w.WriteStartElement("DrawingStyle");
            style.WriteXml(w);
            w.WriteEndElement();
            
            foreach(AutoNumber number in autoNumbers)
            {
                w.WriteStartElement("AutoNumber");
                number.WriteXml(w);
                w.WriteEndElement();
            }
        }
        #endregion
        
        private void BindStyle()
        {
            style.Bind(styleHelper, "Bicolor", "back color");
            style.Bind(styleHelper, "Font", "font size");
        }
        private int NextValue(long position)
        {
            return autoNumbers.Count == 0 ? 1 : NextValueVideo();
        }
        private int NextValueVideo()
        {
            // Consider the whole video for increment and holes.
            int holeIndex = FindFirstHole();
            if(holeIndex >=0)
                return holeIndex;
            
            return autoNumbers[autoNumbers.Count-1].Value + 1;
        }
        private int FindFirstHole()
        {
            // Returns the value that should be in the first found hole.
            for(int i=0;i<autoNumbers.Count;i++)
            {
                if(autoNumbers[i].Value > i + 1)
                   return i + 1;  
            }
            
            return -1;
        }
        private int InsertSorted(AutoNumber item)
        {
            for(int i=0;i<autoNumbers.Count;i++)
            {
                if(autoNumbers[i].Value > item.Value)
                {
                    autoNumbers.Insert(i, item);
                    return i;
                }
            }
            
            autoNumbers.Add(item);
            return autoNumbers.Count - 1;
        }
    }
}


