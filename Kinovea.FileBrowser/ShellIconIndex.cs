using System;
using System.Runtime.InteropServices;

namespace Kinovea.FileBrowser
{
    public static class ShellIconIndex
    {
        public static int Get(string path, bool open, bool isDrive)
        {
            NativeMethods.SHFILEINFO info = new NativeMethods.SHFILEINFO();

            uint flags = NativeMethods.SHGFI_SYSICONINDEX;

            // For ordinary folders, avoid accessing the filesystem merely
            // to retrieve the generic folder icon.
            if (!isDrive)
            {
                flags |= NativeMethods.SHGFI_USEFILEATTRIBUTES;
            }

            if (open)
            {
                flags |= NativeMethods.SHGFI_OPENICON;
            }

            IntPtr result = NativeMethods.SHGetFileInfo(
                path,
                NativeMethods.FILE_ATTRIBUTE_DIRECTORY,
                ref info,
                (uint)Marshal.SizeOf(typeof(NativeMethods.SHFILEINFO)),
                flags);

            return result == IntPtr.Zero ? 0 : info.iIcon;
        }
    }
}