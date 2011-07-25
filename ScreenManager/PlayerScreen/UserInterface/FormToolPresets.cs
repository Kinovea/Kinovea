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
		private List<AbstractStyleElement> m_Elements = new List<AbstractStyleElement>();
		private int m_iEditorsLeft;
		private AbstractDrawingTool m_Preselect;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public FormToolPresets():this(null){}
		public FormToolPresets(AbstractDrawingTool _preselect)
		{
			m_Preselect = _preselect;
			InitializeComponent();
			LoadPresets();
		}
		#endregion
		
		#region Private Methods
		private void LoadPresets()
		{
			// Load the list
			lstPresets.Items.Clear();
			int preselected = -1;
			foreach(AbstractDrawingTool tool in ToolManager.Tools.Values)
			{
				if(tool.StylePreset != null && tool.StylePreset.Elements.Count > 0)
				{
					lstPresets.Items.Add(tool);
					tool.StylePreset.Memorize();
					if(tool == m_Preselect) preselected = lstPresets.Items.Count - 1;
				}
			}
				
			if(lstPresets.Items.Count > 0)
			{
				lstPresets.SelectedIndex = preselected >= 0 ? preselected : 0;
			}
		}
		private void LoadPreset(AbstractDrawingTool _preset)
		{
			// Load a single preset
			// The layout is dynamic. The groupbox and the whole form is resized when needed on a "GrowOnly" basis.
			
			// Tool title and icon
			btnToolIcon.BackColor = Color.Transparent;
			btnToolIcon.Image = _preset.Icon;
			lblToolName.Text = _preset.DisplayName;
			
			// Clean up
			m_Elements.Clear();
			grpConfig.Controls.Clear();
			Graphics helper = grpConfig.CreateGraphics();
			
			Size editorSize = new Size(60,20);
			
			// Initialize the horizontal layout with a minimal value, 
			// it will be fixed later if some of the entries have long text.
			int minimalWidth = btnApply.Width + btnCancel.Width + 10;
			//m_iEditorsLeft = Math.Max(minimalWidth - 20 - editorSize.Width, m_iEditorsLeft);
			m_iEditorsLeft = minimalWidth - 20 - editorSize.Width;
			
			int mimimalHeight = grpConfig.Height;
			int lastEditorBottom = 10;
			
			foreach(KeyValuePair<string, AbstractStyleElement> pair in _preset.StylePreset.Elements)
			{
				AbstractStyleElement styleElement = pair.Value;
				m_Elements.Add(styleElement);
				
				//styleElement.ValueChanged += element_ValueChanged;
				
				Button btn = new Button();
				btn.Image = styleElement.Icon;
				btn.Size = new Size(20,20);
				btn.Location = new Point(10, lastEditorBottom + 15);
				btn.FlatStyle = FlatStyle.Flat;
				btn.FlatAppearance.BorderSize = 0;
				btn.BackColor = Color.Transparent;
				
				Label lbl = new Label();
				lbl.Text = styleElement.DisplayName;
				lbl.AutoSize = true;
				lbl.Location = new Point(btn.Right + 10, lastEditorBottom + 20);
				
				SizeF labelSize = helper.MeasureString(lbl.Text, lbl.Font);
				
				if(lbl.Left + labelSize.Width + 25 > m_iEditorsLeft)
				{
					// dynamic horizontal layout for high dpi and verbose languages.
					m_iEditorsLeft = (int)(lbl.Left + labelSize.Width + 25);
				}
				
				Control miniEditor = styleElement.GetEditor();
				miniEditor.Size = editorSize;
				miniEditor.Location = new Point(m_iEditorsLeft, btn.Top);
				
				lastEditorBottom = miniEditor.Bottom;
				
				grpConfig.Controls.Add(btn);
				grpConfig.Controls.Add(lbl);
				grpConfig.Controls.Add(miniEditor);
			}
			
			helper.Dispose();
			
			// Recheck all mini editors for the left positionning.
			foreach(Control c in grpConfig.Controls)
			{
				if(!(c is Label) && !(c is Button))
				{
					if(c.Left < m_iEditorsLeft) c.Left = m_iEditorsLeft;
				}
			}
			
			//grpConfig.Height = Math.Max(lastEditorBottom + 20, grpConfig.Height);
			//grpConfig.Width = Math.Max(m_iEditorsLeft + editorSize.Width + 20, grpConfig.Width);
			grpConfig.Height = Math.Max(lastEditorBottom + 20, 110);
			grpConfig.Width = m_iEditorsLeft + editorSize.Width + 20;
			lstPresets.Height = grpConfig.Bottom - lstPresets.Top;
			
			btnApply.Top = grpConfig.Bottom + 10;
			btnApply.Left = grpConfig.Right - (btnCancel.Width + 10 + btnApply.Width);
			btnCancel.Top = btnApply.Top;
			btnCancel.Left = btnApply.Right + 10;
			
			int borderLeft = this.Width - this.ClientRectangle.Width;
			this.Width = borderLeft + btnCancel.Right + 10;
			
			int borderTop = this.Height - this.ClientRectangle.Height;
			this.Height = borderTop + btnApply.Bottom + 10;
		}
		private void LstPresetsSelectedIndexChanged(object sender, EventArgs e)
		{
			AbstractDrawingTool preset = lstPresets.SelectedItem as AbstractDrawingTool;
			if(preset != null)
			{
				LoadPreset(preset);
			}
		}
		private void BtnDefaultClick(object sender, EventArgs e)
		{
			// Reset all tools to their default preset.
			foreach(AbstractDrawingTool tool in ToolManager.Tools.Values)
			{
				tool.ResetToDefaultStyle();
			}
			
			LoadPresets();
		}
		
		#region Form closing
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
			foreach(AbstractDrawingTool tool in ToolManager.Tools.Values)
			{
				tool.StylePreset.Revert();
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
		
		#endregion
		
		
	}
}
