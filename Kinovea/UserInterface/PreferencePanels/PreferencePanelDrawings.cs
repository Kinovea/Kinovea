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
using Kinovea.Services;
using System.Collections.Generic;
using Kinovea.ScreenManager.Languages;
using System.Globalization;
using System.IO;
using System.Diagnostics;

namespace Kinovea.Root
{
    /// <summary>
    /// Description of PreferencePanelDrawings.
    /// </summary>
    public partial class PreferencePanelDrawings : UserControl, IPreferencePanel
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
        private List<PreferenceTab> tabs = new List<PreferenceTab> { 
            PreferenceTab.Drawings_General, 
            PreferenceTab.Drawings_Opacity, 
            PreferenceTab.Drawings_Units,
            PreferenceTab.Drawings_Presets,
            PreferenceTab.Drawings_Export,
        };
        private InfosFading defaultFading;
        private bool drawOnPlay;
        private bool enableFiltering;
        private bool enableHighSpeedDerivativesSmoothing;
        private bool enableCustomToolsDebug;
        private TimecodeFormat timecodeFormat;
        private SpeedUnit speedUnit;
        private AccelerationUnit accelerationUnit;
        private AngleUnit angleUnit;
        private AngularVelocityUnit angularVelocityUnit;
        private AngularAccelerationUnit angularAccelerationUnit;
        private string customLengthUnit;
        private string customLengthAbbreviation;
        private CadenceUnit cadenceUnit;
        private KeyframePresetsParameters keyframePresets;
        private CSVDecimalSeparator csvDecimalSeparator;
        private ExportSpace exportSpace;
        private bool exportImagesInDocuments;
        private int presetsCount = 10;
        private string pandocPath;
        #endregion

