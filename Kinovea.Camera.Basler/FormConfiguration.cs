#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Drawing;
using System.Windows.Forms;

using Kinovea.Camera;
using PylonC.NET;

namespace Kinovea.Camera.Basler
{
    public partial class FormConfiguration : Form
    {
        public bool AliasChanged
        {
            get { return iconChanged || tbAlias.Text != summary.Alias;}
        }
        
        public string Alias
        { 
            get { return tbAlias.Text; }
        }
        
        public Bitmap PickedIcon
        { 
            get { return (Bitmap)btnIcon.BackgroundImage; }
        }
        
        public bool SpecificChanged
        {
            get { return specificChanged; }
        }
        
        private bool iconChanged;
        private bool specificChanged;
        private CameraSummary summary;
        private bool loaded;
        private PYLON_DEVICE_HANDLE handle;
        private long gainRaw = 36;
        private long memoGain = 36;
        private double exposureTimeAbs;
        private double memoExposureTimeAbs;
        private string triggerSource = "Software";
        private string memoTriggerSource;
        private bool manualTextbox;
        private bool manualTrackbar;
        private bool manualComboBox;
        private double minExposure;
        private double maxExposure;
        private bool useTrigger;
        private bool memoUseTrigger;
        private LogarithmicMapper exposureLogMapper;
        private double acquisitionFramerate;
        private double memoAcquisitionFramerate;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormConfiguration(CameraSummary summary)
        {
            this.summary = summary;
            
            InitializeComponent();
            
            tbAlias.Text = summary.Alias;
            lblSystemName.Text = summary.Name;
            btnIcon.BackgroundImage = summary.Icon;

            Populate();
            loaded = true;
        }
        
        private void Populate()
        {
            SpecificInfo specific = summary.Specific as SpecificInfo;
            if(specific == null)
                return;
            
            handle = specific.Handle;
            
            try
            {
                PopulateGainRaw();
                PopulateExposureTimeAbs();
                PopulateAcquisitionFramerate();
                PopulateUseTrigger();
                PopulateTriggerSource();
                PopulateRecordingFramerate();
            }
            catch(Exception e)
            {
                log.ErrorFormat(PylonHelper.GetLastError());
            }
        }
        
        private void BtnIconClick(object sender, EventArgs e)
        {
            FormIconPicker fip = new FormIconPicker(IconLibrary.Icons, 5, "Icons");
            LocateForm(fip);
            if(fip.ShowDialog() == DialogResult.OK)
            {
                btnIcon.BackgroundImage = fip.PickedIcon;
                iconChanged = true;
            }
            
            fip.Dispose();
        }
        
        private void LocateForm(Form form)
        {
            // Note: function duplicated from ScreenManager which we don't want to depend upon.
            // Maybe this method would be better in Kinovea.Service or a general Forms utility.
            if (Cursor.Position.X + (form.Width / 2) >= SystemInformation.PrimaryMonitorSize.Width || 
                Cursor.Position.Y + form.Height >= SystemInformation.PrimaryMonitorSize.Height)
                form.StartPosition = FormStartPosition.CenterScreen;
            else
                form.Location = new Point(Cursor.Position.X - (form.Width / 2), Cursor.Position.Y - 20);
        }
        
        #region GainRaw
        private void PopulateGainRaw()
        {
            string featureName = "GainRaw";
            if(!Pylon.DeviceFeatureIsReadable(handle, featureName))
                return;

            long min = Pylon.DeviceGetIntegerFeatureMin(handle, featureName);
            long max = Pylon.DeviceGetIntegerFeatureMax(handle, featureName);
            long incr = Pylon.DeviceGetIntegerFeatureInc(handle, featureName);
            long val = Pylon.DeviceGetIntegerFeature(handle, featureName);
            
            trkGainRaw.Minimum = (int)min;
            trkGainRaw.Maximum = (int)max;
            trkGainRaw.SmallChange = (int)incr;
            trkGainRaw.Value = (int)val;
            
            memoGain = val;
        }
        
        private void TrkGainRaw_ValueChanged(object sender, EventArgs e)
        {
            if(manualTrackbar)
                return;
                
            gainRaw = (long)trkGainRaw.Value;
            UpdateGain();
            
            manualTextbox = true;
            tbGainRaw.Text = string.Format("{0}", gainRaw);
            manualTextbox = false;
        }
        
        private void TbGainRawTextChanged(object sender, EventArgs e)
        {
            if(manualTextbox)
                return;
                
            long tryGainRaw;
            bool parsed = long.TryParse(tbGainRaw.Text, out tryGainRaw);
            
            if(tryGainRaw < trkGainRaw.Minimum || tryGainRaw > trkGainRaw.Maximum)
                parsed = false;
            
            if(!parsed)
                return;
                
            gainRaw = tryGainRaw;
            UpdateGain();
            
            manualTrackbar = true;
            trkGainRaw.Value = (int)gainRaw;
            manualTrackbar = false;
        }
        
