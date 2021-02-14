using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Camera
{
    /// <summary>
    /// Encapsulates the necessary information to load a plugin.
    /// </summary>
    public class CameraManagerPluginInfo
    {
        /// <summary>
        /// The user facing name of the plugin.
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// The directory where the assembly and its dependencies are stored.
        /// This must be a sub directory of the known camera managers plugin directory.
        /// </summary>
        public string Directory { get; private set; }

        /// <summary>
        /// Name of the assembly to load.
        /// </summary>
        public string AssemblyName { get; private set; }

        /// <summary>
        /// Name of the camera manager class to instantiate.
        /// </summary>
        public string ClassName { get; private set; }

        /// <summary>
        /// The version of the plugin API the plugin was built against.
        /// Any version lower or higher than the current running version cannot be loaded.
        /// </summary>
        public string APIVersion { get; private set; }

        public CameraManagerPluginInfo(string name, string directory, string assemblyName, string className, string apiVersion)
        {
            this.Name = name;
            this.Directory = directory;
            this.AssemblyName = assemblyName;
            this.ClassName = className;
            this.APIVersion = apiVersion;
        }
    }
}
