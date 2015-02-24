using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.DirectShow
{
    public static class VendorHelper
    {
        private static Dictionary<string, IPropertyValueMapper> mappers = new Dictionary<string, IPropertyValueMapper>();

        static VendorHelper()
        {
            mappers.Add("default", new PropertyValueMapperDefault());
        }

        public static void IdentifyModel(string identifier)
        {
            if (mappers.ContainsKey(identifier))
                return;

            if (identifier.Contains("usb#vid_046d"))
                mappers.Add(identifier, new PropertyValueMapperLogitech());
            else if (identifier.Contains("usb#vid_05a3&pid_9230"))
                mappers.Add(identifier, new PropertyValueMapperUSBFHD01M());
            else if (identifier.Contains("usb#vid_0ac8&pid_3370"))
                mappers.Add(identifier, new PropertyValueMapperUSB8MP02G());
            else
                mappers.Add(identifier, new PropertyValueMapperDefault());
        }

        public static Func<int, string> GetValueMapper(string identifier, string property)
        {
            if (!mappers.ContainsKey(identifier))
                return mappers["default"].GetMapper(property);
            else
                return mappers[identifier].GetMapper(property);
        }
    }
}
