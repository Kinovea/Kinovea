using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class InfobarCapture : UserControl
    {
        private LoadStatus loadStatus = LoadStatus.OK;

        public InfobarCapture()
        {
            InitializeComponent();
        }

        public void UpdateValues(string signal, string bandwidth, string load, string drops)
        {
            lblSignal.Text = string.Format(Kinovea.ScreenManager.Languages.ScreenManagerLang.infobar_Signal0, signal);
            lblBandwidth.Text = string.Format(Kinovea.ScreenManager.Languages.ScreenManagerLang.infobar_Throughput0, bandwidth);
            lblLoad.Text = string.Format(Kinovea.ScreenManager.Languages.ScreenManagerLang.infobar_Load0, load);
            lblDrops.Text = string.Format(Kinovea.ScreenManager.Languages.ScreenManagerLang.infobar_Drops0, drops);
        }

        public void UpdateLoadStatus(LoadStatus status)
        {
            if (status == this.loadStatus)
                return;

            this.loadStatus = status;
            switch (loadStatus)
            {
                case LoadStatus.Warning:
                    btnLoadStatus.Image = Properties.Resources.load_cloudy;
                    break;
                case LoadStatus.Critical:
                    btnLoadStatus.Image = Properties.Resources.load_rain;
                    break;
                case LoadStatus.OK:
                default:
                    btnLoadStatus.Image = Properties.Resources.load_sun;
                    break;
            }
        }
    }
}
