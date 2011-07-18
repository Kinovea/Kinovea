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
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// The dialog lets the user configure the whole list of tool presets.
	/// Modifications done on the current presets, reload from file to revert.
	/// Replaces FormColorProfile.
	/// </summary>
	public partial class FormToolPresets : Form
	{
		#region Members
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public FormToolPresets()
		{
			InitializeComponent();
			LoadPresets();
		}
		#endregion
		
		#region Private Methods
		private void LoadPresets()
		{
			// Load the list
			lstPresets.Items.Clear();
			ToolManager tm = ToolManager.Instance();
			foreach(AbstractDrawingTool tool in tm.Tools)
			{
				if(tool.StylePreset != null && tool.StylePreset.Elements.Count > 0)
				{
					lstPresets.Items.Add(new ToolStylePreset(tool));
				}
			}
				
			if(lstPresets.Items.Count > 0)
			{
				lstPresets.SelectedIndex = 0;
			}
		}
		private void LoadPreset(ToolStylePreset _preset)
		{
			// Load a single preset
			btnToolIcon.Image = _preset.ToolIcon;
			lblToolName.Text = _preset.ToolDisplayName;
			
			// Layout depends on the list of style element.
			// Currently we only support 2 editable style elements.
			// Example: pencil tool has a color picker and a pen size picker.
			
			AbstractStyleElement firstElement = null;
			AbstractStyleElement secondElement = null;
			foreach(KeyValuePair<string, AbstractStyleElement> styleElement in _preset.Style.Elements)
			{
				if(firstElement == null)
				{
					firstElement = styleElement.Value;
				}
				else if(secondElement == null)
				{
					secondElement = styleElement.Value;
				}
				else
				{
					log.DebugFormat("Discarding style element: \"{0}\". (Only 2 style elements supported).", styleElement.Key);
				}
			}
			
			// Add editor for each style element.
			// The style element is responsible for updating the internal value and the editor appearance.
			grpStyle.Controls.Clear();
			
			if(firstElement != null)
			{
				Control firstEditor = firstElement.GetEditor();
				firstEditor.Size = new Size(70, 20);
				firstEditor.Location = new Point((grpStyle.Width - firstEditor.Width)/2, 15);
				grpStyle.Controls.Add(firstEditor);
				
				if(secondElement != null)
				{
					Control secondEditor = secondElement.GetEditor();
					secondEditor.Size = new Size(70, 20);
					secondEditor.Location = new Point((grpStyle.Width - secondEditor.Width)/2, firstEditor.Bottom + 10);
					grpStyle.Controls.Add(secondEditor);
				}
			}
		}
		private void LstPresetsSelectedIndexChanged(object sender, EventArgs e)
		{
			ToolStylePreset preset = lstPresets.SelectedItem as ToolStylePreset;
			if(preset != null)
			{
				LoadPreset(preset);
			}
		}
		private void BtnDefaultClick(object sender, EventArgs e)
		{
			// Reset all tools to their default preset.
			ToolManager tm = ToolManager.Instance();
			foreach(AbstractDrawingTool tool in tm.Tools)
			{
				tool.ResetToDefaultStyle();
			}
			
			LoadPresets();
		}
		#endregion
		
		
	}
}
