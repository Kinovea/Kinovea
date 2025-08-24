using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Root
{
    public static class NativeMethods
    {
        public static uint WM_COPYDATA = 0x004A;

        /// <summary>
        /// Contains data to be passed to another application by the WM_COPYDATA message.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            /// <summary>
            /// User defined data to be passed to the receiving application.
            /// </summary>
            public IntPtr dwData;

            /// <summary>
            /// The size, in bytes, of the data pointed to by the lpData member.
            /// </summary>
            public int cbData;

            /// <summary>
            /// The data to be passed to the receiving application. This member can be IntPtr.Zero.
            /// </summary>
            public IntPtr lpData;
        }

        /// <summary>
        /// Values used in the struct CHANGEFILTERSTRUCT
        /// </summary>
        public enum MessageFilterInfo : uint
        {
            /// <summary>
            /// Certain messages whose value is smaller than WM_USER are required to pass 
            /// through the filter, regardless of the filter setting. 
            /// There will be no effect when you attempt to use this function to 
            /// allow or block such messages.
            /// </summary>
            None = 0,

            /// <summary>
            /// The message has already been allowed by this window's message filter, 
            /// and the function thus succeeded with no change to the window's message filter. 
            /// Applies to MSGFLT_ALLOW.
            /// </summary>
            AlreadyAllowed = 1,

            /// <summary>
            /// The message has already been blocked by this window's message filter, 
            /// and the function thus succeeded with no change to the window's message filter. 
            /// Applies to MSGFLT_DISALLOW.
            /// </summary>
            AlreadyDisAllowed = 2,

            /// <summary>
            /// The message is allowed at a scope higher than the window.
            /// Applies to MSGFLT_DISALLOW.
            /// </summary>
            AllowedHigher = 3
        }

        /// <summary>
        /// Values used by ChangeWindowMessageFilterEx
        /// </summary>
        public enum ChangeWindowMessageFilterExAction : uint
        {
            /// <summary>
            /// Resets the window message filter for hWnd to the default.
            /// Any message allowed globally or process-wide will get through,
            /// but any message not included in those two categories,
            /// and which comes from a lower privileged process, will be blocked.
            /// </summary>
            Reset = 0,

            /// <summary>
            /// Allows the message through the filter. 
            /// This enables the message to be received by hWnd, 
            /// regardless of the source of the message, 
            /// even it comes from a lower privileged process.
            /// </summary>
            Allow = 1,

            /// <summary>
            /// Blocks the message to be delivered to hWnd if it comes from
            /// a lower privileged process, unless the message is allowed process-wide 
            /// by using the ChangeWindowMessageFilter function or globally.
            /// </summary>
            DisAllow = 2
        }

        /// <summary>
        /// Contains extended result information obtained by calling 
        /// the ChangeWindowMessageFilterEx function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CHANGEFILTERSTRUCT
        {
            /// <summary>
            /// The size of the structure, in bytes. Must be set to sizeof(CHANGEFILTERSTRUCT), 
            /// otherwise the function fails with ERROR_INVALID_PARAMETER.
            /// </summary>
            public uint size;

            /// <summary>
            /// If the function succeeds, this field contains one of the following values, 
            /// <see cref="MessageFilterInfo"/>
            /// </summary>
            public MessageFilterInfo info;
        }

        /// <summary>
        /// Modifies the User Interface Privilege Isolation (UIPI) message filter for a specified window
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window whose UIPI message filter is to be modified.</param>
        /// <param name="msg">The message that the message filter allows through or blocks.</param>
        /// <param name="action">The action to be performed, and can take one of the following values
        /// <see cref="MessageFilterInfo"/></param>
        /// <param name="changeInfo">Optional pointer to a 
        /// <see cref="CHANGEFILTERSTRUCT"/> structure.</param>
        /// <returns>If the function succeeds, it returns TRUE; otherwise, it returns FALSE. 
        /// To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint msg,
        ChangeWindowMessageFilterExAction action, ref CHANGEFILTERSTRUCT changeInfo);


        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);
    }
}
