using Kinovea.Services;
using System;
using System.Windows.Forms;

namespace Kinovea.FileBrowser
{
    public class BufferedTreeView : TreeView
    {
        public BufferedTreeView()
        {
            DoubleBuffered = true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!PlatformHelper.IsNativeWindows)
                return;

            if (!Application.RenderWithVisualStyles)
                return;

            try
            {
                NativeMethods.SetWindowTheme(Handle, "Explorer", null);
            }
            catch (DllNotFoundException)
            {
                // Continue with the normal TreeView appearance.
            }
            catch (EntryPointNotFoundException)
            {
                // Continue with the normal TreeView appearance.
            }
        }
    }
}
