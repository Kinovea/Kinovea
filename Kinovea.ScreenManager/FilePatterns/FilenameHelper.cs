#region License
/*
Copyright © Joan Charmant 2011.
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
using System.IO;
using System.Text;
using Kinovea.Services;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public class FilenameHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool IsFilenameValid(string path, bool allowEmpty)
        {
            bool bIsValid = false;
            
            if(path.Length == 0 && allowEmpty)
                return true;

            try
            {
                new FileInfo(path);
                bIsValid = true;
            }
            catch (ArgumentException)
            {
                log.ErrorFormat("Capture filename has invalid characters. Proposed file was: {0}", path);
            }
            catch (NotSupportedException)
            {
                log.ErrorFormat("Capture filename has a colon. Proposed file was: {0}", path);
            }
            
            return bIsValid;
        }
        
        /// <summary>
        /// Retrieves a string suitable for FFMpeg av_guess_format function in the context of playback.
        /// </summary>
        public static string GetFormatString(string filename)
        {
            string ext = Path.GetExtension(filename).ToLower().Substring(1);

            switch (ext)
            {
                case "mkv" : return "matroska";
                case "avi" : return "avi";
                default: return "mp4";
            }
        }

        /// <summary>
        /// Retrieves a string suitable for FFMpeg av_guess_format function in the context of capture.
        /// </summary>
        public static string GetFormatStringCapture()
        {
            switch (PreferencesManager.CapturePreferences.CapturePathConfiguration.VideoFormat)
            {
                case KinoveaVideoFormat.MKV: return "matroska";
                case KinoveaVideoFormat.AVI: return "avi";
                default: return "mp4";
            }
        }
    }
}

