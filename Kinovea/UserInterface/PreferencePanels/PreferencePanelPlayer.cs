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
        private bool interactiveFrameTracker;
        private TimecodeFormat timecodeFormat;
        private ImageAspectRatio imageAspectRatio;
        private SpeedUnit speedUnit;
        private AccelerationUnit accelerationUnit;
        private AngleUnit angleUnit;
        private AngularVelocityUnit angularVelocityUnit;
        private AngularAccelerationUnit angularAccelerationUnit;
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
            interactiveFrameTracker = PreferencesManager.PlayerPreferences.InteractiveFrameTracker;
            timecodeFormat = PreferencesManager.PlayerPreferences.TimecodeFormat;
            imageAspectRatio = PreferencesManager.PlayerPreferences.AspectRatio;       
            speedUnit = PreferencesManager.PlayerPreferences.SpeedUnit;
            accelerationUnit = PreferencesManager.PlayerPreferences.AccelerationUnit;
            angleUnit = PreferencesManager.PlayerPreferences.AngleUnit;
            angularVelocityUnit = PreferencesManager.PlayerPreferences.AngularVelocityUnit;
            angularAccelerationUnit = PreferencesManager.PlayerPreferences.AngularAccelerationUnit;
            
            syncLockSpeeds = PreferencesManager.PlayerPreferences.SyncLockSpeed;
            
            workingZoneSeconds = PreferencesManager.PlayerPreferences.WorkingZoneSeconds;
            workingZoneMemory = PreferencesManager.PlayerPreferences.WorkingZoneMemory;
        }
        private void InitPage()
        {
            // General tab
            tabGeneral.Text = RootLang.dlgPreferences_ButtonGeneral;
            chkDeinterlace.Text = RootLang.dlgPreferences_DeinterlaceByDefault;
            chkInteractiveTracker.Text = RootLang.dlgPreferences_InteractiveFrameTracker;
            chkLockSpeeds.Text = RootLang.dlgPreferences_SyncLockSpeeds; 
                
            // Combo Image Aspect Ratios (MUST be filled in the order of the enum)
            lblImageFormat.Text = RootLang.dlgPreferences_LabelImageFormat;
            cmbImageFormats.Items.Add(RootLang.dlgPreferences_FormatAuto);
            cmbImageFormats.Items.Add(RootLang.dlgPreferences_Format43);
            cmbImageFormats.Items.Add(RootLang.dlgPreferences_Format169);
            
            // Units tab
            lblTimeMarkersFormat.Text = RootLang.dlgPreferences_LabelTimeFormat + " :";
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Classic);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Frames);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Milliseconds);
            cmbTimeCodeFormat.Items.Add("Total microseconds");
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_TenThousandthOfHours);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_HundredthOfMinutes);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_TimeAndFrames);
            //cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Normalized);
            //cmbTimeCodeFormat.Items.Add("Normalized");
            //cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Timestamps); // Debug purposes.
            
            // Combo Speed units (MUST be filled in the order of the enum)
            //lblSpeedUnit.Text = RootLang.dlgPreferences_LabelSpeedUnit;
            lblSpeedUnit.Text = "Speed:";
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_MetersPerSecond, UnitHelper.SpeedAbbreviation(SpeedUnit.MetersPerSecond)));
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_KilometersPerHour, UnitHelper.SpeedAbbreviation(SpeedUnit.KilometersPerHour)));
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_FeetPerSecond, UnitHelper.SpeedAbbreviation(SpeedUnit.FeetPerSecond)));
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_MilesPerHour, UnitHelper.SpeedAbbreviation(SpeedUnit.MilesPerHour)));
            //cmbSpeedUnit.Items.Add(RootLang.dlgPreferences_Speed_Knots); // Is this useful at all ?

            //lblSpeedUnit.Text = RootLang.dlgPreferences_LabelSpeedUnit;
            lblAccelerationUnit.Text = "Acceleration:";
            cmbAccelerationUnit.Items.Add(String.Format("Meters per second squared ({0})", UnitHelper.AccelerationAbbreviation(AccelerationUnit.MetersPerSecondSquared)));
            cmbAccelerationUnit.Items.Add(String.Format("Feet per second squared ({0})", UnitHelper.AccelerationAbbreviation(AccelerationUnit.FeetPerSecondSquared)));

            lblAngleUnit.Text = "Angle:";
            cmbAngleUnit.Items.Add(String.Format("Degrees ({0})", UnitHelper.AngleAbbreviation(AngleUnit.Degree)));
            cmbAngleUnit.Items.Add(String.Format("Radians ({0})", UnitHelper.AngleAbbreviation(AngleUnit.Radian)));

            lblAngularVelocityUnit.Text = "Angular velocity:";
            cmbAngularVelocityUnit.Items.Add(String.Format("Degrees per second ({0})", UnitHelper.AngularVelocityAbbreviation(AngularVelocityUnit.DegreesPerSecond)));
            cmbAngularVelocityUnit.Items.Add(String.Format("Radians per second ({0})", UnitHelper.AngularVelocityAbbreviation(AngularVelocityUnit.RadiansPerSecond)));
            cmbAngularVelocityUnit.Items.Add(String.Format("Revolutions per minute ({0})", UnitHelper.AngularVelocityAbbreviation(AngularVelocityUnit.RevolutionsPerMinute)));

            lblAngularAcceleration.Text = "Angular acceleration:";
            cmbAngularAccelerationUnit.Items.Add(String.Format("Degrees per second squared ({0})", UnitHelper.AngularAccelerationAbbreviation(AngularAccelerationUnit.DegreesPerSecondSquared)));
            cmbAngularAccelerationUnit.Items.Add(String.Format("Radians per second squared ({0})", UnitHelper.AngularAccelerationAbbreviation(AngularAccelerationUnit.RadiansPerSecondSquared)));

            // Memory tab
            tabMemory.Text = RootLang.dlgPreferences_Capture_tabMemory;
            grpSwitchToAnalysis.Text = RootLang.dlgPreferences_GroupAnalysisMode;
            lblWorkingZoneLogic.Text = RootLang.dlgPreferences_LabelLogic;
            
            // Fill in initial values.            
            chkDeinterlace.Checked = deinterlaceByDefault;
            chkLockSpeeds.Checked = syncLockSpeeds;
            chkInteractiveTracker.Checked = interactiveFrameTracker;
            SelectCurrentUnits();
            SelectCurrentImageFormat();
            
            trkWorkingZoneSeconds.Value = workingZoneSeconds;
            lblWorkingZoneSeconds.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneSeconds, trkWorkingZoneSeconds.Value);
            trkWorkingZoneMemory.Value = workingZoneMemory;
            lblWorkingZoneMemory.Text = String.Format(RootLang.dlgPreferences_LabelWorkingZoneMemory, trkWorkingZoneMemory.Value);
        }
        private void SelectCurrentUnits()
        {
            int time = (int)timecodeFormat;
            cmbTimeCodeFormat.SelectedIndex = time < cmbTimeCodeFormat.Items.Count ? time : 0;

            int speed = (int)speedUnit;
            cmbSpeedUnit.SelectedIndex = speed < cmbSpeedUnit.Items.Count ? speed : 0;

            int acceleration = (int)accelerationUnit;
            cmbAccelerationUnit.SelectedIndex = acceleration < cmbAccelerationUnit.Items.Count ? acceleration : 0;

            int angle = (int)angleUnit;
            cmbAngleUnit.SelectedIndex = angle < cmbAngleUnit.Items.Count ? angle : 0;

            int angularVelocity = (int)angularVelocityUnit;
            cmbAngularVelocityUnit.SelectedIndex = angularVelocity < cmbAngularVelocityUnit.Items.Count ? angularVelocity : 0;

            int angularAcceleration = (int)angularAccelerationUnit;
            cmbAngularAccelerationUnit.SelectedIndex = angularAcceleration < cmbAngularAccelerationUnit.Items.Count ? angularAcceleration : 0;
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
        private void ChkInteractiveTrackerCheckedChanged(object sender, EventArgs e)
        {
            interactiveFrameTracker = chkInteractiveTracker.Checked;
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
        private void cmbAccelerationUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            accelerationUnit = (AccelerationUnit)cmbAccelerationUnit.SelectedIndex;
        }
        private void cmbAngleUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            angleUnit = (AngleUnit)cmbAngleUnit.SelectedIndex;
        }
        private void cmbAngularVelocityUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            angularVelocityUnit = (AngularVelocityUnit)cmbAngularVelocityUnit.SelectedIndex;
        }
        private void cmbAngularAccelerationUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            angularAccelerationUnit = (AngularAccelerationUnit)cmbAngularAccelerationUnit.SelectedIndex;
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
            PreferencesManager.PlayerPreferences.InteractiveFrameTracker = interactiveFrameTracker;
            PreferencesManager.PlayerPreferences.TimecodeFormat = timecodeFormat;
            PreferencesManager.PlayerPreferences.AspectRatio = imageAspectRatio;
            PreferencesManager.PlayerPreferences.SpeedUnit = speedUnit;
            PreferencesManager.PlayerPreferences.AccelerationUnit = accelerationUnit;
            PreferencesManager.PlayerPreferences.AngleUnit = angleUnit;
            PreferencesManager.PlayerPreferences.AngularVelocityUnit = angularVelocityUnit;
            PreferencesManager.PlayerPreferences.AngularAccelerationUnit = angularAccelerationUnit;          
            PreferencesManager.PlayerPreferences.WorkingZoneSeconds = workingZoneSeconds;
            PreferencesManager.PlayerPreferences.WorkingZoneMemory = workingZoneMemory;
        }
    }
}
