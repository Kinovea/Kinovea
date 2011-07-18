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
using System.Drawing;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Wraps a tool description and its style preset. Used in the preset dialog.
	/// </summary>
	public class ToolStylePreset
	{
		#region Properties
		public string ToolDisplayName
		{
			get { return m_ToolDisplayName; }
		}
		public string ToolInternalName
		{
			get { return m_ToolInternalName; }
		}
		public Bitmap ToolIcon
		{
			get { return m_ToolIcon; }
		}
		public DrawingStyle Style
		{
			get { return m_Style; }
			set { m_Style = value;}
		}
		#endregion
		
		#region Members
		private string m_ToolDisplayName = "";
		private string m_ToolInternalName = "";
		private Bitmap m_ToolIcon;
		private DrawingStyle m_Style;
		#endregion
		
		#region Constructor
		public ToolStylePreset(AbstractDrawingTool _tool)
		{
			m_ToolDisplayName = _tool.DisplayName;
			m_ToolInternalName = _tool.InternalName;
			m_ToolIcon = _tool.Icon;
			m_Style = _tool.StylePreset;
		}
		#endregion
		
		#region Public Methods
		public override string ToString()
		{
			return m_ToolDisplayName;
		}
		#endregion
	}
}
