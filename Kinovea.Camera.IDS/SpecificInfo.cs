#region License
/*
Copyright © Joan Charmant 2017.
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

namespace Kinovea.Camera.IDS
{
    /// <summary>
    /// Information about a camera that is specific to the plugin.
    /// This info is opaquely transported inside the CameraSummary.
    /// </summary>
    public class SpecificInfo
    {
        // The camera object is kept here for convenience. It is used to find back the camera from the configuration dialog which is spawned by generic code.
        // This member is not serialized into the specific info XML.
        public uEye.Camera Camera { get; set; }

        public Dictionary<string, CameraProperty> CameraProperties { get; set; }
        public int StreamFormat { get; set; }

        public SpecificInfo()
        {
            CameraProperties = new Dictionary<string, CameraProperty>();
        }
    }
}
