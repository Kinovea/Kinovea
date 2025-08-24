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
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using log4net;
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

        public static string LogsDirectory { get; private set; }


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
        /// Variables and profiles. 
        /// CSV files with custom variables that can be used in paths.
        /// </summary>
        public static string VariablesDirectory { get; private set; }
        
        /// <summary>
        /// The main preferences file.
        /// </summary>
        public static string PreferencesFile { get; private set; }
        
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
        
        /// <summary>
        /// Initializes core properties and create the default directories.
        /// </summary>
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

            PreferencesFile             = Path.Combine(SettingsDirectory, "Preferences.xml");
            CameraCalibrationDirectory  = Path.Combine(SettingsDirectory, "CameraCalibration");
            CameraPluginsDirectory      = Path.Combine(SettingsDirectory, "Plugins", "Camera");
            CameraProfilesDirectory     = Path.Combine(SettingsDirectory, "CameraProfiles");
            ColorProfileDirectory       = Path.Combine(SettingsDirectory, "ColorProfiles");
            LogsDirectory               = Path.Combine(SettingsDirectory, "Logs");
            PointersDirectory           = Path.Combine(SettingsDirectory, "Pointers");
            VariablesDirectory          = Path.Combine(SettingsDirectory, "Variables");
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
        /// Setup the name of the instance.
        /// </summary>
        public static void ConfigureInstance()
        {
            // An instance can be started with an explicit name or not.
            // The first instance may have no name.
            // Further instances with no name will be numbered.
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
        
        /// <summary>
        /// Create application data directories if needed.
        /// </summary>
        public static void SanityCheckDirectories()
        {
            CreateDirectory(CameraCalibrationDirectory);
            CreateDirectory(CameraPluginsDirectory);
            CreateDirectory(CameraProfilesDirectory);
            CreateDirectory(ColorProfileDirectory);
            CreateDirectory(LogsDirectory);
            CreateDirectory(PointersDirectory);
            CreateDirectory(SettingsDirectory);
            CreateDirectory(TempDirectory);
            CreateDirectory(VariablesDirectory);
        }

        private static void CreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        
        /// <summary>
        /// Log basic info.
        /// </summary>
        public static void LogInfo()
        {
            log.Info("--------------------------------------------------");
            log.InfoFormat("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now); 
            log.InfoFormat("{0} {1}, {2}.", ApplicationName, Version.ToString(), (IntPtr.Size == 8) ? "x64" : "x86");
            log.InfoFormat("{0}", Environment.OSVersion.ToString());
            log.InfoFormat(".NET Framework {0}", Environment.Version.ToString());
            log.Info("--------------------------------------------------");
        }


        /// <summary>
        /// Initialize the logging on the right file and level.
        /// </summary>
        public static void ConfigureLogging()
        {
            // Logging starts with whatever is in LogConf.xml.
            // We update based on preferences and instance name.
            Hierarchy logRepository = (Hierarchy)LogManager.GetRepository();
            Logger rootLogger = logRepository.Root;
            RollingFileAppender appender = (RollingFileAppender)rootLogger.GetAppender("RollingFileAppender");
            Level logLevel = PreferencesManager.GeneralPreferences.EnableDebugLog ? Level.Debug : Level.Warn;
            appender.Threshold = logLevel;

            // Each instance gets its own log files.
            string logFile = string.IsNullOrEmpty(InstanceName) ? "log.txt" : string.Format("log.{0}.txt", InstanceName);
            appender.File = Path.Combine(Path.GetDirectoryName(appender.File), logFile);

            appender.ActivateOptions();
            logRepository.Configured = true;
        }

        /// <summary>
        /// Change the logging level.
        /// </summary>
        public static void UpdateLogLevel(bool enableDebug)
        {
            // Change the log level at the appender so all loggers are impacted.
            Hierarchy logRepository = (Hierarchy)LogManager.GetRepository();
            Logger rootLogger = logRepository.Root;
            RollingFileAppender appender = (RollingFileAppender)rootLogger.GetAppender("RollingFileAppender");
            Level logLevel = enableDebug ? Level.Debug : Level.Warn;
            appender.Threshold = logLevel;
            appender.ActivateOptions();
            logRepository.Configured = true;
        }
    }
}
