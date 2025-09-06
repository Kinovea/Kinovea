using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Kinovea.Services
{
    public static class NativeMethods
    {
        public const int SW_RESTORE = 9;
        public static uint WM_COPYDATA = 0x004A;

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern int memcpy(void* dest, void* src, int count);

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("USER32.dll")]
        public static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("USER32.dll")]
        public static extern bool IsIconic(IntPtr handle);

        [DllImport("USER32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("USER32.DLL", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;

            /// <summary>
            /// Only dispose COPYDATASTRUCT if you were the one who allocated it
            /// </summary>
            public void Dispose()
            {
                if (lpData != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(lpData);
                    lpData = IntPtr.Zero;
                    cbData = 0;
                }
            }

            public string AsAnsiString { get { return Marshal.PtrToStringAnsi(lpData, cbData); } }
            public string AsUnicodeString { get { return Marshal.PtrToStringUni(lpData); } }
            public static COPYDATASTRUCT CreateForString(int dwData, string value, bool unicode = false)
            {
                var result = new COPYDATASTRUCT();
                result.dwData = (IntPtr)dwData;
                result.lpData = unicode ? Marshal.StringToCoTaskMemUni(value) : Marshal.StringToCoTaskMemAnsi(value);
                result.cbData = value.Length + 1;
                return result;
            }
        }

    }
}
