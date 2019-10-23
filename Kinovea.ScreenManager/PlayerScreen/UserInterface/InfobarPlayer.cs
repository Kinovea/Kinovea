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

        public void UpdateValues(string filename, string size, string fps, bool replayWatcher)
        {
            lblFilename.Text = filename;
            lblSize.Text = size;
            lblFps.Text = fps;

            btnVideoType.BackgroundImage = replayWatcher ? Properties.Resources.replaywatcher : Properties.Resources.film_small;
        }
    }
}
