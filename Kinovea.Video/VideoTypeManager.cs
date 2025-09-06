#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Kinovea.Video
{
    /// <summary>
    /// Load and provide access to the list of video readers.
    /// Also serves as a notification center for lateral communication between modules with regards to video functions.
    /// </summary>
    public static class VideoTypeManager
    {
        #region Events
        public static event EventHandler<VideoLoadAskedEventArgs> VideoLoadAsked;
        #endregion
        
        #region Members
        // Maps extensions to video readers types.
        private static Dictionary<string, Type> m_VideoReaders = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Public methods

        /// <summary>
        /// Register types implementing the VideoReader base class.
        /// The readers are not instanciated at this phase.
        /// </summary>
        public static void LoadVideoReaders(List<Type> videoReaders)
        {
            try
            {
                foreach (Type t in videoReaders)
                {
                    if (!IsCompatibleType(t))
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
        /// Look for assemblies implementing the VideoReader base class, and register them.
        /// The readers are not instanciated at this phase.
        /// </summary>
        public static void LoadVideoReaders()
        {
            //----------------------------
            // OBSOLETE.
            // For some reason Assembly.LoadFrom() doesn't work for everyone.
            // The loadFromRemoteSources tag is present in the app.exe.config but the load is still failing.
            // Use the explicit list instead for now, since we don't really need these to be dynamically looked for.
            // When we have true plugins we'll need to find a solution.
            //----------------------------

            // Check in this assembly and assemblies in the plugin folder.
            List<Assembly> assemblies = new List<Assembly>();
            assemblies.Add(typeof(VideoTypeManager).Assembly);
            
            string dir = Path.GetDirectoryName(Application.ExecutablePath);
            IEnumerable<string> files = Directory.GetFiles(dir, "Kinovea.Video.*.dll");
            foreach (string filename in files)
                AddAssembly(filename, assemblies);
            
            // Register the VideoReaders implementations with the extensions they support.
            foreach (Assembly a in assemblies)
            {
                try
                {
                    foreach(Type t in a.GetTypes())
                    {
                        if (!IsCompatibleType(t))
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
        
        /// <summary>
        /// Instanciate a video reader that supports the target extension.
        /// </summary>
        public static VideoReader GetVideoReader(string extension)
        {
            VideoReader reader = null;
            Type readerType;
            bool found = m_VideoReaders.TryGetValue(extension, out readerType);
            
            // The FFMpeg plugin will support the wildcard as a fallback mechanism.
            if(!found)
                found = m_VideoReaders.TryGetValue("*", out readerType);
            
            if(found)
            {
                ConstructorInfo ci = readerType.GetConstructor(System.Type.EmptyTypes);
                if(ci != null)
                    reader = (VideoReader)Activator.CreateInstance(readerType, null);
            }
            
            return reader;
        }

        public static VideoReader GetImageSequenceReader()
        {
            // Ask specifically for the FFMpeg video reader.
            VideoReader reader = null;
            Type readerType;
            bool found = m_VideoReaders.TryGetValue("*", out readerType);

            if (found)
            {
                ConstructorInfo ci = readerType.GetConstructor(System.Type.EmptyTypes);
                if (ci != null)
                    reader = (VideoReader)Activator.CreateInstance(readerType, null);
            }

            return reader;
        }
        
        /// <summary>
        /// Return true if the extension is supported at all in any reader we loaded.
        /// </summary>
        public static bool IsSupported(string extension)
        {
            return m_VideoReaders.ContainsKey(extension);
        }
        
        public static void LoadVideo(string path, int target)
        {
            if(VideoLoadAsked != null)
                VideoLoadAsked(null, new VideoLoadAskedEventArgs(path, target));
        }

        /// <summary>
        /// Look for the most recent supported video file in a folder.
        /// The provided path must be in form path/pattern.
        /// </summary>
        public static string GetMostRecentSupportedVideo(string path)
        {
            var directory = new DirectoryInfo(Path.GetDirectoryName(path));
            if (directory == null)
                return null;

            if (!directory.Exists)
                return null;

            FileInfo latest = directory.GetFiles(Path.GetFileName(path))
                .Where(f => VideoTypeManager.IsSupported(f.Extension))
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            if (latest == null)
                return null;

            return latest.FullName;
        }
        #endregion

        #region Private methods
        private static bool IsCompatibleType(Type t)
        {
            return t.BaseType != null && !t.IsAbstract && (t.BaseType.Name == "VideoReader" || t.BaseType.Name == "VideoReaderAlwaysCaching");
        }
        private static void AddAssembly(string _filename, List<Assembly> _list)
        {
            try
            {
                Assembly pluginAssembly = Assembly.LoadFrom(_filename);
                _list.Add(pluginAssembly);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Could not load assembly {0} for file types plugin. {1}", _filename, e.Message);
            }
        }

        private static void ProcessType(Type t)
        {
            // Retrieve the extension list from the attribute, add each entry to the readers dictionary.
            object[] attributes = t.GetCustomAttributes(typeof(SupportedExtensionsAttribute), false);
            if (attributes.Length > 0)
            {
                string[] extensions = ((SupportedExtensionsAttribute)attributes[0]).Extensions;

                log.InfoFormat("Registering extensions for {0} : {1}", t.Name, string.Join("; ", extensions));

                foreach (string extension in extensions)
                {
                    if (!m_VideoReaders.ContainsKey(extension))
                        m_VideoReaders.Add(extension, t);
                }
            }
        }
        #endregion
    }
}
