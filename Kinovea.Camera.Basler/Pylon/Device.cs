#region License
/*
Copyright © Joan Charmant 2013.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using PylonC.NET;

namespace Kinovea.Camera.Basler
{
    public class Device
    {
        #region Properties
        public string Name { get; private set;}
        public string SerialNumber { get; private set;}
        #endregion
        
        private PYLON_DEVICE_INFO_HANDLE infoHandle;
        
        public Device(uint deviceIndex)
        {
            infoHandle = Pylon.GetDeviceInfoHandle(deviceIndex);
            
            Name = GetProperty(Pylon.cPylonDeviceInfoFriendlyNameKey);
            SerialNumber = GetProperty(Pylon.cPylonDeviceInfoSerialNumberKey);
            // MacAddress, IpAddress, PortNr, ModelName, SerialNumber, UserDefinedName.
        }
        
        private string GetProperty(string propertyName)
        {
            return Pylon.DeviceInfoGetPropertyValueByName(infoHandle, propertyName);
        }
        
    }
}
