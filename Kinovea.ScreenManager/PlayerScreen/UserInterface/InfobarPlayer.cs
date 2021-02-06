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
    public partial class InfobarPlayer : UserControl
    {
        public InfobarPlayer()
        {
            InitializeComponent();
        }

        public void UpdateValues(string filename, string size, string fps)
        {
            lblFilename.Text = filename;
            lblSize.Text = size;
            lblFps.Text = fps;
        }

        public void UpdateReplayWatcher(bool replayWatcher, string path)
        {
            btnVideoType.BackgroundImage = replayWatcher ? Properties.Resources.replaywatcher : Properties.Resources.film_small;
            toolTips.SetToolTip(btnVideoType, path);
        }
    }
}
