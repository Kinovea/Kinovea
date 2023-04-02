using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public class PanelDoubleBuffer : Panel
    {
        public PanelDoubleBuffer()
        {
            this.DoubleBuffered = true;
        }
    }
}
