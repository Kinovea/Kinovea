#region License
/*
Copyright © Joan Charmant 2017.
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Kinovea.Camera;
using Kinovea.Services;
using Kinovea.Camera.Languages;

namespace Kinovea.Camera.IDS
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

        public IDSEnum SelectedStreamFormat
        {
            get { return selectedStreamFormat; }
        }

        public Dictionary<string, CameraProperty> CameraProperties
        {
            get { return cameraProperties; }
        } 
        
        private CameraSummary summary;
        private bool iconChanged;
        private bool specificChanged;
        private uEye.Camera camera = new uEye.Camera();
        private long deviceId;
        private IDSEnum selectedStreamFormat;
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
            if (specific == null || specific.Camera == null || !specific.Camera.IsOpened)
                return;

            camera = specific.Camera;
            int temp;
            camera.Device.GetDeviceID(out temp);
            deviceId = (long)temp;

            cameraProperties = CameraPropertyManager.Read(camera, deviceId);
            
            if (cameraProperties.Count != specific.CameraProperties.Count)
                specificChanged = true;

            Populate();
        }

        private bool InitializeDeviceId(string identifier)
        {
            bool found = false;
            uEye.Types.CameraInformation[] devices;
            uEye.Info.Camera.GetCameraList(out devices);
            foreach (uEye.Types.CameraInformation device in devices)
            {
                if (device.SerialNumber != identifier)
                    continue;
                
                deviceId = device.DeviceID;
                found = true;
                break;
            }

            return found;
        }
        
        private void Populate()
        {
            try
            {
                PopulateStreamFormat();
                PopulateCameraControls();
            }
            catch (Exception e)
            {
                log.Error("Error while populating configuration options", e);
            }
        }
        
        private void BtnIconClick(object sender, EventArgs e)
        {
            FormIconPicker fip = new FormIconPicker(IconLibrary.Icons, 5);
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
            // Get the intersection of camera and Kinovea supported formats.
            List<IDSEnum> streamFormats = IDSHelper.GetSupportedStreamFormats(camera, deviceId);

            // Get currently selected option.
            int currentColorMode = IDSHelper.ReadCurrentStreamFormat(camera);

            foreach (IDSEnum streamFormat in streamFormats)
            {
                cmbFormat.Items.Add(streamFormat);
                if (streamFormat.Value == currentColorMode)
                {
                    selectedStreamFormat = streamFormat;
                    cmbFormat.SelectedIndex = cmbFormat.Items.Count - 1;
                }
            }

            // TODO: if the current camera format is not supported in Kinovea, force the camera to switch to a supported mode.
            // What if none of the Kinovea modes are supported by the camera ?
        }

        private void PopulateCameraControls()
        {
            int top = lblAuto.Bottom;
            Func<int, string> defaultValueMapper = (value) => value.ToString();

            AddCameraProperty("width", CameraLang.FormConfiguration_Properties_ImageWidth, defaultValueMapper, top);
            AddCameraProperty("height", CameraLang.FormConfiguration_Properties_ImageHeight, defaultValueMapper, top + 30);
            AddCameraProperty("framerate", CameraLang.FormConfiguration_Properties_Framerate, defaultValueMapper, top + 60);
            AddCameraProperty("exposure", CameraLang.FormConfiguration_Properties_ExposureMicro, defaultValueMapper, top + 90);
            AddCameraProperty("gain", CameraLang.FormConfiguration_Properties_Gain, defaultValueMapper, top + 120);
        }

        private void AddCameraProperty(string key, string text, Func<int, string> valueMapper, int top)
        {
            if (!cameraProperties.ContainsKey(key))
                return;

            CameraProperty property = cameraProperties[key];

            AbstractCameraPropertyView control = null;
             
            switch (property.Representation)
            {
                case CameraPropertyRepresentation.LinearSlider:
                    control = new CameraPropertyLinearView(property, text, valueMapper);
                    break;
                case CameraPropertyRepresentation.LogarithmicSlider:
                    control = new CameraPropertyLogarithmicView(property, text, valueMapper);
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
            CameraPropertyManager.Write(camera, deviceId, cameraProperties[key]);

            specificChanged = true;
        }
        
        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            IDSEnum selected = cmbFormat.SelectedItem as IDSEnum;
            if (selected == null || selectedStreamFormat.Value == selected.Value)
                return;

            selectedStreamFormat = selected;
            specificChanged = true;
        }
    }
}
