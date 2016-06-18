using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public partial class DelayCompositeMultiReviewControl : UserControl
    {
        public DelayCompositeConfiguration Configuration { get; private set; }

        public DelayCompositeMultiReviewControl(DelayCompositeConfiguration current)
        {
            InitializeComponent();
            lblImageCount.Text = ScreenManagerLang.FormConfigureComposite_ImageCount;

            Configuration = new DelayCompositeConfiguration();
            Configuration.CompositeType = DelayCompositeType.MultiReview;

            if (current.CompositeType == DelayCompositeType.MultiReview)
                Configuration.ImageCount = current.ImageCount;
            else
                Configuration.ImageCount = 4;
            
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
        }

        private void cbImageCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            int option;
            bool parsed = int.TryParse(cbImageCount.Text, out option);
            if (parsed)
                Configuration.ImageCount = option;
        }
    }
}
