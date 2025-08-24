using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// Handles the management of active and inactive windows.
    /// </summary>
    public static class WindowManager
    {
        #region Properties
        public static WindowDescriptor ActiveWindow
        {
            get; private set;
        }
        #endregion

        #region Members
        private static XmlWriterSettings xmlWriterSettings;
        private static XmlReaderSettings xmlReaderSettings;
        private static List<WindowDescriptor> windowDescriptors = new List<WindowDescriptor>();
        private static WindowDescriptor lastClosedWindow = null;
        private static DateTime bestSaveTime = DateTime.MinValue;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        static WindowManager()
        {
            xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;

            xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreProcessingInstructions = true;
            xmlReaderSettings.IgnoreWhitespace = true;
            xmlReaderSettings.CloseInput = true;
        }

        /// <summary>
        /// Startup setup, called only once at application startup.
        /// Determines which window to load or create a new one if needed.
        /// </summary>
        public static void Startup(bool isFirstInstance)
        {
            log.Debug("Collecting saved windows.");

            ReadAllDescriptors();

            // Check we were started as a named instance from the command line.
            string name = LaunchSettingsManager.Name;

            if (string.IsNullOrEmpty(name))
            {
                if (isFirstInstance)
                {
                    // No name and first instance -> load the last closed window.
                    if (windowDescriptors.Count > 0 && lastClosedWindow != null)
                    {
                        log.Debug("No name provided, loading last closed window.");
                        LoadLastClosedWindow();
                    }
                    else
                    {
                        log.Debug("No name provided and no saved window, creating a new window.");
                        LoadNewWindow();
                    }
                }
                else
                {
                    log.Debug("No name provided and not the first instance, creating a new window.");
                    LoadNewWindow();
                }
            }
            else
            {
                // Find the window.
                bool found = false;
                foreach (var descriptor in windowDescriptors)
                {
                    if (descriptor.Name == name)
                    {
                        LoadNamedWindow(name);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // Named but not found.
                    // This is a user error. Maybe they are starting in a new application directory
                    // and forgot to bring over the application folder.
                    // In any case we should not create a new instance and force the name, it wouldn't 
                    // match the saved window they are trying to load.
                    // Do not load the last closed window either, the user would still think they are loading
                    // a specific window and could start updating things.
                    // Let's create a new window with no name and report the error.
                    log.ErrorFormat("The named window '{0}' could not be found. Creating a new window.", name);
                    LoadNewWindow();
                }
            }

            // At this point we have an active window.
            if (ActiveWindow == null)
            {
                log.Error("No active window was set after startup.");
                throw new InvalidProgramException();
            }
            else
            {
                log.DebugFormat("Active window is '{0}' with ID {1}.", ActiveWindow.Name, ActiveWindow.Id);
            }
        }

        /// <summary>
        /// Reload all descriptors.
        /// This is done on application startup and after the list may have changed 
        /// (add/delete from a window management dialog in any instance).
        /// Used to find which window to launch on startup and update the list of saved windows.
        /// </summary>
        public static void ReadAllDescriptors()
        {
            lastClosedWindow = null;
            bestSaveTime = DateTime.MinValue;
            windowDescriptors.Clear();
            foreach (var file in Directory.GetFiles(Software.WindowsDirectory, "*.xml"))
            {
                ReadDescriptor(file);
            }
        }

        public static void SaveActiveWindow()
        {
            // Save the state and prefs of this instance.
            WindowDescriptor descriptor = ActiveWindow;

            string filename = descriptor.Id.ToString() + ".xml";
            string path = Path.Combine(Software.WindowsDirectory, filename);

            try
            {
                using (XmlWriter w = XmlWriter.Create(path, xmlWriterSettings))
                {
                    w.WriteStartElement("KinoveaWindow");
                    descriptor.WriteXML(w);
                }
            }
            catch (Exception e)
            {
                log.Error("An error happened during the writing of the window file");
                log.Error(e);
            }
        }

        /// <summary>
        /// Open a new unnamed window.
        /// </summary>
        public static void OpenNewWindow()
        {
            // Just launch the program again.
            // The window manager of the new instance will take care of creating the new window descriptor.
            string path = Path.Combine(AppContext.BaseDirectory, "Kinovea.exe");
            var p = new Process();
            p.StartInfo.FileName = path;
            p.Start();
        }

        /// <summary>
        /// Create a new unnamed window descriptor, set it as the active window
        /// and save it to the Windows directory.
        /// </summary>
        private static void LoadNewWindow()
        {
            WindowDescriptor descriptor = new WindowDescriptor();
            ActiveWindow = descriptor;
            SaveActiveWindow();
        }


        /// <summary>
        /// Set the active window to the last closed window.
        /// </summary>
        private static void LoadLastClosedWindow()
        {
            if (lastClosedWindow == null)
                return;
        
            // TODO: check if it's already active.
            // If it's already active switch to it and close ?

            ActiveWindow = lastClosedWindow;
        }

        /// <summary>
        /// Set the active window to the one with the given name.
        /// </summary>
        private static void LoadNamedWindow(string name)
        {
            throw new NotImplementedException("Loading a named window is not implemented yet.");

            // Only if it's not already active.
            // If it's already active switch to it and close ?
        }


        /// <summary>
        /// Read one descriptor, add it to the list and possibly update last closed window.
        /// </summary>
        private static void ReadDescriptor(string file)
        {
            try
            {
                using (XmlReader r = XmlReader.Create(file, xmlReaderSettings))
                {
                    r.MoveToContent();
                    if (r.Name == "KinoveaWindow")
                    {
                        WindowDescriptor descriptor = new WindowDescriptor();
                        descriptor.ReadXML(r);
                        windowDescriptors.Add(descriptor);

                        if (bestSaveTime < descriptor.LastSave)
                        {
                            lastClosedWindow = descriptor;
                            bestSaveTime = descriptor.LastSave;
                        }
                    }
                    else
                    {
                        log.ErrorFormat("The file {0} is not a valid Kinovea window file.", file);
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("An error happened during the reading of the window file {0}.", file);
                log.Error(e);
            }
        }
    }
}
