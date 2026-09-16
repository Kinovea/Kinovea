using System;

namespace Kinovea.Services
{
    public static class PlatformHelper
    {
        public static bool IsWindows { get; } = Environment.OSVersion.Platform == PlatformID.Win32NT;

        public static bool IsWine { get; } = DetectWine();

        public static bool IsNativeWindows => IsWindows && !IsWine;

        private static bool DetectWine()
        {
            if (!IsWindows)
                return false;

            try
            {
                IntPtr ntdll = NativeMethods.GetModuleHandle("ntdll.dll");

                if (ntdll == IntPtr.Zero)
                {
                    // Treat unknown Windows-compatible environment as Wine.
                    return true;
                }

                IntPtr wineGetVersion = NativeMethods.GetProcAddress(ntdll, "wine_get_version");
                return wineGetVersion != IntPtr.Zero;
            }
            catch (DllNotFoundException)
            {
                return true;
            }
            catch (EntryPointNotFoundException)
            {
                return true;
            }
        }
    }
}