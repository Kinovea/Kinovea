using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public partial class FormConfigureExportImageSequence : Form
    {
        /// <summary>
        /// Frame interval to export at, in timestamps.
        /// </summary>
        public double IntervalTimestamps
        {
            get { return videoInfo.AverageTimeStampsPerFrame * decimationFrames; }
        }

        /// <summary>
        /// Number of frames we will export.
        /// </summary>
        public int RemainingFrames
        {
            get { return (int)Math.Round((float)totalFrames / decimationFrames); }
        }


        private PlayerScreen player;
        private Metadata metadata;
        private VideoInfo videoInfo;
        private int totalFrames;
        private int decimationFrames = 1; // Denominator of the decimation (1 frame every n).
        private int maxDecimationFrames;
        private double frameInterval;
        private List<int> values = new List<int>();
        private const int defaultDecimationFrames = 10;
        private const int limitDecimationFrames = 500;

        public FormConfigureExportImageSequence(PlayerScreen player)
        {
            this.player = player;
            this.metadata = player.FrameServer.Metadata;
            this.videoInfo = player.FrameServer.VideoReader.Info;
            frameInterval = metadata.BaselineFrameInterval;
            totalFrames = (int)((metadata.SelectionEnd - metadata.SelectionStart) / metadata.AverageTimeStampsPerFrame) + 1;
            maxDecimationFrames = totalFrames / 2;
            maxDecimationFrames = Math.Min(limitDecimationFrames, maxDecimationFrames);

            decimationFrames = 1;

            InitializeComponent();
            InitializeCulture();
            Populate();
        }

        private void InitializeCulture()
        {
            this.Text = ScreenManagerLang.formConfigureExport_ImageSequence;

            grpboxConfig.Text = ScreenManagerLang.Generic_Configuration;
            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }

        private void Populate()
        {
            // Build a list of values that actually produce a different result.
            values.Clear();
            int lastFrames = 0;
            int indexOfDefault = 0;
            for (int i = 1; i < maxDecimationFrames; i++)
            {
                int frames = (int)Math.Round((float)totalFrames / i);
                if (frames == lastFrames)
                    continue;

                lastFrames = frames;
                values.Add(i);
                if (i >= defaultDecimationFrames && indexOfDefault == 0)
                    indexOfDefault = values.Count - 1;
            }

            trkDecimate.Minimum = 0;
            trkDecimate.Maximum = values.Count - 1;
            trkDecimate.Value = indexOfDefault;
            decimationFrames = values[indexOfDefault];
        }

        private void UpdateLabels()
        {
            int remainingFrames = (int)Math.Round((float)totalFrames / decimationFrames);

            if (decimationFrames == 1)
                lblFrameDecimation.Text = ScreenManagerLang.dlgRafaleExport_ExportAll;
            else
                lblFrameDecimation.Text = string.Format(ScreenManagerLang.dlgRafaleExport_ExportFrameDecimation, decimationFrames);

            double decimationTime = decimationFrames * frameInterval;
            if (decimationTime >= 1000)
            {
                decimationTime /= 1000;
                lblTimeDecimation.Text = string.Format(ScreenManagerLang.dlgRafaleExport_ExportTimeDecimationSeconds, decimationTime);
            }
            else
            {
                lblTimeDecimation.Text = string.Format(ScreenManagerLang.dlgRafaleExport_ExportTimeDecimationMilliseconds, decimationTime);
            }

            lblTotalFrames.Text = string.Format(ScreenManagerLang.dlgRafaleExport_LabelTotalFrames, remainingFrames);
        }

        private void trkDecimate_ValueChanged(object sender, EventArgs e)
        {
            decimationFrames = values[trkDecimate.Value];
            UpdateLabels();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {

        }
    }
}
