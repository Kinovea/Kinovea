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

            NudHelper.FixNudScroll(nudBitrate);
            NudHelper.FixNudScroll(nudMaxBitrate);
            NudHelper.FixNudScroll(nudMinBitrate);
            NudHelper.FixNudScroll(nudGOPSize);
        }

        private void Initialize()
        {
            // TODO: Populate the list of presets.
            lblPreset.Text = "Export preset:";
            cbPreset.Items.Add("Web");
            cbPreset.Items.Add("Frame by frame");
            cbPreset.Items.Add("Custom");
            cbPreset.SelectedIndex = 0;

            // Video containers.
            lblContainer.Text = "Video container:";
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

            // Quality control.
            lblQualityControl.Text = "Quality control:";
            cbQualityControl.Items.Add("Constant quality");
            cbQualityControl.Items.Add("Constant bitrate");
            cbQualityControl.SelectedIndex = 0;

            // Encoding Quality
            lblEncodingQuality.Text = "Encoding quality:";
            cbEncodingQuality.Items.Add("Perceptually lossless");
            cbEncodingQuality.Items.Add("High");
            cbEncodingQuality.Items.Add("Medium");
            cbEncodingQuality.Items.Add("Low");
            cbEncodingQuality.SelectedIndex = 1;

            // Encoding speed.
            lblEncodingSpeed.Text = "Encoding speed:";
            cbEncodingSpeed.Items.Add("Fast");
            cbEncodingSpeed.Items.Add("Medium");
            cbEncodingSpeed.Items.Add("Slow");
            cbEncodingSpeed.SelectedIndex = 1;
        }

        public void FillValues(ExportProfile profile)
        {
            manualUpdate = true;

            // Preset name.
            bool namedPreset = false;
            for (int i = 0; i < ExportProfile.ExportProfiles.Count; i++)
            {
                if (profile.Name == ExportProfile.ExportProfiles[i].Name)
                {
                    cbPreset.SelectedIndex = i;
                    namedPreset = true;
                }
            }
            
            if (!namedPreset)
            {
                cbPreset.SelectedIndex = ExportProfile.ExportProfiles.Count;
            }

            cbContainer.SelectedIndex = (int)profile.Container;
            cbCodec.SelectedIndex = (int)profile.Codec;
            cbQualityControl.SelectedIndex = profile.UseConstantBitrate ? 1 : 0;
            cbEncodingQuality.SelectedIndex = (int)profile.EncodingQuality;
            cbEncodingSpeed.SelectedIndex = (int)profile.EncodingSpeed;
            nudBitrate.Value = profile.Bitrate;
            nudMaxBitrate.Value = profile.MaxBitrate;
            nudMinBitrate.Value = profile.MinBitrate;
            nudGOPSize.Value = profile.GOPSize;
            UpdateBitrateEnable();
            manualUpdate = false;
        }

        public ExportProfile GetExportProfile()
        {
            ExportProfile profile = new ExportProfile();
            
            profile.Name = presetName;
            profile.Container = (VideoContainer)cbContainer.SelectedIndex;
            profile.Codec = (VideoCodec)cbCodec.SelectedIndex;
            profile.UseConstantBitrate = cbQualityControl.SelectedIndex == 1;
            profile.EncodingQuality = (EncodingQuality)cbEncodingQuality.SelectedIndex;
            profile.EncodingSpeed = (EncodingSpeed)cbEncodingSpeed.SelectedIndex;
            profile.Bitrate = (long)nudBitrate.Value;
            profile.MaxBitrate = (long)nudMaxBitrate.Value;
            profile.MinBitrate = (long)nudMinBitrate.Value;
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
            if (index < ExportProfile.ExportProfiles.Count)
            {
                FillValues(ExportProfile.ExportProfiles[index]);
                presetName = ExportProfile.ExportProfiles[index].Name;
            }
            else
            {
                // Custom preset, do nothing.
                presetName = "Custom";
            }
        }

        private void any_Changed(object sender, EventArgs e)
        {
            if (manualUpdate)
            {
                return;
            }

            UpdateBitrateEnable();
            SetCustom();
        }

        private void SetCustom()
        {
            // Mark the preset as custom if the user changed any value.
            if (cbPreset.SelectedIndex != 2)
            {
                cbPreset.SelectedIndex = 2;
            }
        }

        private void UpdateBitrateEnable()
        {
            bool useConstantBitrate = cbQualityControl.SelectedIndex == 1;
            nudBitrate.Enabled = useConstantBitrate;
            nudMaxBitrate.Enabled = useConstantBitrate;
            nudMinBitrate.Enabled = useConstantBitrate;
        }
    }
}
