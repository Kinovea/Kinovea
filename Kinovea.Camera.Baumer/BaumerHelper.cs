using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BGAPI2;

namespace Kinovea.Camera.Baumer
{
    public static class BaumerHelper
    {
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
    }
}
