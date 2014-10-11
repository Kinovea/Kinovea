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
        
        public int Framerate
        {
            get
            {
                int selected = (int)cmbFramerate.SelectedItem;
                return selected == 0 ? defaultFramerate : selected;
            }
        }

        private bool iconChanged;
        private bool specificChanged;
        private CameraSummary summary;
        private bool loaded;
        private List<FrameSize> frameSizes = new List<FrameSize>();
        private FrameSize defaultFrameSize;
        private List<int> framerates = new List<int>();
        private int defaultFramerate;
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
            frameSizes.Add(new FrameSize("HD 720 (1280 × 720)", new Size(1280, 720)));
            frameSizes.Add(new FrameSize("HD 1080 (1920 × 1080)", new Size(1920, 1080)));
            frameSizes.Add(new FrameSize("2K (2048 × 1080)", new Size(2048, 1080)));
            frameSizes.Add(new FrameSize("4K (4096 × 2160)", new Size(4096, 2160)));

            defaultFrameSize = frameSizes[3];

            framerates.Add(10);
            framerates.Add(15);
            framerates.Add(20);
            framerates.Add(25);
            framerates.Add(30);
            framerates.Add(50);
            framerates.Add(60);
            framerates.Add(100);

            defaultFramerate = 25;
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
            if (Cursor.Position.X + (form.Width / 2) >= SystemInformation.PrimaryMonitorSize.Width || 
                Cursor.Position.Y + form.Height >= SystemInformation.PrimaryMonitorSize.Height)
                form.StartPosition = FormStartPosition.CenterScreen;
            else
                form.Location = new Point(Cursor.Position.X - (form.Width / 2), Cursor.Position.Y - 20);
        }
        
        private void PopulateCapabilities()
        {
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
                selectedFrameSize = defaultFrameSize.Value;
                selectedFrameRate = defaultFramerate;
            }

            int index = 0;
            foreach (FrameSize frameSize in frameSizes)
            {
                cmbFrameSize.Items.Add(frameSize);

                if (frameSize.Value == selectedFrameSize)
                    cmbFrameSize.SelectedIndex = index;

                index++;
            }

            index = 0;
            foreach (int framerate in framerates)
            {
                cmbFramerate.Items.Add(framerate);

                if (framerate == selectedFrameRate)
                    cmbFramerate.SelectedIndex = index;

                index++;
            }
        }
        
        private void SpecificInfo_Changed(object sender, EventArgs e)
        {
            if(loaded)
                specificChanged = true;
        }
    }
}
