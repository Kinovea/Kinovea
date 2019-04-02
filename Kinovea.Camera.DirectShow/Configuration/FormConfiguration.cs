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
using System.Linq;

using AForge.Video.DirectShow;
using Kinovea.Camera;
using Kinovea.Services;
using Kinovea.Camera.Languages;

namespace Kinovea.Camera.DirectShow
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

        public bool NeedsReconnection
        {
            get { return needsReconnection; }
        }
        
        public int SelectedMediaTypeIndex
        {
            get { return selectedMediaTypeIndex; }
        }

        public float SelectedFramerate
        {
            get { return selectedFramerate; }
        }

        public Dictionary<string, CameraProperty> CameraProperties
        {
            get { return cameraProperties; }
        }

        private bool iconChanged;
        private bool specificChanged;
        private CameraSummary summary;
        private string identifier;
        private VideoCaptureDevice device;
        private Dictionary<int, MediaType> mediaTypes;
        private MediaTypeOrganizer organizer = new MediaTypeOrganizer();
        private int selectedMediaTypeIndex;
        private float selectedFramerate;
        private bool canStreamConfig;
        private bool needsReconnection;
        private bool streamConfigInitialized;
        private Dictionary<string, CameraProperty> cameraProperties = new Dictionary<string, CameraProperty>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormConfiguration(CameraSummary summary)
        {
            this.summary = summary;
            this.identifier = summary.Identifier;

            InitializeComponent();
            tbAlias.AutoSize = false;
            tbAlias.Height = 20;
            
            tbAlias.Text = summary.Alias;
            lblSystemName.Text = summary.Name;
            btnIcon.BackgroundImage = summary.Icon;
            
            InitializeMediaTypes(summary);

            if (canStreamConfig)
                PopulateFormats();
            else
                DisableStreamConfig();

            streamConfigInitialized = true;

            cameraProperties = CameraPropertyManager.Read(device);
            PopulateCameraControl();
            Localize();
        }

        private void Localize()
        {
            this.Text = CameraLang.FormConfiguration_Title;
            lblStreamFormat.Text = CameraLang.FormConfiguration_Properties_StreamFormat;
            lblImageSize.Text = CameraLang.FormConfiguration_Properties_ImageSize;
            lblFramerate.Text = CameraLang.FormConfiguration_Properties_Framerate;
            btnDeviceProperties.Text = CameraLang.FormConfiguration_DevicePropertyPages;
            lblAuto.Text = CameraLang.FormConfiguration_Auto;
            btnApply.Text = CameraLang.Generic_Apply;
            btnCancel.Text = CameraLang.Generic_Cancel;
        }

        private void btnIcon_Click(object sender, EventArgs e)
        {
            FormIconPicker fip = new FormIconPicker(IconLibrary.Icons, 5);
            FormsHelper.Locate(fip);
            if (fip.ShowDialog() == DialogResult.OK)
            {
                btnIcon.BackgroundImage = fip.PickedIcon;
                iconChanged = true;
            }

            fip.Dispose();
        }

        private void btnDeviceProperties_Click(object sender, EventArgs e)
        {
            try
            {
                device.DisplayPropertyPage(this.Handle);
            }
            catch (Exception)
            {
                log.ErrorFormat(CameraLang.FormConfiguration_DevicePropertyPages_Error, summary.Alias);
            }
        }

        #region Mediatype and framerate selection    
        private void InitializeMediaTypes(CameraSummary summary)
        {
            device = new VideoCaptureDevice(summary.Identifier);

            if (device.VideoCapabilities == null || device.VideoCapabilities.Length == 0)
            {
                canStreamConfig = false;
                return;
            }

            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info != null)
            {
                selectedMediaTypeIndex = info.MediaTypeIndex;
                selectedFramerate = info.SelectedFramerate;
            }

            mediaTypes = MediaTypeImporter.Import(device);
            if (mediaTypes == null || mediaTypes.Count == 0)
            {
                canStreamConfig = false;
                return;
            }

            // Ensure indexing by selected media type is valid.
            if (!mediaTypes.ContainsKey(selectedMediaTypeIndex))
            {
                selectedMediaTypeIndex = mediaTypes[0].MediaTypeIndex;
                log.ErrorFormat("Mediatype index not found, using first media type.");
            }

            organizer.Organize(mediaTypes, MediaTypeImporter.GetSupportedFramerates(device));

            canStreamConfig = true;
        }

        private void DisableStreamConfig()
        {
            lblStreamFormat.Enabled = false;
            lblImageSize.Enabled = false;
            lblFramerate.Enabled = false;
            cmbFormat.Enabled = false;
            cmbImageSize.Enabled = false;
            cmbFramerate.Enabled = false;
        }
        
        private void PopulateFormats()
        {
            int match = -1;
            foreach (SizeGroup sizeGroup in organizer.FormatGroups.Values)
            {
                cmbFormat.Items.Add(sizeGroup);

                if (mediaTypes[selectedMediaTypeIndex].Compression == sizeGroup.Format)
                    match = cmbFormat.Items.Count - 1;
            }

            if (match != -1)
                cmbFormat.SelectedIndex = match;
            else if (cmbFormat.Items.Count > 0)
                cmbFormat.SelectedIndex = 0;
        }

        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!canStreamConfig)
                return;

            SizeGroup sizeGroup = cmbFormat.SelectedItem as SizeGroup;
            if (sizeGroup == null)
                return;

            PopulateFrameSizes(sizeGroup);
        }

        private void PopulateFrameSizes(SizeGroup sizeGroup)
        {
            // Populate the list of frame sizes with media types that use the current compression.
            // Select the best match according to the current selection.

            cmbImageSize.Items.Clear();

            int match = -1;
            foreach (FramerateGroup framerateGroup in sizeGroup.FramerateGroups.Values)
            {
                cmbImageSize.Items.Add(framerateGroup);

                if (framerateGroup.Size == mediaTypes[selectedMediaTypeIndex].FrameSize)
                    match = cmbImageSize.Items.Count - 1;
            }

            if (match != -1)
                cmbImageSize.SelectedIndex = match;
            else if (cmbImageSize.Items.Count > 0)
                cmbImageSize.SelectedIndex = 0;
        }

        private void cmbImageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!canStreamConfig)
                return;

            FramerateGroup framerateGroup = cmbImageSize.SelectedItem as FramerateGroup;
            if (framerateGroup == null)
                return;

            PopulateFramerates(framerateGroup);
        }

        private void PopulateFramerates(FramerateGroup framerateGroup)
        {
            cmbFramerate.Items.Clear();

            int match = -1;
            foreach (MediaTypeSelection selectable in framerateGroup.Framerates.Values)
            {
                cmbFramerate.Items.Add(selectable);

                if (Math.Abs(selectedFramerate - selectable.Framerate) < 0.001)
                    match = cmbFramerate.Items.Count - 1;
            }

            if (match != -1)
                cmbFramerate.SelectedIndex = match;
            else if (cmbFramerate.Items.Count > 0)
                cmbFramerate.SelectedIndex = 0;
        }

        private void cmbFramerate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!canStreamConfig)
                return;

            MediaTypeSelection selectable = cmbFramerate.SelectedItem as MediaTypeSelection;
            if (selectable == null)
                return;

            selectedMediaTypeIndex = selectable.MediaType.MediaTypeIndex;
            selectedFramerate = selectable.Framerate;

            if (streamConfigInitialized)
            {
                specificChanged = true;
                needsReconnection = true;
            }
        }
        #endregion

        #region Camera controls
        private void PopulateCameraControl()
        {
            int top = lblAuto.Bottom;

            if (cameraProperties.ContainsKey("exposure_logitech"))
                AddCameraProperty("exposure_logitech", CameraLang.FormConfiguration_Properties_Exposure, VendorHelper.GetValueMapper(identifier, "exposure_logitech"), top);
            else if (cameraProperties.ContainsKey("exposure"))
                AddCameraProperty("exposure", CameraLang.FormConfiguration_Properties_Exposure, VendorHelper.GetValueMapper(identifier, "exposure"), top);

            AddCameraProperty("gain", CameraLang.FormConfiguration_Properties_Gain, null, top + 30);
            AddCameraProperty("focus", CameraLang.FormConfiguration_Properties_Focus, null, top + 60);
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
            groupBox1.Controls.Add(control);
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
            CameraPropertyManager.Write(device, cameraProperties[key]);

            specificChanged = true;

            /*
            CameraPropertyView cpv = sender as CameraPropertyView;
            if (cpv == null)
                return;

            CameraControlProperty? property = cpv.Tag as CameraControlProperty?;
            if (property == null || !property.HasValue)
                return;

            CameraControlFlags flags = cpv.Property.Automatic ? CameraControlFlags.Auto : CameraControlFlags.Manual;
            device.SetCameraProperty(property.Value, cpv.Property.Value, flags); */
        }

        /*private void cpvVideoProcAmp_ValueChanged(object sender, EventArgs e)
        {
            CameraPropertyView cpv = sender as CameraPropertyView;
            if (cpv == null)
                return;

            VideoProcAmpProperty? property = cpv.Tag as VideoProcAmpProperty?;
            if (property == null || !property.HasValue)
                return;

            VideoProcAmpFlags flags = cpv.Property.Automatic ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual;
            device.SetVideoProperty(property.Value, cpv.Property.Value, flags);
        }

        private void cpvLogitechExposure_ValueChanged(object sender, EventArgs e)
        {
            CameraPropertyView cpv = sender as CameraPropertyView;
            if (cpv == null)
                return;

            device.Logitech_SetExposure(cpv.Property.Value, !cpv.Property.Automatic);

            // The device might decide to adjust the selected exposure on its own due to internal constraints.
            // However it was found that the actual exposure used is closer to the original asked one than 
            // to the adjusted one. It is possibly just a truncation after a float conversion.
        }*/
        #endregion
    }
}
