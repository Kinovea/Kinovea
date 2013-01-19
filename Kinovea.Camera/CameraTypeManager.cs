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

namespace Kinovea.Camera
{
    /// <summary>
    /// Load and provide access to the list of camera types (DirectShow, HTTP, etc.).
    /// </summary>
    public static class CameraTypeManager
    {
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
        #endregion
        
        #region Public methods
        /// <summary>
        /// Find and instanciate types implementing the CameraManager base class.
        /// </summary>
        public static void LoadCameraManagers()
        {
            List<Assembly> assemblies = new List<Assembly>();
            
            string dir = Path.GetDirectoryName(Application.ExecutablePath) + "\\CameraTypes\\";
            if(Directory.Exists(dir))
            {
                foreach (string fileName in Directory.GetFiles(dir, "*.dll"))
                    AddAssembly(fileName, assemblies);
            }
            
            // Register the camera managers.
            foreach (Assembly a in assemblies)
            {
                foreach(Type t in a.GetTypes())
                {
                    if(!t.IsSubclassOf(typeof(CameraManager)) || t.IsAbstract)
                        continue;
                    
                    ConstructorInfo ci = t.GetConstructor(System.Type.EmptyTypes);
                    if(ci == null)
                        continue;
                    
                    CameraManager manager = (CameraManager)Activator.CreateInstance(t, null);
                    cameraManagers.Add(manager);
                }
            }
        }
        
        public static List<CameraSummary> DiscoverCameras()
        {
            // Read the list of cameras previously seen.
            // Ask each plug-in to discover its cameras.
            List<CameraSummary> summaries = new List<CameraSummary>();
            foreach(CameraManager manager in cameraManagers)
                summaries.AddRange(manager.DiscoverCameras(null));
            
            return summaries;
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
        #endregion
    }
}
