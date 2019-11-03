using System;
using PylonC.NET;
using System.Collections.Generic;

namespace PylonC.NETSupportLibrary
{
    /* Provides methods for listing all available devices. */
    public static class DeviceEnumerator
    {
        /* Data class used for holding device data. */
        public class Device
        {
            public string Name; /* The friendly name of the device. */
            public string FullName; /* The full name string which is unique. */
            public uint Index; /* The index of the device. */
            public string Tooltip; /* The displayed tooltip */
            public PYLON_DEVICE_INFO_HANDLE DeviceInfoHandle;
        }

        /* Queries the number of available devices and creates a list with device data. */
        public static List<Device> EnumerateDevices()
        {
            /* Create a list for the device data. */
            List<Device> list = new List<Device>();

            /* Enumerate all camera devices. You must call
            PylonEnumerateDevices() before creating a device. */
            uint count = Pylon.EnumerateDevices();

            /* Get device data from all devices. */
            for( uint i = 0; i < count; ++i)
            {
                /* Create a new data packet. */
                Device device = new Device();
                /* Get the device info handle of the device. */
                PYLON_DEVICE_INFO_HANDLE hDi = Pylon.GetDeviceInfoHandle(i);
                device.DeviceInfoHandle = hDi;
                /* Get the name. */
                device.Name = Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoFriendlyNameKey);
                /* Get the serial number */
                device.FullName = Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoFullNameKey );
                /* Set the index. */
                device.Index = i;

                /* Create tooltip */
                string tooltip = "";
                uint propertyCount = Pylon.DeviceInfoGetNumProperties(hDi);

                if (propertyCount > 0)
                {
                    for (uint j = 0; j < propertyCount; j++)
                    {
                        tooltip += Pylon.DeviceInfoGetPropertyName(hDi, j) + ": " + Pylon.DeviceInfoGetPropertyValueByIndex(hDi, j);
                        if (j != propertyCount - 1)
                        {
                            tooltip += "\n";
                        }
                    }
                }
                device.Tooltip = tooltip;
                /* Add to the list. */
                list.Add(device);
            }
            return list;
        }
    }
}
