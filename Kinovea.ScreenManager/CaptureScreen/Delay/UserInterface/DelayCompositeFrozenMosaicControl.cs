using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class DelayCompositeFrozenMosaicControl : UserControl
    {
        public DelayCompositeConfiguration Configuration { get; private set; }

        public DelayCompositeFrozenMosaicControl(DelayCompositeConfiguration current)
        {
            InitializeComponent();

            Configuration = new DelayCompositeConfiguration();
            Configuration.CompositeType = DelayCompositeType.FrozenMosaic;

            if (current.CompositeType == DelayCompositeType.FrozenMosaic)
            {
                Configuration.ImageCount = current.ImageCount;
                Configuration.RefreshRate = current.RefreshRate;
                Configuration.Start = current.Start;
                Configuration.Interval = current.Interval;
            }
            else
            {
                Configuration.ImageCount = 16;
                Configuration.RefreshRate = 0.5f;
                Configuration.Start = 0;
                Configuration.Interval = 3;
            }
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            List<int> options = new List<int> { 4, 9, 16, 25 };
            foreach (int option in options)
            {
                cbImageCount.Items.Add(option.ToString());
                if (option == Configuration.ImageCount)
                    cbImageCount.SelectedIndex = cbImageCount.Items.Count - 1;
            }

            tbRefreshRate.Text = Configuration.RefreshRate.ToString();
            tbStart.Text = Configuration.Start.ToString();
            tbInterval.Text = Configuration.Interval.ToString();
        }

        private void cbImageCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            int option;
            bool parsed = int.TryParse(cbImageCount.Text, out option);
            if (parsed)
                Configuration.ImageCount = option;
        }

        private void tbRefreshRate_TextChanged(object sender, EventArgs e)
        {
            float refreshRate;
            bool parsed = float.TryParse(tbRefreshRate.Text, out refreshRate);
            if (parsed && refreshRate > 0)
                Configuration.RefreshRate = refreshRate;
        }

        private void tbStart_TextChanged(object sender, EventArgs e)
        {
            int start;
            bool parsed = int.TryParse(tbStart.Text, out start);
            if (parsed && start > 0)
                Configuration.Start = start;
        }

        private void tbInterval_TextChanged(object sender, EventArgs e)
        {
            int interval;
            bool parsed = int.TryParse(tbInterval.Text, out interval);
            if (parsed && interval > 0)
                Configuration.Interval = interval;
        }
    }
}
