#region license
/*
Copyright © Joan Charmant 2008.
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
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// The dialog lets the user configure the whole color profile.
	/// All the modfications are made on a temporary profile 
	/// which is only comitted when the user submit the form.
	/// </summary>
    public partial class formColorProfile : Form
    {
        #region Members
        private ResourceManager m_ResourceManager;
        private StaticStylePicker m_StylePicker;
		private DrawingType m_DrawingTypePick;		// Used to identify the type of drawing being modified.
													// when we are in the commons events handlers.
		private ColorProfile m_ColorProfile;		// Ref to the original.
        private ColorProfile m_TempColorProfile;	// Working copy.
        #endregion

        #region Construction and Initialization
        public formColorProfile(ColorProfile _colorProfile)
        {
        	m_ColorProfile = _colorProfile;
        	m_TempColorProfile = new ColorProfile();
        	m_TempColorProfile.Load(m_ColorProfile);

            InitializeComponent();
            m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());

            SetupForm();
            LocalizeForm();
        }
        private void SetupForm()
        {
        	// Initialize font size combo.
        	cmbTextSize.Items.Clear();
        	cmbChronoSize.Items.Clear();
            foreach(int size in InfosTextDecoration.AllowedFontSizes)
            {
            	cmbTextSize.Items.Add(size.ToString());
				cmbChronoSize.Items.Add(size.ToString());
            }
        	
        	UpdateColorsAndStyles();

            // Size Picker Control
            m_StylePicker = new StaticStylePicker(DrawingType.Line);
            m_StylePicker.MouseLeft += new StaticStylePicker.DelegateMouseLeft(StylePicker_MouseLeft);
            m_StylePicker.StylePicked += new StaticStylePicker.DelegateStylePicked(StylePicker_StylePicked);
            m_StylePicker.Visible = false;
            this.Controls.Add(m_StylePicker);
            m_StylePicker.BringToFront();

            m_DrawingTypePick = DrawingType.None;
        }
        private void LocalizeForm()
        {
            // Window & Controls
            this.Text = "   " + ScreenManagerLang.dlgColorProfile_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnApply.Text = ScreenManagerLang.Generic_Apply;

            // ToolTips
            toolTips.SetToolTip(btnLoadProfile, ScreenManagerLang.dlgColorProfile_ToolTip_LoadProfile);
            toolTips.SetToolTip(btnSaveProfile, ScreenManagerLang.dlgColorProfile_ToolTip_SaveProfile);
            toolTips.SetToolTip(btnDefaultProfile, ScreenManagerLang.dlgColorProfile_ToolTip_DefaultProfile);

            toolTips.SetToolTip(btnDrawingToolText, ScreenManagerLang.ToolTip_DrawingToolText);
            toolTips.SetToolTip(btnDrawingToolPencil, ScreenManagerLang.ToolTip_DrawingToolPencil);
            toolTips.SetToolTip(btnDrawingToolLine2D, ScreenManagerLang.ToolTip_DrawingToolLine2D);
            toolTips.SetToolTip(btnDrawingToolCircle, ScreenManagerLang.ToolTip_DrawingToolCircle);
            toolTips.SetToolTip(btnDrawingToolCross2D, ScreenManagerLang.ToolTip_DrawingToolCross2D);
            toolTips.SetToolTip(btnDrawingToolAngle2D, ScreenManagerLang.ToolTip_DrawingToolAngle2D);
            toolTips.SetToolTip(btnDrawingToolChrono, ScreenManagerLang.ToolTip_DrawingToolChrono);

            toolTips.SetToolTip(btnTextColor, ScreenManagerLang.ToolTip_DrawingToolText);
            toolTips.SetToolTip(btnPencilColor, ScreenManagerLang.ToolTip_DrawingToolPencil);
            toolTips.SetToolTip(btnLineColor, ScreenManagerLang.ToolTip_DrawingToolLine2D);
            toolTips.SetToolTip(btnCircleColor, ScreenManagerLang.ToolTip_DrawingToolCircle);
            toolTips.SetToolTip(btnCrossColor, ScreenManagerLang.ToolTip_DrawingToolCross2D);
            toolTips.SetToolTip(btnAngleColor, ScreenManagerLang.ToolTip_DrawingToolAngle2D);
            toolTips.SetToolTip(btnChronoColor, ScreenManagerLang.ToolTip_DrawingToolChrono);

            toolTips.SetToolTip(cmbTextSize, ScreenManagerLang.ToolTip_DrawingToolText);
            toolTips.SetToolTip(btnPencilStyle, ScreenManagerLang.ToolTip_DrawingToolPencil);
            toolTips.SetToolTip(btnCircleStyle, ScreenManagerLang.ToolTip_DrawingToolCircle);
            toolTips.SetToolTip(btnLineStyle, ScreenManagerLang.ToolTip_DrawingToolLine2D);
            toolTips.SetToolTip(cmbChronoSize, ScreenManagerLang.ToolTip_DrawingToolChrono);
        }
        #endregion

        #region Color & Style Buttons Handlers
        private void btnTextColor_Click(object sender, EventArgs e)
        {
        	PickColor(DrawingType.Label);
        }
        private void btnPencilColor_Click(object sender, EventArgs e)
        {
        	PickColor(DrawingType.Pencil);
        }
        private void btnLineColor_Click(object sender, EventArgs e)
        {
        	PickColor(DrawingType.Line);
        }
        private void BtnCircleColorClick(object sender, EventArgs e)
        {
			PickColor(DrawingType.Circle);
        }
        private void btnCrossColor_Click(object sender, EventArgs e)
        {
        	PickColor(DrawingType.Cross);
        }
        private void btnAngleColor_Click(object sender, EventArgs e)
        {
        	PickColor(DrawingType.Angle);
        }
        private void btnLineStyle_Click(object sender, EventArgs e)
        {
            m_DrawingTypePick = DrawingType.Line;
            m_StylePicker.DrawingType = m_DrawingTypePick;
            m_StylePicker.Top = grpColors.Top + btnLineStyle.Top;
            m_StylePicker.Left = grpColors.Left + btnLineStyle.Left + btnLineStyle.Width - m_StylePicker.Width;
            m_StylePicker.Visible = true;
        }
        private void BtnCircleStyleClick(object sender, EventArgs e)
        {
        	m_DrawingTypePick = DrawingType.Circle;
            m_StylePicker.DrawingType = DrawingType.Circle;
            m_StylePicker.Top = grpColors.Top + btnCircleStyle.Top;
            m_StylePicker.Left = grpColors.Left + btnCircleStyle.Left + btnCircleStyle.Width - m_StylePicker.Width;
            m_StylePicker.Visible = true;	
        }
        private void btnPencilStyle_Click(object sender, EventArgs e)
        {
            m_DrawingTypePick = DrawingType.Pencil;
            m_StylePicker.DrawingType = m_DrawingTypePick;
            m_StylePicker.Top = grpColors.Top + btnPencilStyle.Top;
            m_StylePicker.Left = grpColors.Left + btnPencilStyle.Left + btnPencilStyle.Width - m_StylePicker.Width;
            m_StylePicker.Visible = true;
        }
        private void btnChronoColor_Click(object sender, EventArgs e)
        {
        	PickColor(DrawingType.Chrono);
        }
        private void btnLineStyle_Paint(object sender, PaintEventArgs e)
        {
            // Ask the style to draw itself on the button canvas.
            Button btn = (Button)sender;
            LineStyle stl = (LineStyle)btn.Tag;

            stl.Draw(e.Graphics, false, Color.Black);
        }
        private void btnPencilStyle_Paint(object sender, PaintEventArgs e)
        {
            // Ask the style to draw itself on the button canvas.
            Button btn = (Button)sender;
            LineStyle stl = (LineStyle)btn.Tag;
            
            stl.Draw(e.Graphics, true, Color.Black);
        }
        void BtnCircleStylePaint(object sender, PaintEventArgs e)
        {
        	// Ask the style to draw itself on the button canvas.
            Button btn = (Button)sender;
            LineStyle stl = (LineStyle)btn.Tag;
            
            stl.Draw(e.Graphics, true, Color.Black);	
        }
        #endregion

        #region Decoration Update Handlers
        private void PickColor(DrawingType _drawingType)
        {
        	FormColorPicker picker = new FormColorPicker();
        	if(picker.ShowDialog() == DialogResult.OK)
        	{
        		m_TempColorProfile.UpdateData(_drawingType, picker.PickedColor);
        		UpdateColorsAndStyles();
        	}
        	picker.Dispose();
        }
        private void StylePicker_StylePicked(object sender, EventArgs e)
        {
        	m_TempColorProfile.UpdateData(m_DrawingTypePick, m_StylePicker.PickedStyle);
        	UpdateColorsAndStyles();
            m_DrawingTypePick = DrawingType.None;
            m_StylePicker.Visible = false;
        }
        private void StylePicker_MouseLeft(object sender, EventArgs e)
        {
            m_DrawingTypePick = DrawingType.None;
            m_StylePicker.Visible = false;
        }
        private void CmbTextSizeSelectedIndexChanged(object sender, EventArgs e)
        {
        	m_TempColorProfile.UpdateData(DrawingType.Label, int.Parse(((ComboBox)sender).Text));
        }
        private void CmbChronoSizeSelectedIndexChanged(object sender, EventArgs e)
        {
        	m_TempColorProfile.UpdateData(DrawingType.Chrono, int.Parse(((ComboBox)sender).Text));
        }
        #endregion
        
        #region Utilities
        private void UpdateColorsAndStyles()
        {
            // Load profile data to controls
            
            // Colors
            btnTextColor.BackColor = m_TempColorProfile.ColorText;
            btnPencilColor.BackColor = m_TempColorProfile.ColorPencil;
            btnLineColor.BackColor = m_TempColorProfile.ColorLine2D;
            btnCircleColor.BackColor = m_TempColorProfile.ColorCircle;
            btnCrossColor.BackColor = m_TempColorProfile.ColorCross2D;
            btnAngleColor.BackColor = m_TempColorProfile.ColorAngle2D;
            btnChronoColor.BackColor = m_TempColorProfile.ColorChrono;
            FixColors();
            
            // Styles
            btnLineStyle.Tag = m_TempColorProfile.StyleLine2D;
            btnPencilStyle.Tag = m_TempColorProfile.StylePencil;
            btnCircleStyle.Tag = m_TempColorProfile.StyleCircle;
            
            // Font sizes
            cmbTextSize.Text = m_TempColorProfile.FontSizeText.ToString();
            cmbChronoSize.Text = m_TempColorProfile.FontSizeChrono.ToString();
            
            // Update window.
            this.Invalidate();
        }
        private void FixColors()
        {
            // Over Back Color should be the same
            btnTextColor.FlatAppearance.MouseOverBackColor = btnTextColor.BackColor;
            btnPencilColor.FlatAppearance.MouseOverBackColor = btnPencilColor.BackColor;
            btnLineColor.FlatAppearance.MouseOverBackColor = btnLineColor.BackColor;
            btnCircleColor.FlatAppearance.MouseOverBackColor = btnCircleColor.BackColor;
            btnCrossColor.FlatAppearance.MouseOverBackColor = btnCrossColor.BackColor;
            btnAngleColor.FlatAppearance.MouseOverBackColor = btnAngleColor.BackColor;
            btnChronoColor.FlatAppearance.MouseOverBackColor = btnChronoColor.BackColor;

            // Put a black frame around white rectangles.
            FixButtonColor(btnTextColor);
            FixButtonColor(btnPencilColor);
            FixButtonColor(btnLineColor);
            FixButtonColor(btnCircleColor);
            FixButtonColor(btnCrossColor);
            FixButtonColor(btnAngleColor);
            FixButtonColor(btnChronoColor);
        }
        private void FixButtonColor(Button btn)
        {
            if (Color.Equals(btn.BackColor, Color.FromArgb(255, 255, 255)) || Color.Equals(btn.BackColor, Color.White))
            {
                btn.FlatAppearance.BorderSize = 1;
            }
            else
            {
                btn.FlatAppearance.BorderSize = 0;
            }
        }
        #endregion

        #region Open/Save Buttons Handlers
        private void btnLoadProfile_Click(object sender, EventArgs e)
        {
            // load file to working copy of the profile
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = m_ResourceManager.GetString("dlgColorProfile_ToolTip_LoadProfile", Thread.CurrentThread.CurrentUICulture);
            openFileDialog.Filter = m_ResourceManager.GetString("dlgColorProfile_FileFilter", Thread.CurrentThread.CurrentUICulture);
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = PreferencesManager.ResourceManager.GetString("ColorProfilesFolder");

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                if (filePath.Length > 0)
                {
                    m_TempColorProfile.Load(filePath);
                    UpdateColorsAndStyles();
                    // Current Dir is modified ?
                }
            }
        }
        private void btnSaveProfile_Click(object sender, EventArgs e)
        {
            // Save current working copy to file

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = m_ResourceManager.GetString("dlgColorProfile_ToolTip_SaveProfile", Thread.CurrentThread.CurrentUICulture);
            saveFileDialog.Filter = m_ResourceManager.GetString("dlgColorProfile_FileFilter", Thread.CurrentThread.CurrentUICulture);
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.InitialDirectory = Application.StartupPath + "\\" + PreferencesManager.ResourceManager.GetString("ColorProfilesFolder");

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                if (filePath.Length > 0)
                {
                    m_TempColorProfile.Save(filePath);
                }
            }
        }
        private void btnDefaults_Click(object sender, EventArgs e)
        {
        	// Reload the built-in default profile.
            m_TempColorProfile.Load(new ColorProfile());
            UpdateColorsAndStyles();
        }
        #endregion
        
        #region Apply Cancel Handlers
        private void btnApply_Click(object sender, EventArgs e)
        {
        	// Comit the changes to the main color profile.
            m_ColorProfile.Load(m_TempColorProfile);
            
            // Serialize it to file.
            string folder = PreferencesManager.SettingsFolder + PreferencesManager.ResourceManager.GetString("ColorProfilesFolder");
            m_ColorProfile.Save(folder + "\\current.xml");
        }
        #endregion
        
        
        
        
        
        
        
        
    }
}