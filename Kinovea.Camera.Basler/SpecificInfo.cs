#region License
/*
Copyright © Joan Charmant 2013.
jcharmant@gmail.com 
 
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
using System.Drawing;
using PylonC.NET;
using System.Collections.Generic;

namespace Kinovea.Camera.Basler
{
    /// <summary>
    /// Information about a camera that is specific to the Basler plugin.
    /// This info is opaquely transported inside the CameraSummary.
    /// </summary>
    public class SpecificInfo
    {
        // The handle is kept here for convenience. It is used to find back the camera from the configuration dialog which is spawned by generic code.
        // This handle is not serialized into the specific info XML.
        public PYLON_DEVICE_HANDLE Handle { get; set; }

        public Dictionary<string, CameraProperty> CameraProperties { get; set; }
        public string StreamFormat { get; set; } 

        public SpecificInfo()
        {
            CameraProperties = new Dictionary<string, CameraProperty>();
        }
    }
}
