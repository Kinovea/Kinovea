#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public class Software
    {
        public static string ApplicationName { get { return "Kinovea";}}
        public static bool Experimental { get { return true;}}
        
        public static int InstanceNumber { get; private set; }

        public static string Version { get; private set; }
        public static bool Is32bit { get; private set; }
        public static string SettingsDirectory { get; private set; }
        public static string ColorProfileDirectory { get; private set; }
        public static string CameraCalibrationDirectory { get; private set; }

        public static string PreferencesFile
        {
            get
            {
                if (!instanceConfigured || InstanceNumber < 2 || !PreferencesManager.GeneralPreferences.InstancesOwnPreferences)
                    return SettingsDirectory + "Preferences.xml";
                else
                    return SettingsDirectory + string.Format("Preferences.{0}.xml", InstanceNumber);
            }
        }
        public static string TempDirectory { get; private set; }
        public static string CaptureHistoryDirectory { get; private set; }
        public static string CameraProfilesDirectory { get; private set; }
        public static string HelpVideosDirectory { get; private set; }
        public static string ManualsDirectory { get; private set; }
        public static string LocalHelpIndex { get; private set; }
        public static string RemoteHelpIndex { get; private set; }
        public static string XSLTDirectory { get; private set; }
        public static string ToolbarsDirectory { get; private set; }
        public static string CustomToolsDirectory { get; private set; }
        public static string StandardToolsDirectory { get; private set; }

        private static bool instanceConfigured;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static void Initialize(Version version)
        {
            Version = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            string applicationDirectory = Application.StartupPath + "\\";

            Is32bit = IntPtr.Size == 4;

            string portableSettings = Path.Combine(applicationDirectory, "AppData");
            if (Directory.Exists(portableSettings))
                SettingsDirectory = portableSettings + "\\";
            else
                SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationName) + "\\";
            
            ColorProfileDirectory = SettingsDirectory + "ColorProfiles\\";
            CameraCalibrationDirectory = SettingsDirectory + "CameraCalibration\\";
            TempDirectory = SettingsDirectory + "Temp\\";
            CaptureHistoryDirectory = Path.Combine(SettingsDirectory, "CaptureHistory");
            CameraProfilesDirectory = Path.Combine(SettingsDirectory, "CameraProfiles");

            HelpVideosDirectory = applicationDirectory + "HelpVideos\\";
            ManualsDirectory = applicationDirectory + "Manuals\\";
            XSLTDirectory = applicationDirectory + "xslt\\";
            LocalHelpIndex = applicationDirectory + "HelpIndex.xml";
            ToolbarsDirectory = applicationDirectory + "\\DrawingTools\\Toolbars\\";
            CustomToolsDirectory = applicationDirectory + "\\DrawingTools\\Custom\\";
            StandardToolsDirectory = applicationDirectory + "\\DrawingTools\\Standard\\";

            RemoteHelpIndex = Experimental ? "http://www.kinovea.org/setup/updatebeta.xml" : "http://www.kinovea.org/setup/update.xml";
        }

        public static void ConfigureInstance()
        {
            Process[] instances = Process.GetProcessesByName("Kinovea");
            InstanceNumber = instances.Length;
            instanceConfigured = true;
        }
        
        public static void SanityCheckDirectories()
        {
        	if(!Directory.Exists(SettingsDirectory))
        		Directory.CreateDirectory (SettingsDirectory);
        	
        	if(!Directory.Exists(ColorProfileDirectory))
        	   	Directory.CreateDirectory(ColorProfileDirectory);

            if (!Directory.Exists(CameraCalibrationDirectory))
                Directory.CreateDirectory(CameraCalibrationDirectory);

            if (!Directory.Exists(CameraProfilesDirectory))
                Directory.CreateDirectory(CameraProfilesDirectory);
            
            if (!Directory.Exists(TempDirectory))
                Directory.CreateDirectory(TempDirectory);
        }
        
        public static void LogInfo()
        {
            log.Info("--------------------------------------------------");
            log.InfoFormat("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now); 
            log.InfoFormat("{0} {1}, {2}.", ApplicationName, Version.ToString(), (IntPtr.Size == 8) ? "x64" : "x86");
            log.InfoFormat("{0}", Environment.OSVersion.ToString());
            log.InfoFormat(".NET Framework {0}", Environment.Version.ToString());
            log.Info("--------------------------------------------------");
        }
    }
}
