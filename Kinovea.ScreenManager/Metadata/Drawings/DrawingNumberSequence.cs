#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Number sequence.
    /// This is a proxy object dispatching to individual number objects in the sequence.
    /// </summary>
    [XmlType("AutoNumbers")]
    public class DrawingNumberSequence : AbstractMultiDrawing, IDecorable, IKvaSerializable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get { return ScreenManagerLang.ToolTip_DrawingToolAutonumbers;}
        }
        public override int ContentHash
        {
            get 
            { 
                return numberSequence.Aggregate(0, (a, n) => a ^ n.GetHashCode()); 
            }
        } 
        public DrawingStyle DrawingStyle
        {
            get { return style;}
        }
        public override AbstractMultiDrawingItem SelectedItem {
            get 
            {
                if(selected >= 0 && selected < numberSequence.Count)
                    return numberSequence[selected];
                else
                    return null;
            }
        }
        public override int Count {
            get { return numberSequence.Count; }
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
        private List<DrawingNumberSequenceItem> numberSequence = new List<DrawingNumberSequenceItem>();
        private int selected = -1;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public DrawingNumberSequence(DrawingStyle preset)
        {
            styleHelper.Bicolor = new Bicolor(Color.Black);
            styleHelper.Font = new Font("Arial", defaultFontSize, FontStyle.Bold);
            if(preset != null)
            {
                style = preset.Clone();
                BindStyle();
            }
        }
        
        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            foreach(DrawingNumberSequenceItem number in numberSequence)
                number.Draw(canvas, transformer, currentTimestamp, styleHelper);
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            if (selected < 0 || selected >= numberSequence.Count)
                return;

            if ((modifierKeys & Keys.Shift) == Keys.Shift)
            {
                // Move all numbers at once.
                foreach (var number in numberSequence)
                    number.MouseMove(dx, dy);
            }
            else
            {
                numberSequence[selected].MouseMove(dx, dy);
            }
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            int currentNumber = 0;
            int handle = -1;
            foreach(DrawingNumberSequenceItem number in numberSequence)
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
        public override PointF GetCopyPoint()
        {
            return PointF.Empty;
        }
        #endregion

        #region AbstractMultiDrawing Implementation
        public override AbstractMultiDrawingItem GetNewItem(PointF point, long position, long averageTimeStampsPerFrame)
        {
            int nextValue = NextValue(position);
            return new DrawingNumberSequenceItem(position, averageTimeStampsPerFrame, point, nextValue);
        }
        public override AbstractMultiDrawingItem GetItem(Guid id)
        {
            return numberSequence.FirstOrDefault(n => n.Id == id);
        }
        public override void Add(AbstractMultiDrawingItem item)
        {
            DrawingNumberSequenceItem number = item as DrawingNumberSequenceItem;
            if(number == null)
                return;
            
            selected = InsertSorted(number);
        }
        public override void Remove(Guid id)
        {
            numberSequence.RemoveAll(a => a.Id == id);
            selected = -1;
        }
        public override void Clear()
        {
            numberSequence.Clear();
            selected = -1;
        }

        public int IndexOf(Guid id)
        {
            return numberSequence.FindIndex(item => item.Id == id);
        }
        #endregion
        
        #region Public methods
        public void ReadXml(XmlReader r, PointF scale, TimestampMapper timestampMapper)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "DrawingStyle":
                        style = new DrawingStyle(r);
                        BindStyle();
                        break;
                    case "AutoNumber":
                        AbstractMultiDrawingItem item = MultiDrawingItemSerializer.Deserialize(r, scale, timestampMapper, parentMetadata);
                        DrawingNumberSequenceItem number = item as DrawingNumberSequenceItem;
                        int index = IndexOf(number.Id);
                        if (index == -1)
                            parentMetadata.AddMultidrawingItem(this, number);
                        else
                            numberSequence[index] = number;
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }	
 
            r.ReadEndElement();
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                foreach (DrawingNumberSequenceItem number in numberSequence)
                    DrawingSerializer.Serialize(w, number as IKvaSerializable, filter);
            }

            if (ShouldSerializeStyle(filter))
            {
                w.WriteStartElement("DrawingStyle");
                style.WriteXml(w);
                w.WriteEndElement();
            }
        }
        
        /// <summary>
        /// Reconfigure the auto-numbers to be placed at the passed locations.
        /// </summary>
        public void Configure(long timestamp, long averageTimeStampsPerFrame, List<PointF> locations)
        {
            Clear();
            
            int value = 0;
            foreach (var location in locations)
            {
                value++;
                DrawingNumberSequenceItem an = new DrawingNumberSequenceItem(timestamp, averageTimeStampsPerFrame, location, value);
                numberSequence.Add(an);
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
            return numberSequence.Count == 0 ? 1 : NextValueVideo();
        }
        private int NextValueVideo()
        {
            // Consider the whole video for increment and holes.
            int holeIndex = FindFirstHole();
            if(holeIndex >=0)
                return holeIndex;
            
            return numberSequence[numberSequence.Count-1].Value + 1;
        }
        private int FindFirstHole()
        {
            // Returns the value that should be in the first found hole.
            for(int i=0;i<numberSequence.Count;i++)
            {
                if(numberSequence[i].Value > i + 1)
                   return i + 1;  
            }
            
            return -1;
        }
        private int InsertSorted(DrawingNumberSequenceItem item)
        {
            for(int i=0;i<numberSequence.Count;i++)
            {
                if(numberSequence[i].Value > item.Value)
                {
                    numberSequence.Insert(i, item);
                    return i;
                }
            }
            
            numberSequence.Add(item);
            return numberSequence.Count - 1;
        }
    }
}


