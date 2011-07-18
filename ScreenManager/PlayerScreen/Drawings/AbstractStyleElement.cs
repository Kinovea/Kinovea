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
	/// A styling property for a drawing / drawing tool.
	/// Concrete style elements related to a drawing are then collected in a DrawingStyle object.
	/// </summary>
	/// <remarks>
	/// If two variables are needed to represent a style element, consider creating a composite style.
	/// For exemple, if x and y (e.g: back color and font size) MUST be edited together,
	/// create a class exposing x and y in a meaningful way, and use it as a whole as a style element..
	/// Otherwise, if x and y can be edited separately, add two separate style elements to the DrawingStyle.
	/// Bottom line: a single value will have to be picked from a graphical editor.
	/// </remarks>
	public abstract class AbstractStyleElement
	{
		/// <summary>
    	/// The current value of the style element.
    	/// </summary>
    	public abstract object Value
    	{
    		get;
    		set;
    	}
    	
    	public abstract Control GetEditor();
    	public abstract AbstractStyleElement Clone();
    	public abstract void WriteXml(XmlWriter _xmlWriter);
    	public abstract void ReadXML(XmlReader _xmlReader);
	}
}
