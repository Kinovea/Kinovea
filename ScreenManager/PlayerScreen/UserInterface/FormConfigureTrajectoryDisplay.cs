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
    	private ResourceManager m_ResourceManager;
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
            chkShowTrajectory.Location = chkShowTitles.Location;
            m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());

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
            btnTextColor.BackColor = m_Track.MainColor;
            FixColors();
            btnLineStyle.Invalidate();
            chkShowTarget.Checked = m_Track.ShowTarget;
            chkShowTitles.Checked = m_Track.ShowKeyframesTitles;
            chkShowTrajectory.Checked = m_Track.ShowTrajectory;
            tbLabel.Text = m_Track.Label;
        }
        private void InitCulture()
        {
        	// Localize
            this.Text = "   " + m_ResourceManager.GetString("dlgConfigureTrajectory_Title", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
            btnOK.Text = m_ResourceManager.GetString("Generic_Apply", Thread.CurrentThread.CurrentUICulture);
            lblMode.Text = m_ResourceManager.GetString("dlgConfigureTrajectory_lblMode", Thread.CurrentThread.CurrentUICulture);
            cmbTrackView.Items.Clear();
            cmbTrackView.Items.Add(m_ResourceManager.GetString("dlgConfigureTrajectory_ModeTrajectory", Thread.CurrentThread.CurrentUICulture));
            cmbTrackView.Items.Add(m_ResourceManager.GetString("dlgConfigureTrajectory_ModeLabelFollows", Thread.CurrentThread.CurrentUICulture));
            cmbTrackView.Items.Add(m_ResourceManager.GetString("dlgConfigureTrajectory_ModeArrowFollows", Thread.CurrentThread.CurrentUICulture));
            cmbTrackView.SelectedIndex = (int)m_Track.NormalModeView;

            grpConfig.Text = m_ResourceManager.GetString("Generic_Configuration", Thread.CurrentThread.CurrentUICulture);
            lblColor.Text = m_ResourceManager.GetString("Generic_ColorPicker", Thread.CurrentThread.CurrentUICulture);
            lblStyle.Text = m_ResourceManager.GetString("dlgConfigureTrajectory_Style", Thread.CurrentThread.CurrentUICulture);
            chkShowTarget.Text = m_ResourceManager.GetString("dlgConfigureTrajectory_chkShowTarget", Thread.CurrentThread.CurrentUICulture);
            chkShowTitles.Text = m_ResourceManager.GetString("dlgConfigureTrajectory_chkShowTitles", Thread.CurrentThread.CurrentUICulture);
            chkShowTrajectory.Text = m_ResourceManager.GetString("dlgConfigureTrajectory_chkShowTrajectory", Thread.CurrentThread.CurrentUICulture);
            lblLabel.Text = m_ResourceManager.GetString("dlgConfigureChrono_Label", Thread.CurrentThread.CurrentUICulture);
        }
        #endregion
        
        #region ColorPicker Handling
        private void btnTextColor_Click(object sender, EventArgs e)
        {
        	// Show the color picker
            m_ColPicker.Top = grpConfig.Top + btnTextColor.Top;
            m_ColPicker.Left = grpConfig.Left + btnTextColor.Left + btnTextColor.Width - m_ColPicker.Width;
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
        	m_StlPicker.Top = grpConfig.Top + btnLineStyle.Top;
            m_StlPicker.Left = grpConfig.Left + btnLineStyle.Left + btnLineStyle.Width - m_StlPicker.Width;
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

        #region Other Options Handlers
        private void cmbTrackView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update value.
            m_Track.NormalModeView = (Track.TrackView)cmbTrackView.SelectedIndex;
            
            // Change the main config box content.
            switch (m_Track.NormalModeView)
            {
                case Track.TrackView.Trajectory:
                    {
                        chkShowTitles.Visible = true;
                        chkShowTarget.Visible = true;
                        chkShowTrajectory.Visible = false;
                        break;
                    }
                case Track.TrackView.ArrowFollows:
                    {
                        chkShowTitles.Visible = false;
                        chkShowTarget.Visible = false;
                        chkShowTrajectory.Visible = true;
                        break;
                    }
                case Track.TrackView.LabelFollows:
                    {
                        chkShowTitles.Visible = false;
                        chkShowTarget.Visible = false;
                        chkShowTrajectory.Visible = true;
                        break;
                    }
            }
            m_SurfaceScreen.Invalidate();
        }
        private void chkShowTarget_CheckedChanged(object sender, EventArgs e)
        {
            m_Track.ShowTarget = chkShowTarget.Checked;
            m_SurfaceScreen.Invalidate();
        }
        private void chkShowTitles_CheckedChanged(object sender, EventArgs e)
        {
            m_Track.ShowKeyframesTitles = chkShowTitles.Checked;
            m_SurfaceScreen.Invalidate();
        }
        private void tbLabel_TextChanged(object sender, EventArgs e)
        {
            m_Track.Label = tbLabel.Text;
            m_SurfaceScreen.Invalidate();
        }
        private void chkShowTrajectory_CheckedChanged(object sender, EventArgs e)
        {
            m_Track.ShowTrajectory = chkShowTrajectory.Checked;
            m_SurfaceScreen.Invalidate();
        }
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