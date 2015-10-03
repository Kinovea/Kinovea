using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Kinovea.ScreenManager
{
    internal static class NativeMethods
    {
        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern uint timeSetEvent(UInt32 msDelay, UInt32 msResolution, TimerCallback handler, UIntPtr dwUser, UInt32 eventType);

        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern uint timeKillEvent(uint timerEventId);

        internal const int TIME_PERIODIC = 0x01;
        internal const int TIME_KILL_SYNCHRONOUS = 0x0100;

        internal delegate void TimerCallback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2);
    }
}
