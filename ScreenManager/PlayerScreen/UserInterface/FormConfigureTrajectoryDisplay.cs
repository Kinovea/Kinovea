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
using Kinovea.ScreenManager.Languages;
using System;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

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
    	// Generic
    	private bool m_bManualClose = false;
    	
    	// Specific to the configure page.
    	private PictureBox m_SurfaceScreen; 		// Used to update the image while configuring.
        private Track m_Track;
        private StaticColorPicker m_ColPicker;
        private StaticStylePicker m_StlPicker;
        #endregion
        
        #region Construction & Initialization
        public formConfigureTrajectoryDisplay(Track _track, PictureBox _SurfaceScreen)
        {
            InitializeComponent();
            m_SurfaceScreen = _SurfaceScreen;
            m_Track = _track;
            
            // Save the current state in case we need to recall it later.
            m_Track.MemorizeState();
            
            InitColorPickerControl();
            InitStylePickerControl();
            SetCurrentOptions();
            InitCulture();
        }
 		private void InitColorPickerControl()
        {
        	// Color Picker Control
            m_ColPicker = new StaticColorPicker();
            m_ColPicker.MouseLeft += new StaticColorPicker.DelegateMouseLeft(ColorPicker_MouseLeft);
            m_ColPicker.ColorPicked += new StaticColorPicker.DelegateColorPicked(ColorPicker_ColorPicked);
            m_ColPicker.Visible = false;
            this.Controls.Add(m_ColPicker);
            m_ColPicker.BringToFront();
        }
 		private void InitStylePickerControl()
        {
        	// Style Picker Control
            m_StlPicker = new StaticStylePicker();
            m_StlPicker.ToolType = DrawingToolType.Cross2D; // This actually means Track for the style picker.
            m_StlPicker.MouseLeft += new StaticStylePicker.DelegateMouseLeft(StylePicker_MouseLeft);
            m_StlPicker.StylePicked += new StaticStylePicker.DelegateStylePicked(StylePicker_StylePicked);
            m_StlPicker.Visible = false;
            this.Controls.Add(m_StlPicker);
            m_StlPicker.BringToFront();
        }
        private void SetCurrentOptions()
        {
        	// Current configuration.
        	
        	// General
        	switch(m_Track.View)
        	{
        		case Track.TrackView.Focus:
        			radioFocus.Checked = true;
        			break;
        		case Track.TrackView.Label:
        			radioLabel.Checked = true;
        			break;
        		case Track.TrackView.Complete:
        		default:
        			radioComplete.Checked = true;
        			break;
        	}
        	tbLabel.Text = m_Track.Label;
        	
        	// Color & style
            btnTextColor.BackColor = m_Track.MainColor;
            FixColors();
            btnLineStyle.Invalidate();
            
            //chkShowTarget.Checked = m_Track.ShowTarget;
            //chkShowTitles.Checked = m_Track.ShowKeyframesTitles;
            //chkShowTrajectory.Checked = m_Track.ShowTrajectory;
        }
        private void InitCulture()
        {
            this.Text = "   " + ScreenManagerLang.dlgConfigureTrajectory_Title;
            
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            radioComplete.Text = ScreenManagerLang.dlgConfigureTrajectory_RadioComplete;
            radioFocus.Text = ScreenManagerLang.dlgConfigureTrajectory_RadioFocus;
            radioLabel.Text = ScreenManagerLang.dlgConfigureTrajectory_RadioLabel;
            lblLabel.Text = ScreenManagerLang.dlgConfigureChrono_Label;
            
			grpAppearance.Text = ScreenManagerLang.Generic_Appearance;
            lblColor.Text = ScreenManagerLang.Generic_ColorPicker;
            lblStyle.Text = ScreenManagerLang.dlgConfigureTrajectory_Style;
            
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        #endregion
        
        #region General
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
            	m_Track.View = Track.TrackView.Complete;
        	}
        	else if(radioFocus.Checked)
        	{
        		m_Track.View = Track.TrackView.Focus;
        	}
        	else
        	{
        		m_Track.View = Track.TrackView.Label;
        	}
        	
        	m_SurfaceScreen.Invalidate();
        }
        private void tbLabel_TextChanged(object sender, EventArgs e)
        {
            m_Track.Label = tbLabel.Text;
            m_SurfaceScreen.Invalidate();
        }
        #endregion
        
        #region Color and Style
        
        #region ColorPicker Handling
        private void btnTextColor_Click(object sender, EventArgs e)
        {
        	// Show the color picker
        	m_ColPicker.Top = grpAppearance.Top + btnTextColor.Top - (m_ColPicker.Height / 2);
            m_ColPicker.Left = grpAppearance.Left + btnTextColor.Left;
            m_ColPicker.Visible = true;
        }
        private void ColorPicker_ColorPicked(object sender, EventArgs e)
        {
        	// The user clicked on a color from the color picker
            btnTextColor.BackColor = m_ColPicker.PickedColor;
            m_Track.MainColor = m_ColPicker.PickedColor;
            FixColors();
            m_ColPicker.Visible = false;
            m_SurfaceScreen.Invalidate();
        }
        private void ColorPicker_MouseLeft(object sender, EventArgs e)
        {
        	// The user left the area of the control without clicking on a color.
            m_ColPicker.Visible = false;
        }
        private void FixColors()
        {
            // Over Back Color should be the same
            btnTextColor.FlatAppearance.MouseOverBackColor = btnTextColor.BackColor;

            // Put a black frame around white rectangles.
            if (Color.Equals(btnTextColor.BackColor, Color.FromArgb(255, 255, 255)) || Color.Equals(btnTextColor.BackColor, Color.White))
            {
                btnTextColor.FlatAppearance.BorderSize = 1;
            }
            else
            {
                btnTextColor.FlatAppearance.BorderSize = 0;
            }
        }
        #endregion

        #region Style Handling
        private void btnLineStyle_MouseClick(object sender, MouseEventArgs e)
        {
        	// Show the style picker
        	m_StlPicker.Top = grpAppearance.Top + btnLineStyle.Top - (m_StlPicker.Height / 2);
            m_StlPicker.Left = grpAppearance.Left + btnLineStyle.Left;
            m_StlPicker.Visible = true;
        }
        private void StylePicker_StylePicked(object sender, EventArgs e)
        {
        	// The user clicked on a style from the style picker.
        	// This will only update the line shape / size. 
        	m_Track.TrajectoryStyle = m_StlPicker.PickedStyle;
        	m_StlPicker.Visible = false;
        	btnLineStyle.Invalidate();
        	m_SurfaceScreen.Invalidate();
        }
        private void StylePicker_MouseLeft(object sender, EventArgs e)
        {
        	// The user left the area of the control without clicking on a style.
        	m_StlPicker.Visible = false;
        }
        private void btnLineStyle_Paint(object sender, PaintEventArgs e)
        {
        	// Show how the selected style looks like.
        	m_Track.TrajectoryStyle.Draw(e.Graphics, false, Color.Black);
        }
        #endregion
		
        #endregion
        
        #region OK/Cancel/Closing
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Nothing more to do. The track has been updated already.
            m_bManualClose = true;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Fall back to memorized decoration.
            m_Track.RecallState();
            m_SurfaceScreen.Invalidate();
            m_bManualClose = true;
        }
        private void formConfigureTrajectoryDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
        	if (!m_bManualClose) 
            {
            	m_Track.RecallState();
            	m_SurfaceScreen.Invalidate();
            }
        }
        #endregion
        
    }
}