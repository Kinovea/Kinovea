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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;
using Kinovea.Services;

namespace Kinovea.Camera
{
    /// <summary>
    /// Load and provide access to the list of camera types (DirectShow, HTTP, etc.).
    /// It also serves as a notification center for lateral communication between modules with regards to camera functions.
    /// </summary>
    public static class CameraTypeManager
    {
        #region Events
        public static event EventHandler<CamerasDiscoveredEventArgs> CamerasDiscovered;
        public static event EventHandler<CameraThumbnailProducedEventArgs> CameraThumbnailProduced;
        public static event EventHandler<CameraSummaryUpdatedEventArgs> CameraSummaryUpdated;
        public static event EventHandler<EventArgs<CameraSummary>> CameraForgotten;
        public static event EventHandler<CameraLoadAskedEventArgs> CameraLoadAsked;
        #endregion
        
        #region Properties
        public static ReadOnlyCollection<CameraManager> CameraManagers
        { 
            get
            {
                return new ReadOnlyCollection<CameraManager>(cameraManagers);
            }
        }
        #endregion

        #region Members
        private static readonly string APIVersion = "3.0";
        private static List<CameraManager> cameraManagers = new List<CameraManager>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Timer timerDiscovery = new Timer();
        #endregion

        #region Public methods
        /// <summary>
        /// Find and instanciate types implementing the CameraManager base class.
        /// </summary>
        public static void LoadCameraManagersPlugins()
        {
            //-----------------------
            // WORK IN PROGRESS.
            //-----------------------

            // Prototyping.
            // Plugins will have a manifest with info.
            List<CameraManagerPluginInfo> plugins = new List<CameraManagerPluginInfo>();
            plugins.Add(new CameraManagerPluginInfo("Basler", "Basler", "Kinovea.Camera.Basler", "Kinovea.Camera.Basler.CameraManagerBasler", "3.0"));
            plugins.Add(new CameraManagerPluginInfo("IDS", "IDS", "Kinovea.Camera.IDS", "Kinovea.Camera.IDS.CameraManagerIDS", "3.0"));
            plugins.Add(new CameraManagerPluginInfo("Daheng", "Daheng", "Kinovea.Camera.Daheng", "Kinovea.Camera.Daheng.CameraManagerDaheng", "3.0"));

            foreach (CameraManagerPluginInfo info in plugins)
            {
                if (info.APIVersion != APIVersion)
                {
                    log.ErrorFormat("The camera plugin \"{0}\" is incompatible with this version of Kinovea. Current API Version: {1}, Plugin API version: {2}.", 
                        info.Name, APIVersion, info.APIVersion);
                    continue;
                }

                LoadCameraManagerPlugin(info);
            }
        }

        private static void LoadCameraManagerPlugin(CameraManagerPluginInfo info)
        {
            // TODO: plugins should go under AppData.
            string appBase = Path.GetDirectoryName(Application.ExecutablePath);
            string dir = Path.Combine(Software.CameraPluginsDirectory, info.Directory);
            if (!Directory.Exists(dir))
            {
                log.ErrorFormat("Could not find directory for camera manager plugin: {0}.", info.Name);
                return;
            }

            string assemblyFile = Path.Combine(dir, info.AssemblyName) + ".dll";
            if (!File.Exists(assemblyFile))
            {
                log.ErrorFormat("Could not find assembly: {0}.", info.AssemblyName);
                return;
            }

            try
            {
                // LoadFrom is problematic on many systems for assemblies downloaded from the Internet.
                // Loading into a different AppDomain is not really possible, the code is too tightly coupled for perfs.
                Assembly a = Assembly.LoadFrom(assemblyFile);

                Type t = a.GetType(info.ClassName);
                LoadCameraManager(t);
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception exception in ex.LoaderExceptions)
                    log.ErrorFormat(exception.Message.ToString());
            }
            catch (Exception e)
            {
                log.ErrorFormat("Could not load camera manager plugin {0}, {1}", info.Name, e.Message);
            }
        }
        
