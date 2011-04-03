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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using Kinovea.Root.Languages;
using Kinovea.Root.Properties;
using Kinovea.ScreenManager;
using Kinovea.Services;

namespace Kinovea.Root
{
	/// <summary>
	/// PreferencePanelGeneral.
	/// </summary>
	public partial class PreferencePanelGeneral : UserControl, IPreferencePanel
	{
		#region IPreferencePanel properties
		public string Description
		{
			get { return m_Description;}
		}
		public Bitmap Icon
		{
			get { return m_Icon;}
		}
		private string m_Description;
		private Bitmap m_Icon;
		#endregion
		
		#region Members
		private string m_UICultureName;
		private int m_iFilesToSave;
        private TimeCodeFormat m_TimeCodeFormat;
        private ImageAspectRatio m_ImageAspectRatio;
		private SpeedUnits m_SpeedUnit;
        
        private PreferencesManager m_prefManager;
		#endregion
		
		#region Construction & Initialization
		public PreferencePanelGeneral()
		{
			InitializeComponent();
			this.BackColor = Color.White;
			
			m_prefManager = PreferencesManager.Instance();
			
			m_Description = RootLang.dlgPreferences_ButtonGeneral;
			m_Icon = Resources.pref_general;
			
			ImportPreferences();
			InitPage();
		}
		private void ImportPreferences()
        {
            CultureInfo ci = m_prefManager.GetSupportedCulture();
            m_UICultureName = ci.IsNeutralCulture ? ci.Name : ci.Parent.Name;
            m_iFilesToSave = m_prefManager.HistoryCount;
            m_TimeCodeFormat = m_prefManager.TimeCodeFormat;
            m_ImageAspectRatio = m_prefManager.AspectRatio;       
			m_SpeedUnit = m_prefManager.SpeedUnit;
        }
		private void InitPage()
		{
			// Localize and fill possible values
			
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
            cmbLanguage.Items.Add(new LanguageIdentifier("lt", PreferencesManager.LanguageLithuanian));
            cmbLanguage.Items.Add(new LanguageIdentifier("nl", PreferencesManager.LanguageDutch));
            cmbLanguage.Items.Add(new LanguageIdentifier("no", PreferencesManager.LanguageNorwegian));
            cmbLanguage.Items.Add(new LanguageIdentifier("pl", PreferencesManager.LanguagePolish));
            cmbLanguage.Items.Add(new LanguageIdentifier("pt", PreferencesManager.LanguagePortuguese));
            cmbLanguage.Items.Add(new LanguageIdentifier("ro", PreferencesManager.LanguageRomanian));
            cmbLanguage.Items.Add(new LanguageIdentifier("fi", PreferencesManager.LanguageFinnish));
            cmbLanguage.Items.Add(new LanguageIdentifier("tr", PreferencesManager.LanguageTurkish));
            cmbLanguage.Items.Add(new LanguageIdentifier("zh-CHS", PreferencesManager.LanguageChinese));
            
            lblHistoryCount.Text = RootLang.dlgPreferences_LabelHistoryCount;

            // Combo TimeCodeFormats (MUST be filled in the order of the enum, see PreferencesManager.TimeCodeFormat)
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
            
            // Fill current values
            SelectCurrentLanguage();
            cmbHistoryCount.SelectedIndex = m_iFilesToSave;
            SelectCurrentTimecodeFormat();
            SelectCurrentSpeedUnit();
            SelectCurrentImageFormat();
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
        private void cmbImageAspectRatio_SelectedIndexChanged(object sender, EventArgs e)
        {
            // the combo box items have been filled in the order of the enum.
            m_ImageAspectRatio = (ImageAspectRatio)cmbImageFormats.SelectedIndex;
        }
		private void cmbSpeedUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            // the combo box items have been filled in the order of the enum.
            m_SpeedUnit = (SpeedUnits)cmbSpeedUnit.SelectedIndex;
        }
		#endregion
		
		public void CommitChanges()
		{
			m_prefManager.UICultureName = m_UICultureName;
			m_prefManager.HistoryCount = m_iFilesToSave;
            m_prefManager.TimeCodeFormat = m_TimeCodeFormat;
            m_prefManager.AspectRatio = m_ImageAspectRatio;
			m_prefManager.SpeedUnit = m_SpeedUnit;
		}
	}
}
