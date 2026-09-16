using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.FileBrowser
{
    public static class NativeMethods
    {
        
        public static uint SHGFI_SMALLICON          = 0x000000001;
        public static uint SHGFI_OPENICON           = 0x000000002;
        public static uint SHGFI_SYSICONINDEX       = 0x000004000;
        public static uint SHGFI_USEFILEATTRIBUTES  = 0x000000010;

        public static uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        public static uint FILE_ATTRIBUTE_NORMAL    = 0x00000080;

        public static int TV_FIRST = 0x1100;
        public static int TVM_SETIMAGELIST = TV_FIRST + 9;
        public static int TVSIL_NORMAL = 0;

        /// <summary>
        /// information about a file object.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        public enum StockIconId
        {
            Folder = 3,
            FolderOpen = 4,

            DriveRemovable = 7,
            DriveFixed = 8,
            DriveNetwork = 9,
            DriveNetworkDisconnected = 10,
            DriveCd = 11,
            DriveRam = 12,

            DriveUnknown = 58,
            DesktopPc = 94
        }

        [Flags]
        public enum StockIconFlags : uint
        {
            SmallIcon = 0x00000001,
            SysIconIndex = 0x00004000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            public uint cbSize;
            public IntPtr hIcon;
            public int iSysImageIndex;
            public int iIcon;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }

        /// <summary>
        /// Retrieves information about an object in the file system, such as a file, folder, directory, or drive root.
        /// </summary>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint flags);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern int SHGetStockIconInfo(StockIconId stockIconId, StockIconFlags flags, ref SHSTOCKICONINFO info);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int SetWindowTheme(IntPtr window, string subApplicationName, string subIdList);








    }
}