        private void UpdateGain()
        {
            if(Pylon.DeviceFeatureIsWritable(handle, "GainRaw"))
                Pylon.DeviceSetIntegerFeature(handle, "GainRaw", gainRaw);
        }
        #endregion
        
        #region ExposureTimeAbs
        private void PopulateExposureTimeAbs()
        {
            string featureName = "ExposureTimeAbs";
            if(!Pylon.DeviceFeatureIsReadable(handle, featureName))
                return;
            
            minExposure = Pylon.DeviceGetFloatFeatureMin(handle, featureName);
            maxExposure = Pylon.DeviceGetFloatFeatureMax(handle, featureName);
            maxExposure = Math.Min(maxExposure, 1000000);
            double val = Pylon.DeviceGetFloatFeature(handle, featureName);
            
            exposureLogMapper = new LogarithmicMapper((int)minExposure, (int)maxExposure, trkExposure.Minimum, trkExposure.Maximum);
            
            int proxyValue = exposureLogMapper.Map((int)val);
            proxyValue = Math.Min(trkExposure.Maximum, Math.Max(trkExposure.Minimum, proxyValue));
            
            manualTrackbar = true;
            trkExposure.Value = proxyValue;
            UpdateExposureText((int)val);
            manualTrackbar = false;
            memoExposureTimeAbs = val;
        }
        
        private void TrkExposureTimeAbs_ValueChanged(object sender, EventArgs e)
        {
            if(manualTrackbar)
                return;
            
            double dataValue = (double)exposureLogMapper.Unmap(trkExposure.Value);
            dataValue = Math.Min(maxExposure, Math.Max(minExposure, dataValue));
            
            exposureTimeAbs = dataValue;
            UpdateExposureTimeAbs();
            
            manualTextbox = true;
            UpdateExposureText((int)exposureTimeAbs);
            manualTextbox = false;
        }
        
        private void TbExposureTimeAbs_TextChanged(object sender, EventArgs e)
        {
            if(manualTextbox)
                return;
                
            AfterExposureTextChanged();
        }

        private void UpdateExposureText(int val)
        {
            manualTextbox = true;
            
            // val is in microseconds.
            if(val < 2000)
            {
                tbExposureTimeAbs.Text = string.Format("{0}", val);
                cbExposureUnit.Text = "µs";
            }
            else
            {
                tbExposureTimeAbs.Text = string.Format("{0}", val/1000);
                cbExposureUnit.Text = "ms";
            }
            
            manualTextbox = false;
        }

        private void UpdateExposureTimeAbs()
        {
            if(Pylon.DeviceFeatureIsWritable(handle, "ExposureTimeAbs"))
                Pylon.DeviceSetFloatFeature(handle, "ExposureTimeAbs", exposureTimeAbs);

            ChangedResultingFramerate();
        }
        
        private void CbExposureUnitSelectedIndexChanged(object sender, EventArgs e)
        {
            if(manualTextbox)
                return;
                
            AfterExposureTextChanged();
        }
        
        private void AfterExposureTextChanged()
        {
            long tryExposure;
            bool parsed = long.TryParse(tbExposureTimeAbs.Text, out tryExposure);
            if(!parsed)
                return;
            
            if(cbExposureUnit.Text == "ms")
                tryExposure *= 1000;
            
            if(tryExposure < minExposure || tryExposure > maxExposure)
                return;
            
            exposureTimeAbs = tryExposure;
            UpdateExposureTimeAbs();
            
            manualTrackbar = true;
            int proxyValue = exposureLogMapper.Map((int)exposureTimeAbs);
            proxyValue = Math.Min(trkExposure.Maximum, Math.Max(trkExposure.Minimum, proxyValue));
            trkExposure.Value = proxyValue;
            manualTrackbar = false;
        }
        #endregion
        
        #region Acquisition framerate
        private void PopulateAcquisitionFramerate()
        {
            if (Pylon.DeviceFeatureIsReadable(handle, "AcquisitionFrameRateAbs"))
                acquisitionFramerate = Pylon.DeviceGetFloatFeature(handle, "AcquisitionFrameRateAbs");

            memoAcquisitionFramerate = acquisitionFramerate;
            
            manualTextbox = true;
            tbFramerate.Text = string.Format("{0:0.000}", acquisitionFramerate);
            manualTextbox = false;

            ChangedResultingFramerate();
        }


        private void tbFramerate_TextChanged(object sender, EventArgs e)
        {
            if (manualTextbox)
                return;
            
            double tryFramerate;
            bool parsed = double.TryParse(tbFramerate.Text, out tryFramerate);
            if (!parsed)
                return;

            acquisitionFramerate = tryFramerate;
            UpdateAcquisitionFramerate();
            ChangedResultingFramerate();
        }

