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
using System.Windows.Forms;
using System.Xml;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

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
			get { return fontSize; }
			set 
			{ 
				fontSize = (value is int) ? (int)value : defaultFontSize;
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
		public override string XmlName
		{
			get { return "FontSize";}
		}
		#endregion
		
		#region Members
		private int fontSize;
		private static readonly int defaultFontSize = 10;
		private static readonly string[] options = { "8", "9", "10", "11", "12", "14", "16", "18", "20", "24", "28", "32", "36" };
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public StyleElementFontSize(int givenDefault)
		{
			fontSize = (Array.IndexOf(options, givenDefault.ToString()) >= 0) ? givenDefault : defaultFontSize;
		}
		public StyleElementFontSize(XmlReader xmlReader)
		{
			ReadXML(xmlReader);
		}
		#endregion

		#region Public Methods
		public override Control GetEditor()
		{
			ComboBox editor = new ComboBox();
			editor.DropDownStyle = ComboBoxStyle.DropDownList;
			editor.Items.AddRange(options);
			editor.SelectedIndex = Array.IndexOf(options, fontSize.ToString());
			editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
			return editor;
		}
		public override AbstractStyleElement Clone()
		{
			AbstractStyleElement clone = new StyleElementFontSize(fontSize);
			clone.Bind(this);
			return clone;
		}
		public override void ReadXML(XmlReader xmlReader)
		{
			xmlReader.ReadStartElement();
			string s = xmlReader.ReadElementContentAsString("Value", "");
			
			int value = defaultFontSize;
			try
			{
				TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
				value = (int)intConverter.ConvertFromString(s);
			}
			catch(Exception)
			{
			    log.ErrorFormat("An error happened while parsing XML for Font size. {0}", s);
			}
			
			// Restrict to the actual list of "athorized" values.
			fontSize = (Array.IndexOf(options, value.ToString()) >= 0) ? value : defaultFontSize;
			
			xmlReader.ReadEndElement();
		}
		public override void WriteXml(XmlWriter xmlWriter)
		{
			xmlWriter.WriteElementString("Value", fontSize.ToString());
		}
		#endregion
		
		#region Private Methods
		private void editor_SelectedIndexChanged(object sender, EventArgs e)
		{
			int i;
			bool parsed = int.TryParse(((ComboBox)sender).Text, out i);
			fontSize = parsed ? i : defaultFontSize;
			RaiseValueChanged();
			((ComboBox)sender).Text = fontSize.ToString();
		}
		#endregion
	}
}
