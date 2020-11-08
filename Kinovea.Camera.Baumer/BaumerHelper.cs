using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BGAPI2;
using Kinovea.Video;

namespace Kinovea.Camera.Baumer
{
    public static class BaumerHelper
    {
        public static float GetResultingFramerate(Device device)
        {
            if (device == null || !device.IsOpen)
                return 0;

            //if (BaumerHelper.NodeIsReadable(device, "ResultingFrameRateAbs"))
            //    return device.RemoteNodeList["ResultingFrameRateAbs"].Value;
            //else if (BaumerHelper.NodeIsReadable(device, "ResultingFrameRate"))
            //    return device.RemoteNodeList["ResultingFrameRate"].Value;

            //if (BaumerHelper.NodeIsReadable(device, "AcquisitionFrameRate"))

            // CurrentAcquisitionFrameRate

            return 0;
        }

        public static Node GetNode(NodeMap map, string name)
        {
            bool present = map.GetNodePresent(name);
            if (!present)
                return null;

            Node node = map[name];
            if (!node.IsImplemented || !node.IsAvailable)
                return null;

            return node;
        }

        public static bool NodeIsReadable(Device device, string name)
        {
            if (device == null || !device.IsOpen)
                throw new InvalidProgramException();

            bool present = device.RemoteNodeList.GetNodePresent(name);
            if (!present)
                return false;

            Node node = device.RemoteNodeList[name];
            if (!node.IsImplemented || !node.IsAvailable)
                return false;

            return node.IsReadable;
        }

        public static int GetInteger(Device device, string name)
        {
            return (int)device.RemoteNodeList[name].Value;
        }

        public static string GetString(Device device, string name)
        {
            return (string)device.RemoteNodeList[name].Value;
        }

        /// <summary>
        /// Takes a Baumer pixel format and determines the output image format.
        /// </summary>
        public static ImageFormat ConvertImageFormat(string pixelFormat, bool demosaicing)
        {
            if (pixelFormat.StartsWith("Bayer"))
            {
                if (!demosaicing)
                    return ImageFormat.Y800;
                else
                    return ImageFormat.RGB24;
            }
            else if (pixelFormat.StartsWith("Mono"))
            {
                return ImageFormat.Y800;
            }
            else
            {
                return ImageFormat.RGB24;
            }
        }

        /// <summary>
        /// Return true if the input buffer format is already grayscale 8-bit per pixel.
        /// This means it can directly be put into the Y800 output frame witohut conversion.
        /// </summary>
        public static bool IsY800(string pixelFormat)
        {
            return pixelFormat == "Mono8" ||
                pixelFormat == "BayerBG8" ||
                pixelFormat == "BayerGB8" ||
                pixelFormat == "BayerGR8" ||
                pixelFormat == "BayerRG8";
        }

        public static bool IsBayer(string pixelFormat)
        {
            return pixelFormat.StartsWith("Bayer");
        }

        public static void WriteEnum(Device device, string enumName, string enumValue)
        {
            if (device == null || !device.IsOpen)
                throw new InvalidProgramException();

            bool present = device.RemoteNodeList.GetNodePresent(enumName);
            if (!present)
                return;

            Node node = device.RemoteNodeList[enumName];
            if (!node.IsImplemented || !node.IsAvailable)
                return;

            node.Value = enumValue;
        }
    }
}
