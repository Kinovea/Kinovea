using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video.DirectShow;

namespace Kinovea.Camera.DirectShow
{
    /// <summary>
    /// Reads and writes a list of supported camera properties from/to the device.
    /// Used for IAMCameraControl and IAMProcAmp type of properties.
    /// </summary>
    public static class CameraPropertyManager
    {
        public static Dictionary<string, CameraProperty> Read(VideoCaptureDevice device)
        {
            Dictionary<string, CameraProperty> properties = new Dictionary<string, CameraProperty>();

            if (device.Logitech_SupportExposureProperty())
                properties.Add("exposure_logitech", ReadLogitechProperty(device));
            else
                properties.Add("exposure", ReadProperty(device, CameraControlProperty.Exposure));

            properties.Add("gain", ReadProperty(device, VideoProcAmpProperty.Gain));
            properties.Add("focus", ReadProperty(device, CameraControlProperty.Focus));
            
            return properties;
        }

        public static void Write(VideoCaptureDevice device, Dictionary<string, CameraProperty> properties)
        {
            if (properties.ContainsKey("exposure_logitech"))
                WriteLogitechProperty(device, properties["exposure_logitech"]);

            if (properties.ContainsKey("exposure"))
                WriteProperty(device, CameraControlProperty.Exposure, properties["exposure"]);

            if (properties.ContainsKey("gain"))
                WriteProperty(device, VideoProcAmpProperty.Gain, properties["gain"]);

            if (properties.ContainsKey("focus"))
                WriteProperty(device, CameraControlProperty.Focus, properties["focus"]);
        }

        private static CameraProperty ReadProperty(VideoCaptureDevice device, CameraControlProperty property)
        {
            CameraProperty p = new CameraProperty();
            p.Type = CameraPropertyType.CameraControl;

            try
            {
                int min;
                int max;
                int step;
                int defaultValue;
                CameraControlFlags flags;
                bool success = device.GetCameraPropertyRange(property, out min, out max, out step, out defaultValue, out flags);

                if (!success)
                {
                    p.Supported = false;
                }
                else
                {
                    p.Supported = true;
                    p.Minimum = min;
                    p.Maximum = max;

                    int currentValue;
                    success = device.GetCameraProperty(property, out currentValue, out flags);

                    if (!success)
                    {
                        p.Supported = false;
                    }
                    else
                    {
                        p.Value = currentValue;
                        p.Automatic = flags == CameraControlFlags.Auto;
                    }
                }
            }
            catch
            {
                p.Supported = false;
            }

            return p;
        }

        private static CameraProperty ReadProperty(VideoCaptureDevice device, VideoProcAmpProperty property)
        {
            CameraProperty p = new CameraProperty();
            p.Type = CameraPropertyType.VideoProcAmp;

            try
            {
                int min;
                int max;
                int step;
                int defaultValue;
                VideoProcAmpFlags flags;
                bool success = device.GetVideoPropertyRange(property, out min, out max, out step, out defaultValue, out flags);

                if (!success)
                {
                    p.Supported = false;
                }
                else
                {
                    p.Supported = true;
                    p.Minimum = min;
                    p.Maximum = max;

                    int currentValue;
                    success = device.GetVideoProperty(property, out currentValue, out flags);

                    if (!success)
                    {
                        p.Supported = false;
                    }
                    else
                    {
                        p.Value = currentValue;
                        p.Automatic = flags == VideoProcAmpFlags.Auto;
                    }
                }
            }
            catch
            {
                p.Supported = false;
            }

            return p;
        }

        private static CameraProperty ReadLogitechProperty(VideoCaptureDevice device)
        {
            // Hardcoded values for min/max according to C920.
            CameraProperty p = new CameraProperty();
            p.Type = CameraPropertyType.Logitech;
            p.Supported = true;

            p.Minimum = 1;
            p.Maximum = 500;
            
            int currentValue;
            bool manual;
            bool success = device.Logitech_GetExposure(out currentValue, out manual);

            if (!success)
            {
                p.Supported = false;
            }
            else
            {
                p.Value = Math.Min(p.Maximum, Math.Max(p.Minimum, currentValue));
                p.Automatic = !manual;
            }

            return p;
        }

        private static void WriteProperty(VideoCaptureDevice device, CameraControlProperty property, CameraProperty value)
        {
            CameraControlFlags flags = value.Automatic ? CameraControlFlags.Auto : CameraControlFlags.Manual;
            device.SetCameraProperty(property, value.Value, flags);
        }

        private static void WriteProperty(VideoCaptureDevice device, VideoProcAmpProperty property, CameraProperty value)
        {
            VideoProcAmpFlags flags = value.Automatic ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual;
            device.SetVideoProperty(property, value.Value, flags);
        }

        private static void WriteLogitechProperty(VideoCaptureDevice device, CameraProperty value)
        {
            device.Logitech_SetExposure(value.Value, !value.Automatic);
        }
    }
}
