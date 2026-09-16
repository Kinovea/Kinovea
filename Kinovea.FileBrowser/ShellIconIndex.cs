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

        public static int GetStockIconIndex(NativeMethods.StockIconId stockIconId, int fallbackIndex)
        {
            try
            {
                NativeMethods.SHSTOCKICONINFO info = new NativeMethods.SHSTOCKICONINFO
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SHSTOCKICONINFO))
                };

                var flags = NativeMethods.StockIconFlags.SysIconIndex | NativeMethods.StockIconFlags.SmallIcon;
                int result = NativeMethods.SHGetStockIconInfo(stockIconId, flags, ref info);

                // SHGetStockIconInfo returns an HRESULT.
                return result >= 0 ? info.iSysImageIndex : fallbackIndex;
            }
            catch (DllNotFoundException)
            {
                return fallbackIndex;
            }
            catch (EntryPointNotFoundException)
            {
                return fallbackIndex;
            }
        }
    }
}