#region License
/*
Copyright © Joan Charmant 2013.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Kinovea.ScreenManager
{
    public static class UIHelper
    {
        /// <summary>
        /// Returns the active rectangle that the image should be drawn onto in order to completely fill the container.
        /// </summary>
        public static Rectangle RatioStretch(Size imageSize, Size containerSize)
        {
            float ratioWidth = (float)imageSize.Width / containerSize.Width;
            float ratioHeight = (float)imageSize.Height / containerSize.Height;
            float ratio = Math.Max(ratioWidth, ratioHeight);

            int width = (int)(imageSize.Width / ratio);
            int height = (int)(imageSize.Height / ratio);
            int left = (containerSize.Width - width)/2;
            int top = (containerSize.Height - height)/2;
            
            return new Rectangle(left, top, width, height);
        }

        public static double GetMonitorFramerate(IntPtr handle)
        {
            // Based on https://github.com/rickbrew/RefreshRateWpf/blob/master/RefreshRateWpfApp/MainWindow.xaml.cs
            double defaultFramerate = 60;

            IntPtr hmonitor = NativeMethods.MonitorFromWindow(handle, NativeMethods.MONITOR_DEFAULTTONEAREST);
            if (hmonitor == IntPtr.Zero)
                return defaultFramerate;

            // Get more info about the monitor.
            NativeMethods.MONITORINFOEXW monitorInfo = new NativeMethods.MONITORINFOEXW();
            monitorInfo.cbSize = (uint)Marshal.SizeOf<NativeMethods.MONITORINFOEXW>();
            bool result = NativeMethods.GetMonitorInfoW(hmonitor, ref monitorInfo);
            if (!result)
                return defaultFramerate;

            // Get the current display settings for that monitor.
            NativeMethods.DEVMODEW devMode = new NativeMethods.DEVMODEW();
            devMode.dmSize = (ushort)Marshal.SizeOf<NativeMethods.DEVMODEW>();
            result = NativeMethods.EnumDisplaySettingsW(monitorInfo.szDevice, NativeMethods.ENUM_CURRENT_SETTINGS, out devMode);
            if (!result)
                return defaultFramerate;

            return (double)devMode.dmDisplayFrequency;
        }
    }
}
