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
	/// This singleton class exposes the list of tools as AbstractDrawingTools.
	/// The class itself is not dependant on specifc tools, except for loading of the core tools.
	/// </summary>
	public class ToolManager
	{
		#region Properties
		public IEnumerable<AbstractDrawingTool> Tools
		{
			get { return m_Tools; }
		}
		#endregion
		
		#region Members
		private static ToolManager m_instance = null;
		private List<AbstractDrawingTool> m_Tools = new List<AbstractDrawingTool>();
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
        
		#region Constructor & Singleton
		public static ToolManager Instance()
        {
            if (m_instance == null)
            {
                m_instance = new ToolManager();
            }
            return m_instance;
        }
        private ToolManager()
        {
            LoadTools();
        }
        #endregion
        
        #region Private Methods
        private void LoadTools()
        {
        	// The core drawing tools are loaded statically.
        	// Maybe in the future we can have a plug-in system with .dll containing extensions tools.
        	// Note that the pointer "tool" is not listed here.
        	m_Tools.Add(new DrawingToolAngle2D());
        	m_Tools.Add(new DrawingToolChrono());
        	m_Tools.Add(new DrawingToolCircle());
        	m_Tools.Add(new DrawingToolCross2D());
        	m_Tools.Add(new DrawingToolLine2D());
        	m_Tools.Add(new DrawingToolPencil());
        	m_Tools.Add(new DrawingToolText());
        	
        	// some more to test the dynamics.
        	m_Tools.Add(new DrawingToolPencil());
        	m_Tools.Add(new DrawingToolText());
        	m_Tools.Add(new DrawingToolChrono());
        	m_Tools.Add(new DrawingToolCircle());
        	m_Tools.Add(new DrawingToolPencil());
        	m_Tools.Add(new DrawingToolText());
        	m_Tools.Add(new DrawingToolChrono());
        	m_Tools.Add(new DrawingToolCircle());
        }
        #endregion
        
	}
}
