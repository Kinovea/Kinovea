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
using System.Collections.Generic;
using System.Xml;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Represents the styling elements of a drawing or drawing tool preset.
	/// Host a list of style elements needed to decorate the drawing.
	/// </summary>
	public class DrawingStyle
	{
		#region Properties
		public Dictionary<string, AbstractStyleElement> Elements
		{
			get { return m_StyleElements; }
		}
		#endregion
		
		#region Members
		private Dictionary<string, AbstractStyleElement> m_StyleElements = new Dictionary<string, AbstractStyleElement>();
		private Dictionary<string, AbstractStyleElement> m_Memo = new Dictionary<string, AbstractStyleElement>();
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public DrawingStyle(){}
		public DrawingStyle(XmlReader _xmlReader)
		{
			ReadXml(_xmlReader);
		}
		#endregion
		
		#region Public Methods
		public DrawingStyle Clone()
		{
			DrawingStyle clone = new DrawingStyle();
			foreach(KeyValuePair<string, AbstractStyleElement> element in m_StyleElements)
			{
				clone.Elements.Add(element.Key, element.Value.Clone());
			}
			return clone;
		}
		public void ReadXml(XmlReader _xmlReader)
		{			
			m_StyleElements.Clear();
			
			_xmlReader.ReadStartElement();	// <ToolPreset Key="ToolName"> or <DrawingStyle>
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				AbstractStyleElement styleElement = null;
				string key = _xmlReader.GetAttribute("Key");
				
				switch(_xmlReader.Name)
				{
					case "Color":
						styleElement = new StyleElementColor(_xmlReader);
						break;
					case "FontSize":
						styleElement = new StyleElementFontSize(_xmlReader);
						break;
					case "PenSize":
						styleElement = new StyleElementPenSize(_xmlReader);
						break;
					case "LineSize":
						styleElement = new StyleElementLineSize(_xmlReader);
						break;
					case "Arrows":
						styleElement = new StyleElementLineEnding(_xmlReader);
						break;
					case "TrackShape":
						styleElement = new StyleElementTrackShape(_xmlReader);
						break;	
					default:
						log.ErrorFormat("Could not import style element \"{0}\"", _xmlReader.Name);
						log.ErrorFormat("Content was: {0}", _xmlReader.ReadOuterXml());
						break;
				}
				
				if(styleElement != null)
				{
					m_StyleElements.Add(key, styleElement);
				}
			}
			
			_xmlReader.ReadEndElement();
		}
		public void WriteXml(XmlWriter _xmlWriter)
		{
			foreach(KeyValuePair<string, AbstractStyleElement> element in m_StyleElements)
			{
				_xmlWriter.WriteStartElement(element.Value.XmlName);
				_xmlWriter.WriteAttributeString("Key", element.Key);
				element.Value.WriteXml(_xmlWriter);
				_xmlWriter.WriteEndElement();
			}
		}
		
		/// <summary>
		/// Binds a property in the style helper to an editable style element.
		/// Once bound, each time the element is edited in the UI, the property is updated,
		/// so the actual drawing automatically changes its style.
		/// 
		/// Style elements and properties need not be of the same type. The style helper knows how to
		/// map a FontSize element to its own Font property for example.
		/// </summary>
		/// <param name="_target">The drawing's style helper object</param>
		/// <param name="_targetProperty">The name of the property in the style helper that needs automatic update</param>
		/// <param name="_source">The style element that will push its change to the property</param>
		public void Bind(StyleHelper _target, string _targetProperty, string _source)
		{
		    AbstractStyleElement elem;
		    bool found = m_StyleElements.TryGetValue(_source, out elem);
		    if(found && elem != null)
		    {
                elem.Bind(_target, _targetProperty);
		    }
		    else
			{
				log.ErrorFormat("The element \"{0}\" was not found.", _source);
			}
		}
		public void RaiseValueChanged()
		{
			foreach(KeyValuePair<string, AbstractStyleElement> element in m_StyleElements)
			{
				element.Value.RaiseValueChanged();
			}
		}
		public void ReadValue()
		{
			foreach(KeyValuePair<string, AbstractStyleElement> element in m_StyleElements)
			{
				element.Value.ReadValue();
			}
		}
		public void Memorize()
		{
			m_Memo.Clear();
			foreach(KeyValuePair<string, AbstractStyleElement> element in m_StyleElements)
			{
				m_Memo.Add(element.Key, element.Value.Clone());
			}
		}
		public void Memorize(DrawingStyle _memo)
		{
			// This is used when the whole DrawingStyle has been recreated and we want it to 
			// remember its state before the recreation.
			// Used for style presets to carry the memo after XML load.
			m_Memo.Clear();
			foreach(KeyValuePair<string, AbstractStyleElement> element in _memo.Elements)
			{
				m_Memo.Add(element.Key, element.Value.Clone());
			}
		}
		public void Revert()
		{
			m_StyleElements.Clear();
			foreach(KeyValuePair<string, AbstractStyleElement> element in m_Memo)
			{
				m_StyleElements.Add(element.Key, element.Value.Clone());
			}
		}
		public void Dump()
		{
			foreach(KeyValuePair<string, AbstractStyleElement> element in m_StyleElements)
			{
				log.DebugFormat("{0}: {1}", element.Key, element.Value.ToString());
			}
			
			foreach(KeyValuePair<string, AbstractStyleElement> element in m_Memo)
			{
				log.DebugFormat("Memo: {0}: {1}", element.Key, element.Value.ToString());
			}
		}
		#endregion
	}
}