        private void ChangedResultingFramerate()
        {
            if (Pylon.DeviceFeatureIsReadable(handle, "ResultingFrameRateAbs"))
            {
                double resulting = Pylon.DeviceGetFloatFeature(handle, "ResultingFrameRateAbs");
                resulting = Math.Round(resulting, 3);
                
                lblResultingFrameRate.Text = string.Format("Forced to : {0:0.000}", resulting);
                lblResultingFrameRate.Visible = (resulting < acquisitionFramerate);
            }
        }

        private void UpdateAcquisitionFramerate()
        {
            try
            {
                if (Pylon.DeviceFeatureIsWritable(handle, "AcquisitionFrameRateAbs"))
                    Pylon.DeviceSetFloatFeature(handle, "AcquisitionFrameRateAbs", acquisitionFramerate);
            }
            catch
            {
                log.DebugFormat("Error while trying to set the acquisition framerate.");
            }
        }
        #endregion
        
        #region Use trigger
        private void PopulateUseTrigger()
        {
            if (Pylon.DeviceFeatureIsAvailable(handle, "EnumEntry_TriggerSelector_FrameStart"))
            {
                Pylon.DeviceFeatureFromString(handle, "TriggerSelector", "FrameStart");
                string value = PylonHelper.DeviceGetStringFeature(handle, "TriggerMode");
                useTrigger = (value == "On");
                memoUseTrigger = useTrigger;
            }

            chkTriggerMode.Checked = useTrigger;
        }
        private void ChkTriggerModeCheckedChanged(object sender, EventArgs e)
        {
            useTrigger = chkTriggerMode.Checked;
            gpTriggerOptions.Enabled = useTrigger;

            UpdateUseTrigger();

            lblAcquisitionFramerate.Enabled = !useTrigger;
            tbFramerate.Enabled = !useTrigger;
            lblResultingFrameRate.Enabled = !useTrigger;
        }
        private void UpdateUseTrigger()
        {
            if (Pylon.DeviceFeatureIsAvailable(handle, "EnumEntry_TriggerSelector_FrameStart"))
            {
                Pylon.DeviceFeatureFromString(handle, "TriggerSelector", "FrameStart");
                Pylon.DeviceFeatureFromString(handle, "TriggerMode", useTrigger ? "On" : "Off");
            }

            Pylon.DeviceFeatureFromString(handle, "AcquisitionFrameRateEnable", useTrigger ? "false" : "true");
        }
        #endregion

        #region Trigger source
        private void PopulateTriggerSource()
        {
            string source = PylonHelper.DeviceGetStringFeature(handle, "TriggerSource");
            memoTriggerSource = source;
            
            manualComboBox = true;
            if(source == "Software")
            {
                cbTriggerSource.SelectedIndex = 0;
                btnSoftwareTrigger.Enabled = true;
            }
            else if(source == "Line1")
            {
                cbTriggerSource.SelectedIndex = 1;
                btnSoftwareTrigger.Enabled = false;
            }
            
            manualComboBox = false;
        }
        
        private void CbTriggerSourceSelectedIndexChanged(object sender, EventArgs e)
        {
            if(manualComboBox)
                return;
                
            Pylon.DeviceFeatureFromString(handle, "TriggerSelector", "FrameStart");
                
            if(cbTriggerSource.SelectedIndex == 0)
            {
                triggerSource = "Software";
                btnSoftwareTrigger.Enabled = true;
            }
            else if(cbTriggerSource.SelectedIndex == 1)
            {
                triggerSource = "Line1";
                btnSoftwareTrigger.Enabled = false;
            }
            
            UpdateTriggerSource();
        }
        private void UpdateTriggerSource()
        {
            Pylon.DeviceFeatureFromString(handle, "TriggerSource", triggerSource);
        }
        #endregion
        
        #region Recording framerate
        private void PopulateRecordingFramerate()
        {
        
        }
        #endregion
        
        private void BtnCancelClick(object sender, EventArgs e)
        {
            // Restore memo.
            if (gainRaw != memoGain)
            {
                gainRaw = memoGain;
                UpdateGain();
            }
            
            if (exposureTimeAbs != memoExposureTimeAbs)
            {
                exposureTimeAbs = memoExposureTimeAbs;
                UpdateExposureTimeAbs();
            }

            if (useTrigger != memoUseTrigger)
            {
                useTrigger = memoUseTrigger;
                UpdateUseTrigger();
            }
            
            if (triggerSource != memoTriggerSource)
            {
                triggerSource = memoTriggerSource;
                UpdateTriggerSource();
            }
        }
        
        private void BtnSoftwareTrigger_Click(object sender, EventArgs e)
        {
            Pylon.DeviceExecuteCommandFeature(handle, "TriggerSoftware");
        }
    }
}
