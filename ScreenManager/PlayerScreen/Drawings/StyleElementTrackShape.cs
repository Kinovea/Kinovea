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

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Style element to represent track line shape.
	/// Editor: owner drawn combo box.
	/// </summary>
	public class StyleElementTrackShape : AbstractStyleElement
	{
		#region Properties
		public override object Value
		{
			get { return m_TrackShape; }
			set 
			{ 
				m_TrackShape = (value is TrackShape) ? (TrackShape)value : TrackShape.Solid;
				RaiseValueChanged();
			}
		}
		public override Bitmap Icon
		{
			get { return Properties.Drawings.trackshape;}
		}
		public override string DisplayName
		{
			get { return "Track shape :";}
		}
		public override string XmlName
		{
			get { return "TrackShape";}
		}
		#endregion
		
		#region Members
		private TrackShape m_TrackShape;
		private static readonly int m_iLineWidth = 3;
		private static readonly TrackShape[] m_Options = { TrackShape.Solid, TrackShape.Dash, TrackShape.SolidSteps, TrackShape.DashSteps };
		#endregion
		
		#region Constructor
		public StyleElementTrackShape(TrackShape _default)
		{
			m_TrackShape = (Array.IndexOf(m_Options, _default) >= 0) ? _default : TrackShape.Solid;
		}
		public StyleElementTrackShape(XmlReader _xmlReader)
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
			editor.SelectedIndex = Array.IndexOf(m_Options, m_TrackShape);
			editor.DrawItem += new DrawItemEventHandler(editor_DrawItem);
			editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
			return editor;
		}
		public override AbstractStyleElement Clone()
		{
			AbstractStyleElement clone = new StyleElementTrackShape(m_TrackShape);
			clone.Bind(this);
			return clone;
		}
		public override void ReadXML(XmlReader _xmlReader)
		{
			_xmlReader.ReadStartElement();
			string s = _xmlReader.ReadElementContentAsString("Value", "");
			
			TrackShape value = TrackShape.Solid;
			try
			{
				TypeConverter trackShapeConverter = TypeDescriptor.GetConverter(typeof(TrackShape));
				value = (TrackShape)trackShapeConverter.ConvertFromString(s);
			}
			catch(Exception)
			{
				// The input XML couldn't be parsed. Keep the default value.
			}
			
			// Restrict to the actual list of "athorized" values.
			m_TrackShape = (Array.IndexOf(m_Options, value) >= 0) ? value : TrackShape.Solid;
			
			_xmlReader.ReadEndElement();
		}
		public override void WriteXml(XmlWriter _xmlWriter)
		{
			TypeConverter converter = TypeDescriptor.GetConverter(m_TrackShape);
			string s = converter.ConvertToString(m_TrackShape);
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
				p.DashStyle = m_Options[e.Index].DashStyle;
				
				int top = e.Bounds.Height / 2;
				
				e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Top + top, e.Bounds.Left + e.Bounds.Width, e.Bounds.Top + top);
				
				if(m_Options[e.Index].ShowSteps)
				{
					Pen stepPen = new Pen(Color.Black, 2);
	            	int margin = (int)(m_iLineWidth * 1.5);
	            	int diameter = margin *2;
	            	int left = e.Bounds.Width / 2;
	            	e.Graphics.DrawEllipse(stepPen, e.Bounds.Left + left - margin, e.Bounds.Top + top - margin, diameter, diameter);
	            	stepPen.Dispose();
				}
				
				p.Dispose();
			}
		}
		private void editor_SelectedIndexChanged(object sender, EventArgs e)
		{
			int index = ((ComboBox)sender).SelectedIndex;
			if( index >= 0 && index < m_Options.Length)
			{
				m_TrackShape = m_Options[index];
				RaiseValueChanged();
			}
		}
		#endregion
	}
}
