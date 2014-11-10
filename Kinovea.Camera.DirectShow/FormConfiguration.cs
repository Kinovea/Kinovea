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

        public int SelectedExposure
        {
            get { return selectedExposure; }
        }

        public bool CanSetExposure
        {
            get { return canExposure; }
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
        private Dictionary<int, List<float>> possibleFramerates;
        private int selectedMediaTypeIndex;
        private float selectedFramerate;
        private bool canStreamConfig;

        private int minExposure;
        private int maxExposure;
        private int selectedExposure;
        private bool canExposure;
        private bool useLogitechExposure;
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
                PopulateColorSpaces();
            else
                DisableStreamConfig();

            PopulateCameraControl();
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

            possibleFramerates = MediaTypeImporter.GetSupportedFramerates(device);

            canStreamConfig = true;
        }

        private void DisableStreamConfig()
        {
            lblColorSpace.Enabled = false;
            lblImageSize.Enabled = false;
            lblFramerate.Enabled = false;
            cmbColorSpace.Enabled = false;
            cmbImageSize.Enabled = false;
            cmbFramerate.Enabled = false;
        }
        
        private void PopulateColorSpaces()
        {
            HashSet<string> compressionOptions = GetCompressionOptions(mediaTypes);

            int match = -1;
            foreach (string compression in compressionOptions)
            {
                cmbColorSpace.Items.Add(compression);
                
                if (mediaTypes[selectedMediaTypeIndex].Compression == compression)
                    match = cmbColorSpace.Items.Count - 1;
            }

            if (match != -1)
                cmbColorSpace.SelectedIndex = match;
            else if (cmbColorSpace.Items.Count > 0)
                cmbColorSpace.SelectedIndex = 0;
        }

        private HashSet<string> GetCompressionOptions(Dictionary<int, MediaType> mediaTypes)
        {
            HashSet<string> options = new HashSet<string>();
            foreach (MediaType mt in mediaTypes.Values)
                options.Add(mt.Compression);

            return options;
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
        
        private void cmbColorSpace_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!canStreamConfig)
                return;

            string selectedCompression = cmbColorSpace.SelectedItem as string;
            if (selectedCompression == null)
                return;

            PopulateFrameSizes(selectedCompression);
        }

        private void PopulateFrameSizes(string selectedCompression)
        {
            // Populate the list of frame sizes with media types that use the current compression.
            // Select the best match according to the current selection.

            cmbImageSize.Items.Clear();

            int indexMatch = -1;
            int sizeMatch = -1;
            foreach (MediaType mt in mediaTypes.Values)
            {
                if (mt.Compression != selectedCompression)
                    continue;

                cmbImageSize.Items.Add(mt);

                if (mt.MediaTypeIndex == selectedMediaTypeIndex)
                    indexMatch = cmbImageSize.Items.Count - 1;
                
                if (mt.FrameSize == mediaTypes[selectedMediaTypeIndex].FrameSize)
                    sizeMatch = cmbImageSize.Items.Count - 1;
            }

            if (indexMatch != -1)
                cmbImageSize.SelectedIndex = indexMatch;
            else if (sizeMatch != -1)
                cmbImageSize.SelectedIndex = sizeMatch;
            else if (cmbImageSize.Items.Count > 0)
                 cmbImageSize.SelectedIndex = 0;
        }
        
        private void btnDeviceProperties_Click(object sender, EventArgs e)
        {
            try
            {
                device.DisplayPropertyPage(this.Handle);
            }
            catch(Exception)
            {
                log.ErrorFormat("Error happened while trying to display the device property page for {0}.", summary.Alias);
            }
        }

        private void cmbImageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            MediaType mt = cmbImageSize.SelectedItem as MediaType;
            if (mt == null)
                return;

            selectedMediaTypeIndex = mt.MediaTypeIndex;
            specificChanged = true;

            PopulateFramerates(mt);
        }

        private void PopulateFramerates(MediaType mt)
        {
            List<float> framerates = possibleFramerates[selectedMediaTypeIndex];
            cmbFramerate.Items.Clear();

            int match = -1;
            foreach (float framerate in framerates)
            {
                cmbFramerate.Items.Add(string.Format("{0:0.000}", framerate));
                
                if (Math.Abs(selectedFramerate - framerate) < 0.001)
                    match = cmbFramerate.Items.Count - 1;
            }

            if (match != -1)
                cmbFramerate.SelectedIndex = match;
            else if (cmbFramerate.Items.Count > 0)
                cmbFramerate.SelectedIndex = 0;
        }

        private void cmbFramerate_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = cmbFramerate.SelectedIndex;
            if (index < 0)
                return;

            List<float> framerates = possibleFramerates[selectedMediaTypeIndex];
            selectedFramerate = framerates[index];

            specificChanged = true;
        }
        #endregion
    
        private void PopulateCameraControl()
        {
            canExposure = false;

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

            if (!canExposure)
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
            bool success = device.Logitech_GetExposure(out currentValue);
            selectedExposure = Math.Min(maxExposure, Math.Max(minExposure, currentValue));
            UpdateExposureLabel();

            updatingExposure = true;
            tbExposure.Value = selectedExposure;
            updatingExposure = false;
            
            canExposure = true;
        }

        private void PopulateGenericExposure()
        {
            int step;
            int defaultValue;
            CameraControlFlags flags;
            bool success = device.GetCameraPropertyRange(CameraControlProperty.Exposure, out minExposure, out maxExposure, out step, out defaultValue, out flags);
            if (!success || step != 1)
                return;

            tbExposure.Minimum = minExposure;
            tbExposure.Maximum = maxExposure;

            int currentValue;
            success = device.GetCameraProperty(CameraControlProperty.Exposure, out currentValue, out flags);

            if (!success)
                return;

            updatingExposure = true;

            int value = Math.Min(maxExposure, Math.Max(minExposure, currentValue));
            tbExposure.Value = value;

            updatingExposure = false;

            canExposure = true;
        }

        private void tbExposure_ValueChanged(object sender, EventArgs e)
        {
            if (updatingExposure)
                return;

            if (tbExposure.Value < minExposure || tbExposure.Value > maxExposure)
                return;

            selectedExposure = tbExposure.Value;

            if (useLogitechExposure)
                UpdateLogitechExposure();
            else
                UpdateGenericExposure();
        }

        private void UpdateLogitechExposure()
        {
            device.Logitech_SetExposure(selectedExposure);
            
            int currentValue;
            bool success = device.Logitech_GetExposure(out currentValue);
            selectedExposure = Math.Min(maxExposure, Math.Max(minExposure, currentValue));
            UpdateExposureLabel();
        }

        private void UpdateGenericExposure()
        {
            device.SetCameraProperty(CameraControlProperty.Exposure, selectedExposure, CameraControlFlags.Manual);
        }

        private void UpdateExposureLabel()
        {
            // The value from the Logitech LP1 propset is expressed in 100µs units.
            if (selectedExposure < 10)
                lblExposureValue.Text = string.Format("{0} µs", selectedExposure * 100);
            else
                lblExposureValue.Text = string.Format("{0:0.#} ms", selectedExposure / 10F);
        }
    }
}
