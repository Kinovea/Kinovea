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
	/// Host of the list of tools. 
	/// </summary>
	public class ToolManager
	{
		#region Properties
		/// <summary>
		///   Returns the cached list of tools.
		/// </summary>
		public static Dictionary<string, AbstractDrawingTool> Tools
		{
			get 
			{
				if (object.ReferenceEquals(m_Tools, null))
				{
					Initialize();
				}
				
				return m_Tools; 
			}
		}
		
		// Maybe we could find a way to generate this list of properties automatically.
		// A custom tool in the vein of the ResXFileCodeGenerator that would take an XML file in,
		// and creates a set of accessor properties.
		public static DrawingToolAngle2D Angle
		{
			get { return (DrawingToolAngle2D)Tools["Angle"]; }
		}
		public static DrawingToolChrono Chrono
		{
			get { return (DrawingToolChrono)Tools["Chrono"]; }
		}
		public static DrawingToolCircle Circle
		{
			get { return (DrawingToolCircle)Tools["Circle"]; }
		}
		public static DrawingToolCross2D CrossMark
		{
			get { return (DrawingToolCross2D)Tools["CrossMark"]; }
		}
		public static DrawingToolLine2D Line
		{
			get { return (DrawingToolLine2D)Tools["Line"]; }
		}
		public static DrawingToolPencil Pencil
		{
			get { return (DrawingToolPencil)Tools["Pencil"]; }
		}
		public static DrawingToolText Label
		{
			get { return (DrawingToolText)Tools["Label"]; }
		}
		public static DrawingToolPlane Plane
		{
			get { return (DrawingToolPlane)Tools["Plane"]; }
		}
		public static DrawingToolMagnifier Magnifier
		{
			get { return (DrawingToolMagnifier)Tools["Magnifier"]; }
		}
		#endregion
		
		#region Members
		private static Dictionary<string, AbstractDrawingTool> m_Tools = null;
		#endregion
        
        #region Private Methods
        private static void Initialize()
        {
        	m_Tools = new Dictionary<string, AbstractDrawingTool>();
        	
        	// The core drawing tools are loaded statically.
        	// Maybe in the future we can have a plug-in system with .dll containing extensions tools.
        	// Note that the pointer "tool" is not listed, as each screen must have its own.
        	m_Tools.Add("Angle", new DrawingToolAngle2D());
        	m_Tools.Add("Chrono", new DrawingToolChrono());
        	m_Tools.Add("Circle", new DrawingToolCircle());
        	m_Tools.Add("CrossMark", new DrawingToolCross2D());
        	m_Tools.Add("Line", new DrawingToolLine2D());
        	m_Tools.Add("Pencil", new DrawingToolPencil());
        	m_Tools.Add("Label", new DrawingToolText());
        	m_Tools.Add("Plane", new DrawingToolPlane());
        	m_Tools.Add("Magnifier", new DrawingToolMagnifier());
        }
        #endregion
	}
}
