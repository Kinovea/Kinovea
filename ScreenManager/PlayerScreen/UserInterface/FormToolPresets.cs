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
		private bool m_bManualClose;
		private AbstractStyleElement m_firstElement;
		private AbstractStyleElement m_secondElement;
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
			foreach(KeyValuePair<string, AbstractDrawingTool> pair in tm.Tools)
			{
				if(pair.Value.StylePreset != null && pair.Value.StylePreset.Elements.Count > 0)
				{
					lstPresets.Items.Add(new ToolStylePreset(pair.Value));
					pair.Value.StylePreset.Memorize();
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
			btnToolIcon.BackColor = Color.Transparent;
			btnToolIcon.Image = _preset.ToolIcon;
			lblToolName.Text = _preset.ToolDisplayName;
			
			// Layout depends on the list of style elements.
			// Currently we only support 2 editable style elements.
			// Example: pencil tool has a color picker and a pen size picker.
			
			m_firstElement = null;
			m_secondElement = null;
			
			foreach(KeyValuePair<string, AbstractStyleElement> styleElement in _preset.Style.Elements)
			{
				if(m_firstElement == null)
				{
					m_firstElement = styleElement.Value;
				}
				else if(m_secondElement == null)
				{
					m_secondElement = styleElement.Value;
				}
				else
				{
					log.DebugFormat("Discarding style element: \"{0}\". (Only 2 style elements supported).", styleElement.Key);
				}
			}
			
			// Configure editor line for each element.
			// The style element is responsible for updating the internal value and the editor appearance.
			// The element internal value might also be bound to a style helper property so that the underlying drawing will get updated.
			
			// Clean up
			btnFirstElement.Visible = false;
			lblFirstElement.Visible = false;
			grpConfig.Controls.RemoveByKey("firstEditor");
			btnSecondElement.Visible = false;
			lblSecondElement.Visible = false;
			grpConfig.Controls.RemoveByKey("secondEditor");
			
			if(m_firstElement != null)
			{
				btnFirstElement.BackColor = Color.Transparent;
				btnFirstElement.Image = m_firstElement.Icon;
				btnFirstElement.Visible = true;
				lblFirstElement.Text = m_firstElement.DisplayName;
				lblFirstElement.Visible = true;
				
				int editorsLeft = 150; // works in High DPI ?
				
				Control firstEditor = m_firstElement.GetEditor();
				firstEditor.Size = new Size(50, 20);
				firstEditor.Location = new Point(editorsLeft, btnFirstElement.Top);
				firstEditor.Name = "firstEditor";
				grpConfig.Controls.Add(firstEditor);
				
				if(m_secondElement != null)
				{
					btnSecondElement.BackColor = Color.Transparent;
					btnSecondElement.Image = m_secondElement.Icon;
					btnSecondElement.Visible = true;
					lblSecondElement.Text = m_secondElement.DisplayName;
					lblSecondElement.Visible = true;

					Control secondEditor = m_secondElement.GetEditor();
					secondEditor.Size = new Size(50, 20);
					secondEditor.Location = new Point(editorsLeft, btnSecondElement.Top);
					secondEditor.Name = "secondEditor";
					grpConfig.Controls.Add(secondEditor);
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
			foreach(KeyValuePair<string, AbstractDrawingTool> pair in tm.Tools)
			{
				pair.Value.ResetToDefaultStyle();
			}
			
			LoadPresets();
		}
		private void Form_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(!m_bManualClose)
			{
				Revert();
			}
		}
		private void Revert()
		{
			// Revert to memos
			ToolManager tm = ToolManager.Instance();
			foreach(KeyValuePair<string, AbstractDrawingTool> pair in tm.Tools)
			{
				pair.Value.StylePreset.Revert();	
			}
		}
		private void BtnCancel_Click(object sender, EventArgs e)
		{	
			Revert();
			m_bManualClose = true;
		}
		private void BtnOK_Click(object sender, EventArgs e)
		{
			m_bManualClose = true;	
		}
		#endregion
		
		
	}
}
