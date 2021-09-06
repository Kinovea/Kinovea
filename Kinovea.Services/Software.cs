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
        public static bool Experimental { get { return false;}}
        
        public static string InstanceName { get; private set; }

        public static string Version { get; private set; }

        public static string CameraPluginAPIVersion { get; private set; }
        public static bool Is32bit { get; private set; }
        public static string SettingsDirectory { get; private set; }
        public static string ColorProfileDirectory { get; private set; }
        public static string CameraCalibrationDirectory { get; private set; }
        public static string CameraPluginsDirectory { get; private set; }
        public static string PreferencesFile
        {
            get
            {
                if (!instanceConfigured || string.IsNullOrEmpty(InstanceName) || !PreferencesManager.GeneralPreferences.InstancesOwnPreferences)
                    return SettingsDirectory + "Preferences.xml";
                else
                    return SettingsDirectory + string.Format("Preferences.{0}.xml", InstanceName);
            }
        }
        public static string TempDirectory { get; private set; }
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
            CameraPluginAPIVersion = "3.0";

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
            CameraProfilesDirectory = Path.Combine(SettingsDirectory, "CameraProfiles");
            CameraPluginsDirectory = Path.Combine(SettingsDirectory, "Plugins", "Camera");

            HelpVideosDirectory = applicationDirectory + "HelpVideos\\";
            ManualsDirectory = applicationDirectory + "Manuals\\";
            XSLTDirectory = applicationDirectory + "xslt\\";
            LocalHelpIndex = applicationDirectory + "HelpIndex.xml";
            ToolbarsDirectory = applicationDirectory + "\\DrawingTools\\Toolbars\\";
            CustomToolsDirectory = applicationDirectory + "\\DrawingTools\\Custom\\";
            StandardToolsDirectory = applicationDirectory + "\\DrawingTools\\Standard\\";

            RemoteHelpIndex = Experimental ? "http://www.kinovea.org/setup/updatebeta.xml" : "http://www.kinovea.org/setup/update.xml";
        }

        /// <summary>
        /// Setup the name of the instance. Used for the window title and to select a preferences profile.
        /// </summary>
        public static void ConfigureInstance()
        {
            if (!string.IsNullOrEmpty(LaunchSettingsManager.Name))
            {
                InstanceName = LaunchSettingsManager.Name;
            }
            else
            {
                Process[] instances = Process.GetProcessesByName("Kinovea");
                int instanceNumber = instances.Length;
                if (instanceNumber == 1)
                    InstanceName = null;
                else
                    InstanceName = instanceNumber.ToString();
            }

            instanceConfigured = true;
        }
        
        public static void SanityCheckDirectories()
        {
            CreateDirectory(SettingsDirectory);
            CreateDirectory(ColorProfileDirectory);
            CreateDirectory(CameraCalibrationDirectory);
            CreateDirectory(CameraProfilesDirectory);
            CreateDirectory(CameraPluginsDirectory);
            CreateDirectory(TempDirectory);
        }

        private static void CreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
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
