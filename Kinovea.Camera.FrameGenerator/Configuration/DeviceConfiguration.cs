using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Video;

namespace Kinovea.Camera.FrameGenerator
{
    public class DeviceConfiguration
    {
        public ImageFormat ImageFormat { get; set; } = ImageFormat.RGB24;
        public int Width { get; set; } = 1280;
        public int Height { get; set; } = 720;
        public int Framerate { get; set; } = 60;

        public static DeviceConfiguration Default
        {
            get { return defaultConfiguration; }
        }

        private static DeviceConfiguration defaultConfiguration;

        public DeviceConfiguration()
        {
        }

        public DeviceConfiguration(ImageFormat imageFormat, int width, int height, int framerate)
        {
            this.ImageFormat = imageFormat;
            this.Width = width;
            this.Height = height;
            this.Framerate = framerate;
        }

        static DeviceConfiguration()
        {
            defaultConfiguration = new DeviceConfiguration();
        }
    }
}
