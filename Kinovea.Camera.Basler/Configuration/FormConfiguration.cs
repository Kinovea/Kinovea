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
using Kinovea.Services;

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

        public StreamFormat SelectedStreamFormat
        {
            get { return selectedStreamFormat; }
        }

        public Dictionary<string, CameraProperty> CameraProperties
        {
            get { return cameraProperties; }
        } 
        
        private CameraSummary summary;
        private PYLON_DEVICE_HANDLE deviceHandle;
        private bool iconChanged;
        private bool specificChanged;
        private StreamFormat selectedStreamFormat;
        private Dictionary<string, CameraProperty> cameraProperties = new Dictionary<string, CameraProperty>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormConfiguration(CameraSummary summary)
        {
            this.summary = summary;
            
            InitializeComponent();
            tbAlias.AutoSize = false;
            tbAlias.Height = 20;
            
            tbAlias.Text = summary.Alias;
            lblSystemName.Text = summary.Name;
            btnIcon.BackgroundImage = summary.Icon;

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null || specific.Handle == null || !specific.Handle.IsValid)
                return;

            deviceHandle = specific.Handle;

            cameraProperties = CameraPropertyManager.Read(deviceHandle);
            Populate();
        }
        
        private void Populate()
        {
            try
            {
                PopulateStreamFormat();
                PopulateCameraControls();
            }
            catch
            {
                log.ErrorFormat(PylonHelper.GetLastError());
            }
        }
        
        private void BtnIconClick(object sender, EventArgs e)
        {
            FormIconPicker fip = new FormIconPicker(IconLibrary.Icons, 5, "Icons");
            FormsHelper.Locate(fip);
            if(fip.ShowDialog() == DialogResult.OK)
            {
                btnIcon.BackgroundImage = fip.PickedIcon;
                iconChanged = true;
            }
            
            fip.Dispose();
        }

        private void PopulateStreamFormat()
        {
            bool readable = Pylon.DeviceFeatureIsReadable(deviceHandle, "PixelFormat");
            if (!readable)
            {
                cmbFormat.Enabled = false;
                return;
            }

            string currentValue = Pylon.DeviceFeatureToString(deviceHandle, "PixelFormat");

            List<StreamFormat> streamFormats = PylonHelper.GetSupportedStreamFormats(deviceHandle);
            if (streamFormats == null)
            {
                cmbFormat.Enabled = false;
                return;
            }

            foreach (StreamFormat streamFormat in streamFormats)
            {
                cmbFormat.Items.Add(streamFormat);
                if (currentValue == streamFormat.Symbol)
                {
                    selectedStreamFormat = streamFormat;
                    cmbFormat.SelectedIndex = cmbFormat.Items.Count - 1;
                }
            }
        }

        private void PopulateCameraControls()
        {
            int top = lblAuto.Bottom;
            Func<int, string> defaultValueMapper = (value) => value.ToString();

            AddCameraProperty("width", "Width (px):", defaultValueMapper, top);
            AddCameraProperty("height", "Height (px):", defaultValueMapper, top + 30);
            AddCameraProperty("framerate", "Framerate (Hz):", defaultValueMapper, top + 60);
            AddCameraProperty("exposure", "Exposure (µs):", defaultValueMapper, top + 90);
            AddCameraProperty("gain", "Gain:", defaultValueMapper, top + 120);
        }

        private void AddCameraProperty(string key, string localizationToken, Func<int, string> valueMapper, int top)
        {
            if (!cameraProperties.ContainsKey(key))
                return;

            CameraProperty property = cameraProperties[key];

            AbstractCameraPropertyView control = null;
             
            switch (property.Representation)
            {
                case CameraPropertyRepresentation.LinearSlider:
                    control = new CameraPropertyLinearView(property, localizationToken, valueMapper);
                    break;
                case CameraPropertyRepresentation.LogarithmicSlider:
                    control = new CameraPropertyLogarithmicView(property, localizationToken, valueMapper);
                    break;

                default:
                    break;
            }

            if (control == null)
                return;

            control.Tag = key;
            control.ValueChanged += cpvCameraControl_ValueChanged;
            control.Left = 20;
            control.Top = top;
            gbProperties.Controls.Add(control);
        }

        private void cpvCameraControl_ValueChanged(object sender, EventArgs e)
        {
            AbstractCameraPropertyView control = sender as AbstractCameraPropertyView;
            if (control == null)
                return;

            string key = control.Tag as string;
            if (string.IsNullOrEmpty(key) || !cameraProperties.ContainsKey(key))
                return;

            CameraProperty property = control.Property;
            CameraPropertyManager.Write(deviceHandle, cameraProperties[key]);

            specificChanged = true;
        }
        
        /*
        
        #region GainRaw
        private void PopulateGainRaw()
        {
            string featureName = "GainRaw";
            if(!Pylon.DeviceFeatureIsReadable(deviceHandle, featureName))
                return;

            long min = Pylon.DeviceGetIntegerFeatureMin(deviceHandle, featureName);
            long max = Pylon.DeviceGetIntegerFeatureMax(deviceHandle, featureName);
            long incr = Pylon.DeviceGetIntegerFeatureInc(deviceHandle, featureName);
            long val = Pylon.DeviceGetIntegerFeature(deviceHandle, featureName);
            
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
            if(Pylon.DeviceFeatureIsWritable(deviceHandle, "GainRaw"))
                Pylon.DeviceSetIntegerFeature(deviceHandle, "GainRaw", gainRaw);
        }
        #endregion
        
        #region ExposureTimeAbs
        private void PopulateExposureTimeAbs()
        {
            string featureName = "ExposureTimeAbs";
            if(!Pylon.DeviceFeatureIsReadable(deviceHandle, featureName))
                return;
            
            minExposure = Pylon.DeviceGetFloatFeatureMin(deviceHandle, featureName);
            maxExposure = Pylon.DeviceGetFloatFeatureMax(deviceHandle, featureName);
            maxExposure = Math.Min(maxExposure, 1000000);
            double val = Pylon.DeviceGetFloatFeature(deviceHandle, featureName);
            
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
            if(Pylon.DeviceFeatureIsWritable(deviceHandle, "ExposureTimeAbs"))
                Pylon.DeviceSetFloatFeature(deviceHandle, "ExposureTimeAbs", exposureTimeAbs);

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
            if (Pylon.DeviceFeatureIsReadable(deviceHandle, "AcquisitionFrameRateAbs"))
                acquisitionFramerate = Pylon.DeviceGetFloatFeature(deviceHandle, "AcquisitionFrameRateAbs");

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
            if (Pylon.DeviceFeatureIsReadable(deviceHandle, "ResultingFrameRateAbs"))
            {
                double resulting = Pylon.DeviceGetFloatFeature(deviceHandle, "ResultingFrameRateAbs");
                resulting = Math.Round(resulting, 3);
                
                lblResultingFrameRate.Text = string.Format("Forced to : {0:0.000}", resulting);
                lblResultingFrameRate.Visible = (resulting < acquisitionFramerate);
            }
        }

        private void UpdateAcquisitionFramerate()
        {
            try
            {
                if (Pylon.DeviceFeatureIsWritable(deviceHandle, "AcquisitionFrameRateAbs"))
                    Pylon.DeviceSetFloatFeature(deviceHandle, "AcquisitionFrameRateAbs", acquisitionFramerate);
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
            if (Pylon.DeviceFeatureIsAvailable(deviceHandle, "EnumEntry_TriggerSelector_FrameStart"))
            {
                Pylon.DeviceFeatureFromString(deviceHandle, "TriggerSelector", "FrameStart");
                string value = PylonHelper.DeviceGetStringFeature(deviceHandle, "TriggerMode");
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
            if (Pylon.DeviceFeatureIsAvailable(deviceHandle, "EnumEntry_TriggerSelector_FrameStart"))
            {
                Pylon.DeviceFeatureFromString(deviceHandle, "TriggerSelector", "FrameStart");
                Pylon.DeviceFeatureFromString(deviceHandle, "TriggerMode", useTrigger ? "On" : "Off");
            }

            Pylon.DeviceFeatureFromString(deviceHandle, "AcquisitionFrameRateEnable", useTrigger ? "false" : "true");
        }
        #endregion

        #region Trigger source
        private void PopulateTriggerSource()
        {
            string source = PylonHelper.DeviceGetStringFeature(deviceHandle, "TriggerSource");
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
                
            Pylon.DeviceFeatureFromString(deviceHandle, "TriggerSelector", "FrameStart");
                
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
            Pylon.DeviceFeatureFromString(deviceHandle, "TriggerSource", triggerSource);
        }
        #endregion
        
         */

        private void BtnCancelClick(object sender, EventArgs e)
        {
            // Restore memo.
            /*if (gainRaw != memoGain)
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
            }*/
        }

        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            StreamFormat selected = cmbFormat.SelectedItem as StreamFormat;
            if (selected == null || selectedStreamFormat.Symbol == selected.Symbol)
                return;

            selectedStreamFormat = selected;
            specificChanged = true;
        }
        
        /*private void BtnSoftwareTrigger_Click(object sender, EventArgs e)
        {
            Pylon.DeviceExecuteCommandFeature(deviceHandle, "TriggerSoftware");
        }*/
    }
}
