#region License
/*
Copyright © Joan Charmant 2011.
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Style element to represent a color used by the drawing.
	/// Editor: clickable area giving access to a color picker dialog.
	/// </summary>
	public class StyleElementColor : AbstractStyleElement
	{
		#region Properties
		public override object Value
		{
			get { return value; }
			set 
			{ 
				this.value = (value is Color) ? (Color)value : defaultValue;
				ExportValueToData();
			}
		}
		public override Bitmap Icon
		{
			get { return Properties.Drawings.editorcolor;}
		}
		public override string DisplayName
		{
			get { return displayName;}
		}
		public override string XmlName
		{
			get { return "Color";}
		}
        #endregion

        public static readonly Color defaultValue = Color.Black;

		#region Members
		private Color value;
        private string displayName = ScreenManagerLang.Generic_ColorPicker;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public StyleElementColor(Color initialValue)
		{
			value = initialValue;
		}
        public StyleElementColor(Color initialValue, string displayName)
        {
            value = initialValue;
            this.displayName = displayName;
        }
        public StyleElementColor(XmlReader xmlReader)
		{
			ReadXML(xmlReader);
		}
		#endregion
		
		#region Public Methods
		public override Control GetEditor()
		{
			Control editor = new Control();
			editor.Click += new EventHandler(editor_Click);
			editor.Paint += new PaintEventHandler(editor_Paint);
			return editor;
		}
		public override AbstractStyleElement Clone()
		{
			AbstractStyleElement clone = new StyleElementColor(value);
			clone.BindClone(this);
			return clone;
		}
		public override void ReadXML(XmlReader xmlReader)
		{
		    // Do not use the .NET color converter at reading time either, it breaks on some installations.
			xmlReader.ReadStartElement();
			string s = xmlReader.ReadElementContentAsString("Value", "");
			value = XmlHelper.ParseColor(s, Color.Black);
			xmlReader.ReadEndElement();
		}
		public override void WriteXml(XmlWriter xmlWriter)
		{
			// Note: we don't use the .NET color converter at writing time.
			// The color would be translated to its display name (DarkOliveGreen, CadetBlue, etc.), which is not portable.
			string s = String.Format("{0};{1};{2};{3}", value.A, value.R, value.G, value.B);
			xmlWriter.WriteElementString("Value", s);
		}
		#endregion
		
		#region Private Methods
		private void editor_Paint(object sender, PaintEventArgs e)
		{
			using(SolidBrush b = new SolidBrush(value))
			{
				e.Graphics.FillRectangle(b, e.ClipRectangle);
				e.Graphics.DrawRectangle(Pens.LightGray, e.ClipRectangle.Left, e.ClipRectangle.Top, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);	
			}
		}
		private void editor_Click(object sender, EventArgs e)
		{
			FormColorPicker picker = new FormColorPicker(value);
			FormsHelper.Locate(picker);
			if (picker.ShowDialog() == DialogResult.OK)
			{
				value = picker.PickedColor;
				ExportValueToData();
				((Control)sender).Invalidate();
			}

			picker.Dispose();
		}	
		#endregion
	}
}
