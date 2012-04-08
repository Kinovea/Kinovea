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

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Auto Numbers.
	/// This is the proxy object dispatching all individual numbers requests. (draw, hit testing, etc.)
	/// </summary>
	public class AutoNumberManager : AbstractMultiDrawing
	{
		#region Properties
		public override object SelectedItem {
		    get 
		    {
                if(m_iSelected >= 0 && m_iSelected < m_AutoNumbers.Count)
                    return m_AutoNumbers[m_iSelected];
                else
                    return null;
		    }
		}
        public override int Count {
		    get { return m_AutoNumbers.Count; }
        }
		
		// Fading is not currently modifiable from outside.
        public override InfosFading  infosFading
        {
            get { throw new NotImplementedException("Spotlight, The method or operation is not implemented."); }
            set { throw new NotImplementedException("Spotlight, The method or operation is not implemented."); }
        }
        public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.None; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
		}
		#endregion
		
		#region Members
		private List<AutoNumber> m_AutoNumbers = new List<AutoNumber>();
		private int m_iSelected = -1;
		#endregion
		
		#region AbstractDrawing Implementation
		public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
		{
		    foreach(AutoNumber number in m_AutoNumbers)
                number.Draw(_canvas, _transformer, _iCurrentTimestamp);
		}
		public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
		{
		    if(m_iSelected >= 0 && m_iSelected < m_AutoNumbers.Count)
				m_AutoNumbers[m_iSelected].MouseMove(_deltaX, _deltaY);
		}
		public override void MoveHandle(Point point, int handleNumber, Keys modifiers)
		{
		    if(m_iSelected >= 0 && m_iSelected < m_AutoNumbers.Count)
				m_AutoNumbers[m_iSelected].MoveHandleTo(point);
		}
		public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
		    int currentNumber = 0;
		    int handle = -1;
		    foreach(AutoNumber number in m_AutoNumbers)
		    {
		        handle = number.HitTest(_point, _iCurrentTimestamp);
		        if(handle >= 0)
		        {
		            m_iSelected = currentNumber;
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
            
		    m_AutoNumbers.Add(number);
		    m_iSelected = m_AutoNumbers.Count - 1;
		}
        public override void Remove(object _item)
		{
            AutoNumber number = _item as AutoNumber;
            if(number == null)
                return;
            
		    m_AutoNumbers.Remove(number);
		    m_iSelected = -1;
		}
        public override void Clear()
        {
            m_AutoNumbers.Clear();
            m_iSelected = -1;
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
		    
			m_AutoNumbers.Add(new AutoNumber(_iPosition, _iAverageTimeStampsPerFrame, _point, nextValue));
			m_iSelected = m_AutoNumbers.Count - 1;
		}
		#endregion
		
		private int NextValue(long _iPosition)
		{
		    if(m_AutoNumbers.Count == 0)
		    {
		        return 1;
		    }
		    else
		    {
		        int prev = m_AutoNumbers[0].Value;
		        
		        // TODO: handle reset points.
		        
		        for(int i=1;i<m_AutoNumbers.Count;i++)
		        {
		            if(m_AutoNumbers[i].Value > m_AutoNumbers[i-1].Value + 1)
		            {
		                //hole = true;
		                prev = m_AutoNumbers[i-1].Value;
		                break;
		            }
		            else
		            {
		                prev = m_AutoNumbers[i].Value;
		            }
		        }
		        
		        return prev + 1;
		    }
		        
		        
		        //int maxValue = 9;
		        
		        // Get the highest visible number, and increment.
		        /*int highestValue = 0;
		        foreach(AutoNumber number in m_AutoNumbers)
                {
		            if(number.IsVisible(_iPosition))
		               highestValue = Math.Max(highestValue, number.Value);
		        }
		        
		        // If no number is visible, reset.
		        //if(highestValue == 0)
		           //highestValue = m_AutoNumbers[m_AutoNumbers.Count - 1].Value;
		        
		        int next = highestValue + 1;
		        
		        //if(next > maxValue)
		          //  next = 1;* /
		        
		        return next;
		    }*/
		}
	}
}


