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
using Kinovea.Video;

namespace Kinovea.Camera.FrameGenerator
{
    /// <summary>
    /// Configuration dialog for the simulated camera.
    /// Loosely modeled on the configuration dialog for the machine vision cameras modules.
    /// </summary>
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
        
        public SpecificInfo SpecificInfo
        {
            get { return specific; }
        }

        private CameraSummary summary;
        private bool iconChanged;
        private bool specificChanged;
        private SpecificInfo specific;
        private ImageFormat selectedStreamFormat;
        private Dictionary<string, CameraProperty> cameraProperties = new Dictionary<string, CameraProperty>();
        private Dictionary<string, AbstractCameraPropertyView> propertiesControls = new Dictionary<string, AbstractCameraPropertyView>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormConfiguration(CameraSummary summary)
        {
            this.summary = summary;
            specific = summary.Specific as SpecificInfo;

            InitializeComponent();
            
            tbAlias.Text = summary.Alias;
            lblSystemName.Text = summary.Name;
            btnIcon.BackgroundImage = summary.Icon;

            cameraProperties = CameraPropertyManager.Read(specific);
            PopulateStreamFormat();
            PopulateCameraControls();
        }

        void BtnIconClick(object sender, EventArgs e)
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
            ImageFormat currentImageFormat = specific.ImageFormat;
            List<ImageFormat> supported = new List<ImageFormat>() { ImageFormat.RGB24, ImageFormat.JPEG };
            foreach (ImageFormat f in supported)
            {
                cmbFormat.Items.Add(f);
                if (f == currentImageFormat)
                {
                    selectedStreamFormat = f;
                    cmbFormat.SelectedIndex = cmbFormat.Items.Count - 1;
                }
            }
        }

        private void PopulateCameraControls()
        {
            int top = lblAuto.Bottom;

            AddCameraProperty("width", CameraLang.FormConfiguration_Properties_ImageWidth, top);
            AddCameraProperty("height", CameraLang.FormConfiguration_Properties_ImageHeight, top + 30);
            AddCameraProperty("framerate", CameraLang.FormConfiguration_Properties_Framerate, top + 60);
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

            CameraPropertyManager.Write(specific, control.Property);
            specificChanged = true;
            FixWidth();
        }

        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            ImageFormat selected = (ImageFormat)cmbFormat.SelectedItem;
            if (selectedStreamFormat == selected)
                return;

            selectedStreamFormat = selected;
            specific.ImageFormat = selected;
            specificChanged = true;
            FixWidth();
        }

        private void FixWidth()
        {
            // Align width to the previous multiple of 4 for JPEG streams.
            if (selectedStreamFormat == ImageFormat.JPEG)
                specific.Width -= (specific.Width % 4);
        }
    }
}
