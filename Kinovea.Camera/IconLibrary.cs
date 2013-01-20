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
using System.Collections.Generic;
using System.Drawing;

namespace Kinovea.Camera
{
    public static class IconLibrary
    {
        private static Dictionary<string, Bitmap> icons = new Dictionary<string, Bitmap>();
        private static bool initialized;
        
        public static void Initialize()
        {
            icons.Clear();
            
            icons.Add("camcorder", Properties.Icons.camcorder);
            icons.Add("camera", Properties.Icons.camera);
            icons.Add("camera_black", Properties.Icons.camera_black);
            icons.Add("camera_lens", Properties.Icons.camera_lens);
            icons.Add("camera_small", Properties.Icons.camera_small);
            icons.Add("camera_small_black", Properties.Icons.camera_small_black);
            icons.Add("footprint", Properties.Icons.footprint);
            icons.Add("media_player_phone", Properties.Icons.media_player_phone);
            icons.Add("memory", Properties.Icons.memory);
            icons.Add("network", Properties.Icons.network);
            icons.Add("network_cloud", Properties.Icons.network_cloud);
            icons.Add("network_wireless", Properties.Icons.network_wireless);
            icons.Add("pda", Properties.Icons.pda);
            icons.Add("processor", Properties.Icons.processor);
            icons.Add("usb_flash_drive_logo", Properties.Icons.usb_flash_drive_logo);
            icons.Add("webcam", Properties.Icons.webcam);
            icons.Add("wi_fi", Properties.Icons.wi_fi);
            
            initialized = true;
        }
        
        public static Bitmap GetIcon(string key)
        {
            if(!initialized)
                Initialize();
            
            if(!icons.ContainsKey(key))
                return icons["camcorder"];
            else
                return icons[key];
        }
    }
}
