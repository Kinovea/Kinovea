using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class NativeTreeView : TreeView
{
    public NativeTreeView()
    {
        DoubleBuffered = true;
    }

    protected override void CreateHandle()
    {
        base.CreateHandle();
        Kinovea.ExpTreeLib2.NativeMethods.SetWindowTheme(this.Handle, "Explorer", null);
    }
}
