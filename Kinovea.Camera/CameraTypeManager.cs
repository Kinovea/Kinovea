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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

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
        public static event EventHandler<CameraImageReceivedEventArgs> CameraImageReceived;
        public static event EventHandler<CameraSummaryUpdatedEventArgs> CameraSummaryUpdated;
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
        private static List<CameraManager> cameraManagers = new List<CameraManager>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Timer timerDiscovery = new Timer();
        #endregion
        
        #region Public methods
        /// <summary>
        /// Find and instanciate types implementing the CameraManager base class.
        /// </summary>
        public static void LoadCameraManagers()
        {
            List<Assembly> assemblies = new List<Assembly>();
            
            string dir = Path.GetDirectoryName(Application.ExecutablePath);
            if(Directory.Exists(dir))
            {
                foreach (string fileName in Directory.GetFiles(dir, "*.dll"))
                    AddAssembly(fileName, assemblies);
            }
            
            // Register the camera managers.
            foreach (Assembly a in assemblies)
            {
                try
                {
                    foreach(Type t in a.GetTypes())
                    {
                        if(t.BaseType == null || t.BaseType.Name != "CameraManager" || t.IsAbstract)
                            continue;
                        
                        ConstructorInfo ci = t.GetConstructor(System.Type.EmptyTypes);
                        if(ci == null)
                            continue;
                        
                        CameraManager manager = (CameraManager)Activator.CreateInstance(t, null);
                        manager.CameraImageReceived += CameraManager_CameraImageReceived;
                        cameraManagers.Add(manager);
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {        
                    foreach (Exception exception in ex.LoaderExceptions)
                        log.ErrorFormat(exception.Message.ToString());
                }
            }
        }

        public static void DiscoverCameras()
        {
            if(timerDiscovery.Enabled)
                return;
                
            timerDiscovery.Interval = 1000;
            timerDiscovery.Tick += timerDiscovery_Tick;
            timerDiscovery.Enabled = true;
        }
        
        public static void StopDiscoveringCameras()
        {
            timerDiscovery.Enabled = false;
            timerDiscovery.Tick -= timerDiscovery_Tick;
        }

        public static void UpdatedCameraSummary(CameraSummary summary)
        {
            summary.Manager.UpdatedCameraSummary(summary);
            
            if(CameraSummaryUpdated != null)
                CameraSummaryUpdated(null, new CameraSummaryUpdatedEventArgs(summary));
        }
        
        public static void LoadCamera(CameraSummary summary, int target)
        {
            if(CameraLoadAsked != null)
                CameraLoadAsked(null, new CameraLoadAskedEventArgs(summary, target));
        }
        
        public static void DeleteCamera(CameraSummary summary)
        {
            PreferencesManager.CapturePreferences.RemoveCamera(summary.Identifier);
            PreferencesManager.Save();
        }
        
        #endregion
        
        #region Private methods
        private static void AddAssembly(string filename, List<Assembly> list)
        {
            try
            {
                Assembly pluginAssembly = Assembly.LoadFrom(filename);
                list.Add(pluginAssembly);
            }
            catch (Exception)
            {
                log.ErrorFormat("Could not load assembly {0} for camera types plugin", filename);
            }
        }
        
        private static void timerDiscovery_Tick(object sender, EventArgs e)
        {
            CheckCameras();
         }
        
        private static void CheckCameras()
        {
            // Ask each plug-in to discover its cameras.
            // This can be dynamic or based on previously saved data.
            // Camera managers should also try to connect to the cameras and raise the CameraImageReceived event.
            
            IEnumerable<CameraBlurb> cameraBlurbs = PreferencesManager.CapturePreferences.CameraBlurbs;
            
            List<CameraSummary> summaries = new List<CameraSummary>();
            foreach(CameraManager manager in cameraManagers)
                summaries.AddRange(manager.DiscoverCameras(cameraBlurbs));
            
            if(CamerasDiscovered != null)
                CamerasDiscovered(null, new CamerasDiscoveredEventArgs(summaries));
        }
        
        /// <summary>
        /// Receive a single image from a camera and forward it upstream.
        /// </summary>
        private static void CameraManager_CameraImageReceived(object sender, CameraImageReceivedEventArgs e)
        {
            // This runs in a worker thread.
            // The final event handler will have to merge back into the UI thread before using the bitmap.
            if(CameraImageReceived != null /*&& timerDiscovery.Enabled*/)
                CameraImageReceived(sender, e);
        }
        #endregion
    }
}
