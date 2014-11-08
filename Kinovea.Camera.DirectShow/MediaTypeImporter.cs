using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video.DirectShow;

namespace Kinovea.Camera.DirectShow
{
    public static class MediaTypeImporter
    {
        /// <summary>
        /// Import the capabilities published by the device and convert them to our local representation.
        /// </summary>
        public static List<MediaType> Import(VideoCaptureDevice device)
        {
            List<MediaType> result = new List<MediaType>();

            VideoCapabilities[] capabilities = device.VideoCapabilities;
            if (capabilities.Length == 0)
                return result;

            foreach (VideoCapabilities cap in capabilities)
            {
                MediaType mt = new MediaType(cap.Compression, cap.FrameSize, cap.AverageFrameRate, cap.Index, cap.BitCount, null);
                result.Add(mt);
            }

            return result;
        }

        public static List<double> GetSupportedFramerates(VideoCaptureDevice device, MediaType mediaType)
        {
            //VideoCapabilities[] capabilities = device.VideoCapabilities;
            //VideoCapabilities match = capabilities.FirstOrDefault(c => c.Index == mediaType.MediaTypeIndex);

            List<double> result = new List<double>();

            result.Add(mediaType.SelectedFramerate);
            
            return result;
        }
    }
}
