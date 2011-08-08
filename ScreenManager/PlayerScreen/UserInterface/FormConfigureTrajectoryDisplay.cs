#region License
/*
Copyright © Joan Charmant 2009.
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
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// The dialog lets the user configure a track instance.
	/// Some of the logic is the same as for formConfigureDrawing.
	/// Specifically, we work and update the actual instance in real time. 
	/// If the user finally decide to cancel there's a "fallback to memo" mechanism. 
	/// </summary>
    public partial class formConfigureTrajectoryDisplay : Form
    {
    	#region Members
    	private bool m_bManualClose = false;
    	private Action m_Invalidate;
        private Track m_Track;
        private List<AbstractStyleElement> m_Elements = new List<AbstractStyleElement>();
        #endregion
        
        #region Construction
        public formConfigureTrajectoryDisplay(Track _track, Action _invalidate)
        {
            InitializeComponent();
            m_Invalidate = _invalidate;
            m_Track = _track;
            m_Track.DrawingStyle.ReadValue();
			
            // Save the current state in case of cancel.
            m_Track.MemorizeState();
            m_Track.DrawingStyle.Memorize();
            
            InitExtraDataCombo();
            SetupStyleControls();
            SetCurrentOptions();
            InitCulture();
        }
        #endregion
        
        #region Init
        private void InitExtraDataCombo()
 		{
 			// Combo must be filled in the order of the enum.
 			cmbExtraData.Items.Add(ScreenManagerLang.dlgConfigureTrajectory_ExtraData_None);
            cmbExtraData.Items.Add(ScreenManagerLang.dlgConfigureTrajectory_ExtraData_TotalDistance);
            cmbExtraData.Items.Add(ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Speed);
 		}
 		private void SetupStyleControls()
 		{
 			// Dynamic loading of track styles but only semi dynamic UI (restricted to 3) for simplicity.
 			// Styles should be Color, LineSize and TrackShape.
 			foreach(KeyValuePair<string, AbstractStyleElement> pair in m_Track.DrawingStyle.Elements)
			{
 				m_Elements.Add(pair.Value);
 			}
 			
 			if(m_Elements.Count == 3)
 			{
 				int editorsLeft = 200;
 				int lastEditorBottom = 10;
 				Size editorSize = new Size(60,20);
 				
 				foreach(AbstractStyleElement styleElement in m_Elements)
 				{
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
					
					Control miniEditor = styleElement.GetEditor();
					miniEditor.Size = editorSize;
					miniEditor.Location = new Point(editorsLeft, btn.Top);
					
					lastEditorBottom = miniEditor.Bottom;
				
					grpAppearance.Controls.Add(btn);
					grpAppearance.Controls.Add(lbl);
					grpAppearance.Controls.Add(miniEditor);
 				}
 			}
 		}
 		private void SetCurrentOptions()
        {
        	// Current configuration.
        	
        	// General
        	switch(m_Track.View)
        	{
        		case TrackView.Focus:
        			radioFocus.Checked = true;
        			break;
        		case TrackView.Label:
        			radioLabel.Checked = true;
        			break;
        		case TrackView.Complete:
        		default:
        			radioComplete.Checked = true;
        			break;
        	}
        	tbLabel.Text = m_Track.Label;
        	cmbExtraData.SelectedIndex = (int)m_Track.ExtraData;
        }
        private void InitCulture()
        {
            this.Text = "   " + ScreenManagerLang.dlgConfigureTrajectory_Title;
            
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            radioComplete.Text = ScreenManagerLang.dlgConfigureTrajectory_RadioComplete;
            radioFocus.Text = ScreenManagerLang.dlgConfigureTrajectory_RadioFocus;
            radioLabel.Text = ScreenManagerLang.dlgConfigureTrajectory_RadioLabel;
            lblLabel.Text = ScreenManagerLang.dlgConfigureChrono_Label;
            lblExtra.Text = ScreenManagerLang.dlgConfigureTrajectory_LabelExtraData;
			grpAppearance.Text = ScreenManagerLang.Generic_Appearance;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        #endregion
        
        #region Event handlers
        private void btnComplete_Click(object sender, EventArgs e)
        {
        	radioComplete.Checked = true;	
        }
        private void btnFocus_Click(object sender, EventArgs e)
        {
        	radioFocus.Checked = true;
        }
        private void btnLabel_Click(object sender, EventArgs e)
        {
        	radioLabel.Checked = true;
        }
        private void RadioViews_CheckedChanged(object sender, EventArgs e)
        {
        	if(radioComplete.Checked)
        	{
            	m_Track.View = TrackView.Complete;
        	}
        	else if(radioFocus.Checked)
        	{
        		m_Track.View = TrackView.Focus;
        	}
        	else
        	{
        		m_Track.View = TrackView.Label;
        	}
        	
        	if(m_Invalidate != null) m_Invalidate();
        }
        private void tbLabel_TextChanged(object sender, EventArgs e)
        {
            m_Track.Label = tbLabel.Text;
            if(m_Invalidate != null) m_Invalidate();
        }
        private void CmbExtraData_SelectedIndexChanged(object sender, EventArgs e)
        {
        	m_Track.ExtraData = (TrackExtraData)cmbExtraData.SelectedIndex;
        	if(m_Invalidate != null) m_Invalidate();
        }
        private void element_ValueChanged(object sender, EventArgs e)
		{
			if(m_Invalidate != null) m_Invalidate();
		}
        #endregion
        
        #region OK/Cancel/Closing
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
        	if (!m_bManualClose) 
            {
            	UnhookEvents();
				Revert();
            }
        }
        private void UnhookEvents()
		{
			// Unhook style event handlers
			foreach(AbstractStyleElement element in m_Elements)
			{
				element.ValueChanged -= element_ValueChanged;
			}
		}
        private void Revert()
		{
			// Revert to memo and re-update data.
			m_Track.DrawingStyle.Revert();
			m_Track.DrawingStyle.RaiseValueChanged();
			m_Track.RecallState();
			if(m_Invalidate != null) m_Invalidate();
		}
        private void btnOK_Click(object sender, EventArgs e)
        {
            UnhookEvents();
            m_bManualClose = true;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            UnhookEvents();
			Revert();
            m_bManualClose = true;
        }
        #endregion
    }
}