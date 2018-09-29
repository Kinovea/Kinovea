#region License
/*
Copyright © Joan Charmant 2013.
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
using PylonC.NET;
using Kinovea.Services;
using Kinovea.Camera.Languages;

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

        public GenApiEnum SelectedStreamFormat
        {
            get { return selectedStreamFormat; }
        }

        public bool Debayering
        {
            get { return debayering; }
        }

        public Dictionary<string, CameraProperty> CameraProperties
        {
            get { return cameraProperties; }
        } 
        
        private CameraSummary summary;
        private PYLON_DEVICE_HANDLE deviceHandle;
        private bool iconChanged;
        private bool specificChanged;
        private GenApiEnum selectedStreamFormat;
        private bool debayering;
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
            cameraProperties = CameraPropertyManager.Read(specific.Handle, summary.Identifier);
            
            if (cameraProperties.Count != specific.CameraProperties.Count)
                specificChanged = true;

            Populate(specific);
        }
        
        private void Populate(SpecificInfo specific)
        {
            try
            {
                PopulateStreamFormat(specific);
                PopulateCameraControls();
            }
            catch
            {
                log.ErrorFormat(PylonHelper.GetLastError());
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

        private void PopulateStreamFormat(SpecificInfo specific)
        {
            lblColorSpace.Text = CameraLang.FormConfiguration_Properties_StreamFormat;

            bool readable = Pylon.DeviceFeatureIsReadable(deviceHandle, "PixelFormat");
            if (!readable)
            {
                cmbFormat.Enabled = false;
                return;
            }

            string currentValue = Pylon.DeviceFeatureToString(deviceHandle, "PixelFormat");

            List<GenApiEnum> streamFormats = PylonHelper.ReadEnum(deviceHandle, "PixelFormat");
            if (streamFormats == null)
            {
                cmbFormat.Enabled = false;
                return;
            }

            foreach (GenApiEnum streamFormat in streamFormats)
            {
                cmbFormat.Items.Add(streamFormat);
                if (currentValue == streamFormat.Symbol)
                {
                    selectedStreamFormat = streamFormat;
                    cmbFormat.SelectedIndex = cmbFormat.Items.Count - 1;
                }
            }

            debayering = specific.Debayering;
            chkDebayering.Text = CameraLang.FormConfiguration_Properties_Debayering;
            chkDebayering.Enabled = cmbFormat.Enabled;
            chkDebayering.Checked = debayering;
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
            CameraPropertyManager.Write(deviceHandle, cameraProperties[key]);

            specificChanged = true;
        }
        
        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenApiEnum selected = cmbFormat.SelectedItem as GenApiEnum;
            if (selected == null || selectedStreamFormat.Symbol == selected.Symbol)
                return;

            selectedStreamFormat = selected;
            specificChanged = true;
        }

        private void chkDebayering_CheckedChanged(object sender, EventArgs e)
        {
            debayering = chkDebayering.Checked;
            specificChanged = true;
        }
    }
}
