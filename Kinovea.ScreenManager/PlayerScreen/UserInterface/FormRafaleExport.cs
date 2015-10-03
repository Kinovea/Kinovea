using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.Video;
using Kinovea.ScreenManager.Languages;
using System.IO;

namespace Kinovea.ScreenManager
{
    public partial class FormRafaleExport : Form
    {
        #region Members
        private PlayerScreenUserInterface playerScreenUserInterface;
        private Metadata metadata;
        private VideoInfo info;
        private string fullPath;
        private int totalFrames;
        private int maxDecimationFrames;
        private int decimationFrames;
        private double frameInterval;
        private int defaultDecimationFrames = 10;
        private int limitDecimationFrames = 500;
        private List<int> values = new List<int>();
        #endregion

        public FormRafaleExport(PlayerScreenUserInterface playerScreenUserInterface, Metadata metadata, string fullPath, VideoInfo info)
        {
            this.playerScreenUserInterface = playerScreenUserInterface;
            this.metadata = metadata;
            this.fullPath = fullPath;
            this.info = info;

            frameInterval = metadata.UserInterval;
            totalFrames = (int)((metadata.SelectionEnd - metadata.SelectionStart) / metadata.AverageTimeStampsPerFrame) + 1;
            maxDecimationFrames = totalFrames / 2;
            maxDecimationFrames = Math.Min(limitDecimationFrames, maxDecimationFrames);
            
            InitializeComponent();
            SetupUICulture();
            Populate();
        }

        private void SetupUICulture()
        {
            this.Text = "   " + ScreenManagerLang.dlgRafaleExport_Title;
            
            grpboxConfig.Text = ScreenManagerLang.Generic_Configuration;
            chkKeyframesOnly.Text = ScreenManagerLang.dlgRafaleExport_LabelKeyframesOnly;
            chkKeyframesOnly.Enabled = metadata.Count > 0;
            
            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        
        private void Populate()
        {
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
        }

        private void trkDecimate_ValueChanged(object sender, EventArgs e)
        {
            decimationFrames = values[trkDecimate.Value];
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            int frames = (int)Math.Round((float)totalFrames / decimationFrames);

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

            lblTotalFrames.Text = string.Format(ScreenManagerLang.dlgRafaleExport_LabelTotalFrames, frames);
        }

        private void chkKeyframesOnly_CheckedChanged(object sender, EventArgs e)
        {
            grpboxConfig.Enabled = !chkKeyframesOnly.Checked;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgSaveSequenceTitle;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = ScreenManagerLang.FileFilter_SaveImage;
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(fullPath);

            Hide();

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                Show();
                return;
            }

            if (string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                Close();
                return;
            }

            int frames = (int)Math.Round((float)totalFrames / decimationFrames);
            long interval = info.AverageTimeStampsPerFrame * decimationFrames;
            formFramesExport ffe = new formFramesExport(playerScreenUserInterface, saveFileDialog.FileName, interval, true, chkKeyframesOnly.Checked, frames);
            ffe.ShowDialog();
            ffe.Dispose();

            Close();
        }


    }
}
