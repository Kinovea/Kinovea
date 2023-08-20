using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    public class OpenPoseFrame
    {
        public float version { get; set; }
        public IList<OpenPosePerson> people { get; set; }
    }
}
