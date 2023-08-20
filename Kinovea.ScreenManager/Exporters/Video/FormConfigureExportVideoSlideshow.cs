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
    public partial class FormConfigureExportVideoSlideshow : Form
    {
        /// <summary>
        /// Duratiion of the key image slides, in milliseconds.
        /// </summary>
        public double SlideDurationMilliseconds
        {
            get { return trkInterval.Value; }
        }

        public FormConfigureExportVideoSlideshow()
        {
            InitializeComponent();
            InitializeCulture();
            Populate();
        }

        private void InitializeCulture()
        {
            this.Text = ScreenManagerLang.CommandExportVideo_FriendlyName;
            grpboxConfig.Text = ScreenManagerLang.Generic_Configuration;
            
            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }

        private void Populate()
        {
            // trkInterval values in milliseconds.
            trkInterval.Minimum = 40;
            trkInterval.Maximum = 8000;
            trkInterval.Value = 2000;
            trkInterval.TickFrequency = 250;
        }

        private void trkInterval_ValueChanged(object sender, EventArgs e)
        {
            UpdateLabels();
        }
        private void UpdateLabels()
        {
            // Frequency
            double intervalSeconds = (double)trkInterval.Value / 1000;
            if (intervalSeconds < 1.0)
            {
                int iHundredth = (int)(intervalSeconds * 100);
                lblInfosFrequency.Text = String.Format(ScreenManagerLang.dlgDiapoExport_LabelFrequencyHundredth, iHundredth);
            }
            else
            {
                lblInfosFrequency.Text = String.Format(ScreenManagerLang.dlgDiapoExport_LabelFrequencySeconds, intervalSeconds);
            }
        }
    }
}
