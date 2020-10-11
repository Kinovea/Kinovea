#region License
/*
Copyright © Joan Charmant 2011.
jcharmant@gmail.com 
 
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
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;
using System.Collections.Generic;
using System.Globalization;

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
        public List<PreferenceTab> Tabs
        {
            get { return tabs; }
        }
        #endregion
        
        #region Members
        private string description;
        private Bitmap icon;
        private List<PreferenceTab> tabs = new List<PreferenceTab> { PreferenceTab.Player_General, PreferenceTab.Player_Units, PreferenceTab.Player_Memory };
        private bool deinterlaceByDefault;
        private bool detectImageSequences;
        private bool interactiveFrameTracker;
        private TimecodeFormat timecodeFormat;
        private ImageAspectRatio imageAspectRatio;
        private SpeedUnit speedUnit;
        private AccelerationUnit accelerationUnit;
        private AngleUnit angleUnit;
        private AngularVelocityUnit angularVelocityUnit;
        private AngularAccelerationUnit angularAccelerationUnit;
        private string customLengthUnit;
        private string customLengthAbbreviation;
        private bool syncLockSpeeds;
        private bool syncByMotion;
        private int memoryBuffer;
        #endregion
        
        #region Construction & Initialization
        public PreferencePanelPlayer()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            
            description = RootLang.dlgPreferences_tabPlayback;
            icon = Resources.video;
            
            ImportPreferences();
            InitPage();
        }

        public void OpenTab(PreferenceTab tab)
        {
            int index = tabs.IndexOf(tab);
            if (index < 0)
                return;

            tabSubPages.SelectedIndex = index;
        }

        public void Close()
        {
        }

        private void ImportPreferences()
        {
            deinterlaceByDefault = PreferencesManager.PlayerPreferences.DeinterlaceByDefault;
            detectImageSequences = PreferencesManager.PlayerPreferences.DetectImageSequences;
            interactiveFrameTracker = PreferencesManager.PlayerPreferences.InteractiveFrameTracker;
            timecodeFormat = PreferencesManager.PlayerPreferences.TimecodeFormat;
            imageAspectRatio = PreferencesManager.PlayerPreferences.AspectRatio;       
            speedUnit = PreferencesManager.PlayerPreferences.SpeedUnit;
            accelerationUnit = PreferencesManager.PlayerPreferences.AccelerationUnit;
            angleUnit = PreferencesManager.PlayerPreferences.AngleUnit;
            angularVelocityUnit = PreferencesManager.PlayerPreferences.AngularVelocityUnit;
            angularAccelerationUnit = PreferencesManager.PlayerPreferences.AngularAccelerationUnit;
            customLengthUnit = PreferencesManager.PlayerPreferences.CustomLengthUnit;
            customLengthAbbreviation = PreferencesManager.PlayerPreferences.CustomLengthAbbreviation;
            
            syncLockSpeeds = PreferencesManager.PlayerPreferences.SyncLockSpeed;
            syncByMotion = PreferencesManager.PlayerPreferences.SyncByMotion;
            
            memoryBuffer = PreferencesManager.PlayerPreferences.WorkingZoneMemory;
        }
        private void InitPage()
        {
            InitPageGeneral();
            InitPageMemory();
            InitPageUnits();
        }

        private void InitPageGeneral()
        {
            tabGeneral.Text = RootLang.dlgPreferences_tabGeneral;
            chkDetectImageSequences.Text = RootLang.dlgPreferences_Player_ImportImageSequences;
            chkLockSpeeds.Text = RootLang.dlgPreferences_Player_SyncLockSpeeds;
            chkSyncByMotion.Text = "Use motion synchronization mode"; // RootLang.dlgPreferences_Player_SyncLockSpeeds;
            chkInteractiveTracker.Text = RootLang.dlgPreferences_Player_InteractiveFrameTracker;

            // Combo Image Aspect Ratios (MUST be filled in the order of the enum)
            lblImageFormat.Text = RootLang.dlgPreferences_Player_lblImageFormat;
            cmbImageFormats.Items.Add(ScreenManagerLang.mnuFormatAuto);
            cmbImageFormats.Items.Add(ScreenManagerLang.mnuFormatForce43);
            cmbImageFormats.Items.Add(ScreenManagerLang.mnuFormatForce169);
            
            chkDeinterlace.Text = RootLang.dlgPreferences_Player_DeinterlaceByDefault;

            chkDetectImageSequences.Checked = detectImageSequences;
            chkLockSpeeds.Checked = syncLockSpeeds;
            chkSyncByMotion.Checked = syncByMotion;
            chkInteractiveTracker.Checked = interactiveFrameTracker;
            SelectCurrentImageFormat();
            chkDeinterlace.Checked = deinterlaceByDefault;
        }

        private void InitPageUnits()
        {
            // enum Kinovea.Services.TimecodeFormat.
            tabUnits.Text = RootLang.dlgPreferences_Player_tabUnits;
            lblTimeMarkersFormat.Text = RootLang.dlgPreferences_Player_UnitTime;
            //cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Classic);
            cmbTimeCodeFormat.Items.Add("[h:][mm:]ss.xx[x]");
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Frames);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Milliseconds);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_Microseconds);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_TenThousandthOfHours);
            cmbTimeCodeFormat.Items.Add(RootLang.TimeCodeFormat_HundredthOfMinutes);
            cmbTimeCodeFormat.Items.Add("[h:][mm:]ss.xx[x] + " + RootLang.TimeCodeFormat_Frames);
#if DEBUG
            cmbTimeCodeFormat.Items.Add("Normalized");
            cmbTimeCodeFormat.Items.Add("Timestamps");
#endif

            // enum Kinovea.Services.SpeedUnit.
            lblSpeedUnit.Text = RootLang.dlgPreferences_Player_UnitsSpeed;
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_MetersPerSecond, UnitHelper.SpeedAbbreviation(SpeedUnit.MetersPerSecond)));
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_KilometersPerHour, UnitHelper.SpeedAbbreviation(SpeedUnit.KilometersPerHour)));
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_FeetPerSecond, UnitHelper.SpeedAbbreviation(SpeedUnit.FeetPerSecond)));
            cmbSpeedUnit.Items.Add(String.Format(RootLang.dlgPreferences_Speed_MilesPerHour, UnitHelper.SpeedAbbreviation(SpeedUnit.MilesPerHour)));

            // enum Kinovea.Services.AccelerationUnit.
            lblAccelerationUnit.Text = RootLang.dlgPreferences_Player_UnitsAcceleration;
            cmbAccelerationUnit.Items.Add(String.Format(RootLang.dlgPreferences_Player_UnitsMetersPerSecondSquared, UnitHelper.AccelerationAbbreviation(AccelerationUnit.MetersPerSecondSquared)));
            cmbAccelerationUnit.Items.Add(String.Format(RootLang.dlgPreferences_Player_UnitsFeetPerSecondSquared, UnitHelper.AccelerationAbbreviation(AccelerationUnit.FeetPerSecondSquared)));

            // enum Kinovea.Services.AngleUnit.
            lblAngleUnit.Text = RootLang.dlgPreferences_Player_UnitsAngle;
            cmbAngleUnit.Items.Add(String.Format(RootLang.dlgPreferences_Player_UnitsDegrees, UnitHelper.AngleAbbreviation(AngleUnit.Degree)));
            cmbAngleUnit.Items.Add(String.Format(RootLang.dlgPreferences_Player_UnitsRadians, UnitHelper.AngleAbbreviation(AngleUnit.Radian)));

            // enum Kinovea.Services.AngularVelocityUnit.
            lblAngularVelocityUnit.Text = RootLang.dlgPreferences_Player_UnitsAngularVelocity;
            cmbAngularVelocityUnit.Items.Add(String.Format(RootLang.dlgPreferences_Player_UnitsDegreesPerSecond, UnitHelper.AngularVelocityAbbreviation(AngularVelocityUnit.DegreesPerSecond)));
            cmbAngularVelocityUnit.Items.Add(String.Format(RootLang.dlgPreferences_Player_UnitsRadiansPerSecond, UnitHelper.AngularVelocityAbbreviation(AngularVelocityUnit.RadiansPerSecond)));
            cmbAngularVelocityUnit.Items.Add(String.Format(RootLang.dlgPreferences_Player_UnitsRevolutionsPerMinute, UnitHelper.AngularVelocityAbbreviation(AngularVelocityUnit.RevolutionsPerMinute)));

            // enum Kinovea.Services.AngularAccelerationUnit.
            lblAngularAcceleration.Text = RootLang.dlgPreferences_Player_UnitsAngularAcceleration;
            cmbAngularAccelerationUnit.Items.Add(String.Format(RootLang.dlgPreferences_Player_UnitsDegreesPerSecondSquared, UnitHelper.AngularAccelerationAbbreviation(AngularAccelerationUnit.DegreesPerSecondSquared)));
            cmbAngularAccelerationUnit.Items.Add(String.Format(RootLang.dlgPreferences_Player_UnitsRadiansPerSecondSquared, UnitHelper.AngularAccelerationAbbreviation(AngularAccelerationUnit.RadiansPerSecondSquared)));

            lblCustomLength.Text = RootLang.dlgPreferences_Player_UnitsCustom;

            SelectCurrentUnits();
        }

        private void InitPageMemory()
        {
            tabMemory.Text = RootLang.dlgPreferences_Capture_tabMemory;

            int maxMemoryBuffer = MemoryHelper.MaxMemoryBuffer();
            trkMemoryBuffer.Maximum = maxMemoryBuffer;

            memoryBuffer = Math.Min(memoryBuffer, trkMemoryBuffer.Maximum);
            trkMemoryBuffer.Value = memoryBuffer;
            UpdateMemoryLabel();
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

            if (string.IsNullOrEmpty(customLengthUnit))
            {
                tbCustomLengthUnit.Text = RootLang.dlgPreferences_Player_TrackingPercentage;
                tbCustomLengthAb.Text = "%";
            }
            else
            {
                tbCustomLengthUnit.Text = customLengthUnit;
                tbCustomLengthAb.Text = customLengthAbbreviation;
            }

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
        private void ChkDetectImageSequencesCheckedChanged(object sender, EventArgs e)
        {
            detectImageSequences = chkDetectImageSequences.Checked;
        }
        private void ChkLockSpeedsCheckedChanged(object sender, EventArgs e)
        {
            syncLockSpeeds = chkLockSpeeds.Checked;
        }
        private void chkSyncByMotion_CheckedChanged(object sender, EventArgs e)
        {
            syncByMotion = chkSyncByMotion.Checked;
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
        private void trkWorkingZoneMemory_ValueChanged(object sender, EventArgs e)
        {
            memoryBuffer = trkMemoryBuffer.Value;
            UpdateMemoryLabel();
        }
        private void UpdateMemoryLabel()
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            string formatted = memoryBuffer.ToString("#,0", nfi);

            lblWorkingZoneMemory.Text = string.Format(RootLang.dlgPreferences_Player_lblMemory, formatted);
        }
        private void tbCustomLengthUnit_TextChanged(object sender, EventArgs e)
        {
            customLengthUnit = tbCustomLengthUnit.Text;
        }
        private void tbCustomLengthAb_TextChanged(object sender, EventArgs e)
        {
            customLengthAbbreviation = tbCustomLengthAb.Text;
        }
        #endregion
        
        public void CommitChanges()
        {
            PreferencesManager.PlayerPreferences.DeinterlaceByDefault = deinterlaceByDefault;
            PreferencesManager.PlayerPreferences.DetectImageSequences = detectImageSequences;
            PreferencesManager.PlayerPreferences.SyncLockSpeed = syncLockSpeeds;
            PreferencesManager.PlayerPreferences.SyncByMotion = syncByMotion;
            PreferencesManager.PlayerPreferences.InteractiveFrameTracker = interactiveFrameTracker;
            PreferencesManager.PlayerPreferences.TimecodeFormat = timecodeFormat;
            PreferencesManager.PlayerPreferences.AspectRatio = imageAspectRatio;
            PreferencesManager.PlayerPreferences.SpeedUnit = speedUnit;
            PreferencesManager.PlayerPreferences.AccelerationUnit = accelerationUnit;
            PreferencesManager.PlayerPreferences.AngleUnit = angleUnit;
            PreferencesManager.PlayerPreferences.AngularVelocityUnit = angularVelocityUnit;
            PreferencesManager.PlayerPreferences.AngularAccelerationUnit = angularAccelerationUnit;
            PreferencesManager.PlayerPreferences.WorkingZoneMemory = memoryBuffer;

            // Special case for the custom unit length.
            if (customLengthUnit == RootLang.dlgPreferences_Player_TrackingPercentage)
            {
                PreferencesManager.PlayerPreferences.CustomLengthUnit = "";
                PreferencesManager.PlayerPreferences.CustomLengthAbbreviation = "";
            }
            else
            {
                PreferencesManager.PlayerPreferences.CustomLengthUnit = customLengthUnit;
                PreferencesManager.PlayerPreferences.CustomLengthAbbreviation = customLengthAbbreviation;
            }
        }
    }
}
