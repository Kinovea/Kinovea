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
        
        public MediaType SelectedMediaType
        {
            get { return selectedMediaType; }
        }

        private bool iconChanged;
        private bool specificChanged;
        private CameraSummary summary;
        private VideoCaptureDevice device;
        private MediaType previousMediaType;
        private MediaType selectedMediaType;
        private List<MediaType> mediaTypes;
        private List<double> possibleFramerates;
        private bool canStreamConfig;
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
        }

        private void InitializeMediaTypes(CameraSummary summary)
        {
            device = new VideoCaptureDevice(summary.Identifier);

            if (device.VideoCapabilities == null || device.VideoCapabilities.Length == 0)
            {
                canStreamConfig = false;
                return;
            }

            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info != null && info.MediaType != null)
                previousMediaType = info.MediaType;

            mediaTypes = MediaTypeImporter.Import(device);
            if (mediaTypes == null || mediaTypes.Count == 0)
            {
                canStreamConfig = false;
                return;
            }

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
            // We do not have access to the device object here as we are called from a generic context.
            // Rebuild the device with its capabilities.

            HashSet<string> compressionOptions = GetCompressionOptions(mediaTypes);
            foreach (string compression in compressionOptions)
            {
                cmbColorSpace.Items.Add(compression);
                
                if (previousMediaType != null && previousMediaType.Compression == compression)
                    cmbColorSpace.SelectedIndex = cmbColorSpace.Items.Count - 1;
            }

            if (cmbColorSpace.SelectedIndex < 0 && cmbColorSpace.Items.Count > 0)
                cmbColorSpace.SelectedItem = cmbColorSpace.Items[0];
        }

        private HashSet<string> GetCompressionOptions(List<MediaType> mediaTypes)
        {
            HashSet<string> options = new HashSet<string>();
            foreach (MediaType mt in mediaTypes)
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
            cmbImageSize.Items.Clear();

            foreach (MediaType mt in mediaTypes)
            {
                if (mt.Compression != selectedCompression)
                    continue;

                cmbImageSize.Items.Add(mt);
                if (selectedMediaType != null)
                {
                    if (selectedMediaType.FrameSize == mt.FrameSize)
                        cmbImageSize.SelectedIndex = cmbImageSize.Items.Count - 1;
                }
                else if (previousMediaType != null)
                {
                    if (previousMediaType.FrameSize == mt.FrameSize)
                        cmbImageSize.SelectedIndex = cmbImageSize.Items.Count - 1;
                }
            }

            if (cmbImageSize.SelectedIndex == -1 && cmbImageSize.Items.Count > 0)
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

            selectedMediaType = mt;
            specificChanged = true;

            PopulateFramerates();
        }

        private void PopulateFramerates()
        {
            possibleFramerates = MediaTypeImporter.GetSupportedFramerates(device, selectedMediaType);
            cmbFramerate.Items.Clear();

            foreach (double fps in possibleFramerates)
            {
                cmbFramerate.Items.Add(string.Format("{0:0.000}", fps));
                
                if (selectedMediaType != null)
                {
                    if (selectedMediaType.SelectedFramerate - fps < 0.001)
                        cmbFramerate.SelectedIndex = cmbFramerate.Items.Count - 1;
                }
                else if (previousMediaType != null)
                {
                    if (previousMediaType.SelectedFramerate - fps < 0.001)
                        cmbFramerate.SelectedIndex = cmbFramerate.Items.Count - 1;
                }
            }

            if (cmbFramerate.SelectedIndex == -1 && cmbFramerate.Items.Count > 0)
                cmbImageSize.SelectedIndex = 0;
        }
    }
}
