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
		// toxml, fromxml (foreach element: toxml, fromxml)
	}
}
