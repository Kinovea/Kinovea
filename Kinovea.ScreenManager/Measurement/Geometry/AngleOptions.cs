using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public class AngleOptions
    {
        public bool Signed
        {
            get { return signed; }
        }
        public bool CCW
        {
            get { return ccw; }
        }
        public bool Supplementary
        {
            get { return supplementary; }
        }

        private bool signed;
        private bool ccw;
        private bool supplementary;

        public AngleOptions(bool signed, bool ccw, bool supplementary)
        {
            this.signed = signed;
            this.ccw = ccw;
            this.supplementary = supplementary;
        }
    }
}
