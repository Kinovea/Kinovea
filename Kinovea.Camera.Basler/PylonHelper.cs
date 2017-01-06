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
    // A few utility functions not present in the Pylon.NETSupportLibrary namespace.
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
            // Similar to ImageProvider.GetLastError().
            string lastErrorMessage = GenApi.GetLastErrorMessage();
            string lastErrorDetail  = GenApi.GetLastErrorDetail();

            string lastErrorText = lastErrorMessage;
            if (lastErrorDetail.Length > 0)
            {
                lastErrorText += "\n\nDetails:\n";
            }
            lastErrorText += lastErrorDetail;
            return lastErrorText;
        }

        public static List<GenApiEnum> ReadEnum(PYLON_DEVICE_HANDLE deviceHandle, string enumerationName)
        {
            // We get a list of all possible values from GenICam API.
            // We cannot use the Pylon .NET enum names because of casing mismatches.
            // Then for each possible value, we poll for availability through Pylon API.

            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, enumerationName);
            if (!nodeHandle.IsValid)
                return null;

            EGenApiNodeType nodeType = GenApi.NodeGetType(nodeHandle);
            if (nodeType != EGenApiNodeType.EnumerationNode)
                return null;

            List<GenApiEnum> supportedList = new List<GenApiEnum>();
            uint total = GenApi.EnumerationGetNumEntries(nodeHandle);

            for (uint i = 0; i < total; i++)
            {
                NODE_HANDLE enumEntryHandle = GenApi.EnumerationGetEntryByIndex(nodeHandle, i);

                string symbol = GenApi.EnumerationEntryGetSymbolic(enumEntryHandle);
                //string symbol = GenApi.NodeGetDisplayName(entryHandle);

                //string featureName = string.Format("EnumEntry_{0}_{1}", enumerationName, symbol);
                //bool supported = Pylon.DeviceFeatureIsAvailable(deviceHandle, featureName);
                bool supported = GenApi.NodeIsAvailable(enumEntryHandle);

                if (supported)
                {
                    string displayName = GenApi.NodeGetDisplayName(enumEntryHandle);
                    supportedList.Add(new GenApiEnum(symbol, displayName));
                }
            }

            return supportedList;
        }
    
        public static GenApiEnum ReadEnumCurrentValue(PYLON_DEVICE_HANDLE deviceHandle, string enumerationName)
        {
            GenApiEnum sf = null;

            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, enumerationName);
            if (!nodeHandle.IsValid)
                return null;

            string selected = GenApi.NodeToString(nodeHandle);

            uint itemCount = GenApi.EnumerationGetNumEntries(nodeHandle);
            for (uint i = 0; i < itemCount; i++)
            {
                NODE_HANDLE entryHandle = GenApi.EnumerationGetEntryByIndex(nodeHandle, i);
                string symbol = GenApi.EnumerationEntryGetSymbolic(entryHandle);
                if (selected != symbol)
                    continue;

                if (!GenApi.NodeIsAvailable(entryHandle))
                    continue;

                string displayName = GenApi.NodeGetDisplayName(entryHandle);
                sf = new GenApiEnum(symbol, displayName);
                break;
            }

            return sf;
        }

        public static void WriteEnum(PYLON_DEVICE_HANDLE deviceHandle, string enumerationName, string enumerationValue)
        {
            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, enumerationName);
            if (!nodeHandle.IsValid)
                return;

            try
            {
                bool available = GenApi.NodeIsAvailable(nodeHandle);
                if (!available)
                    return;

                uint itemCount = GenApi.EnumerationGetNumEntries(nodeHandle);
                for (uint i = 0; i < itemCount; i++)
                {
                    NODE_HANDLE entryHandle = GenApi.EnumerationGetEntryByIndex(nodeHandle, i);

                    if (!GenApi.NodeIsAvailable(entryHandle))
                        continue;

                    string value = GenApi.EnumerationEntryGetSymbolic(entryHandle);
                    if (value != enumerationValue)
                        continue;

                    if (GenApi.NodeToString(nodeHandle) == value)
                        continue;

                    GenApi.NodeFromString(nodeHandle, value);
                    break;
                }
            }
            catch
            {
                // Silent catch.
            }
        }
    }
}
