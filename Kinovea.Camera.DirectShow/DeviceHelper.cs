using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video.DirectShow;
using System.Threading;

namespace Kinovea.Camera.DirectShow
{
    public static class DeviceHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void StopDevice(VideoCaptureDevice device)
        {
            device.SignalToStop();
            device.WaitForStop();

            // Sometimes the thread just won't die. 
            // This may happen when the thread is locked on the camera handle and there is some issue at a lower level.
            // This not only prevent usage of the camera, it also make the whole application enter a comatose state needing a reboot.
            int maxAttempts = 10;
            int attempts = 0;

            while (device.IsRunning && attempts < maxAttempts)
                Thread.Sleep(50);

            if (device.IsRunning)
            {
                log.ErrorFormat("Aborting device thread.");
                device.Stop();
            }
        }
    }
}
