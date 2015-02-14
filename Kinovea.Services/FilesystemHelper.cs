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
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;

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

            Stream s = null;

            try
            {
                s = new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.None);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (s != null)
                    s.Close();
            }

            return true;
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
    }
}
