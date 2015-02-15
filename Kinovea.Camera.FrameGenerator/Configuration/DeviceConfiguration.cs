using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Video;

namespace Kinovea.Camera.FrameGenerator
{
    public class DeviceConfiguration
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int FrameIntervalMicroseconds { get; set; }
        public ImageFormat ImageFormat { get; set; }

        public static DeviceConfiguration Default
        {
            get { return defaultConfiguration; }
        }

        private static DeviceConfiguration defaultConfiguration;

        public DeviceConfiguration(int width, int height, int frameIntervalMicroseconds, ImageFormat imageFormat)
        {
            this.Width = width;
            this.Height = height;
            this.FrameIntervalMicroseconds = frameIntervalMicroseconds;
            this.ImageFormat = imageFormat;
        }

        static DeviceConfiguration()
        {
            defaultConfiguration = new DeviceConfiguration(1920, 1080, 20000, ImageFormat.RGB24);
        }
    }
}
