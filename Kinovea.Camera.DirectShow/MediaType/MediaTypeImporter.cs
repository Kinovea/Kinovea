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
        public static Dictionary<int, MediaType> Import(VideoCaptureDevice device)
        {
            Dictionary<int, MediaType> result = new Dictionary<int, MediaType>();

            VideoCapabilities[] capabilities = device.VideoCapabilities;
            if (capabilities.Length == 0)
                return result;

            foreach (VideoCapabilities cap in capabilities)
            {
                MediaType mt = new MediaType(cap.Compression, cap.FrameSize, cap.Index, cap.BitCount);
                result.Add(mt.MediaTypeIndex, mt);
            }

            return result;
        }

        public static Dictionary<int, List<float>> GetSupportedFramerates(VideoCaptureDevice device)
        {
            Dictionary<int, List<float>> lists = new Dictionary<int, List<float>>();

            VideoCapabilities[] capabilities = device.VideoCapabilities;
            if (capabilities.Length == 0)
                return lists;

            foreach (VideoCapabilities cap in capabilities)
                lists.Add(cap.Index, cap.FrameRateList);
            
            return lists;
        }
    }
}
