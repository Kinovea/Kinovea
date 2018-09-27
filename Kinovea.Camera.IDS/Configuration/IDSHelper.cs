using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Video;

namespace Kinovea.Camera.IDS
{
    public static class IDSHelper
    {
        /// <summary>
        /// Returns the intersection between the camera's supported stream formats and Kinovea supported stream formats.
        /// </summary>
        public static List<IDSEnum> GetSupportedStreamFormats(uEye.Camera camera, long cameraId)
        {
            List<IDSEnum> list = GetKinoveaSupportedStreamFormats();

            // Remove formats not supported by that specific camera.
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!isStreamFormatSupported(camera, list[i].Value))
                    list.RemoveAt(i);
            }

            return list;
        }

        private static List<IDSEnum> GetKinoveaSupportedStreamFormats()
        {
            // Hard coded list of known good formats.
            // TODO: support JPG for the XS model.
            List<IDSEnum> list = new List<IDSEnum>();
            list.Add(new IDSEnum((int)uEye.Defines.ColorMode.Mono8, "Mono 8"));
            list.Add(new IDSEnum((int)uEye.Defines.ColorMode.BGR8Packed, "RGB 24"));
            list.Add(new IDSEnum((int)uEye.Defines.ColorMode.BGRA8Packed, "RGB 32"));
            return list;
        }

        private static bool isStreamFormatSupported(uEye.Camera camera, int colorMode)
        {
            uEye.Defines.ColorConvertMode converterMode = 0;
            uEye.Defines.Status statusRet = camera.Color.Converter.GetSupported((uEye.Defines.ColorMode)colorMode, out converterMode);

            // The value returned in converterMode is a bitfield of supported converters, like Hardware3x3, Software5x5, etc.
            // As long as one converter is supported we should get the correct frame content. (Not sure about that JPEG converter though).
            return statusRet == uEye.Defines.Status.Success && converterMode != uEye.Defines.ColorConvertMode.None;
        }

        public static int ReadCurrentStreamFormat(uEye.Camera camera)
        {
            uEye.Defines.ColorMode currentColorMode;
            camera.PixelFormat.Get(out currentColorMode);
            return (int)currentColorMode;
        }

        public static void WriteStreamFormat(uEye.Camera camera, int format)
        {
            camera.PixelFormat.Set((uEye.Defines.ColorMode)format);
        }

        public static ImageFormat GetImageFormat(uEye.Camera camera)
        {
            uEye.Defines.ColorMode colorMode;
            camera.PixelFormat.Get(out colorMode);
            return GetImageFormat(colorMode);
        }

        public static float GetFramerate(uEye.Camera camera)
        {
            double currentValue;
            camera.Timing.Framerate.Get(out currentValue);
            return (float)currentValue;
        }

        private static ImageFormat GetImageFormat(uEye.Defines.ColorMode colorMode)
        {
            ImageFormat format = ImageFormat.None;

            switch (colorMode)
            {
                case uEye.Defines.ColorMode.BGR8Packed:
                    format = ImageFormat.RGB24;
                    break;
                case uEye.Defines.ColorMode.BGRA8Packed:
                    format = ImageFormat.RGB32;
                    break;
                case uEye.Defines.ColorMode.Mono8:
                    format = ImageFormat.Y800;
                    break;
                case uEye.Defines.ColorMode.RGB8Packed:
                case uEye.Defines.ColorMode.RGBA8Packed:
                default:
                    format = ImageFormat.None;
                    break;
            }

            return format;
        }
    }
}
