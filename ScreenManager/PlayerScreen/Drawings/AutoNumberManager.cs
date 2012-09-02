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
        public override InfosFading  infosFading
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
		
		public AutoNumberManager(DrawingStyle _preset)
		{
            styleHelper.Bicolor = new Bicolor(Color.Black);
            styleHelper.Font = new Font("Arial", defaultFontSize, FontStyle.Bold);
            if(_preset != null)
            {
                style = _preset.Clone();
                BindStyle();
            }
		}
		public AutoNumberManager(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback, long _iAverageTimeStampsPerFrame)
		    : this(ToolManager.AutoNumbers.StylePreset.Clone())
		{
		    ReadXml(_xmlReader, _scale, _remapTimestampCallback, _iAverageTimeStampsPerFrame);
		}
		
		
		#region AbstractDrawing Implementation
		public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
		{
		    foreach(AutoNumber number in autoNumbers)
                number.Draw(_canvas, _transformer, _iCurrentTimestamp);
		}
		public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
		{
		    if(selected >= 0 && selected < autoNumbers.Count)
				autoNumbers[selected].MouseMove(_deltaX, _deltaY);
		}
		public override void MoveHandle(Point point, int handleNumber, Keys modifiers)
		{
		    if(selected >= 0 && selected < autoNumbers.Count)
				autoNumbers[selected].MoveHandleTo(point);
		}
		public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
		    int currentNumber = 0;
		    int handle = -1;
		    foreach(AutoNumber number in autoNumbers)
		    {
		        handle = number.HitTest(_point, _iCurrentTimestamp);
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
		public override void Add(object _item)
        {
		    // Used in the context of redo.
            AutoNumber number = _item as AutoNumber;
            if(number == null)
                return;
            
		    autoNumbers.Add(number);
		    selected = autoNumbers.Count - 1;
		}
        public override void Remove(object _item)
		{
            AutoNumber number = _item as AutoNumber;
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
		public override string ToString()
        {
            return "Auto numbers"; //ScreenManagerLang.ToolTip_DrawingToolAutoNumbers;
        }
		public void Add(Point _point, long _iPosition, long _iAverageTimeStampsPerFrame)
		{
		    // Equivalent to GetNewDrawing() for regular drawing tools.
		    int nextValue = NextValue(_iPosition);
		    selected = InsertSorted(new AutoNumber(_iPosition, _iAverageTimeStampsPerFrame, _point, nextValue, styleHelper));
		}
		public void ReadXml(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback, long _iAverageTimeStampsPerFrame)
		{
		    _xmlReader.ReadStartElement();
		    
		    while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "DrawingStyle":
				        style = new DrawingStyle(_xmlReader);
						BindStyle();
						break;
		            case "AutoNumber":
                        AutoNumber number = new AutoNumber(_xmlReader, _scale, _remapTimestampCallback, _iAverageTimeStampsPerFrame, styleHelper);
                        InsertSorted(number);
						break;
		            default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}	
		    
		    _xmlReader.ReadEndElement();
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
		private int NextValue(long _iPosition)
		{
		    if(autoNumbers.Count == 0)
		    {
		        return 1;
		    }
		    else
		    {
		        return NextValueVideo(_iPosition);
		    }
		}
		private int NextValueVideo(long _iPosition)
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