        /// <summary>
        /// Load one camera manager by type.
        /// At this point the assembly hosting the type must be loaded.
        /// </summary>
        public static void LoadCameraManager(Type t)
        {
            try
            {
                if (!IsCompatibleType(t))
                    return;

                ConstructorInfo ci = t.GetConstructor(System.Type.EmptyTypes);
                if (ci == null)
                    return;

                CameraManager manager = (CameraManager)Activator.CreateInstance(t, null);
                AddCameraManager(manager);
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception exception in ex.LoaderExceptions)
                    log.ErrorFormat(exception.Message.ToString());
            }
        }

        /// <summary>
        /// Start the camera discovery timer.
        /// </summary>
        public static void StartDiscoveringCameras()
        {
            log.DebugFormat("Start discovering cameras");
            
            if(timerDiscovery.Enabled)
                return;

            log.DebugFormat("Start discovering cameras");
            timerDiscovery.Interval = 1000;
            timerDiscovery.Tick += timerDiscovery_Tick;
            timerDiscovery.Enabled = true;
            CheckCameras();
        }
        
        /// <summary>
        /// Stop the camera discovery timer and cancel any thumbnail in progress.
        /// </summary>
        public static void StopDiscoveringCameras()
        {
            log.DebugFormat("Stop discovering cameras");
            timerDiscovery.Enabled = false;
            timerDiscovery.Tick -= timerDiscovery_Tick;

            CancelThumbnails();
        }

        /// <summary>
        /// Stop any camera thumbnail going on.
        /// </summary>
        public static void CancelThumbnails()
        {
            log.DebugFormat("Cancelling all thumbnails.");
            foreach (CameraManager manager in cameraManagers)
              manager.StopAllThumbnails();
        }

        /// <summary>
        /// Find the manager that host this camera.
        /// This is used to match launch settings with already discovered cameras.
        /// </summary>
        public static CameraSummary GetCameraSummary(string alias)
        {
            foreach (CameraManager manager in cameraManagers)
            {
                CameraSummary summary = manager.GetCameraSummary(alias);
                if (summary != null)
                    return summary;
            }

            return null;
        }

        public static void UpdatedCameraSummary(CameraSummary summary)
        {
            summary.Manager.UpdatedCameraSummary(summary);
            
            if(CameraSummaryUpdated != null)
                CameraSummaryUpdated(null, new CameraSummaryUpdatedEventArgs(summary));
        }
        
        public static void LoadCamera(CameraSummary summary, int target)
        {
            if (CameraLoadAsked != null)
                CameraLoadAsked(null, new CameraLoadAskedEventArgs(summary, target));
        }
        
        public static void ForgetCamera(CameraSummary summary)
        {
            summary.Manager.ForgetCamera(summary);

            PreferencesManager.CapturePreferences.RemoveCamera(summary.Identifier);
            PreferencesManager.Save();

            if (CameraForgotten != null)
                CameraForgotten(null, new EventArgs<CameraSummary>(summary));
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Returns true if the type is a camera manager.
        /// </summary>
        private static bool IsCompatibleType(Type t)
        {
            return t.BaseType != null && !t.IsAbstract && t.BaseType.Name == "CameraManager";
        }

        /// <summary>
        /// Try to add an instantiated camera manager object to our rooster.
        /// </summary>
        private static void AddCameraManager(CameraManager manager)
        {
            try
            {
                if (!manager.Enabled)
                {
                    log.InfoFormat("{0} camera manager is disabled.", manager.CameraTypeFriendlyName);
                    return;
                }

                if (manager.SanityCheck())
                {
                    manager.CameraThumbnailProduced += CameraManager_CameraThumbnailProduced;
                    cameraManagers.Add(manager);
                    log.InfoFormat("Initialized {0} camera manager.", manager.CameraTypeFriendlyName);
                }
                else
                {
                    log.InfoFormat("{0} camera manager failed sanity check.", manager.CameraTypeFriendlyName);
                }
            }
            catch (FileNotFoundException e)
            {
                log.InfoFormat("{0} camera manager is missing dependencies. {1}", manager.CameraTypeFriendlyName, e.Message);
            }
            catch (Exception e)
            {
                log.InfoFormat("Error while initializing {0}. {1}", manager.CameraTypeFriendlyName, e.Message);
            }
        }
        
        private static void timerDiscovery_Tick(object sender, EventArgs e)
        {
            CheckCameras();
        }

        /// <summary>
        /// Ask each camera manager plugin to discover its cameras.
        /// This can be dynamic or based on previously saved data.
        /// Camera managers should also try to connect to the cameras and raise the CameraThumbnailProduced event.
        /// </summary>
        private static void CheckCameras()
        {
            IEnumerable<CameraBlurb> cameraBlurbs = PreferencesManager.CapturePreferences.CameraBlurbs;
            
            List<CameraSummary> summaries = new List<CameraSummary>();
            foreach(CameraManager manager in cameraManagers)
                summaries.AddRange(manager.DiscoverCameras(cameraBlurbs));
            
            if(CamerasDiscovered != null)
                CamerasDiscovered(null, new CamerasDiscoveredEventArgs(summaries));
        }

        /// <summary>
        /// Receive a new thumbnail from a camera and raise CameraThumbnailProduced in turn.
        /// The camera manager should make sure this runs in the UI thread.
        /// This event should always be raised, even if the thumbnail could not be retrieved.
        /// </summary>
        private static void CameraManager_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            CameraThumbnailProduced?.Invoke(sender, e);
        }
        #endregion
    }
}
