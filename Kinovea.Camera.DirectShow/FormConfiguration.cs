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

using AForge.Video.DirectShow;
using Kinovea.Camera;

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
        
        public VideoCapabilities Capability
        {
            get { return capabilities[cmbCapabilities.SelectedIndex];}
        }
        
        private bool iconChanged;
        private bool specificChanged;
        private CameraSummary summary;
        private List<VideoCapabilities> capabilities = new List<VideoCapabilities>();
        private bool loaded;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormConfiguration(CameraSummary summary)
        {
            this.summary = summary;
            
            InitializeComponent();
            
            tbAlias.Text = summary.Alias;
            lblSystemName.Text = summary.Name;
            btnIcon.BackgroundImage = summary.Icon;

            // TODO: Get moniker from identifier.
            PopulateCapabilities();
            loaded = true;
        }
        
        void BtnIconClick(object sender, EventArgs e)
        {
            FormIconPicker fip = new FormIconPicker(IconLibrary.Icons, 5, "Icons");
            LocateForm(fip);
            if(fip.ShowDialog() == DialogResult.OK)
            {
                btnIcon.BackgroundImage = fip.PickedIcon;
                iconChanged = true;
            }
            
            fip.Dispose();
        }
        
        private void LocateForm(Form form)
        {
            // Note: function duplicated from ScreenManager which we don't want to depend upon.
            // Maybe this method would be better in Kinovea.Service.
            if (Cursor.Position.X + (form.Width / 2) >= SystemInformation.PrimaryMonitorSize.Width || 
                Cursor.Position.Y + form.Height >= SystemInformation.PrimaryMonitorSize.Height)
                form.StartPosition = FormStartPosition.CenterScreen;
            else
                form.Location = new Point(Cursor.Position.X - (form.Width / 2), Cursor.Position.Y - 20);
        }
        
        private void PopulateCapabilities()
        {
            VideoCaptureDevice device = new VideoCaptureDevice(summary.Identifier);
            
            if(device.VideoCapabilities == null || device.VideoCapabilities.Length == 0)
                return;
            
            Size selectedFrameSize = Size.Empty;
            int selectedFrameRate = 0;
            
            SpecificInfo info = summary.Specific as SpecificInfo;
            if(info != null)
            {
                selectedFrameSize = info.SelectedFrameSize;
                selectedFrameRate = info.SelectedFrameRate;
            }
            else
            {
                selectedFrameSize = device.VideoCapabilities[0].FrameSize;
                selectedFrameRate = device.VideoCapabilities[0].AverageFrameRate;
            }
            
            for(int i = 0; i<device.VideoCapabilities.Length; i++)
            {
                VideoCapabilities capability = device.VideoCapabilities[i];
                
                capabilities.Add(capability);
                cmbCapabilities.Items.Add(CapabilityToString(capability));
                
                if(capability.FrameSize == selectedFrameSize && capability.AverageFrameRate == selectedFrameRate)
                    cmbCapabilities.SelectedIndex = i;
            }
        }
        
        private string CapabilityToString(VideoCapabilities capability)
        {
            return string.Format("{0}×{1} @ {2} fps", capability.FrameSize.Width, capability.FrameSize.Height, capability.AverageFrameRate);
        }
        
        private void CmbCapabilitiesSelectedIndexChanged(object sender, EventArgs e)
        {
            if(loaded)
                specificChanged = true;
        }
        
        private void BtnDevicePropertiesClick(object sender, EventArgs e)
        {
            VideoCaptureDevice device = new VideoCaptureDevice(summary.Identifier);
            
            try
            {
                device.DisplayPropertyPage(this.Handle);
            }
            catch(Exception)
            {
                log.ErrorFormat("Error happened while trying to display the device property page for {0}.", summary.Alias);
            }
        }
    }
}
