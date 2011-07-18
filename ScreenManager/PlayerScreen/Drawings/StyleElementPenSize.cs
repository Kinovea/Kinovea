#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Style element to represent a pen size.
	/// Editor: owner drawn combo box.
	/// Very similar to StyleElementLineStyle, just the rendering changes. (lines vs circles)
	/// </summary>
	public class StyleElementPenSize : AbstractStyleElement
	{
		#region Properties
		public override object Value
		{
			get { return m_iPenSize; }
			set { m_iPenSize = (value is int) ? (int)value : m_iDefaultSize;}
		}
		#endregion
		
		#region Members
		private static readonly int[] m_Sizes = { 2, 3, 4, 5, 7, 9, 11, 13, 16, 19, 22, 25 };
		private static readonly int m_iDefaultSize = 3;
		private int m_iPenSize;
		#endregion
		
		#region Constructor
		public StyleElementPenSize(int _default)
		{
			m_iPenSize = (_default >= m_Sizes[0] && _default <= m_Sizes[m_Sizes.Length-1]) ? _default : m_iDefaultSize;
		}
		#endregion
		
		#region Public Methods
		public override Control GetEditor()
		{
			ComboBox editor = new ComboBox();
			editor.DropDownStyle = ComboBoxStyle.DropDownList;
			editor.ItemHeight = m_Sizes[m_Sizes.Length-1] + 2;
			editor.DrawMode = DrawMode.OwnerDrawFixed;
			foreach(int i in m_Sizes) editor.Items.Add(new object());
			editor.DrawItem += new DrawItemEventHandler(editor_DrawItem);
			editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
			
			// Set current value
			int index = -1;
			for(int i=0;i<m_Sizes.Length;i++)
			{
				if(m_iPenSize == m_Sizes[i])
					index = i;
			}
			editor.SelectedIndex = index;
			
			return editor;
		}
		public override AbstractStyleElement Clone()
		{
			AbstractStyleElement clone = new StyleElementPenSize(m_iPenSize);
			return clone;
		}
		public override void ReadXML(XmlReader _xmlReader)
		{
			throw new NotImplementedException();
		}
		public override void WriteXml(XmlWriter _xmlWriter)
		{
			throw new NotImplementedException();
		}
		#endregion
		
		#region Private Methods
		private void editor_DrawItem(object sender, DrawItemEventArgs e)
		{
			if(e.Index >= 0 && e.Index < m_Sizes.Length)
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				int itemPenSize = m_Sizes[e.Index];
				int left = (e.Bounds.Width - itemPenSize) / 2;
				int top = (e.Bounds.Height - itemPenSize) / 2;
				e.Graphics.FillEllipse(Brushes.Black, e.Bounds.Left + left, e.Bounds.Top + top, itemPenSize, itemPenSize);
			}
		}
		private void editor_SelectedIndexChanged(object sender, EventArgs e)
		{
			int index = ((ComboBox)sender).SelectedIndex;
			if( index >= 0 && index < m_Sizes.Length)
			{
				m_iPenSize = m_Sizes[index];
			}
		}
		#endregion
	}
}
