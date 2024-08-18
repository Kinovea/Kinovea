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
    public partial class FormConfigureExportVideo : Form
    {
        /// <summary>
        /// Whether to take slow motion into account for the output frame rate.
        /// </summary>
        public bool UseSlowMotion
        {
            get { return checkSlowMotion.Checked; }
        }

        private PlayerScreen player;

        public FormConfigureExportVideo(PlayerScreen player)
        {
            this.player = player;
            
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
            // Note: this is not concerned with the mapping from capture time to file time.
            // That mapping is used to calibrate time display. Here we are only concerned with changing the 
            // output framerate with regards to the input framerate.
            string inputOutputRate = "";
            if (player.view.SpeedPercentage < 100)
                inputOutputRate = string.Format("{0:0.##}%", player.view.SpeedPercentage);
            else
                inputOutputRate = string.Format("{0:0.##}x", player.view.SpeedPercentage / 100.0);

            checkSlowMotion.Text = string.Format(ScreenManagerLang.FormConfigureExportVideo_UseCurrentPlaybackSpeed, inputOutputRate);

            bool isNominal = player.view.SpeedPercentage == 100;
            checkSlowMotion.Checked = isNominal;
            checkSlowMotion.Enabled = !isNominal;
        }
    }
}
