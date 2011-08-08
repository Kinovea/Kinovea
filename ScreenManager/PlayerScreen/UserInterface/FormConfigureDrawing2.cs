﻿#region License
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

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Dialog that let the user change the style of a specific drawing.
	/// Works on the original and revert to a copy in case of cancel.
	/// </summary>
	public partial class FormConfigureDrawing2 : Form
	{
		#region Members
		private DrawingStyle m_Style;
		private Action m_Invalidate;
		private bool m_bManualClose;
		private List<AbstractStyleElement> m_Elements = new List<AbstractStyleElement>();
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public FormConfigureDrawing2(DrawingStyle _style, Action _invalidate)
		{
			m_Style = _style;
			m_Style.ReadValue();
			m_Style.Memorize();
			m_Invalidate = _invalidate;
			InitializeComponent();
			LocalizeForm();
			SetupForm();
		}
		#endregion
		
		#region Private Methods
		private void LocalizeForm()
		{
			this.Text = "   " + ScreenManagerLang.dlgConfigureDrawing_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
		}
		private void SetupForm()
		{
			// Dynamic layout:
			// Any number of mini editor lines. (must scale vertically)
			// High dpi vs normal dpi (scales vertically and horizontally)
			// Verbose languages (scales horizontally)
			
			// Clean up
			grpConfig.Controls.Clear();
			Graphics helper = grpConfig.CreateGraphics();
			
			Size editorSize = new Size(60,20);
			// Initialize the horizontal layout with a minimal value, 
			// it will be fixed later if some of the entries have long text.
			int minimalWidth = btnOK.Width + btnCancel.Width + 10;
			int editorsLeft = minimalWidth - 20 - editorSize.Width;
			
			int lastEditorBottom = 10;
			
			foreach(KeyValuePair<string, AbstractStyleElement> pair in m_Style.Elements)
			{
				AbstractStyleElement styleElement = pair.Value;
				m_Elements.Add(styleElement);
				
				styleElement.ValueChanged += element_ValueChanged;
				
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
				
				if(lbl.Left + labelSize.Width + 25 > editorsLeft)
				{
					// dynamic horizontal layout for high dpi and verbose languages.
					editorsLeft = (int)(lbl.Left + labelSize.Width + 25);
				}
				
				Control miniEditor = styleElement.GetEditor();
				miniEditor.Size = editorSize;
				miniEditor.Location = new Point(editorsLeft, btn.Top);
				
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
					if(c.Left < editorsLeft) c.Left = editorsLeft;
				}
			}
			
			grpConfig.Height = lastEditorBottom + 20;
			grpConfig.Width = editorsLeft + editorSize.Width + 20;
			
			btnOK.Top = grpConfig.Bottom + 10;
			btnOK.Left = grpConfig.Right - (btnCancel.Width + 10 + btnOK.Width);
			btnCancel.Top = btnOK.Top;
			btnCancel.Left = btnOK.Right + 10;
			
			int borderLeft = this.Width - this.ClientRectangle.Width;
			this.Width = borderLeft + btnCancel.Right + 10;
			
			int borderTop = this.Height - this.ClientRectangle.Height;
			this.Height = borderTop + btnOK.Bottom + 10;
		}
		private void element_ValueChanged(object sender, EventArgs e)
		{
			if(m_Invalidate != null) m_Invalidate();
		}
		
		#region Closing
		private void Form_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(!m_bManualClose)
			{
				UnhookEvents();
				Revert();
			}
		}
		private void UnhookEvents()
		{
			// Unhook event handlers
			foreach(AbstractStyleElement element in m_Elements)
			{
				element.ValueChanged -= element_ValueChanged;
			}
		}
		private void Revert()
		{
			// Revert to memo and re-update data.
			m_Style.Revert();
			m_Style.RaiseValueChanged();
			
			// Update main UI.
			if(m_Invalidate != null) m_Invalidate();
		}
		private void BtnCancel_Click(object sender, EventArgs e)
		{	
			UnhookEvents();
			Revert();
			m_bManualClose = true;
		}
		private void BtnOK_Click(object sender, EventArgs e)
		{
			UnhookEvents();
			m_bManualClose = true;	
		}
		#endregion
		
		#endregion
		
		
	}
}
