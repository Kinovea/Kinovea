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
using System.Windows.Forms;
using System.Xml;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Style element to represent line shape and size.
	/// </summary>
	public class StyleElementLineStyle : AbstractStyleElement
	{
		#region Properties
		public override object Value
		{
			get { return m_iPenSize; }
			set { m_iPenSize = (value is int) ? (int)value : 5;}
		}
		#endregion
		
		#region Members
		private static readonly string[] m_AllowedSizes = { "2", "3", "4", "5", "7", "9", "11", "13", "16", "19", "22", "25" };
		private static readonly int m_MinSize = int.Parse(m_AllowedSizes[0]);
		private static readonly int m_MaxSize = int.Parse(m_AllowedSizes[m_AllowedSizes.Length - 1]);
		private static readonly int m_iDefaultSize = 3;
		private int m_iPenSize;
		#endregion
		
		#region Constructor
		public StyleElementLineStyle(int _default)
		{
			m_iPenSize = (_default >= m_MinSize && _default <= m_MaxSize) ? _default : m_iDefaultSize;
		}
		#endregion
		
		#region Public Methods
		public override Control GetEditor()
		{
			// TEMPORARY : We use a simple combobox until the pen size picker is fixed.
			ComboBox editor = new ComboBox();
			editor.Items.AddRange(m_AllowedSizes);
			editor.Text = m_iPenSize.ToString();
			editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
			return editor;
		}
		public override AbstractStyleElement Clone()
		{
			AbstractStyleElement clone = new StyleElementLineStyle(m_iPenSize);
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
			int parsedSize = parsed ? i : m_iDefaultSize;
			m_iPenSize = (parsedSize >= m_MinSize && parsedSize <= m_MaxSize) ? parsedSize : m_iDefaultSize;
			((ComboBox)sender).Text = m_iPenSize.ToString();
		}	
		#endregion
	}
}
