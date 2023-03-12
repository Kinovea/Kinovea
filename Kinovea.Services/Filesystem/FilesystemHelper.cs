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
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Kinovea.Services
{
    public static class FilesystemHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static void DeleteFile(string filepath)
        {
            if(!File.Exists(filepath))
                return;
                
            try
            {
                FileSystem.DeleteFile(filepath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
            }
            catch(OperationCanceledException)
            {
                // User cancelled confirmation box.
            }
            catch
            {
                // Other error. e.g. file is opened somewhere else.
            }
        }
        
        public static void LocateDirectory(string path)
        {
            if (!Directory.Exists(path))
                return;

            //string arg = "\"" + path + "\"";
            string arg = path;
            System.Diagnostics.Process.Start("explorer.exe", arg);
        }
        
        public static void LocateFile(string path)
        {
            if (!File.Exists(path))
                return;

            string arg = @"/select, " + path;
            System.Diagnostics.Process.Start("explorer.exe", arg);
        }
        
        public static void RenameFile(string oldFilePath, string newFilePath)
        {
            if(!File.Exists(oldFilePath) || File.Exists(newFilePath))
                return;
                
            try
            {
                File.Move(oldFilePath, newFilePath);
            }
            catch(ArgumentException)
            {
                log.ErrorFormat("Couldn't rename file. {0}", newFilePath);
            }
            catch(UnauthorizedAccessException e)
            {
                log.ErrorFormat("Couldn't rename file due to unsufficient permissions. {0}", newFilePath);
                log.ErrorFormat(e.ToString());
            }
            catch(Exception e)
            {
                log.ErrorFormat("Couldn't rename file. {0}", e.ToString());
            }
        }

        public static int GetFilterIndex(string filter, KinoveaImageFormat format)
        {
            int defaultIndex = 1;
            if (string.IsNullOrEmpty(filter))
                return defaultIndex;

            string[] splits = filter.Split('|');
            if (splits.Length < 2 || splits.Length % 2 != 0)
                return defaultIndex;

            Dictionary<KinoveaImageFormat, int> mapping = new Dictionary<KinoveaImageFormat, int>();
            for (int i = 1; i < splits.Length; i+=2)
            {
                // Filter index is 1-based.
                int index = ((i - 1)  / 2) + 1;

                if (splits[i].Contains("*.jpg"))
                    mapping.Add(KinoveaImageFormat.JPG, index);
                else if (splits[i].Contains("*.bmp"))
                    mapping.Add(KinoveaImageFormat.BMP, index);
                else if (splits[i].Contains("*.png"))
                    mapping.Add(KinoveaImageFormat.PNG, index);
            }

            return mapping.ContainsKey(format) ? mapping[format] : defaultIndex;
        }

        public static KinoveaImageFormat GetImageFormat(string filename)
        {
            KinoveaImageFormat format = KinoveaImageFormat.JPG;
            string extension = Path.GetExtension(filename).ToLower();
            if (extension == ".jpg" || extension == ".jpeg")
                format = KinoveaImageFormat.JPG;
            else if (extension == ".png")
                format = KinoveaImageFormat.PNG;
            else if (extension == ".bmp")
                format = KinoveaImageFormat.BMP;

            return format;
        }
        
        public static int GetFilterIndex(string filter, KinoveaVideoFormat format)
        {
            int defaultIndex = 1;
            if (string.IsNullOrEmpty(filter))
                return defaultIndex;

            string[] splits = filter.Split('|');
            if (splits.Length < 2 || splits.Length % 2 != 0)
                return defaultIndex;

            Dictionary<KinoveaVideoFormat, int> mapping = new Dictionary<KinoveaVideoFormat, int>();
            for (int i = 1; i < splits.Length; i += 2)
            {
                // Filter index is 1-based.
                int index = ((i - 1) / 2) + 1;

                if (splits[i].Contains("*.mkv"))
                    mapping.Add(KinoveaVideoFormat.MKV, index);
                else if (splits[i].Contains("*.avi"))
                    mapping.Add(KinoveaVideoFormat.AVI, index);
                else if (splits[i].Contains("*.mp4"))
                    mapping.Add(KinoveaVideoFormat.MP4, index);
            }

            return mapping.ContainsKey(format) ? mapping[format] : defaultIndex;
        }

        public static KinoveaVideoFormat GetVideoFormat(string filename)
        {
            KinoveaVideoFormat format = KinoveaVideoFormat.MKV;
            string extension = Path.GetExtension(filename).ToLower();
            if (extension == ".mkv")
                format = KinoveaVideoFormat.MKV;
            else if (extension == ".mp4")
                format = KinoveaVideoFormat.MP4;
            else if (extension == ".avi")
                format = KinoveaVideoFormat.AVI;

            return format;
        }

        public static bool CanWrite(string filename)
        {
            // This may suffer from a race condition but should be fine as we mainly use this to test overwriting the file Open in Kinovea itself.
            if (!File.Exists(filename))
                return true;

            return !IsFileLocked(filename, FileAccess.Write, FileShare.None);
        }

        public static bool CanRead(string filename)
        {
            if (!File.Exists(filename))
                return false;

            return !IsFileLocked(filename, FileAccess.Read, FileShare.None);
        }

        private static bool IsFileLocked(string filename, FileAccess fileAccess, FileShare fileShare)
        {
            Stream s = null;

            try
            {
                s = new FileStream(filename, FileMode.Open, fileAccess, fileShare);
            }
            catch
            {
                return true;
            }
            finally
            {
                if (s != null)
                    s.Close();
            }

            return false;
        }

        /// <summary>
        /// Creates a missing directory before we write the file into it.
        /// Input is the whole file path.
        /// </summary>
        public static bool CreateDirectory(string filepath)
        {
            string directory = Path.GetDirectoryName(filepath);
            if (Directory.Exists(directory))
                return true;

            try
            { 
                Directory.CreateDirectory(directory);

                return Directory.Exists(directory);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Capture directory could not be created at {0}. Error: {1}", directory, e.Message);
            }

            return false;
        }

        public static void DeleteOrphanFiles()
        {
            // Delete orphaned temporary files.
            // Called when the user cancels the recovery process or when it itself fails.
            // We do the recursion manually to avoid deleting and recreating the directory at each launch, 
            // especially since the normal case is that there's nothing in there.
            DeleteDirectoryContent(Software.TempDirectory);
        }

        private static void DeleteDirectoryContent(string path)
        {
            if (!Directory.Exists(path))
                return;

            foreach (string entry in Directory.GetFiles(path))
                File.Delete(entry);

            foreach (string entry in Directory.GetDirectories(path))
            {
                DeleteDirectoryContent(entry);
                Directory.Delete(entry);
            }
        }

        /// <summary>
        /// Returns the filename with a sequence pattern in it, or null if the passed file is not part of an image sequence.
        /// This generates a file pattern suitable for ffmpeg, for example: "image%04d.jpg".
        /// The criteria is that there are at least 3 other files following the one passed in parameter.
        /// </summary>
        public static string GetSequenceFilename(string path)
        {
            if (!PreferencesManager.PlayerPreferences.DetectImageSequences)
                return null;

            int sequenceThreshold = 3;

            string directory = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);

            // Only do this for images.
            List<string> imageExtensions = new List<string>() { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp" };
            if (!imageExtensions.Contains(extension))
                return null;

            Regex r = new Regex(@"\d+");
            MatchCollection mc = r.Matches(filename);

            if (mc.Count == 0)
                return null;

            // We did find some numbers, check if there are other files following the sequence of the last matched number.
            Match m = mc[mc.Count - 1];
            int digits = m.Value.Length;
            int value;
            bool parsed = int.TryParse(m.Value, out value);
            if (!parsed)
                return null;

            // Build the next filenames in the sequence and look for the corresponding files.
            // We need this to be somewhat similar to what ffmpeg will do, we don't want 
            // false positives where we report a sequence and ffmpeg can't find the files.
            for (int i = 0; i < sequenceThreshold; i++)
            {
                value++;
                string token = value.ToString().PadLeft(digits, '0');
                string nextFilename = r.Replace(filename, token, 1, m.Index);
                string nextPath = Path.Combine(directory, nextFilename + extension);
                if (!File.Exists(nextPath))
                    return null;
            }

            // Build the final file pattern.
            string patternToken = string.Format("%0{0}d", digits);
            string patternFilename = r.Replace(filename, patternToken, 1, m.Index);
            return patternFilename + extension;
        }

        /// <summary>
        /// Return true if this path is a special path used to detect replay watcher mode.
        /// </summary>
        public static bool IsReplayWatcher(string path)
        {
            // We simply detect if the file has a filter pattern.
            string filename = Path.GetFileNameWithoutExtension(path);
            return filename.Contains("*") || filename.Contains("?");
        }

        public static string SaveImageFilter()
        {
            return "JPEG|*.jpg;*.jpeg|PNG|*.png|Bitmap|*.bmp";
        }

        public static string SaveVideoFilter()
        {
            return "Matroska|*.mkv|MP4|*.mp4|AVI|*.avi";
        }

        public static string SaveKVAFilter()
        {
            return "Kinovea|*.kva";
        }

        public static string SaveWorkspaceFilter()
        {
            return "Kinovea workspace|*.xml";
        }

        public static string SaveXMLFilter()
        {
            return "XML|*.xml";
        }

        public static string SaveCSVFilter()
        {
            return "Comma-separated values|*.csv";
        }

        public static string SavePNGFilter()
        {
            return "PNG|*.png";
        }

        public static string OpenImageFilter(string labelAllSupported)
        {
            string all = labelAllSupported + "|*.svg;*.jpg;*.png;*.bmp;*.gif";
            string svg = "Scalable Vector Graphics|*.svg";
            string jpg = "JPEG|*.jpg;*.jpeg";
            string png = "PNG|*.png";
            string bmp = "Bitmap|*.bmp";
            string gif = "GIF|*.gif";
            string totalFilter = string.Join("|", new string[] { all, svg, jpg, png, bmp, gif});
            return totalFilter;
        }

        public static string OpenKVAFilter(string labelAllSupported)
        {
            string all = labelAllSupported + "|*.kva;*.srt;*.json;*.xml";
            string kva = "Kinovea|*.kva";
            string srt = "SubRip Subtitle|*.srt";
            string openPose = "OpenPose|*.json";
            string totalFilter = string.Join("|", new string[] { all, kva, srt, openPose });
            return totalFilter;
        }

        public static string OpenXMLFilter()
        {
            return "XML|*.xml";
        }

        public static string OpenINIFilter()
        {
            return "INI|*.ini";
        }
    }
}
