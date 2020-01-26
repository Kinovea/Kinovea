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
using Kinovea.Services;
using Kinovea.Camera.Languages;
using System.Globalization;
using GxIAPINET;

namespace Kinovea.Camera.Daheng
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

        public Dictionary<string, CameraProperty> CameraProperties
        {
            get { return cameraProperties; }
        } 

        private CameraSummary summary;
        private IGXDevice device;
        private bool iconChanged;
        private bool specificChanged;
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

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null || specific.Device == null)
                return;

            device = specific.Device;
            cameraProperties = CameraPropertyManager.Read(device);
            if (cameraProperties.Count != specific.CameraProperties.Count)
               specificChanged = true;

            PopulateCameraControls();

            this.Text = CameraLang.FormConfiguration_Title;
            btnApply.Text = CameraLang.Generic_Apply;
            UpdateResultingFramerate();
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

        private void PopulateCameraControls()
        {
            int top = lblAuto.Bottom;
            AddCameraProperty("Width", CameraLang.FormConfiguration_Properties_ImageWidth, top);
            AddCameraProperty("Height", CameraLang.FormConfiguration_Properties_ImageHeight, top + 30);
            AddCameraProperty("AcquisitionFrameRate", CameraLang.FormConfiguration_Properties_Framerate, top + 60);
            AddCameraProperty("ExposureTime", CameraLang.FormConfiguration_Properties_ExposureMicro, top + 90);
            AddCameraProperty("Gain", CameraLang.FormConfiguration_Properties_Gain, top + 120);
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

        private void cpvCameraControl_ValueChanged(object sender, EventArgs e)
        {
            AbstractCameraPropertyView control = sender as AbstractCameraPropertyView;
            if (control == null)
                return;

            string key = control.Tag as string;
            if (string.IsNullOrEmpty(key) || !cameraProperties.ContainsKey(key))
                return;

            CameraPropertyManager.Write(device, control.Property);
            UpdateResultingFramerate();
            specificChanged = true;
        }

        private void BtnReconnect_Click(object sender, EventArgs e)
        {
            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info == null)
                return;

            info.CameraProperties = this.CameraProperties;
            summary.UpdateDisplayRectangle(Rectangle.Empty);
            CameraTypeManager.UpdatedCameraSummary(summary);

            disconnect();
            connect();

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null || specific.Device == null)
                return;

            device = specific.Device;
            cameraProperties = CameraPropertyManager.Read(device);

            RemoveCameraControls();
            PopulateCameraControls();
            UpdateResultingFramerate();
        }

        private void UpdateResultingFramerate()
        {
            float resultingFramerate = (float)DahengHelper.GetResultingFramerate(device);
            lblResultingFramerateValue.Text = string.Format("{0:0.##}", resultingFramerate);

            bool discrepancy = false;
            if (cameraProperties.ContainsKey("AcquisitionFrameRate") && cameraProperties["AcquisitionFrameRate"].Supported)
            {
                float framerate;
                bool parsed = float.TryParse(cameraProperties["AcquisitionFrameRate"].CurrentValue, NumberStyles.Any, CultureInfo.InvariantCulture, out framerate);
                if (parsed && Math.Abs(framerate - resultingFramerate) > 1)
                    discrepancy = true;
            }

            lblResultingFramerateValue.ForeColor = discrepancy ? Color.Red : Color.Black;
        }

    }
}
