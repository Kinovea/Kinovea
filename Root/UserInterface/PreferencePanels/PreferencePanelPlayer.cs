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
using System.Windows.Forms;

using Kinovea.Root.Languages;
using Kinovea.Root.Properties;
using Kinovea.ScreenManager;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.Root
{
	/// <summary>
	/// PreferencePanelPlayer.
	/// </summary>
	public partial class PreferencePanelPlayer : UserControl, IPreferencePanel
	{
		#region IPreferencePanel properties
		public string Description
		{
			get { return description;}
		}
		public Bitmap Icon
		{
			get { return icon;}
		}
		#endregion
		
		#region Members
        private string description;
		private Bitmap icon;
		private bool deinterlaceByDefault;
        private TimecodeFormat timecodeFormat;
        private ImageAspectRatio imageAspectRatio;
		private SpeedUnit speedUnit;
		private bool syncLockSpeeds;
        private int workingZoneSeconds;
        private int workingZoneMemory;
        #endregion
		
		#region Construction & Initialization
		public PreferencePanelPlayer()
		{
			InitializeComponent();
			this.BackColor = Color.White;
			
			description = RootLang.dlgPreferences_ButtonPlayAnalyze;
			icon = Resources.video;
			
			ImportPreferences();
			InitPage();
		}
		private void ImportPreferences()
        {
 			deinterlaceByDefault = PreferencesManager.PlayerPreferences.DeinterlaceByDefault;
 			timecodeFormat = PreferencesManager.PlayerPreferences.TimecodeFormat;
 			imageAspectRatio = PreferencesManager.PlayerPreferences.AspectRatio;       
			speedUnit = PreferencesManager.PlayerPreferences.SpeedUnit;
			syncLockSpeeds = PreferencesManager.PlayerPreferences.SyncLockSpeed;
			
 			workingZoneSeconds = PreferencesManager.PlayerPreferences.WorkingZoneSeconds;
 			workingZoneMemory = PreferencesManager.PlayerPreferences.WorkingZoneMemory;
		}
		private void InitPage()
		{
            // General tab
            tabGeneral.Text = RootLang.dlgPreferences_ButtonGeneral;
		    chkDeinterlace.Text = RootLang.dlgPreferences_DeinterlaceByDefault;
            chkLockSpeeds.Text = RootLang.dlgPreferences_SyncLockSpeeds; 
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
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_MetersPerSecond, CalibrationHelper.GetSpeedAbbreviationFromUnit(SpeedUnit.MetersPerSecond)));
			cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_KilometersPerHour, CalibrationHelper.GetSpeedAbbreviationFromUnit(SpeedUnit.KilometersPerHour)));
			cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_FeetPerSecond, CalibrationHelper.GetSpeedAbbreviationFromUnit(SpeedUnit.FeetPerSecond)));
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_MilesPerHour, CalibrationHelper.GetSpeedAbbreviationFromUnit(SpeedUnit.MilesPerHour)));
            //cmbSpeedUnit.Items.Add(RootLang.dlgPreferences_Speed_Knots);		// Is this useful at all ?
            	
	        // Combo Image Aspect Ratios (MUST be filled in the order of the enum)
            lblImageFormat.Text = RootLang.dlgPreferences_LabelImageFormat;
            cmbImageFormats.Items.Add(RootLang.dlgPreferences_FormatAuto);
            cmbImageFormats.Items.Add(RootLang.dlgPreferences_Format43);
            cmbImageFormats.Items.Add(RootLang.dlgPreferences_Format169);
            
            // Memory tab
            tabMemory.Text = RootLang.dlgPreferences_Capture_tabMemory;
            grpSwitchToAnalysis.Text = RootLang.dlgPreferences_GroupAnalysisMode;
            lblWorkingZoneLogic.Text = RootLang.dlgPreferences_LabelLogic;
            
            // Fill in initial values.            
            chkDeinterlace.Checked = deinterlaceByDefault;
            chkLockSpeeds.Checked = syncLockSpeeds;
            SelectCurrentTimecodeFormat();
            SelectCurrentSpeedUnit();
            SelectCurrentImageFormat();
            
            trkWorkingZoneSeconds.Value = workingZoneSeconds;
            lblWorkingZoneSeconds.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneSeconds, trkWorkingZoneSeconds.Value);
            trkWorkingZoneMemory.Value = workingZoneMemory;
            lblWorkingZoneMemory.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneMemory, trkWorkingZoneMemory.Value);
		}
		private void SelectCurrentTimecodeFormat()
        {
            int selected = (int)timecodeFormat;
            cmbTimeCodeFormat.SelectedIndex = selected < cmbTimeCodeFormat.Items.Count ? selected : 0;
        }
		private void SelectCurrentSpeedUnit()
        {
		    int selected = (int)speedUnit;
            cmbSpeedUnit.SelectedIndex = selected < cmbSpeedUnit.Items.Count ? selected : 0;
        }
        private void SelectCurrentImageFormat()
        {
            int selected = (int)imageAspectRatio;
            cmbImageFormats.SelectedIndex = selected < cmbImageFormats.Items.Count ? selected : 0;
        }
		#endregion
		
		#region Handlers
        private void ChkDeinterlaceCheckedChanged(object sender, EventArgs e)
        {
        	deinterlaceByDefault = chkDeinterlace.Checked;
        }
        private void ChkLockSpeedsCheckedChanged(object sender, EventArgs e)
		{
            syncLockSpeeds = chkLockSpeeds.Checked;
        }
        private void cmbTimeCodeFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            timecodeFormat = (TimecodeFormat)cmbTimeCodeFormat.SelectedIndex;
        }
        private void cmbImageAspectRatio_SelectedIndexChanged(object sender, EventArgs e)
        {
            imageAspectRatio = (ImageAspectRatio)cmbImageFormats.SelectedIndex;
        }
		private void cmbSpeedUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            speedUnit = (SpeedUnit)cmbSpeedUnit.SelectedIndex;
        }
        private void trkWorkingZoneSeconds_ValueChanged(object sender, EventArgs e)
        {
            lblWorkingZoneSeconds.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneSeconds, trkWorkingZoneSeconds.Value);
            workingZoneSeconds = trkWorkingZoneSeconds.Value;
        }
        private void trkWorkingZoneMemory_ValueChanged(object sender, EventArgs e)
        {
            lblWorkingZoneMemory.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneMemory, trkWorkingZoneMemory.Value);
            workingZoneMemory = trkWorkingZoneMemory.Value;
        }
        
		#endregion
		
		public void CommitChanges()
		{
            PreferencesManager.PlayerPreferences.DeinterlaceByDefault = deinterlaceByDefault;
            PreferencesManager.PlayerPreferences.SyncLockSpeed = syncLockSpeeds;
            PreferencesManager.PlayerPreferences.TimecodeFormat = timecodeFormat;
            PreferencesManager.PlayerPreferences.AspectRatio = imageAspectRatio;
			PreferencesManager.PlayerPreferences.SpeedUnit = speedUnit;
            PreferencesManager.PlayerPreferences.WorkingZoneSeconds = workingZoneSeconds;
            PreferencesManager.PlayerPreferences.WorkingZoneMemory = workingZoneMemory;
		}
	}
}
