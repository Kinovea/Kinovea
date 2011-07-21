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
		private AbstractStyleElement m_firstElement;
		private AbstractStyleElement m_secondElement;
		private DelegateScreenInvalidate m_Invalidate;
		private bool m_bManualClose;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public FormConfigureDrawing2(DrawingStyle _style, DelegateScreenInvalidate _invalidate)
		{
			m_Style = _style;
			m_Style.ReadValue();
			m_Style.Memorize();
			m_Invalidate = _invalidate;
			InitializeComponent();
			SetupForm();
			LocalizeForm();
		}
		#endregion
		
		#region Private Methods
		private void SetupForm()
		{
			// Layout depends on the list of style elements.
			// Currently we only support 2 editable style elements.
			// Example: pencil tool has a color picker and a pen size picker.
			
			foreach(KeyValuePair<string, AbstractStyleElement> styleElement in m_Style.Elements)
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
			if(m_firstElement != null)
			{
				btnFirstElement.BackColor = Color.Transparent;
				btnFirstElement.Image = m_firstElement.Icon;
				lblFirstElement.Text = m_firstElement.DisplayName;
				
				m_firstElement.ValueChanged += element_ValueChanged;
				
				int editorsLeft = 150; // works in High DPI ?
				
				Control firstEditor = m_firstElement.GetEditor();
				firstEditor.Size = new Size(50, 20);
				firstEditor.Location = new Point(editorsLeft, btnFirstElement.Top);
				grpConfig.Controls.Add(firstEditor);
				
				if(m_secondElement != null)
				{
					btnSecondElement.BackColor = Color.Transparent;
					btnSecondElement.Image = m_secondElement.Icon;
					lblSecondElement.Text = m_secondElement.DisplayName;

					m_secondElement.ValueChanged += element_ValueChanged;
					
					Control secondEditor = m_secondElement.GetEditor();
					secondEditor.Size = new Size(50, 20);
					secondEditor.Location = new Point(editorsLeft, btnSecondElement.Top);
					grpConfig.Controls.Add(secondEditor);
				}
				else
				{
					btnSecondElement.Visible = false;
					lblSecondElement.Visible = false;
				}
			}
			else
			{
				// Shouldn't happen. Only ask for the dialog if style.Elements.Count > 0.
				btnFirstElement.Visible = false;
				lblFirstElement.Visible = false;
			}
		}
		private void LocalizeForm()
		{
			this.Text = "   " + ScreenManagerLang.dlgConfigureDrawing_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
		}
		private void element_ValueChanged()
		{
			if(m_Invalidate != null)
				m_Invalidate();
		}
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
			if(m_firstElement != null)
			{
				m_firstElement.ValueChanged -= element_ValueChanged;
			}
			
			if(m_secondElement != null)
			{
				m_secondElement.ValueChanged -= element_ValueChanged;
			}
		}
		private void Revert()
		{
			// Revert to memo and re-update data.
			m_Style.Revert();
			m_Style.RaiseValueChanged();
			
			// Update main UI.
			if(m_Invalidate != null)
				m_Invalidate();
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
		
		
	}
}
