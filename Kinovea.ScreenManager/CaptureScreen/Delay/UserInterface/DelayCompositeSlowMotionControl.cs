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
    public partial class DelayCompositeSlowMotionControl : UserControl
    {
        public DelayCompositeConfiguration Configuration { get; private set; }

        public DelayCompositeSlowMotionControl(DelayCompositeConfiguration current)
        {
            InitializeComponent();

            Configuration = new DelayCompositeConfiguration();
            Configuration.CompositeType = DelayCompositeType.SlowMotion;

            if (current.CompositeType == DelayCompositeType.SlowMotion)
            {
                Configuration.ImageCount = current.ImageCount;
                Configuration.RefreshRate = current.RefreshRate;
            }
            else
            {
                Configuration.ImageCount = 4;
                Configuration.RefreshRate = 0.5f;
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
        }

        private void cbImageCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            int imageCount;
            bool parsed = int.TryParse(cbImageCount.Text, out imageCount);
            if (parsed)
                Configuration.ImageCount = imageCount;
        }

        private void tbRefreshRate_TextChanged(object sender, EventArgs e)
        {
            float refreshRate;
            bool parsed = float.TryParse(tbRefreshRate.Text, out refreshRate);
            if (parsed && refreshRate > 0)
                Configuration.RefreshRate = refreshRate;
        }
    }
}
