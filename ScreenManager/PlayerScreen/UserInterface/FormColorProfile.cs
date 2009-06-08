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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Threading;
using Videa.Services;
using System.Drawing.Drawing2D;

namespace Videa.ScreenManager
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
        private StaticColorPicker m_ColPicker;
        private StaticStylePicker m_StylePicker;
		private DrawingToolType m_DrawingToolPick;	// Used to identify the tool being modified 
													// when we are in the commons events handlers.
		private ColorProfile m_ColorProfile;		// Ref to the original
        private ColorProfile m_TempColorProfile;	// Working copy.
        #endregion

        #region Construction and Initialization
        public formColorProfile(ColorProfile _colorProfile)
        {
        	m_ColorProfile = _colorProfile;
        	m_TempColorProfile = new ColorProfile();
        	m_TempColorProfile.Load(m_ColorProfile);

            InitializeComponent();
            m_ResourceManager = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());

            SetupForm();
            LocalizeForm();
        }
        private void SetupForm()
        {
        	UpdateColorsAndStyles();

            // Color Picker Control
            m_ColPicker = new StaticColorPicker();
            m_ColPicker.MouseLeft += new StaticColorPicker.DelegateMouseLeft(ColorPicker_MouseLeft);
            m_ColPicker.ColorPicked += new StaticColorPicker.DelegateColorPicked(ColorPicker_ColorPicked);
            m_ColPicker.Visible = false;
            this.Controls.Add(m_ColPicker);
            m_ColPicker.BringToFront();

            // Size Picker Control
            m_StylePicker = new StaticStylePicker(DrawingToolType.Line2D);
            m_StylePicker.MouseLeft += new StaticStylePicker.DelegateMouseLeft(StylePicker_MouseLeft);
            m_StylePicker.StylePicked += new StaticStylePicker.DelegateStylePicked(StylePicker_StylePicked);
            m_StylePicker.Visible = false;
            this.Controls.Add(m_StylePicker);
            m_StylePicker.BringToFront();

            m_DrawingToolPick = DrawingToolType.Pointer;
        }
        private void LocalizeForm()
        {
            // Window & Controls
            this.Text = "   " + m_ResourceManager.GetString("dlgColorProfile_Title", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
            btnApply.Text = m_ResourceManager.GetString("Generic_Apply", Thread.CurrentThread.CurrentUICulture);

            // ToolTips
            toolTips.SetToolTip(btnLoadProfile, m_ResourceManager.GetString("dlgColorProfile_ToolTip_LoadProfile", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnSaveProfile, m_ResourceManager.GetString("dlgColorProfile_ToolTip_SaveProfile", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnDefaultProfile, m_ResourceManager.GetString("dlgColorProfile_ToolTip_DefaultProfile", Thread.CurrentThread.CurrentUICulture));

            toolTips.SetToolTip(btnDrawingToolText, m_ResourceManager.GetString("ToolTip_DrawingToolText", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnDrawingToolPencil, m_ResourceManager.GetString("ToolTip_DrawingToolPencil", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnDrawingToolLine2D, m_ResourceManager.GetString("ToolTip_DrawingToolLine2D", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnDrawingToolCross2D, m_ResourceManager.GetString("ToolTip_DrawingToolCross2D", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnDrawingToolAngle2D, m_ResourceManager.GetString("ToolTip_DrawingToolAngle2D", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnDrawingToolChrono, m_ResourceManager.GetString("ToolTip_DrawingToolChrono", Thread.CurrentThread.CurrentUICulture));

            toolTips.SetToolTip(btnTextColor, m_ResourceManager.GetString("ToolTip_DrawingToolText", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnPencilColor, m_ResourceManager.GetString("ToolTip_DrawingToolPencil", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnLineColor, m_ResourceManager.GetString("ToolTip_DrawingToolLine2D", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnCrossColor, m_ResourceManager.GetString("ToolTip_DrawingToolCross2D", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnAngleColor, m_ResourceManager.GetString("ToolTip_DrawingToolAngle2D", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnChronoColor, m_ResourceManager.GetString("ToolTip_DrawingToolChrono", Thread.CurrentThread.CurrentUICulture));

            toolTips.SetToolTip(cmbTextSize, m_ResourceManager.GetString("ToolTip_DrawingToolText", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnPencilStyle, m_ResourceManager.GetString("ToolTip_DrawingToolPencil", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnLineStyle, m_ResourceManager.GetString("ToolTip_DrawingToolLine2D", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(cmbChronoSize, m_ResourceManager.GetString("ToolTip_DrawingToolChrono", Thread.CurrentThread.CurrentUICulture));
        }
        #endregion

        #region Color & Style Buttons Handlers
        private void btnTextColor_Click(object sender, EventArgs e)
        {
            m_DrawingToolPick = DrawingToolType.Text;

            m_ColPicker.Top = grpColors.Top + btnTextColor.Top;
            m_ColPicker.Left = grpColors.Left + btnTextColor.Left + btnTextColor.Width - m_ColPicker.Width;
         
            m_ColPicker.Visible = true;
        }
        private void btnPencilColor_Click(object sender, EventArgs e)
        {
            m_DrawingToolPick = DrawingToolType.Pencil;

            m_ColPicker.Top = grpColors.Top + btnPencilColor.Top;
            m_ColPicker.Left = grpColors.Left + btnPencilColor.Left + btnPencilColor.Width - m_ColPicker.Width;

            m_ColPicker.Visible = true;
        }
        private void btnLineColor_Click(object sender, EventArgs e)
        {
            m_DrawingToolPick = DrawingToolType.Line2D;

            m_ColPicker.Top = grpColors.Top + btnLineColor.Top + (btnLineColor.Height/2) - (m_ColPicker.Height/2);
            m_ColPicker.Left = grpColors.Left + btnLineColor.Left + btnLineColor.Width - m_ColPicker.Width;

            m_ColPicker.Visible = true;
        }
        private void btnCrossColor_Click(object sender, EventArgs e)
        {
            m_DrawingToolPick = DrawingToolType.Cross2D;

            m_ColPicker.Top = grpColors.Top + btnCrossColor.Top + btnAngleColor.Height - m_ColPicker.Height;
            m_ColPicker.Left = grpColors.Left + btnCrossColor.Left + btnCrossColor.Width - m_ColPicker.Width;

            m_ColPicker.Visible = true;
        }
        private void btnAngleColor_Click(object sender, EventArgs e)
        {
            m_DrawingToolPick = DrawingToolType.Angle2D;

            m_ColPicker.Top = grpColors.Top + btnAngleColor.Top + btnAngleColor.Height - m_ColPicker.Height;
            m_ColPicker.Left = grpColors.Left + btnAngleColor.Left + btnAngleColor.Width - m_ColPicker.Width;

            m_ColPicker.Visible = true;
        }
        private void btnLineStyle_Click(object sender, EventArgs e)
        {
            m_DrawingToolPick = DrawingToolType.Line2D;
            m_StylePicker.ToolType = m_DrawingToolPick;
            m_StylePicker.Top = grpColors.Top + btnLineStyle.Top;
            m_StylePicker.Left = grpColors.Left + btnLineStyle.Left + btnLineStyle.Width - m_StylePicker.Width;
            m_StylePicker.Visible = true;
        }
        private void btnPencilStyle_Click(object sender, EventArgs e)
        {
            m_DrawingToolPick = DrawingToolType.Pencil;
            m_StylePicker.ToolType = m_DrawingToolPick;
            m_StylePicker.Top = grpColors.Top + btnPencilStyle.Top;
            m_StylePicker.Left = grpColors.Left + btnPencilStyle.Left + btnPencilStyle.Width - m_StylePicker.Width;
            m_StylePicker.Visible = true;
        }
        private void btnChronoColor_Click(object sender, EventArgs e)
        {
            m_DrawingToolPick = DrawingToolType.Chrono;

            m_ColPicker.Top = grpColors.Top + btnChronoColor.Top + btnChronoColor.Height - m_ColPicker.Height;
            m_ColPicker.Left = grpColors.Left + btnChronoColor.Left + btnChronoColor.Width - m_ColPicker.Width;

            m_ColPicker.Visible = true;
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
        #endregion

        #region Decoration Update Handlers
        private void ColorPicker_ColorPicked(object sender, EventArgs e)
        {
        	m_TempColorProfile.UpdateData(m_DrawingToolPick, m_ColPicker.PickedColor);
        	UpdateColorsAndStyles();
            m_DrawingToolPick = DrawingToolType.Pointer;
            m_ColPicker.Visible = false;
        }
        private void ColorPicker_MouseLeft(object sender, EventArgs e)
        {
            m_DrawingToolPick = DrawingToolType.Pointer;
            m_ColPicker.Visible = false;
        }
        private void StylePicker_StylePicked(object sender, EventArgs e)
        {
        	m_TempColorProfile.UpdateData(m_DrawingToolPick, m_StylePicker.PickedStyle);
        	UpdateColorsAndStyles();
            m_DrawingToolPick = DrawingToolType.Pointer;
            m_StylePicker.Visible = false;
        }
        private void StylePicker_MouseLeft(object sender, EventArgs e)
        {
            m_DrawingToolPick = DrawingToolType.Pointer;
            m_StylePicker.Visible = false;
        }
        private void CmbTextSizeSelectedIndexChanged(object sender, EventArgs e)
        {
        	m_TempColorProfile.UpdateData(DrawingToolType.Text, int.Parse(((ComboBox)sender).Text));
        }
        private void CmbChronoSizeSelectedIndexChanged(object sender, EventArgs e)
        {
        	m_TempColorProfile.UpdateData(DrawingToolType.Chrono, int.Parse(((ComboBox)sender).Text));
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
            btnCrossColor.BackColor = m_TempColorProfile.ColorCross2D;
            btnAngleColor.BackColor = m_TempColorProfile.ColorAngle2D;
            btnChronoColor.BackColor = m_TempColorProfile.ColorChrono;
            FixColors();
            
            // Styles
            btnLineStyle.Tag = m_TempColorProfile.StyleLine2D;
            btnPencilStyle.Tag = m_TempColorProfile.StylePencil;
            
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
            btnCrossColor.FlatAppearance.MouseOverBackColor = btnCrossColor.BackColor;
            btnAngleColor.FlatAppearance.MouseOverBackColor = btnAngleColor.BackColor;
            btnChronoColor.FlatAppearance.MouseOverBackColor = btnChronoColor.BackColor;

            // Put a black frame around white rectangles.
            FixButtonColor(btnTextColor);
            FixButtonColor(btnPencilColor);
            FixButtonColor(btnLineColor);
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