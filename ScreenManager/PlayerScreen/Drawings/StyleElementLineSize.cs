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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Style element to represent line width.
	/// Editor: owner drawn combo box.
	/// Very similar to StyleElementPenSize, just the rendering changes. (lines vs circles)
	/// </summary>
	public class StyleElementLineSize : AbstractStyleElement
	{
		#region Properties
		public override object Value
		{
			get { return m_iPenSize; }
			set 
			{ 
				m_iPenSize = (value is int) ? (int)value : m_iDefaultSize;
				RaiseValueChanged();
			}
		}
		public override Bitmap Icon
		{
			get { return Properties.Drawings.linesize;}
		}
		public override string DisplayName
		{
			get { return "Line size :";}
		}
		public override string XmlName
		{
			get { return "LineSize";}
		}
		#endregion
		
		#region Members
		private static readonly int[] m_Options = { 2, 3, 4, 5, 7, 9, 11, 13 };
		private static readonly int m_iDefaultSize = 3;
		private int m_iPenSize;
		#endregion
		
		#region Constructor
		public StyleElementLineSize(int _default)
		{
			m_iPenSize = (Array.IndexOf(m_Options, _default) >= 0) ? _default : m_iDefaultSize;
		}
		public StyleElementLineSize(XmlReader _xmlReader)
		{
			ReadXML(_xmlReader);
		}
		#endregion
		
		#region Public Methods
		public override Control GetEditor()
		{
			ComboBox editor = new ComboBox();
			editor.DropDownStyle = ComboBoxStyle.DropDownList;
			editor.ItemHeight = m_Options[m_Options.Length-1] + 4;
			editor.DrawMode = DrawMode.OwnerDrawFixed;
			foreach(int i in m_Options) editor.Items.Add(new object());
			editor.SelectedIndex = Array.IndexOf(m_Options, m_iPenSize);
			editor.DrawItem += new DrawItemEventHandler(editor_DrawItem);
			editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
			return editor;
		}
		public override AbstractStyleElement Clone()
		{
			AbstractStyleElement clone = new StyleElementLineSize(m_iPenSize);
			clone.Bind(this);
			return clone;
		}
		public override void ReadXML(XmlReader _xmlReader)
		{
			_xmlReader.ReadStartElement();
			string s = _xmlReader.ReadElementContentAsString("Value", "");
			
			int value = m_iDefaultSize;
			try
			{
				TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
				value = (int)intConverter.ConvertFromString(s);
			}
			catch(Exception)
			{
				// The input XML couldn't be parsed. Keep the default value.
			}
			
			// Restrict to the actual list of "athorized" values.
			m_iPenSize = (Array.IndexOf(m_Options, value) >= 0) ? value : m_iDefaultSize;
			
			_xmlReader.ReadEndElement();
		}
		public override void WriteXml(XmlWriter _xmlWriter)
		{
			_xmlWriter.WriteElementString("Value", m_iPenSize.ToString());
		}
		#endregion
		
		#region Private Methods
		private void editor_DrawItem(object sender, DrawItemEventArgs e)
		{
			if(e.Index >= 0 && e.Index < m_Options.Length)
			{
				int itemPenSize = m_Options[e.Index];
				int top = (e.Bounds.Height - itemPenSize) / 2;
				e.Graphics.FillRectangle(Brushes.Black, e.Bounds.Left, e.Bounds.Top + top, e.Bounds.Width, itemPenSize);
			}
		}
		private void editor_SelectedIndexChanged(object sender, EventArgs e)
		{
			int index = ((ComboBox)sender).SelectedIndex;
			if( index >= 0 && index < m_Options.Length)
			{
				m_iPenSize = m_Options[index];
				RaiseValueChanged();
			}
		}
		#endregion
	}
}
