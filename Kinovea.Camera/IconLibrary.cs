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
using System.Linq;

namespace Kinovea.Camera
{
    public static class IconLibrary
    {
        public static IEnumerable<Bitmap> Icons
        {
            get 
            { 
                if(!initialized)
                    Initialize();
                
                return icons.Values.Cast<Bitmap>();
            }
        }

        private static Dictionary<string, Bitmap> icons = new Dictionary<string, Bitmap>();
        private static bool initialized;
        private static string defaultKey;

        public static void Initialize()
        {
            icons.Clear();
            
            icons.Add("camcorder", Properties.Icons.camcorder);
            icons.Add("webcam", Properties.Icons.webcam);
            
            icons.Add("camera", Properties.Icons.camera);
            icons.Add("camera_black", Properties.Icons.camera_black);
            icons.Add("camera_lens", Properties.Icons.camera_lens);
            icons.Add("camera_small", Properties.Icons.camera_small);
            icons.Add("camera_small_black", Properties.Icons.camera_small_black);
            
            icons.Add("network", Properties.Icons.network);
            icons.Add("network_cloud", Properties.Icons.network_cloud);
            icons.Add("network_wireless", Properties.Icons.network_wireless);
            
            icons.Add("media_player_phone", Properties.Icons.media_player_phone);
            icons.Add("pda", Properties.Icons.pda);
            
            icons.Add("footprint", Properties.Icons.footprint);
            icons.Add("processor", Properties.Icons.processor);
            icons.Add("memory", Properties.Icons.memory);
            
            icons.Add("usb_flash_drive_logo", Properties.Icons.usb_flash_drive_logo);
            icons.Add("wi_fi", Properties.Icons.wi_fi);
            
            icons.Add("target", Properties.Icons.target);
            icons.Add("spectrum", Properties.Icons.spectrum);
            icons.Add("system-monitor", Properties.Icons.system_monitor);
            
            icons.Add("number01", Properties.Icons.number01);
            icons.Add("number02", Properties.Icons.number02);
            icons.Add("number03", Properties.Icons.number03);
            icons.Add("number04", Properties.Icons.number04);
            icons.Add("number05", Properties.Icons.number05);
            icons.Add("number06", Properties.Icons.number06);
            icons.Add("number07", Properties.Icons.number07);
            icons.Add("number08", Properties.Icons.number08);
            icons.Add("number09", Properties.Icons.number09);
            icons.Add("number10", Properties.Icons.number10);

            icons.Add("dashboard", Properties.Icons.dashboard);
            icons.Add("counter", Properties.Icons.counter);
            icons.Add("construction", Properties.Icons.construction);
            icons.Add("na", Properties.Icons.na);

            icons.Add("logitech", Properties.Icons.logitech);
            icons.Add("microsoft", Properties.Icons.microsoft);
            icons.Add("playstation", Properties.Icons.playstation);
            icons.Add("basler", Properties.Icons.basler2);
            
            defaultKey = "camcorder";
            
            initialized = true;
        }
        
        public static Bitmap GetIcon(string key)
        {
            if(!initialized)
                Initialize();
            
            if(!icons.ContainsKey(key))
                return icons[defaultKey];
            else
                return icons[key];
        }
    }
}
