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

namespace Kinovea.Camera.GenICam
{
    /// <summary>
    /// Information about a camera that is specific to the plugin.
    /// This info is opaquely transported inside the CameraSummary.
    /// </summary>
    public class SpecificInfo
    {
        /// <summary>
        /// Handle to the device. 
        /// This is not serialized, it is set by the manager on discovery.
        /// </summary>
        public Device Device;
        
        /// <summary>
        /// Name of the selected stream format.
        /// </summary>
        public string StreamFormat { get; set; }
        
        /// <summary>
        /// Whether to use software debayering.
        /// </summary>
        public bool Demosaicing { get; set; }
        
        /// <summary>
        /// Whether to use hardware JPEG compression.
        /// </summary>
        public bool Compression { get; set; }

        /// <summary>
        /// Complete dictionary of camera properties we handle.
        /// </summary>
        public Dictionary<string, CameraProperty> CameraProperties { get; set; }

        public SpecificInfo()
        {
            CameraProperties = new Dictionary<string, CameraProperty>();
            Demosaicing = false;
            Compression = false;
        }
    }
}

