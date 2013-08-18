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
    // A few utility functions not present in the Basler Pylon class.
    public static class PylonHelper
    {
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
    }
}
