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
using Kinovea.Camera;
using Kinovea.Services;

namespace Kinovea.Camera.FrameGenerator
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
        
        public Size FrameSize
        {
            get 
            { 
                FrameSize selected = cmbFrameSize.SelectedItem as FrameSize;
                return selected == null ? defaultFrameSize.Value : selected.Value;
            }
        }
        
        public int FrameInterval
        {
            get
            {
                Framerate framerate = (Framerate)cmbFramerate.SelectedItem;
                return framerate == null ? defaultFrameInterval : framerate.FrameInterval;
            }
        }

        private bool iconChanged;
        private bool specificChanged;
        private CameraSummary summary;
        private bool loaded;
        private List<FrameSize> frameSizes = new List<FrameSize>();
        private List<Framerate> framerates = new List<Framerate>();
        private FrameSize defaultFrameSize;
        private int defaultFrameInterval = 20000;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormConfiguration(CameraSummary summary)
        {
            this.summary = summary;
            
            InitializeComponent();

            InitializeParameters();
            
            tbAlias.Text = summary.Alias;
            lblSystemName.Text = summary.Name;
            btnIcon.BackgroundImage = summary.Icon;

            PopulateCapabilities();
            loaded = true;
        }

        private void InitializeParameters()
        {
            frameSizes.Clear();
            frameSizes.Add(new FrameSize("QVGA (320 × 240)", new Size(320, 240)));
            frameSizes.Add(new FrameSize("VGA (640 × 480)", new Size(640, 480)));
            frameSizes.Add(new FrameSize("DV (720 × 480)", new Size(720, 480)));
            frameSizes.Add(new FrameSize("SVGA (800 × 600)", new Size(800, 600)));
            frameSizes.Add(new FrameSize("720p (1280 × 720)", new Size(1280, 720)));
            frameSizes.Add(new FrameSize("1080p (1920 × 1080)", new Size(1920, 1080)));
            frameSizes.Add(new FrameSize("2K (2048 × 1080)", new Size(2048, 1080)));
            frameSizes.Add(new FrameSize("4K UHDTV (3840 × 2160)", new Size(3840, 2160)));
            frameSizes.Add(new FrameSize("4K (4096 × 2160)", new Size(4096, 2160)));
            //frameSizes.Add(new FrameSize("8K UHDTV (7680 × 4320)", new Size(7680, 4320)));
            //frameSizes.Add(new FrameSize("8K (8192 × 4320)", new Size(8192, 4320)));
            
            defaultFrameSize = frameSizes[3];

            List<double> commonFramerates = new List<double>() { 10, 15, 20, 24, 25, 29.97, 30, 50, 60, 100, 120, 200, 300, 500, 1000 };
            
            framerates.Clear();
            foreach (double framerate in commonFramerates)
            {
                int interval = (int)Math.Round(1000000.0 / framerate);
                framerates.Add(new Framerate(interval));
            }
        }
        
        void BtnIconClick(object sender, EventArgs e)
        {
            FormIconPicker fip = new FormIconPicker(IconLibrary.Icons, 5, "Icons");
            FormsHelper.Locate(fip);
            if(fip.ShowDialog() == DialogResult.OK)
            {
                btnIcon.BackgroundImage = fip.PickedIcon;
                iconChanged = true;
            }
            
            fip.Dispose();
        }
        
        private void PopulateCapabilities()
        {
            int selectedFrameInterval = 0;
            Size selectedFrameSize = Size.Empty;

            SpecificInfo info = summary.Specific as SpecificInfo;
            if(info != null)
            {
                selectedFrameInterval = info.FrameInterval;
                selectedFrameSize = info.FrameSize;
            }
            else
            {
                selectedFrameInterval = defaultFrameInterval;
                selectedFrameSize = defaultFrameSize.Value;
            }

            int sizeIndex = 0;
            foreach (FrameSize frameSize in frameSizes)
            {
                cmbFrameSize.Items.Add(frameSize);

                if (frameSize.Value == selectedFrameSize)
                    cmbFrameSize.SelectedIndex = sizeIndex;

                sizeIndex++;
            }

            int fpsIndex = 0;

            foreach (Framerate framerate in framerates)
            {
                cmbFramerate.Items.Add(framerate);
                
                if (framerate.FrameInterval == selectedFrameInterval)
                    cmbFramerate.SelectedIndex = fpsIndex;

                fpsIndex++;
            }
        }
        
        private void SpecificInfo_Changed(object sender, EventArgs e)
        {
            if(loaded)
                specificChanged = true;
        }
    }
}
