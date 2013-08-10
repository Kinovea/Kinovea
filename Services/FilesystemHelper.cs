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
    }
}