        #region Construction & Initialization
        public PreferencePanelDrawings()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            
            description = RootLang.dlgPreferences_tabDrawings;
            icon = Resources.notes_30;
            
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
            drawOnPlay = PreferencesManager.PlayerPreferences.DrawOnPlay;
            enableFiltering = PreferencesManager.PlayerPreferences.EnableFiltering;
            enableHighSpeedDerivativesSmoothing = PreferencesManager.PlayerPreferences.EnableHighSpeedDerivativesSmoothing;
            enableCustomToolsDebug = PreferencesManager.PlayerPreferences.EnableCustomToolsDebugMode;
            defaultFading = new InfosFading(0, 0);
            timecodeFormat = PreferencesManager.PlayerPreferences.TimecodeFormat;
            speedUnit = PreferencesManager.PlayerPreferences.SpeedUnit;
            accelerationUnit = PreferencesManager.PlayerPreferences.AccelerationUnit;
            angleUnit = PreferencesManager.PlayerPreferences.AngleUnit;
            angularVelocityUnit = PreferencesManager.PlayerPreferences.AngularVelocityUnit;
            angularAccelerationUnit = PreferencesManager.PlayerPreferences.AngularAccelerationUnit;
            customLengthUnit = PreferencesManager.PlayerPreferences.CustomLengthUnit;
            customLengthAbbreviation = PreferencesManager.PlayerPreferences.CustomLengthAbbreviation;
            cadenceUnit = PreferencesManager.PlayerPreferences.CadenceUnit;
            keyframePresets = PreferencesManager.PlayerPreferences.KeyframePresets;
            csvDecimalSeparator = PreferencesManager.PlayerPreferences.CSVDecimalSeparator;
            exportSpace = PreferencesManager.PlayerPreferences.ExportSpace;
            exportImagesInDocuments = PreferencesManager.PlayerPreferences.ExportImagesInDocuments;
            pandocPath = PreferencesManager.PlayerPreferences.PandocPath;
        }
        private void InitPage()
        {
            InitTabGeneral();
            InitTabOpacity();
            InitTabUnits();
            InitTabPresets();
            InitTabExport();
        }
        private void InitTabGeneral()
        {
            tabGeneral.Text = RootLang.dlgPreferences_tabGeneral;
            chkDrawOnPlay.Text = RootLang.dlgPreferences_Drawings_chkDrawOnPlay;
            chkEnableFiltering.Text = RootLang.dlgPreferences_Drawings_chkEnableFiltering;
            //chkEnableHSDS.Text = ""
            chkCustomToolsDebug.Text = RootLang.dlgPreferences_Drawings_chkCustomToolsDebugMode;

            chkDrawOnPlay.Checked = drawOnPlay;
            chkEnableFiltering.Checked = enableFiltering;
            chkEnableHSDS.Checked = enableHighSpeedDerivativesSmoothing;
            chkCustomToolsDebug.Checked = enableCustomToolsDebug;
        }
        private void InitTabOpacity()
        {
            tabPersistence.Text = ScreenManagerLang.Generic_Opacity;
            lblDefaultOpacity.Text = RootLang.dlgPreferences_Drawings_lblDefaultOpacity;
            chkAlwaysVisible.Text = RootLang.dlgPreferences_Drawings_rbAlwaysVisible;
            lblMax.Text = ScreenManagerLang.dlgConfigureOpacity_lblMax;
            lblOpaque.Text = ScreenManagerLang.dlgConfigureOpacity_lblOpaque;
            lblFading.Text = ScreenManagerLang.dlgConfigureOpacity_lblFading;

            chkAlwaysVisible.Checked = defaultFading.AlwaysVisible;
            nudMax.Value = (decimal)(defaultFading.MasterFactor * 100);
            nudOpaque.Value = (decimal)defaultFading.OpaqueFrames;
            nudFading.Value = (decimal)defaultFading.FadingFrames;

            NudHelper.FixNudScroll(nudMax);
            NudHelper.FixNudScroll(nudOpaque);
            NudHelper.FixNudScroll(nudFading);
        }
        private void InitTabUnits()
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
            
            if (Debugger.IsAttached)
            {
                cmbTimeCodeFormat.Items.Add("Normalized");
                cmbTimeCodeFormat.Items.Add("Timestamps");
            }

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

            // enum Kinovea.Services.CadenceUnit.
            lblCadenceUnit.Text = ScreenManagerLang.Cadence;
            cmbCadenceUnit.Items.Add(String.Format(RootLang.dlgPreferences_Player_UnitsHertz, UnitHelper.FrequencyAbbreviation(CadenceUnit.Hertz)));
            cmbCadenceUnit.Items.Add(String.Format(RootLang.PreferencePanelDrawings_InitTabUnits_CyclesPerSecond, UnitHelper.FrequencyAbbreviation(CadenceUnit.CyclesPerSecond)));
            cmbCadenceUnit.Items.Add(String.Format(RootLang.PreferencePanelDrawings_InitTabUnits_CyclesPerMinute, UnitHelper.FrequencyAbbreviation(CadenceUnit.CyclesPerMinute)));
            cmbCadenceUnit.Items.Add(String.Format(RootLang.PreferencePanelDrawings_InitTabUnits_StepsPerSecond, UnitHelper.FrequencyAbbreviation(CadenceUnit.StepsPerSecond)));
            cmbCadenceUnit.Items.Add(String.Format(RootLang.PreferencePanelDrawings_InitTabUnits_StepsPerMinute, UnitHelper.FrequencyAbbreviation(CadenceUnit.StepsPerMinute)));
            cmbCadenceUnit.Items.Add(String.Format(RootLang.PreferencePanelDrawings_InitTabUnits_StrokesPerSecond, UnitHelper.FrequencyAbbreviation(CadenceUnit.StrokesPerSecond)));
            cmbCadenceUnit.Items.Add(String.Format(RootLang.PreferencePanelDrawings_InitTabUnits_StrokesPerMinute, UnitHelper.FrequencyAbbreviation(CadenceUnit.StrokesPerMinute)));
            cmbCadenceUnit.Items.Add(String.Format(RootLang.dlgPreferences_Player_UnitsRevolutionsPerMinute, UnitHelper.FrequencyAbbreviation(CadenceUnit.RevolutionsPerMinute)));

            SelectCurrentUnits();
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

            int cadence = (int)cadenceUnit;
            cmbCadenceUnit.SelectedIndex = cadence < cmbCadenceUnit.Items.Count ? cadence : 0;

        }

        private void InitTabPresets()
        {
            tabPresets.Controls.Clear();

            // Dynamically create preset controls.
            int top = 26;
            int margin = 10;
            for (int i = 0; i < presetsCount; i++)
            {
                // Make sure we have enough presets on the Preferences side.
                if (keyframePresets.Presets.Count < i + 1)
                    keyframePresets.Presets.Add(new KeyframePreset("", Color.SteelBlue));

                // Create the controls.
                Label lblPresetId = new Label();
                lblPresetId.Left = 26;
                lblPresetId.Top = top;
                lblPresetId.AutoSize = true;
                lblPresetId.Text = string.Format(Kinovea.Root.Languages.RootLang.lblPreset, i + 1);

                TextBox tbPresetName = new TextBox();
                tbPresetName.Left = 120;
                tbPresetName.Top = top - 2;
                tbPresetName.Width = 154;
                tbPresetName.Height = 20;

                Button btnPresetColor = new Button();
                btnPresetColor.Left = tbPresetName.Right + 20;
                btnPresetColor.Top = top;
                btnPresetColor.Width = 100;
                btnPresetColor.Height = 20;
                btnPresetColor.FlatStyle = FlatStyle.Flat;
                btnPresetColor.FlatAppearance.BorderSize = 0;
                btnPresetColor.Cursor = Cursors.Hand;

                // Add them to the page.
                tabPresets.Controls.Add(lblPresetId);
                tabPresets.Controls.Add(tbPresetName);
                tabPresets.Controls.Add(btnPresetColor);

                // Import existing preferences.
                tbPresetName.Text = keyframePresets.Presets[i].Name;
                SetPresetButtonColor(btnPresetColor, keyframePresets.Presets[i].Color);

                // Handle updates.
                int index = i;
                tbPresetName.TextChanged += (s, e) => {
                    KeyframePreset preset = new KeyframePreset(tbPresetName.Text, btnPresetColor.BackColor);
                    keyframePresets.Presets[index] = preset;
                };

                btnPresetColor.Click += (s, e) =>
                {
                    FormColorPicker picker = new FormColorPicker(btnPresetColor.BackColor);
                    FormsHelper.Locate(picker);
                    if (picker.ShowDialog() == DialogResult.OK)
                    {
                        SetPresetButtonColor(btnPresetColor, picker.PickedColor);
                        KeyframePreset preset = new KeyframePreset(tbPresetName.Text, picker.PickedColor);
                        keyframePresets.Presets[index] = preset;
                    }
                    picker.Dispose();
                };

                top += lblPresetId.Height + margin;
            }
        }

        private void SetPresetButtonColor(Button button, Color color)
        {
            button.BackColor = color;
            button.FlatAppearance.MouseDownBackColor = color;
            button.FlatAppearance.MouseOverBackColor = color;
        }

        private void InitTabExport()
        {
            tabExport.Text = RootLang.dlgPreferences_Drawings_TabExport;
            lblCSVDelimiter.Text = RootLang.dlgPreferences_Drawings_CSVDelimiter;

            string systemDelimiter = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string systemDelimiterText = systemDelimiter;
            switch (systemDelimiter)
            {
                case ".":
                    systemDelimiterText = RootLang.dlgPreferences_Drawings_CSVPoint;
                    break;
                case ",":
                    systemDelimiterText = RootLang.dlgPreferences_Drawings_CSVComma;
                    break;
            }
            
            cmbDelimiter.Items.Add(string.Format(RootLang.dlgPreferences_Drawing_CSVSystem, systemDelimiterText));
            cmbDelimiter.Items.Add(RootLang.dlgPreferences_Drawings_CSVPoint);
            cmbDelimiter.Items.Add(RootLang.dlgPreferences_Drawings_CSVComma);
            int separator = (int)csvDecimalSeparator;
            cmbDelimiter.SelectedIndex = separator < cmbDelimiter.Items.Count ? separator : 0;

            lblExportSpace.Text = RootLang.dlgPreferences_Drawings_ExportSpace;
            cmbExportSpace.Items.Add(RootLang.dlgPreferences_Drawings_ExportCalibrated);
            cmbExportSpace.Items.Add(RootLang.dlgPreferences_Drawings_ExportPixels);
            int option = (int)exportSpace;
            cmbExportSpace.SelectedIndex = option < cmbExportSpace.Items.Count ? option : 0;

            cbExportImagesInDocs.Text = RootLang.PreferencePanelDrawings_InitTabExport_IncludeImages;
            cbExportImagesInDocs.Checked = exportImagesInDocuments;

            lblPandocPath.Text = RootLang.dlgPreferences_General_PathToPandoc;
            tbPandocPath.Text = pandocPath;
        }
        #endregion

        #region Handlers
        #region General
        private void chkDrawOnPlay_CheckedChanged(object sender, EventArgs e)
        {
            drawOnPlay = chkDrawOnPlay.Checked;
        }
        private void chkEnableFiltering_CheckedChanged(object sender, EventArgs e)
        {
            enableFiltering = chkEnableFiltering.Checked;
        }
        private void chkEnableHSDS_CheckedChanged(object sender, EventArgs e)
        {
            enableHighSpeedDerivativesSmoothing = chkEnableHSDS.Checked;
        }
        private void chkCustomToolsDebug_CheckedChanged(object sender, EventArgs e)
        {
            enableCustomToolsDebug = chkCustomToolsDebug.Checked;
        }
        #endregion

        #region Opacity
        private void chkAlwaysVisible_CheckedChanged(object sender, EventArgs e)
        {
            defaultFading.AlwaysVisible = chkAlwaysVisible.Checked;
        }

        private void nudMax_ValueChanged(object sender, EventArgs e)
        {
            defaultFading.MasterFactor = (float)nudMax.Value / 100;
        }

        private void nudOpaque_ValueChanged(object sender, EventArgs e)
        {
            defaultFading.OpaqueFrames = (int)nudOpaque.Value;
        }

        private void nudFading_ValueChanged(object sender, EventArgs e)
        {
            defaultFading.FadingFrames = (int)nudFading.Value;
        }
        #endregion

        #region Units
        private void cmbTimeCodeFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            timecodeFormat = (TimecodeFormat)cmbTimeCodeFormat.SelectedIndex;
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
        private void tbCustomLengthUnit_TextChanged(object sender, EventArgs e)
        {
            customLengthUnit = tbCustomLengthUnit.Text;
        }
        private void tbCustomLengthAb_TextChanged(object sender, EventArgs e)
        {
            customLengthAbbreviation = tbCustomLengthAb.Text;
        }
        private void cmbCadenceUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            cadenceUnit = (CadenceUnit)cmbCadenceUnit.SelectedIndex;
        }
        #endregion

        #region Export
        private void cmbDelimiter_SelectedIndexChanged(object sender, EventArgs e)
        {
            csvDecimalSeparator = (CSVDecimalSeparator)cmbDelimiter.SelectedIndex;
        }
        private void cmbExportSpace_SelectedIndexChanged(object sender, EventArgs e)
        {
            exportSpace = (ExportSpace)cmbExportSpace.SelectedIndex;
        }
        private void tbPandocPath_TextChanged(object sender, EventArgs e)
        {
            pandocPath = tbPandocPath.Text;
        }

        private void btnPandocPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            string initialDirectory = "";
            if (!string.IsNullOrEmpty(pandocPath) && File.Exists(pandocPath))
                initialDirectory = Path.GetDirectoryName(pandocPath);

            if (!string.IsNullOrEmpty(initialDirectory))
                dialog.InitialDirectory = initialDirectory;
            else
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            dialog.RestoreDirectory = true;
            dialog.Filter = "*.exe|*.exe";
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == DialogResult.OK)
                tbPandocPath.Text = dialog.FileName;
        }

        private void cbExportImagesInDocs_CheckedChanged(object sender, EventArgs e)
        {
            exportImagesInDocuments = cbExportImagesInDocs.Checked;
        }
        #endregion
        #endregion

        public void CommitChanges()
        {
            PreferencesManager.PlayerPreferences.DrawOnPlay = drawOnPlay;
            PreferencesManager.PlayerPreferences.EnableFiltering = enableFiltering;
            PreferencesManager.PlayerPreferences.EnableHighSpeedDerivativesSmoothing = enableHighSpeedDerivativesSmoothing;
            PreferencesManager.PlayerPreferences.EnableCustomToolsDebugMode = enableCustomToolsDebug;
            PreferencesManager.PlayerPreferences.DefaultFading.FromInfosFading(defaultFading);
            PreferencesManager.PlayerPreferences.TimecodeFormat = timecodeFormat;
            PreferencesManager.PlayerPreferences.SpeedUnit = speedUnit;
            PreferencesManager.PlayerPreferences.AccelerationUnit = accelerationUnit;
            PreferencesManager.PlayerPreferences.AngleUnit = angleUnit;
            PreferencesManager.PlayerPreferences.AngularVelocityUnit = angularVelocityUnit;
            PreferencesManager.PlayerPreferences.AngularAccelerationUnit = angularAccelerationUnit;
            PreferencesManager.PlayerPreferences.CadenceUnit = cadenceUnit;
            PreferencesManager.PlayerPreferences.KeyframePresets = keyframePresets;
            PreferencesManager.PlayerPreferences.CSVDecimalSeparator = csvDecimalSeparator;
            PreferencesManager.PlayerPreferences.ExportSpace = exportSpace;
            PreferencesManager.PlayerPreferences.ExportImagesInDocuments = exportImagesInDocuments;
            PreferencesManager.PlayerPreferences.PandocPath = pandocPath;

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
