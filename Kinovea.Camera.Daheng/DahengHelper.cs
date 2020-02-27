using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GxIAPINET;
using Kinovea.Video;

namespace Kinovea.Camera.Daheng
{
    public static class DahengHelper
    {
        private const uint PIXEL_FORMATE_BIT = 0x00FF0000;                    ///<For the current data format and operation to get the current data bits
        private const uint GX_PIXEL_8BIT = 0x00080000;                        ///<8 bit data image format

        /// <summary>
        /// Get the best 8 bit by GX_PIXEL_FORMAT_ENTRY
        /// </summary>
        /// <param name="em">image format</param>
        /// <returns>best bit bit</returns>
        public static GX_VALID_BIT_LIST GetBestValidBit(GX_PIXEL_FORMAT_ENTRY emPixelFormatEntry)
        {
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;
            switch (emPixelFormatEntry)
            {
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG8:
                    {
                        emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG10:
                    {
                        emValidBits = GX_VALID_BIT_LIST.GX_BIT_2_9;
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG12:
                    {
                        emValidBits = GX_VALID_BIT_LIST.GX_BIT_4_11;
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO14:
                    {
                        //There is no such data format to be upgraded
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG16:
                    {
                        //There is no such data format to be upgraded
                        break;
                    }
                default:
                    break;
            }
            return emValidBits;
        }

        /// <summary>
        /// Check whether the PixelFormat is 8 bits
        /// </summary>
        /// <param name="emPixelFormatEntry">image format</param>
        /// <returns>true:  8 bit  false: not 8 bit</returns>
        public static bool IsPixelFormat8(GX_PIXEL_FORMAT_ENTRY emPixelFormatEntry)
        {
            bool bIsPixelFormat8 = false;
            uint uiPixelFormatEntry = (uint)emPixelFormatEntry;
            if ((uiPixelFormatEntry & PIXEL_FORMATE_BIT) == GX_PIXEL_8BIT)
            {
                bIsPixelFormat8 = true;
            }
            return bIsPixelFormat8;
        }

        public static bool IsColor(IGXFeatureControl featureControl)
        {
            bool result = false;
            if (featureControl.IsImplemented("PixelColorFilter"))
            {
                string pixelColorFilter = featureControl.GetEnumFeature("PixelColorFilter").GetValue();
                if (pixelColorFilter != "None")
                    result = true;
            }

            return result;
        }

        public static List<DahengStreamFormat> GetSupportedStreamFormats(IGXFeatureControl featureControl)
        {
            List<DahengStreamFormat> list = new List<DahengStreamFormat>();
            if (IsColor(featureControl))
            {
                list.Add(DahengStreamFormat.RGB);
                list.Add(DahengStreamFormat.Raw);
            }
            else
            {
                list.Add(DahengStreamFormat.Mono);
            }

            return list;
        }

        public static ImageFormat ConvertImageFormat(DahengStreamFormat format)
        {
            switch (format)
            {
                case DahengStreamFormat.RGB:
                    return ImageFormat.RGB24;
                case DahengStreamFormat.Mono:
                case DahengStreamFormat.Raw:
                default:
                    return ImageFormat.Y800;
            }
        }

        /// <summary>
        /// Make sure the feature is triggered at least once, 
        /// either it's currently in continuous mode or we trigger it manually.
        /// </summary>
        private static void ContinuousOrOnce(IGXFeatureControl featureControl, string identifier)
        {
            if (featureControl == null)
                return;

            bool implemented = featureControl.IsImplemented(identifier);
            bool readable = featureControl.IsReadable(identifier);
            bool writeable = featureControl.IsWritable(identifier);

            if (implemented && readable && writeable)
            {
                string currentValue = featureControl.GetEnumFeature(identifier).GetValue().ToString();
                if (currentValue == "Off")
                    featureControl.GetEnumFeature(identifier).SetValue("Once");
            }
        }

        public static void AfterOpen(IGXFeatureControl featureControl)
        {
            if (featureControl == null)
                return;

            featureControl.GetEnumFeature("AcquisitionMode").SetValue("Continuous");
            featureControl.GetEnumFeature("TriggerMode").SetValue("Off");

            // Force white balance at least once.
            ContinuousOrOnce(featureControl, "BalanceWhiteAuto");

            // This will allow the camera to send the max bandwidth it can, possibly saturating the link.
            if (featureControl.IsImplemented("DeviceLinkThroughputLimitMode") && featureControl.IsWritable("DeviceLinkThroughputLimitMode"))
                featureControl.GetEnumFeature("DeviceLinkThroughputLimitMode").SetValue("Off");

            // Make sure the user's custom framerate is respected.
            if (featureControl.IsImplemented("AcquisitionFrameRateMode") && featureControl.IsWritable("AcquisitionFrameRateMode"))
                featureControl.GetEnumFeature("AcquisitionFrameRateMode").SetValue("On");
        }

        public static double GetResultingFramerate(IGXDevice device)
        {
            if (device == null)
                return 0;

            IGXFeatureControl featureControl = device.GetRemoteFeatureControl();
            if (featureControl == null)
                return 0;

            string identifier = "CurrentAcquisitionFrameRate";
            bool implemented = featureControl.IsImplemented(identifier);
            bool readable = featureControl.IsReadable(identifier);

            if (!implemented || !readable)
                return 0;

            return featureControl.GetFloatFeature(identifier).GetValue();
        }
    }
}
