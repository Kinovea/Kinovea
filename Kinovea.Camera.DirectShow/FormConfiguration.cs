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
using System.Linq;

using AForge.Video.DirectShow;
using Kinovea.Camera;
using Kinovea.Services;

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
        
        public int SelectedMediaTypeIndex
        {
            get { return selectedMediaTypeIndex; }
        }

        public float SelectedFramerate
        {
            get { return selectedFramerate; }
        }

        public bool HasExposureControl
        {
            get { return hasExposureControl; }
        }

        public bool ManualExposure
        {
            get { return manualExposure; }
        }
        
        public int ExposureValue
        {
            get { return selectedExposure; }
        }

        public bool UseLogitechExposure
        {
            get { return useLogitechExposure; }
        }

        private bool iconChanged;
        private bool specificChanged;
        private CameraSummary summary;
        private VideoCaptureDevice device;
        private Dictionary<int, MediaType> mediaTypes;
        private MediaTypeOrganizer organizer = new MediaTypeOrganizer();
        private int selectedMediaTypeIndex;
        private float selectedFramerate;
        private bool canStreamConfig;

        private bool hasExposureControl;
        private bool manualExposure;
        private bool useLogitechExposure;
        private int minExposure;
        private int maxExposure;
        private int selectedExposure;
        private bool updatingExposure;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormConfiguration(CameraSummary summary)
        {
            this.summary = summary;
   
            InitializeComponent();
            
            tbAlias.Text = summary.Alias;
            lblSystemName.Text = summary.Name;
            btnIcon.BackgroundImage = summary.Icon;
            
            InitializeMediaTypes(summary);

            if (canStreamConfig)
                PopulateFormats();
            else
                DisableStreamConfig();

            PopulateCameraControl();
        }

        private void btnIcon_Click(object sender, EventArgs e)
        {
            FormIconPicker fip = new FormIconPicker(IconLibrary.Icons, 5, "Icons");
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
                log.ErrorFormat("Error happened while trying to display the device property page for {0}.", summary.Alias);
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
            lblColorSpace.Enabled = false;
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
            specificChanged = true;
        }
        #endregion

        #region Exposure
        private void PopulateCameraControl()
        {
            hasExposureControl = false;

            try
            {
                useLogitechExposure = device.Logitech_SupportExposureProperty();
                
                if (useLogitechExposure)
                    PopulateLogitechExposure();
                else
                    PopulateGenericExposure();
            }
            catch
            {
                log.ErrorFormat("Error while trying to get camera control properties.");
            }

            if (!hasExposureControl)
            {
                lblExposure.Enabled = false;
                tbExposure.Enabled = false;
            }

            if (!useLogitechExposure)
                lblExposureValue.Visible = false;
        }

        private void PopulateLogitechExposure()
        {
            // Set min to what is supported by the C920.
            minExposure = 1;
            maxExposure = 500;
            tbExposure.Minimum = minExposure;
            tbExposure.Maximum = maxExposure;

            int currentValue;
            bool manual;
            bool success = device.Logitech_GetExposure(out currentValue, out manual);

            if (!success)
                return;

            selectedExposure = Math.Min(maxExposure, Math.Max(minExposure, currentValue));
            manualExposure = manual;
            UpdateExposureLabel();

            updatingExposure = true;
            tbExposure.Value = selectedExposure;
            updatingExposure = false;
            
            hasExposureControl = true;
        }

        private void PopulateGenericExposure()
        {
            int step;
            int defaultValue;
            CameraControlFlags flags;
            bool success = device.GetCameraPropertyRange(CameraControlProperty.Exposure, out minExposure, out maxExposure, out step, out defaultValue, out flags);
            
            if (!success || step != 1)
                return;

            int currentValue;
            success = device.GetCameraProperty(CameraControlProperty.Exposure, out currentValue, out flags);

            if (!success)
                return;

            tbExposure.Minimum = minExposure;
            tbExposure.Maximum = maxExposure;

            selectedExposure = currentValue;
            manualExposure = flags == CameraControlFlags.Manual;

            updatingExposure = true;
            tbExposure.Value = selectedExposure;
            updatingExposure = false;

            hasExposureControl = true;
        }

        private void tbExposure_ValueChanged(object sender, EventArgs e)
        {
            if (updatingExposure)
                return;

            if (tbExposure.Value < minExposure || tbExposure.Value > maxExposure)
                return;

            selectedExposure = tbExposure.Value;
            manualExposure = true;

            if (useLogitechExposure)
                UpdateLogitechExposure();
            else
                UpdateGenericExposure();
        }

        private void UpdateLogitechExposure()
        {
            device.Logitech_SetExposure(selectedExposure, true);
            
            // The device might decide to adjust the selected exposure on its own due to internal constraints.
            // Read it back to have the actual final value in the label.
            int currentValue;
            bool manual;
            bool success = device.Logitech_GetExposure(out currentValue, out manual);
            selectedExposure = Math.Min(maxExposure, Math.Max(minExposure, currentValue));
            manualExposure = manual;
            
            UpdateExposureLabel();
        }

        private void UpdateGenericExposure()
        {
            device.SetCameraProperty(CameraControlProperty.Exposure, selectedExposure, CameraControlFlags.Manual);
        }

        private void UpdateExposureLabel()
        {
            // At the moment this label is only active for Logitech cameras,
            // as they are the only cameras for which we have solid values.
            // The value from the Logitech LP1 propset is expressed in 100µs units.
            if (selectedExposure < 10)
                lblExposureValue.Text = string.Format("{0} µs", selectedExposure * 100);
            else
                lblExposureValue.Text = string.Format("{0:0.#} ms", selectedExposure / 10F);
        }
        #endregion
    }
}
