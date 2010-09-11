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

using Kinovea.ScreenManager;
using System;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Root.Languages;
using Kinovea.Services;

namespace Kinovea.Root
{
    public partial class formPreferences : Form
    {
        #region enum
        private enum Pages
        {
            General,
            Player, 
            Drawings,
            NumberOfPages
        }
        #endregion

        #region Members
        // Prefs values
        private int m_iFilesToSave;
        private string m_UICultureName;
        private TimeCodeFormat m_TimeCodeFormat;
        private SpeedUnits m_SpeedUnit;
        private ImageAspectRatio m_ImageAspectRatio;
        private bool m_bDeinterlaceByDefault;
        private Color m_GridColor;
        private Color m_Plane3DColor;
        private int m_iWorkingZoneSeconds;
        private int m_iWorkingZoneMemory;
        private InfosFading m_DefaultFading;
        private bool m_bDrawOnPlay;
        
        // Helpers 
        private PreferencesManager m_prefManager;
        #endregion

        #region Constructor
        public formPreferences()
        {
            InitializeComponent();
            m_prefManager = PreferencesManager.Instance();
            ImportPreferences();
            InitPages();
        }
        #endregion

        #region Pages navigation and Setup
        private void ImportPreferences()
        {
            // Put the values in the current pref file into local variables
            // They will then be used to fill the controls.

            m_iFilesToSave = m_prefManager.HistoryCount;
            m_UICultureName = m_prefManager.GetSupportedCulture().Name;
            m_TimeCodeFormat = m_prefManager.TimeCodeFormat;
            m_SpeedUnit = m_prefManager.SpeedUnit;
 			m_ImageAspectRatio = m_prefManager.AspectRatio;       
            m_bDeinterlaceByDefault = m_prefManager.DeinterlaceByDefault;
 			m_GridColor = m_prefManager.GridColor;
            m_Plane3DColor = m_prefManager.Plane3DColor;
            m_iWorkingZoneSeconds = m_prefManager.WorkingZoneSeconds;
            m_iWorkingZoneMemory = m_prefManager.WorkingZoneMemory;
            m_DefaultFading = new InfosFading(0, 0);
            m_bDrawOnPlay = m_prefManager.DrawOnPlay;
        }
        private void InitPages()
        {
            //this.Size = new Size(608, 356);
            //this.Height = 356;

            // Culture
            this.Text = "   " + RootLang.dlgPreferences_Title;
            btnGeneral.Text = RootLang.dlgPreferences_ButtonGeneral;
            grpGeneral.Text = RootLang.dlgPreferences_ButtonGeneral;
            
            lblLanguage.Text = RootLang.dlgPreferences_LabelLanguages;
            cmbLanguage.Items.Clear();
            
            // Order : Native Alphabetical.
            // Deutsh, Greek, English, Español, Français, Italiano, Nederlands, Norsk, Polski, Portuges, Romana, Suomi, chinese.
            cmbLanguage.Items.Add(new LanguageIdentifier("de", PreferencesManager.LanguageGerman));
            cmbLanguage.Items.Add(new LanguageIdentifier("el", PreferencesManager.LanguageGreek));
            cmbLanguage.Items.Add(new LanguageIdentifier("en", PreferencesManager.LanguageEnglish));
            cmbLanguage.Items.Add(new LanguageIdentifier("es", PreferencesManager.LanguageSpanish));
            cmbLanguage.Items.Add(new LanguageIdentifier("fr", PreferencesManager.LanguageFrench));
            cmbLanguage.Items.Add(new LanguageIdentifier("it", PreferencesManager.LanguageItalian));
            cmbLanguage.Items.Add(new LanguageIdentifier("nl", PreferencesManager.LanguageDutch));
            cmbLanguage.Items.Add(new LanguageIdentifier("no", PreferencesManager.LanguageNorwegian));
            cmbLanguage.Items.Add(new LanguageIdentifier("pl", PreferencesManager.LanguagePolish));
            cmbLanguage.Items.Add(new LanguageIdentifier("pt", PreferencesManager.LanguagePortuguese));
            cmbLanguage.Items.Add(new LanguageIdentifier("ro", PreferencesManager.LanguageRomanian));
            cmbLanguage.Items.Add(new LanguageIdentifier("fi", PreferencesManager.LanguageFinnish));
            cmbLanguage.Items.Add(new LanguageIdentifier("tr", PreferencesManager.LanguageTurkish));
            cmbLanguage.Items.Add(new LanguageIdentifier("zh-CHS", PreferencesManager.LanguageChinese));

            lblHistoryCount.Text = RootLang.dlgPreferences_LabelHistoryCount;

            // Combo TimeCodeFormats (MUST be filled in the order of the enum)
            lblTimeMarkersFormat.Text = RootLang.dlgPreferences_LabelTimeFormat + " :";
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Classic);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Frames);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Milliseconds);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_TenThousandthOfHours);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_HundredthOfMinutes);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_TimeAndFrames);
            //cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Timestamps);	// Debug purposes.

            // Combo Speed units (MUST be filled in the order of the enum)
            lblSpeedUnit.Text = RootLang.dlgPreferences_LabelSpeedUnit;
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_MetersPerSecond, CalibrationHelper.GetSpeedAbbreviationFromUnit(SpeedUnits.MetersPerSecond)));
			cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_KilometersPerHour, CalibrationHelper.GetSpeedAbbreviationFromUnit(SpeedUnits.KilometersPerHour)));
			cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_FeetPerSecond, CalibrationHelper.GetSpeedAbbreviationFromUnit(SpeedUnits.FeetPerSecond)));
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_MilesPerHour, CalibrationHelper.GetSpeedAbbreviationFromUnit(SpeedUnits.MilesPerHour)));
            //cmbSpeedUnit.Items.Add(RootLang.dlgPreferences_Speed_Knots);		// Is this useful at all ?
            	
	        // Combo Image Aspect Ratios (MUST be filled in the order of the enum)
            lblImageFormat.Text = RootLang.dlgPreferences_LabelImageFormat;
            cmbImageFormats.Items.Add(RootLang.dlgPreferences_FormatAuto);
            cmbImageFormats.Items.Add(RootLang.dlgPreferences_Format43);
            cmbImageFormats.Items.Add(RootLang.dlgPreferences_Format169);
               
            chkDeinterlace.Text = RootLang.dlgPreferences_DeinterlaceByDefault;
            
            // Playback page.
            btnPlayAnalyze.Text = RootLang.dlgPreferences_ButtonPlayAnalyze;
            grpColors.Text = RootLang.dlgPreferences_GroupColors;
            lblGrid.Text = RootLang.dlgPreferences_LabelGrid;
            lblPlane3D.Text = RootLang.dlgPreferences_LabelPlane3D;
            
            grpSwitchToAnalysis.Text = RootLang.dlgPreferences_GroupAnalysisMode;
            lblWorkingZoneLogic.Text = RootLang.dlgPreferences_LabelLogic;

            // Drawings page.
            btnDrawings.Text = RootLang.dlgPreferences_btnDrawings;
            grpDrawingsFading.Text = RootLang.dlgPreferences_grpPersistence;
            chkEnablePersistence.Text = RootLang.dlgPreferences_chkEnablePersistence;
            chkDrawOnPlay.Text = RootLang.dlgPreferences_chkDrawOnPlay;
			chkAlwaysVisible.Text = RootLang.dlgPreferences_chkAlwaysVisible;
            
            btnSave.Text = RootLang.Generic_Save;
            btnCancel.Text = RootLang.Generic_Cancel;

            // Set up the controls with current prefs values.
            SelectCurrentLanguage();
            SelectCurrentTimecodeFormat();
            SelectCurrentSpeedUnit();
            SelectCurrentImageFormat();
            chkDeinterlace.Checked = m_bDeinterlaceByDefault;
            cmbHistoryCount.SelectedIndex = m_iFilesToSave;
            btnGridColor.BackColor = m_GridColor;
            btn3DPlaneColor.BackColor = m_Plane3DColor;
            FixColors();
            trkWorkingZoneSeconds.Value = m_iWorkingZoneSeconds;
            trkWorkingZoneMemory.Value = m_iWorkingZoneMemory;
            lblWorkingZoneSeconds.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneSeconds, trkWorkingZoneSeconds.Value);
            lblWorkingZoneMemory.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneMemory, trkWorkingZoneMemory.Value);


            chkEnablePersistence.Checked = m_DefaultFading.Enabled;
            trkFading.Value = m_DefaultFading.FadingFrames;
            chkAlwaysVisible.Checked = m_DefaultFading.AlwaysVisible;
            EnableDisableFadingOptions();
            chkDrawOnPlay.Checked = m_bDrawOnPlay;
            lblFading.Text = String.Format(RootLang.dlgPreferences_lblFading, trkFading.Value);

            ShowPage(Pages.General);
            PositionAllPages();
        }
        private void PositionAllPages()
        {
            int iTop = 8;
            pageGeneral.Top = iTop;
            pagePlayAnalyze.Top = iTop;
            pageDrawings.Top = iTop;
        }
        private void ShowPage(Pages _page)
        {
            HideAllPages();

            switch (_page)
            {
                case Pages.Player:
                    pagePlayAnalyze.Visible = true;
                    btnPlayAnalyze.BackColor = Color.WhiteSmoke;
                    break;
                case Pages.Drawings:
                    pageDrawings.Visible = true;
                    btnDrawings.BackColor = Color.WhiteSmoke;
                    break;
                case Pages.General:
                default:
                    btnGeneral.BackColor = Color.WhiteSmoke;
                    pageGeneral.Visible = true;
                    break;   
            }
        }
        private void HideAllPages()
        {
            pageGeneral.Visible = false;
            pagePlayAnalyze.Visible = false;
            pageDrawings.Visible = false;

            btnGeneral.BackColor = Color.White;
            btnPlayAnalyze.BackColor = Color.White;
            btnDrawings.BackColor = Color.White;
        }
        private void btnGeneral_Click(object sender, EventArgs e)
        {
            ShowPage(Pages.General);
        }
        private void btnPlayAnalyze_Click(object sender, EventArgs e)
        {
            ShowPage(Pages.Player);
        }
        private void btnDrawings_Click(object sender, EventArgs e)
        {
            ShowPage(Pages.Drawings);
        }
        private void SelectCurrentLanguage()
        {
            bool found = false;
            for(int i=0;i<cmbLanguage.Items.Count;i++)
            {
                LanguageIdentifier li = (LanguageIdentifier)cmbLanguage.Items[i];
                
                if (li.CultureName.Equals(m_UICultureName))
                {
                    // Matching
                    cmbLanguage.SelectedIndex = i;            
                    found = true;
                }
            }
            if(!found)
            {
                // The supported language is not in the combo box. (error).
                cmbLanguage.SelectedIndex = 0;   
            }
        }
        private void SelectCurrentTimecodeFormat()
        {
            // the combo box items have been filled in the order of the enum.
            if ((int)m_TimeCodeFormat < cmbTimeCodeFormat.Items.Count)
            {
                cmbTimeCodeFormat.SelectedIndex = (int)m_TimeCodeFormat;
            }
            else
            {
                cmbTimeCodeFormat.SelectedIndex = 0;
            }
        }
        private void SelectCurrentSpeedUnit()
        {
            // the combo box items have been filled in the order of the enum.
            if ((int)m_SpeedUnit < cmbSpeedUnit.Items.Count)
            {
                cmbSpeedUnit.SelectedIndex = (int)m_SpeedUnit;
            }
            else
            {
                cmbSpeedUnit.SelectedIndex = 0;
            }
        }
        private void SelectCurrentImageFormat()
        {
        	// the combo box items have been filled in the order of the enum.
            if ((int)m_ImageAspectRatio < cmbImageFormats.Items.Count)
            {
                cmbImageFormats.SelectedIndex = (int)m_ImageAspectRatio;
            }
            else
            {
                cmbImageFormats.SelectedIndex = 0;
            }
        }
        #endregion

        #region Handlers
  
        #region Page:General
        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_UICultureName = ((LanguageIdentifier)cmbLanguage.Items[cmbLanguage.SelectedIndex]).CultureName;
        }
        private void cmbHistoryCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_iFilesToSave = cmbHistoryCount.SelectedIndex;
        }
        private void cmbTimeCodeFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            // the combo box items have been filled in the order of the enum.
            m_TimeCodeFormat = (TimeCodeFormat)cmbTimeCodeFormat.SelectedIndex;
        }
        private void cmbSpeedUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            // the combo box items have been filled in the order of the enum.
            m_SpeedUnit = (SpeedUnits)cmbSpeedUnit.SelectedIndex;
        }
        private void cmbImageAspectRatio_SelectedIndexChanged(object sender, EventArgs e)
        {
            // the combo box items have been filled in the order of the enum.
            m_ImageAspectRatio = (ImageAspectRatio)cmbImageFormats.SelectedIndex;
        }
        private void ChkDeinterlaceCheckedChanged(object sender, EventArgs e)
        {
        	m_bDeinterlaceByDefault = chkDeinterlace.Checked;
        }
        #endregion
      
        #region Page:Play/Analysis
        private void btnGridColor_Click(object sender, EventArgs e)
        {
        	FormColorPicker picker = new FormColorPicker();
        	if(picker.ShowDialog() == DialogResult.OK)
        	{
        		btnGridColor.BackColor = picker.PickedColor;
                m_GridColor = picker.PickedColor;
                FixColors();
        	}
        	picker.Dispose();
        }
        private void btn3DPlaneColor_Click(object sender, EventArgs e)
        {
        	FormColorPicker picker = new FormColorPicker();
        	if(picker.ShowDialog() == DialogResult.OK)
        	{
        		btn3DPlaneColor.BackColor = picker.PickedColor;
                m_Plane3DColor = picker.PickedColor;
                FixColors();
        	}
        	picker.Dispose();
            
        }
        private void trkWorkingZoneSeconds_ValueChanged(object sender, EventArgs e)
        {
            lblWorkingZoneSeconds.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneSeconds, trkWorkingZoneSeconds.Value);
            m_iWorkingZoneSeconds = trkWorkingZoneSeconds.Value;
        }
        private void trkWorkingZoneMemory_ValueChanged(object sender, EventArgs e)
        {
            lblWorkingZoneMemory.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneMemory, trkWorkingZoneMemory.Value);
            m_iWorkingZoneMemory = trkWorkingZoneMemory.Value;
        }
        #endregion
        
        #region Page:Drawings
        private void chkFading_CheckedChanged(object sender, EventArgs e)
        {
            m_DefaultFading.Enabled = chkEnablePersistence.Checked;
            EnableDisableFadingOptions();
        }
        private void EnableDisableFadingOptions()
        {
            trkFading.Enabled = chkEnablePersistence.Checked;
            lblFading.Enabled = chkEnablePersistence.Checked;
            chkAlwaysVisible.Enabled = chkEnablePersistence.Checked;
        }
        private void trkFading_ValueChanged(object sender, EventArgs e)
        {
            lblFading.Text = String.Format(RootLang.dlgPreferences_lblFading, trkFading.Value);
            m_DefaultFading.FadingFrames = trkFading.Value;
            chkAlwaysVisible.Checked = false;
        }
        private void chkAlwaysVisible_CheckedChanged(object sender, EventArgs e)
        {
        	m_DefaultFading.AlwaysVisible = chkAlwaysVisible.Checked;	
        }
        private void chkDrawOnPlay_CheckedChanged(object sender, EventArgs e)
        {
            m_bDrawOnPlay = chkDrawOnPlay.Checked;
        }
        #endregion
        
        #endregion

        private void FixColors()
        {
            // Put a black frame around white rectangles.
            // set the mouse over color to the same color.

            btnGridColor.FlatAppearance.MouseOverBackColor = btnGridColor.BackColor;
            if (Color.Equals(btnGridColor.BackColor, Color.FromArgb(255, 255, 255)) || Color.Equals(btnGridColor.BackColor, Color.White))
            {
                btnGridColor.FlatAppearance.BorderSize = 1;
            }
            else
            {
                btnGridColor.FlatAppearance.BorderSize = 0;
            }

            btn3DPlaneColor.FlatAppearance.MouseOverBackColor = btn3DPlaneColor.BackColor;
            if (Color.Equals(btn3DPlaneColor.BackColor, Color.FromArgb(255, 255, 255)) || Color.Equals(btn3DPlaneColor.BackColor, Color.White))
            {
                btn3DPlaneColor.FlatAppearance.BorderSize = 1;
            }
            else
            {
                btn3DPlaneColor.FlatAppearance.BorderSize = 0;
            }

        }

        #region Save & Cancel Handlers
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Save prefs.
            
            m_prefManager.HistoryCount = m_iFilesToSave;
            m_prefManager.UICultureName = m_UICultureName;
            m_prefManager.TimeCodeFormat = m_TimeCodeFormat;
            m_prefManager.SpeedUnit = m_SpeedUnit;
            m_prefManager.AspectRatio = m_ImageAspectRatio;
            m_prefManager.DeinterlaceByDefault = m_bDeinterlaceByDefault;
            m_prefManager.GridColor = m_GridColor;
            m_prefManager.Plane3DColor = m_Plane3DColor;
            m_prefManager.WorkingZoneSeconds = m_iWorkingZoneSeconds;
            m_prefManager.WorkingZoneMemory = m_iWorkingZoneMemory;
            m_prefManager.DefaultFading.FromInfosFading(m_DefaultFading);
            m_prefManager.DrawOnPlay = m_bDrawOnPlay;

            // persist to file
            m_prefManager.Export();
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        
        
        
        
        
        
    }
}