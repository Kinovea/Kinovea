using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.ScreenManager.Exporters.Video
{
    public partial class ControlEncodingSettings : UserControl
    {
        private bool manualUpdate = false;
        private string presetName = "";

        public ControlEncodingSettings()
        {
            InitializeComponent();

            manualUpdate = true;
            Initialize();
            manualUpdate = false;

            NudHelper.FixNudScroll(nudGOPSize);
        }

        private void Initialize()
        {
            // TODO: Populate the list of presets.
            lblPreset.Text = "Preset:";
            for (int i = 0; i < ExportProfile.NamedProfilesCount; i++)
            {
                cbPreset.Items.Add(ExportProfile.ExportProfiles[i].Name);
            }
            cbPreset.Items.Add("Custom");
            cbPreset.SelectedIndex = 0;

            // Video containers.
            lblContainer.Text = "Format:";
            cbContainer.Items.Add("MP4");
            cbContainer.Items.Add("MKV");
            cbContainer.Items.Add("AVI");
            cbContainer.SelectedIndex = 0;

            // Video codecs.
            lblCodec.Text = "Video codec:";
            cbCodec.Items.Add("MJPEG");
            cbCodec.Items.Add("H.264");
            cbCodec.Items.Add("H.265");
            cbCodec.SelectedIndex = 0;

            // Encoding Quality
            lblEncodingQuality.Text = "Encoding quality:";
            cbEncodingQuality.Items.Add("Perceptually lossless");
            cbEncodingQuality.Items.Add("High");
            cbEncodingQuality.Items.Add("Good");
            cbEncodingQuality.Items.Add("Medium");
            cbEncodingQuality.SelectedIndex = 1;

            // Encoding speed.
            lblEncodingSpeed.Text = "Encoding speed:";
            cbEncodingSpeed.Items.Add("Fast");
            cbEncodingSpeed.Items.Add("Medium");
            cbEncodingSpeed.Items.Add("Slow");
            cbEncodingSpeed.SelectedIndex = 1;

            lblGOPSize.Text = "GOP size:";
        }

        public void FillValues(ExportProfile profile, string filename = null)
        {
            manualUpdate = true;

            // Preset name.
            bool namedPreset = false;
            for (int i = 0; i < ExportProfile.NamedProfilesCount; i++)
            {
                if (profile.Name == ExportProfile.ExportProfiles[i].Name)
                {
                    cbPreset.SelectedIndex = i;
                    namedPreset = true;
                } 
            }

            if (!namedPreset)
            {
                cbPreset.SelectedIndex = ExportProfile.NamedProfilesCount;
            }


            // If the filename is set we are just coming from the file selection dialog
            // where the user picked a file with a known extension.
            // Select the container format that matches the extension.
            // If we don't have a filename we are changing the preset here and we want to 
            // follow the preset container format.
            if (filename != null)
            {
                string extension = System.IO.Path.GetExtension(filename).ToLower();
                if (extension == ".mp4")
                {
                    cbContainer.SelectedIndex = (int)VideoContainer.MP4;
                }
                else if (extension == ".mkv")
                {
                    cbContainer.SelectedIndex = (int)VideoContainer.MKV;
                }
                else if (extension == ".avi")
                {
                    cbContainer.SelectedIndex = (int)VideoContainer.AVI;
                }
            }
            else 
            {
                cbContainer.SelectedIndex = (int)profile.Container;
            }

            cbCodec.SelectedIndex = (int)profile.Codec;
            cbEncodingQuality.SelectedIndex = (int)profile.EncodingQuality;
            cbEncodingSpeed.SelectedIndex = (int)profile.EncodingSpeed;
            nudGOPSize.Value = profile.GOPSize;
            manualUpdate = false;

            UpdateEnabled();
        }

        public ExportProfile GetExportProfile()
        {
            ExportProfile profile = new ExportProfile();
            
            profile.Name = presetName;
            profile.Container = (VideoContainer)cbContainer.SelectedIndex;
            profile.Codec = (VideoCodec)cbCodec.SelectedIndex;
            profile.EncodingQuality = (EncodingQuality)cbEncodingQuality.SelectedIndex;
            profile.EncodingSpeed = (EncodingSpeed)cbEncodingSpeed.SelectedIndex;
            profile.GOPSize = (int)nudGOPSize.Value;
            return profile;
        }

        private void cbPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
            {
                return;
            }

            int index = cbPreset.SelectedIndex;
            if (index < ExportProfile.NamedProfilesCount)
            {
                FillValues(ExportProfile.ExportProfiles[index]);
                presetName = ExportProfile.ExportProfiles[index].Name;
            }
            else
            {
                // Custom preset, do nothing.
                presetName = "Custom";
            }

            UpdateEnabled();
        }

        private void any_Changed(object sender, EventArgs e)
        {
            if (manualUpdate)
            {
                return;
            }

            UpdateEnabled();
            SetCustom();
        }

        private void UpdateEnabled()
        {
            nudGOPSize.Enabled = cbCodec.SelectedIndex != (int)VideoCodec.MJPEG;
        }

        private void SetCustom()
        {
            // Mark the preset as custom if the user changed any value.
            if (cbPreset.SelectedIndex != ExportProfile.NamedProfilesCount)
            {
                cbPreset.SelectedIndex = ExportProfile.NamedProfilesCount;
            }
        }
    }
}
