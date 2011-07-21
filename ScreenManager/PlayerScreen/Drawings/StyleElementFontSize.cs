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
using System.Windows.Forms;
using System.Xml;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Style element to represent the size of font used by the drawing.
	/// Editor: regular combo box.
	/// </summary>
	public class StyleElementFontSize : AbstractStyleElement
	{
		#region Properties
		public override object Value
		{
			get { return m_iFontSize; }
			set 
			{ 
				m_iFontSize = (value is int) ? (int)value : m_iDefaultFontSize;
				RaiseValueChanged();
			}
		}
		public override Bitmap Icon
		{
			get { return Properties.Drawings.editortext;}
		}
		public override string DisplayName
		{
			get { return ScreenManagerLang.Generic_FontSizePicker;}
		}
		#endregion
		
		#region Members
		private int m_iFontSize;
		private static readonly int m_iDefaultFontSize = 10;
		private static readonly string[] AllowedFontSizes = { "8", "9", "10", "11", "12", "14", "16", "18", "20", "24", "28", "32", "36" };
		#endregion
		
		#region Constructor
		public StyleElementFontSize(int _default)
		{
			m_iFontSize = _default;
		}
		#endregion

		#region Public Methods
		public override Control GetEditor()
		{
			ComboBox editor = new ComboBox();
			editor.DropDownStyle = ComboBoxStyle.DropDownList;
			editor.Items.AddRange(AllowedFontSizes);
			editor.Text = m_iFontSize.ToString();
			editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
			return editor;
		}
		public override AbstractStyleElement Clone()
		{
			AbstractStyleElement clone = new StyleElementFontSize(m_iFontSize);
			clone.Bind(this);
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
		private void editor_SelectedIndexChanged(object sender, EventArgs e)
		{
			int i;
			bool parsed = int.TryParse(((ComboBox)sender).Text, out i);
			m_iFontSize = parsed ? i : m_iDefaultFontSize;
			RaiseValueChanged();
			((ComboBox)sender).Text = m_iFontSize.ToString();
		}
		#endregion
	}
}
