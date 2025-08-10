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

        /// <summary>
        /// Top level settings directory. All other directories are subdirectories of this one.
        /// </summary>
        public static string SettingsDirectory { get; private set; }
        
        /// <summary>
        /// Default values for tools.
        /// </summary>
        public static string ColorProfileDirectory { get; private set; }
        
        /// <summary>
        /// Lens calibration files.
        /// </summary>
        public static string CameraCalibrationDirectory { get; private set; }
        
        /// <summary>
        /// Camera plugins binaries.
        /// </summary>
        public static string CameraPluginsDirectory { get; private set; }

        /// <summary>
        /// Images used for custom pointers.
        /// </summary>
        public static string PointersDirectory { get; private set; }

        /// <summary>
        /// Application level profiles are csv files with custom variables that can be used in paths.
        /// </summary>
        public static string ProfilesDirectory { get; private set; }
        
        /// <summary>
        /// The main preferences file.
        /// Possibly instance-specific.
        /// </summary>
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

        /// <summary>
        /// Directory where auto-save files are stored.
        /// </summary>
        public static string TempDirectory { get; private set; }
        
        /// <summary>
        /// Collection of settings for cameras.
        /// </summary>
        public static string CameraProfilesDirectory { get; private set; }
        
        public static string RemoteHelpIndex { get; private set; }
        public static string XSLTDirectory { get; private set; }
        public static string ToolbarsDirectory { get; private set; }
        public static string CustomToolsDirectory { get; private set; }
        public static string StandardToolsDirectory { get; private set; }

        private static bool instanceConfigured;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static void Initialize(Version version)
        {
            Version = version.Build == 0 ? 
                string.Format("{0}.{1}", version.Major, version.Minor) : 
                string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);

            CameraPluginAPIVersion = "3.0";

            string applicationDirectory = Application.StartupPath + "\\";

            Is32bit = IntPtr.Size == 4;

            string portableSettings = Path.Combine(applicationDirectory, "AppData");
            if (Directory.Exists(portableSettings))
                SettingsDirectory = portableSettings + "\\";
            else
                SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationName) + "\\";
            
            CameraCalibrationDirectory  = Path.Combine(SettingsDirectory, "CameraCalibration");
            CameraProfilesDirectory     = Path.Combine(SettingsDirectory, "CameraProfiles");
            ColorProfileDirectory       = Path.Combine(SettingsDirectory, "ColorProfiles");
            CameraPluginsDirectory      = Path.Combine(SettingsDirectory, "Plugins", "Camera");
            PointersDirectory           = Path.Combine(SettingsDirectory, "Pointers");
            ProfilesDirectory           = Path.Combine(SettingsDirectory, "Profiles");
            TempDirectory               = Path.Combine(SettingsDirectory, "Temp");

            XSLTDirectory           = Path.Combine(applicationDirectory, "xslt");
            ToolbarsDirectory       = Path.Combine(applicationDirectory, "DrawingTools", "Toolbars");
            CustomToolsDirectory    = Path.Combine(applicationDirectory, "DrawingTools", "Custom");
            StandardToolsDirectory  = Path.Combine(applicationDirectory, "DrawingTools", "Standard");

            RemoteHelpIndex = Experimental ? 
                "https://www.kinovea.org/setup/updatebeta.xml" : 
                "https://www.kinovea.org/setup/update.xml";
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
            CreateDirectory(CameraCalibrationDirectory);
            CreateDirectory(CameraProfilesDirectory);
            CreateDirectory(ProfilesDirectory);
            CreateDirectory(ColorProfileDirectory);
            CreateDirectory(CameraPluginsDirectory);
            CreateDirectory(PointersDirectory);
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
