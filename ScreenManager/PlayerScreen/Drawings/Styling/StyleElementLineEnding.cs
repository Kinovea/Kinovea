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

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Style element to represent line endings (for arrows).
	/// Editor: owner drawn combo box.
	/// </summary>
	public class StyleElementLineEnding : AbstractStyleElement
	{
		#region Properties
		public override object Value
		{
			get { return m_LineEnding; }
			set 
			{ 
				m_LineEnding = (value is LineEnding) ? (LineEnding)value : LineEnding.None;
				RaiseValueChanged();
			}
		}
		public override Bitmap Icon
		{
			get { return Properties.Drawings.arrows;}
		}
		public override string DisplayName
		{
			get { return ScreenManagerLang.Generic_ArrowPicker;}
		}
		public override string XmlName
		{
			get { return "Arrows";}
		}
		#endregion
		
		#region Members
		private LineEnding m_LineEnding;
		private static readonly int m_iLineWidth = 6;
		private static readonly LineEnding[] m_Options = { LineEnding.None, LineEnding.StartArrow, LineEnding.EndArrow, LineEnding.DoubleArrow };
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public StyleElementLineEnding(LineEnding _default)
		{
			m_LineEnding = (Array.IndexOf(m_Options, _default) >= 0) ? _default : LineEnding.None;
		}
		public StyleElementLineEnding(XmlReader _xmlReader)
		{
			ReadXML(_xmlReader);
		}
		#endregion
		
		#region Public Methods
		public override Control GetEditor()
		{
			ComboBox editor = new ComboBox();
			editor.DropDownStyle = ComboBoxStyle.DropDownList;
			editor.ItemHeight = 15;
			editor.DrawMode = DrawMode.OwnerDrawFixed;
			for(int i=0;i<m_Options.Length;i++) editor.Items.Add(new object());
			editor.SelectedIndex = Array.IndexOf(m_Options, m_LineEnding);
			editor.DrawItem += new DrawItemEventHandler(editor_DrawItem);
			editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
			return editor;
		}
		public override AbstractStyleElement Clone()
		{
			AbstractStyleElement clone = new StyleElementLineEnding(m_LineEnding);
			clone.Bind(this);
			return clone;
		}
		public override void ReadXML(XmlReader _xmlReader)
		{
			_xmlReader.ReadStartElement();
			string s = _xmlReader.ReadElementContentAsString("Value", "");
			
			LineEnding value = LineEnding.None;
			try
			{
				TypeConverter lineEndingConverter = TypeDescriptor.GetConverter(typeof(LineEnding));
				value = (LineEnding)lineEndingConverter.ConvertFromString(s);
			}
			catch(Exception)
			{
				log.ErrorFormat("An error happened while parsing XML for Line ending. {0}", s);
			}
			
			// Restrict to the actual list of "athorized" values.
			m_LineEnding = (Array.IndexOf(m_Options, value) >= 0) ? value : LineEnding.None;
			
			_xmlReader.ReadEndElement();
		}
		public override void WriteXml(XmlWriter _xmlWriter)
		{
			TypeConverter converter = TypeDescriptor.GetConverter(m_LineEnding);
			string s = converter.ConvertToString(m_LineEnding);
			_xmlWriter.WriteElementString("Value", s);
		}
		#endregion
		
		#region Private Methods
		private void editor_DrawItem(object sender, DrawItemEventArgs e)
		{
			if(e.Index >= 0 && e.Index < m_Options.Length)
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				
				Pen p = new Pen(Color.Black, m_iLineWidth);
				p.StartCap = m_Options[e.Index].StartCap;
				p.EndCap = m_Options[e.Index].EndCap;
				
				int top = e.Bounds.Height / 2;
				
				e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Top + top, e.Bounds.Left + e.Bounds.Width, e.Bounds.Top + top);
				p.Dispose();
			}
		}
		private void editor_SelectedIndexChanged(object sender, EventArgs e)
		{
			int index = ((ComboBox)sender).SelectedIndex;
			if( index >= 0 && index < m_Options.Length)
			{
				m_LineEnding = m_Options[index];
				RaiseValueChanged();
			}
		}
		#endregion
	}
}
