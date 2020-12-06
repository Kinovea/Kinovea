#region License
/*
Copyright © Joan Charmant 2020.
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
using System.Collections.Generic;
using BGAPI2;

namespace Kinovea.Camera.Baumer
{
    /// <summary>
    /// Information about a camera that is specific to the plugin.
    /// This info is opaquely transported inside the CameraSummary.
    /// </summary>
    public class SpecificInfo
    {
        // Temporary info used to find and open the camera.
        public string SystemKey;
        public string InterfaceKey;
        public string DeviceKey;
        public Device Device;
        
        public string StreamFormat { get; set; }
        public bool Demosaicing { get; set; }
        public bool Compression { get; set; }

        public Dictionary<string, CameraProperty> CameraProperties { get; set; }

        public SpecificInfo()
        {
            CameraProperties = new Dictionary<string, CameraProperty>();
            Demosaicing = false;
            Compression = false;
        }
    }
}

