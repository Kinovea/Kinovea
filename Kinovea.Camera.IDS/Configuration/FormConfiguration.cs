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
using System.IO;

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
        private Dictionary<string, AbstractCameraPropertyView> propertiesControls = new Dictionary<string, AbstractCameraPropertyView>();
        private Action disconnect;
        private Action connect;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormConfiguration(CameraSummary summary, Action disconnect, Action connect)
        {
            this.summary = summary;
            this.disconnect = disconnect;
            this.connect = connect;

            InitializeComponent();
            tbAlias.AutoSize = false;
            tbAlias.Height = 20;
            
            tbAlias.Text = summary.Alias;
            lblSystemName.Text = summary.Name;
            btnIcon.BackgroundImage = summary.Icon;
            btnReconnect.Text = CameraLang.FormConfiguration_Reconnect;
            btnImport.Text = CameraLang.FormConfiguration_ImportParameters;

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
            this.Text = CameraLang.FormConfiguration_Title;
            btnApply.Text = CameraLang.Generic_Apply;
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
            lblColorSpace.Text = CameraLang.FormConfiguration_Properties_StreamFormat;

            // Get the intersection of camera and Kinovea supported formats.
            List<IDSEnum> streamFormats = IDSHelper.GetSupportedStreamFormats(camera, deviceId);

            // Get currently selected option.
            int currentColorMode = IDSHelper.ReadCurrentStreamFormat(camera);
            cmbFormat.Items.Clear();

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
            AddCameraProperty("width", CameraLang.FormConfiguration_Properties_ImageWidth, top);
            AddCameraProperty("height", CameraLang.FormConfiguration_Properties_ImageHeight, top + 30);
            AddCameraProperty("pixelclock", "Pixel clock (MHz):", top + 60);
            AddCameraProperty("framerate", CameraLang.FormConfiguration_Properties_Framerate, top + 90);
            AddCameraProperty("exposure", CameraLang.FormConfiguration_Properties_ExposureMicro, top + 120);
            AddCameraProperty("gain", CameraLang.FormConfiguration_Properties_Gain, top + 150);
            AddCameraProperty("gainboost", "Gain boost", top + 180);
        }

        private void RemoveCameraControls()
        {
            foreach (var pair in propertiesControls)
            {
                pair.Value.ValueChanged -= cpvCameraControl_ValueChanged;
                gbProperties.Controls.Remove(pair.Value);
            }

            propertiesControls.Clear();
        }

        private void AddCameraProperty(string key, string text, int top)
        {
            if (!cameraProperties.ContainsKey(key))
                return;

            CameraProperty property = cameraProperties[key];

            AbstractCameraPropertyView control = null;
             
            switch (property.Representation)
            {
                case CameraPropertyRepresentation.LinearSlider:
                    control = new CameraPropertyViewLinear(property, text, null);
                    break;
                case CameraPropertyRepresentation.LogarithmicSlider:
                    control = new CameraPropertyViewLogarithmic(property, text, null);
                    break;
                case CameraPropertyRepresentation.Checkbox:
                    control = new CameraPropertyViewCheckbox(property, text);
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
            propertiesControls.Add(key, control);
        }

        private void ReloadProperty(string key)
        {
            if (!propertiesControls.ContainsKey(key) || !cameraProperties.ContainsKey(key))
                return;

            // Reload the property in case the range or current value changed.
            CameraProperty prop = CameraPropertyManager.Read(camera, deviceId, key);
            if (prop == null)
                return;

            cameraProperties[key] = prop;
            propertiesControls[key].Repopulate(prop);
        }

        private void cpvCameraControl_ValueChanged(object sender, EventArgs e)
        {
            AbstractCameraPropertyView control = sender as AbstractCameraPropertyView;
            if (control == null)
                return;

            string key = control.Tag as string;
            if (string.IsNullOrEmpty(key) || !cameraProperties.ContainsKey(key))
                return;

            CameraPropertyManager.Write(camera, deviceId, control.Property);
            
            // Dependencies:
            // - Pixel clock changes the range and current value of framerate.
            // - Framerate changes the range and current value of exposure.
            if (key == "height" || key == "width")
            {
                ReloadProperty("framerate");
                ReloadProperty("exposure");
            }
            else if (key == "pixelclock")
            {
                ReloadProperty("framerate");
                ReloadProperty("exposure");
            }
            else if (key == "framerate")
            {
                ReloadProperty("exposure");
            }
            
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

        private void BtnReconnect_Click(object sender, EventArgs e)
        {
            if (SelectedStreamFormat == null)
            {
                // This happens when we load the config window and the camera isn't connected.
                return;
            }

            // Changing the image size will trigger a memory re-allocation inside uEye, 
            // and we'll stop receiving the frame events which is causing all kinds of problems.
            // We can't just wait until we get out of this form because the framerate range depends on image size.
            // We also can't simply disconnect when user is changing size, because then we no longer have access to exposure and framerate.
            // Force a full connection cycle to reload the camera on the new settings.
            // This may reallocate the delay buffer.
            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info == null)
                return;

            info.StreamFormat = this.SelectedStreamFormat.Value;
            info.CameraProperties = this.CameraProperties;
            summary.UpdateDisplayRectangle(Rectangle.Empty);
            CameraTypeManager.UpdatedCameraSummary(summary);

            disconnect();
            connect();

            ReloadProperty("framerate");
            ReloadProperty("exposure");
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            // Locate an .ini file.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = CameraLang.FormConfiguration_ImportParameters;
            //openFileDialog.InitialDirectory = Path.GetDirectoryName(ProfileHelper.GetProfileFilename(summary.Identifier));
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = Services.FilesystemHelper.OpenINIFilter();
            openFileDialog.FilterIndex = 0;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            string filename = openFileDialog.FileName;
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                return;

            // The timing here is finnicky.
            // connect() will start the delay buffer allocation on the current image size and start receiving frames.
            // disconnect prevents reading the new values from the camera.
            // Load with new sizes while the camera is streaming will fail because the buffers are wrong.
            // So we need to load the new values with the camera opened but not streaming.

            this.SuspendLayout();

            disconnect();
            ProfileHelper.Replace(summary.Identifier, filename);

            // Reopen the camera but do not start grabbing.
            uEye.Defines.Status status = camera.Init((Int32)deviceId | (Int32)uEye.Defines.DeviceEnumeration.UseDeviceID);
            if (status != uEye.Defines.Status.SUCCESS)
            {
                log.ErrorFormat("Error trying to open IDS uEye camera.");
                return;
            }

            // Load new parameters.
            ProfileHelper.Load(camera, summary.Identifier);
            cameraProperties = CameraPropertyManager.Read(camera, deviceId);
            SpecificInfo info = summary.Specific as SpecificInfo;
            PopulateStreamFormat();
            info.StreamFormat = this.SelectedStreamFormat.Value;
            info.CameraProperties = cameraProperties;
            summary.UpdateDisplayRectangle(Rectangle.Empty);
            CameraTypeManager.UpdatedCameraSummary(summary);

            // Reconnect.
            camera.Exit();
            connect();

            // Reload UI.
            RemoveCameraControls();
            PopulateCameraControls();

            this.ResumeLayout();
        }
    }
}
