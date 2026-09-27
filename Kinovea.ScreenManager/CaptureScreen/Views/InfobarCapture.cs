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

        public void UpdateValues(double signal, double load, int drops, int queue)
        {
            string signalText = string.Format(" {0:0.00} fps", signal);
            string loadText = string.Format(" {0:0.00} %", load);
            string dropsText = string.Format(" {0}", drops);
            string queueText = string.Format(" {0}", queue);

            lblSignal.Text = string.Format(Languages.ScreenManagerLang.infobar_Signal0, signalText);
            lblLoad.Text = string.Format(Languages.ScreenManagerLang.infobar_Load0, loadText);
            lblDrops.Text = string.Format(Languages.ScreenManagerLang.infobar_Drops0, dropsText);
            lblQueue.Text = string.Format("Queue:{0}", queueText);
        }

        public void RefreshUICulture()
        {
            // Setup the width and position of the controls.

            string signalText = string.Format(" {0:0.00} fps", 1000);
            string signalLabelText = string.Format(Languages.ScreenManagerLang.infobar_Signal0, signalText);
            lblSignal.AutoSize = false;
            lblSignal.Width = TextRenderer.MeasureText(signalLabelText, lblSignal.Font).Width;

            string loadText = string.Format(" {0:0.00} %", 100);
            string loadLabelText = string.Format(Languages.ScreenManagerLang.infobar_Load0, loadText);
            lblLoad.AutoSize = false;
            lblLoad.Width = TextRenderer.MeasureText(loadLabelText, lblLoad.Font).Width;

            string dropsText = string.Format(" {0}", 100);
            string dropsLabelText = string.Format(Languages.ScreenManagerLang.infobar_Drops0, dropsText);
            lblDrops.AutoSize = false;
            lblDrops.Width = TextRenderer.MeasureText(dropsLabelText, lblDrops.Font).Width;

            string queueText = string.Format(" {0}", 100);
            string queueLabelText = string.Format("Queue:{0}", queueText);
            lblQueue.AutoSize = false;
            lblQueue.Width = TextRenderer.MeasureText(queueLabelText, lblQueue.Font).Width;

            btnSignal.Left = 5;
            lblSignal.Left = btnSignal.Right + 2;

            btnLoad.Left = lblSignal.Right + 5;
            lblLoad.Left = btnLoad.Right + 2;

            btnDrops.Left = lblLoad.Right + 5;
            lblDrops.Left = btnDrops.Right + 2;

            btnQueue.Left = lblDrops.Right + 5;
            lblQueue.Left = btnQueue.Right + 2;
        }

        public void UpdateLoadStatus(LoadStatus status)
        {
            if (status == this.loadStatus)
                return;

            this.loadStatus = status;
            switch (loadStatus)
            {
                case LoadStatus.Warning:
                    btnLoad.Image = Properties.Resources.load_cloudy;
                    break;
                case LoadStatus.Critical:
                    btnLoad.Image = Properties.Resources.load_rain;
                    break;
                case LoadStatus.OK:
                default:
                    btnLoad.Image = Properties.Resources.load_sun;
                    break;
            }
        }
    }
}
