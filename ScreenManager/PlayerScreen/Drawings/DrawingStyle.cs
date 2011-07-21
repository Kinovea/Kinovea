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
	/// <remarks>
	/// Currently the UI will only allow for at most 2 elements to be edited per style.
	/// Use composite style elements when several style properties must be edited at once.
	/// </remarks>
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
		
		public DrawingStyle Clone()
		{
			DrawingStyle clone = new DrawingStyle();
			foreach(KeyValuePair<string, AbstractStyleElement> element in m_StyleElements)
			{
				clone.Elements.Add(element.Key, element.Value.Clone());
			}
			return clone;
		}
		public void WriteXml(XmlWriter _xmlWriter)
		{
			foreach(KeyValuePair<string, AbstractStyleElement> element in m_StyleElements)
			{
				element.Value.WriteXml(_xmlWriter);
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
			AbstractStyleElement elem = m_StyleElements[_source];
			if(elem != null)
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
		}
		
	}
}
