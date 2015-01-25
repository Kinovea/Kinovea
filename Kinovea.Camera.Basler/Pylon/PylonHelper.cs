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
using System.Collections.Generic;

namespace Kinovea.Camera.Basler
{
    // A few utility functions not present in the Basler Pylon class.
    public static class PylonHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string DeviceGetStringFeature(PYLON_DEVICE_HANDLE handle, string featureName)
        {
            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(handle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, featureName);
            string featureValue = GenApi.NodeToString(nodeHandle);
            return featureValue;
        }
        
        public static string GetLastError()
        {
            string lastErrorMessage = GenApi.GetLastErrorMessage();
            string lastErrorDetail  = GenApi.GetLastErrorDetail();
            return string.Format("{0}, Details:{1}", lastErrorMessage, lastErrorDetail);
        }

        public static List<StreamFormat> GetSupportedStreamFormats(PYLON_DEVICE_HANDLE deviceHandle)
        {
            // We get a list of all possible values from GenICam API.
            // (We cannot use the Pylon .NET enum names because of casing mismatches).
            // Then for each possible value, we poll for availability through Pylon API.

            string enumerationName = "PixelFormat";

            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, enumerationName);
            if (!nodeHandle.IsValid)
                return null;

            EGenApiNodeType nodeType = GenApi.NodeGetType(nodeHandle);
            if (nodeType != EGenApiNodeType.EnumerationNode)
                return null;

            List<StreamFormat> supportedList = new List<StreamFormat>();
            uint total = GenApi.EnumerationGetNumEntries(nodeHandle);

            for (uint i = 0; i < total; i++)
            {
                NODE_HANDLE enumEntryHandle = GenApi.EnumerationGetEntryByIndex(nodeHandle, i);
                string symbol = GenApi.EnumerationEntryGetSymbolic(enumEntryHandle);
                
                string featureName = string.Format("EnumEntry_{0}_{1}", enumerationName, symbol);
                bool supported = Pylon.DeviceFeatureIsAvailable(deviceHandle, featureName);

                if (supported)
                {
                    string displayName = GenApi.NodeGetDisplayName(enumEntryHandle);
                    supportedList.Add(new StreamFormat(symbol, displayName));
                }
            }

            return supportedList;
        }
    }
}
