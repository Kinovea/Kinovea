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
        private static List<CameraManager> cameraManagers = new List<CameraManager>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Timer timerDiscovery = new Timer();
        #endregion

        #region Public methods

        /// <summary>
        /// Instanciate types implementing the CameraManager base class.
        /// </summary>
        public static void LoadCameraManagers(List<Type> mgrs)
        {
            try
            {
                foreach (Type t in mgrs)
                {
                    if (!IsCompatibleType(t))
                        continue;

                    ConstructorInfo ci = t.GetConstructor(System.Type.EmptyTypes);
                    if (ci == null)
                        continue;

                    ProcessType(t);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception exception in ex.LoaderExceptions)
                    log.ErrorFormat(exception.Message.ToString());
            }
        }


        /// <summary>
        /// Find and instanciate types implementing the CameraManager base class.
        /// </summary>
        public static void LoadCameraManagers()
        {
            //----------------------------
            // OBSOLETE.
            // For some reason Assembly.LoadFrom() doesn't work for everyone.
            // The loadFromRemoteSources tag is present in the app.exe.config but the load is still failing.
            // Use the explicit list instead for now, since we don't really need these to be dynamically looked for.
            // When we have true plugins we'll need to find a solution.
            //----------------------------

            List<Assembly> assemblies = new List<Assembly>();
            
            string dir = Path.GetDirectoryName(Application.ExecutablePath);
            IEnumerable<string> files = Directory.GetFiles(dir, "Kinovea.Camera.*.dll");
            foreach (string filename in files)
                AddAssembly(filename, assemblies);
                        
            // Register the camera managers.
            foreach (Assembly a in assemblies)
            {
                try
                {
                    foreach(Type t in a.GetTypes())
                    {
                        if (!IsCompatibleType(t))
                            continue;

                        ConstructorInfo ci = t.GetConstructor(System.Type.EmptyTypes);
                        if (ci == null)
                            continue;

                        ProcessType(t);
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
        private static bool IsCompatibleType(Type t)
        {
            return t.BaseType != null && !t.IsAbstract && t.BaseType.Name == "CameraManager";
        }
        private static void AddAssembly(string filename, List<Assembly> list)
        {
            try
            {
                Assembly pluginAssembly = Assembly.LoadFrom(filename);
                list.Add(pluginAssembly);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Could not load assembly {0} for camera types plugin. {1}", filename, e.Message);
            }
        }

        private static void ProcessType(Type t)
        {
            CameraManager manager = (CameraManager)Activator.CreateInstance(t, null);

            if (manager.Enabled)
            {
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
            else
            {
                log.InfoFormat("{0} camera manager is disabled.", manager.CameraTypeFriendlyName);
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
            // Camera managers should also try to connect to the cameras and raise the CameraThumbnailProduced event.
            
            IEnumerable<CameraBlurb> cameraBlurbs = PreferencesManager.CapturePreferences.CameraBlurbs;
            
            List<CameraSummary> summaries = new List<CameraSummary>();
            foreach(CameraManager manager in cameraManagers)
                summaries.AddRange(manager.DiscoverCameras(cameraBlurbs));
            
            if(CamerasDiscovered != null)
                CamerasDiscovered(null, new CamerasDiscoveredEventArgs(summaries));
        }
        
        /// <summary>
        /// Receive a new thumbnail from a camera and forward it upstream.
        /// </summary>
        private static void CameraManager_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            // This runs in a worker thread.
            // The final event handler will have to merge back into the UI thread before using the bitmap.
            if (CameraThumbnailProduced != null)
                CameraThumbnailProduced(sender, e);
        }
        #endregion
    }
}
