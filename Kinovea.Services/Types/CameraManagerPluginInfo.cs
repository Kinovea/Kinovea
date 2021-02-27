
using System;
using System.IO;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// Encapsulates the necessary information to load a camera manager plugin.
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
        /// Name of the assembly to load, including the .dll extension.
        /// </summary>
        public string AssemblyName { get; private set; }

        /// <summary>
        /// Fully qualified name of the camera manager class to instantiate.
        /// </summary>
        public string ClassName { get; private set; }

        /// <summary>
        /// The version of the plugin API the plugin was built against.
        /// Any version lower or higher than the current running version is incompatible.
        /// </summary>
        public string APIVersion { get; private set; }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private CameraManagerPluginInfo(string name, string directory, string assemblyName, string className, string apiVersion)
        {
            this.Name = name;
            this.Directory = directory;
            this.AssemblyName = assemblyName;
            this.ClassName = className;
            this.APIVersion = apiVersion;
        }

        public static CameraManagerPluginInfo Load(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            if (!File.Exists(filename))
            {
                log.ErrorFormat("The plugin manifest could not be found. {0}", filename);
                return null;
            }

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            XmlReader reader = null;
            reader = XmlReader.Create(filename, settings);

            string directory = Path.GetFileName(Path.GetDirectoryName(filename));
            CameraManagerPluginInfo plugin = null;
            try
            {
                plugin = ReadXML(reader, directory);
            }
            catch (Exception e)
            {
                log.ErrorFormat("An error happened during the parsing of the plugin manifest. {0}", Path.GetFileName(filename));
                log.Error(e);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return plugin;
        }

        private static CameraManagerPluginInfo ReadXML(XmlReader reader, string directory)
        {
            reader.MoveToContent();
            bool isEmpty = reader.IsEmptyElement;

            if (reader.Name != "CameraPlugin" && reader.Name != "KinoveaCameraPlugin" || isEmpty)
            {
                reader.ReadOuterXml();
                return null;
            }

            reader.ReadStartElement();
            string version = reader.ReadElementContentAsString("FormatVersion", "");

            if (version != Software.CameraPluginAPIVersion)
            {
                log.ErrorFormat("The camera plugin in the directory \"{0}\" is incompatible with this version of Kinovea. Current API Version: {1}, Plugin API version: {2}.",
                        directory, Software.CameraPluginAPIVersion, version);
                return null;
            }

            string name = "";
            string assemblyName = "";
            string className = "";

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "Name":
                        name = reader.ReadElementContentAsString();
                        break;
                    case "Assembly":
                        assemblyName = reader.ReadElementContentAsString();
                        break;
                    case "Class":
                        className = reader.ReadElementContentAsString();
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }

            reader.ReadEndElement();

            bool incomplete = string.IsNullOrEmpty(name) || string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(className);
            if (incomplete)
            {
                log.ErrorFormat("The camera plugin in the directory \"{0}\" is incomplete.");
                return null;
            }

            return new CameraManagerPluginInfo(name, directory, assemblyName, className, version);
        }
    }
}
