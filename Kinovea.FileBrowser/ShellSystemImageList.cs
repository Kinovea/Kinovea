using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Kinovea.FileBrowser
{
    public static class ShellSystemImageList
    {
        private static IntPtr smallImageList;

        public static void Attach(TreeView treeView)
        {
            treeView.HandleCreated -= TreeView_HandleCreated;
            treeView.HandleCreated += TreeView_HandleCreated;

            if (treeView.IsHandleCreated)
            {
                Apply(treeView);
            }
        }

        private static void TreeView_HandleCreated(object sender, EventArgs e)
        {
            Apply((TreeView)sender);
        }

        private static void Apply(TreeView treeView)
        {
            if (treeView.IsDisposed)
                return;

            IntPtr imageList = GetSmallImageList();

            // Failing to retrieve icons should not prevent the tree from working.
            if (imageList == IntPtr.Zero)
                return;

            // Set the tree view image list to the system image list.
            NativeMethods.SendMessage(
                treeView.Handle, 
                NativeMethods.TVM_SETIMAGELIST, 
                new IntPtr(NativeMethods.TVSIL_NORMAL), 
                imageList);
        }

        private static IntPtr GetSmallImageList()
        {
            if (smallImageList != IntPtr.Zero)
                return smallImageList;

            NativeMethods.SHFILEINFO info = new NativeMethods.SHFILEINFO();

            // Get the system image list for small icons.
            // SHGFI_SYSICONINDEX: retrieve the system image list index.
            // SHGFI_USEFILEATTRIBUTES: the path is not actually accessed so we can use a dummy file name.
            // the return value is the handle to Windows shared system image list with all 
            // the standard shell icons.
            // The returned handle is owned by the system and should not be destroyed.
            smallImageList = NativeMethods.SHGetFileInfo(
                "dummy.txt",
                NativeMethods.FILE_ATTRIBUTE_NORMAL,
                ref info,
                (uint)Marshal.SizeOf(typeof(NativeMethods.SHFILEINFO)),
                NativeMethods.SHGFI_SYSICONINDEX |
                NativeMethods.SHGFI_SMALLICON |
                NativeMethods.SHGFI_USEFILEATTRIBUTES);

            return smallImageList;
        }
    }
}