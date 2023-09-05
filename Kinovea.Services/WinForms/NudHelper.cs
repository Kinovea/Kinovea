using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public static class NudHelper
    {
        /// <summary>
        /// Fixes the mouse scroll behavior of NUDs so that any amount of mouse scroll produces one increment/decrement.
        /// The normal NUD behavior changes the value too much on each scroll notch which makes it impossible to be precise.
        /// </summary>
        public static void FixNudScroll(NumericUpDown nud)
        {
            nud.MouseWheel += Nud_Scroll;
        }

        /// <summary>
        /// Catch the mouse scroll event and override the amount of value change.
        /// </summary>
        private static void Nud_Scroll(object sender, MouseEventArgs e)
        {
            NumericUpDown nud = sender as NumericUpDown;
            HandledMouseEventArgs handledArgs = e as HandledMouseEventArgs;
            handledArgs.Handled = true;

            decimal delta = handledArgs.Delta > 0 ? nud.Increment : -nud.Increment;
            nud.Value = Math.Max(Math.Min(nud.Value + delta, nud.Maximum), nud.Minimum);
        }
    }
}
