#region License
/*
Copyright © Joan Charmant 2011.
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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// FilenameHelper computes the next file name.
    /// </summary>
    public class FilenameHelper
    {
        // The goal of this class is to compute the next file name for snapshot and recording feature on capture screen.
        // For "free text with increment" type of naming (default) :
        // We try to make it look like "it just works" for the user.
        // The compromise :
        // - We try to increment taking both screens into account.
        // - User should always be able to modify the text manually if he wants to.
        // hence: We do not try to update both screens simultaneously with the same number.
        // Each screen tracks his own file name.
        // 
        // When using pattern, both screen will use the same pattern and they will be updated after each save.
        private static string defaultFileName = "Capture";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public string GetImageFilename()
        {           
            if(PreferencesManager.CapturePreferences.CaptureUsePattern)
                return ConvertPattern(PreferencesManager.CapturePreferences.Pattern, PreferencesManager.CapturePreferences.CaptureImageCounter);
            else if(!string.IsNullOrEmpty(PreferencesManager.CapturePreferences.ImageFile))
                return ComputeNextFilename(PreferencesManager.CapturePreferences.ImageFile);
            
            log.DebugFormat("We never saved a file before, return the default file name : {0}", defaultFileName);
            return defaultFileName;
        }
        public string GetVideoFilename()
        {
            if(PreferencesManager.CapturePreferences.CaptureUsePattern)
                return ConvertPattern(PreferencesManager.CapturePreferences.Pattern, PreferencesManager.CapturePreferences.CaptureVideoCounter);
            else if(!string.IsNullOrEmpty(PreferencesManager.CapturePreferences.VideoFile))
                return ComputeNextFilename(PreferencesManager.CapturePreferences.VideoFile);
            
            log.DebugFormat("We never saved a file before, return the default file name : {0}", defaultFileName);
            return defaultFileName;
        }
        public string ComputeNextFilename(string current)
        {
            //---------------------------------------------------------------------
            // Increments an existing file name.
            // DO NOT use this function when using naming pattern, always use InitImage/InitVideo.
            // This function is oblivious to the type of file being recorded (image/video).
            // if the existing name has a number in it, we increment this number.
            // if not, we create a suffix.
            // We do not care about extension here, it will be appended afterwards.
            //---------------------------------------------------------------------
            
            if(PreferencesManager.CapturePreferences.CaptureUsePattern)
                throw new NotImplementedException("Not implemented when using pattern. Use InitImage or InitVideo");

            if(string.IsNullOrEmpty(current))
                return "";
            
            string next = "";
            
            bool hasEmbeddedDirectory = false;
            string embeddedDirectory = Path.GetDirectoryName(current);
            if(!string.IsNullOrEmpty(embeddedDirectory))
               hasEmbeddedDirectory = true;
            
            string currentFileName = hasEmbeddedDirectory ? Path.GetFileNameWithoutExtension(current) : current;
        
            // Find all numbers in the name, if any.
            Regex r = new Regex(@"\d+");
            MatchCollection mc = r.Matches(currentFileName);
            
            if(mc.Count > 0)
            {
                // Number(s) found. Increment the last one.
                // TODO: handle leading zeroes in the original (001 -> 002).
                Match m = mc[mc.Count - 1];
                int number = int.Parse(m.Value);
                number++;
                
                // Replace the number in the original.
                next = r.Replace(currentFileName, number.ToString(), 1, m.Index );
            }
            else
            {
                // No number found, add suffix.
                next = String.Format("{0} - 2", Path.GetFileNameWithoutExtension(currentFileName));
            }
            
            string finalFileName = hasEmbeddedDirectory ? String.Format("{0}\\{1}", embeddedDirectory, next) : next;
            
            log.DebugFormat("Current file name : {0}, next file name : {1}", current, finalFileName);
            return finalFileName;
        }
        public bool ValidateFilename(string filename, bool allowEmpty)
        {
            bool bIsValid = false;
            
            if(filename.Length == 0 && allowEmpty)
            {
                // special case for when the user is currently typing.
                bIsValid = true;
            }
            else
            {
                try
                {
                    new FileInfo(filename);
                    bIsValid = true;
                }
                catch (ArgumentException)
                {
                    log.ErrorFormat("Capture filename has invalid characters. Proposed file was: {0}", filename);
                }
                catch (NotSupportedException)
                {
                    log.ErrorFormat("Capture filename has a colon in the middle. Proposed file was: {0}", filename);
                }
            }
            
            return bIsValid;
        }
        public string ConvertPattern(string input, long autoIncrement)
        {
            // Convert pattern into file name.
            // Codes : %y, %mo, %d, %h, %mi, %s, %i.
            string output = "";
            
            if (!string.IsNullOrEmpty(input))
            {
                StringBuilder sb = new StringBuilder(input);
                
                // Date and time.
                DateTime dt = DateTime.Now;
                sb.Replace("%y", String.Format("{0:0000}", dt.Year));
                sb.Replace("%mo", String.Format("{0:00}", dt.Month));
                sb.Replace("%d", String.Format("{0:00}", dt.Day));
                sb.Replace("%h", String.Format("{0:00}", dt.Hour));
                sb.Replace("%mi", String.Format("{0:00}", dt.Minute));
                sb.Replace("%s", String.Format("{0:00}", dt.Second));
               
                // auto-increment
                sb.Replace("%i", String.Format("{0}", autoIncrement));
                
                output = sb.ToString();
            }
            
            return output;
        }
        public void AutoIncrement(bool isImage)
        {
            // Autoincrement (only if needed and only the corresponding type).
            if(!PreferencesManager.CapturePreferences.Pattern.Contains("%i"))
                return;
            
            if(isImage)
                PreferencesManager.CapturePreferences.CaptureImageCounter++;
            else
                PreferencesManager.CapturePreferences.CaptureVideoCounter++;
        }
        public string GetImageFileExtension()
        {
            switch(PreferencesManager.CapturePreferences.ImageFormat)
            {
                case KinoveaImageFormat.PNG: return ".png";
                case KinoveaImageFormat.BMP: return ".bmp";
                default : return ".jpg";
            }
        }
        public string GetVideoFileExtension()
        {
            switch(PreferencesManager.CapturePreferences.VideoFormat)
            {
                case KinoveaVideoFormat.MP4: return ".mp4";
                case KinoveaVideoFormat.AVI: return ".avi";
                default : return ".mkv";
            }
        }
    }
}

