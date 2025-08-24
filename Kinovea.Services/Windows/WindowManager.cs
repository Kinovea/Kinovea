using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// Handles the management of active and inactive windows.
    /// </summary>
    public static class WindowManager
    {
        #region Properties

        /// <summary>
        /// The name to use in the title bar for the active window. 
        /// </summary>
        public static string TitleName
        {
            get; private set;
        }

        public static WindowDescriptor ActiveWindow
        {
            get; private set;
        }

        public static List<WindowDescriptor> WindowDescriptors
        {
            get { return windowDescriptors; }
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
            log.Info("Window startup.");

            ReadAllDescriptors();

            // Check we were started as a named instance from the command line.
            // At the end of this we should have updated LaunchSettingsManager.Name
            // with the final name based on the window we are loading, if any.
            string requestedName = LaunchSettingsManager.RequestedWindowName;
            Guid requestedId = Guid.Empty;
            bool parsed = Guid.TryParse(LaunchSettingsManager.RequestedWindowId, out requestedId);

            if (!string.IsNullOrEmpty(requestedName))
            {
                // Requesting a specific named window.
                bool found = false;
                foreach (var descriptor in windowDescriptors)
                {
                    if (descriptor.Name == requestedName)
                    {
                        log.InfoFormat("Loading window by name: \"{0}\"", requestedName);
                        LoadSpecificWindow(descriptor);
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
                    log.ErrorFormat("Named window \"{0}\" not found. Creating a new window.", requestedName);
                    LoadNewWindow();
                }
            }
            else if (requestedId != Guid.Empty)
            {
                // Requesting a specific window by id.
                // This typically only happens via the "Reopen window" menu when clicking 
                // on an anonymous window.
                bool found = false;
                foreach (var descriptor in windowDescriptors)
                {
                    if (descriptor.Id == requestedId)
                    {
                        log.InfoFormat("Loading window by id: \"{0}\"", requestedId);
                        LoadSpecificWindow(descriptor);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    log.ErrorFormat("Window with Id \"{0}\" not found. Creating a new window.", requestedId);
                    LoadNewWindow();
                }
            }
            else
            {
                // Starting without a name or id.
                // This happens when clicking the executable or from the menu Open new window.
                if (isFirstInstance)
                {
                    // No name and first instance -> load the last closed window.
                    if (windowDescriptors.Count > 0 && lastClosedWindow != null)
                    {
                        log.Info("No name provided, loading last closed window.");
                        LoadLastClosedWindow();
                    }
                    else
                    {
                        log.Info("No name provided and no saved window, creating a new window.");
                        LoadNewWindow();
                    }
                }
                else
                {
                    
                    log.Info("No name provided and not the first instance, creating a new window.");
                    LoadNewWindow();
                }
            }

            if (ActiveWindow == null)
            {
                // This is when we have found that the user just wanted to re-open an already opened window.
                // In that case we have brought it to front and we can just close.
                log.WarnFormat("Requested window is already active. Closing.");
            }
            else
            {
                // We have loaded a valid window descriptor in the ActiveWindow.
                // Update the launch settings and title so the whole program configures itself.
                UpdateLaunchSettings();
                SetTitleName();
                log.DebugFormat("Loaded active window: \"{0}\", Id:{1}.", ActiveWindow.Name, ActiveWindow.Id);
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


        public static void ReopenWindow(WindowDescriptor d)
        {
            // TODO: Check if the window is already active by name or id.
            // If so, bring it to front and return.
            //
            // Corner case: it is possible that the user is asking for the window
            // that was started first and that has erased its name from the title.
            // Restarting an active window is considered user error and we just try to 
            // protect against it on a best-effort basis.
            // A solution for this case would be for each window to grab a global mutex 
            // corresponding to their id and check if it's available.
            // We would still not be able to bring it to front but at least we wouldn't start it a second time.

            string args = "";
            if (!string.IsNullOrEmpty(d.Name))
            {
                args = string.Format("-name \"{0}\"", d.Name);
                log.DebugFormat("Reopening named window: {0}.", d.Name);
            }
            else
            {
                args = string.Format("-id \"{0}\"", d.Id);
                log.DebugFormat("Reopening unnamed window: {0}.", d.Id);
            }

            // Launch the program again with the name argument.
            // The window manager of the new instance will take care of loading the named window descriptor.
            string path = Path.Combine(AppContext.BaseDirectory, "Kinovea.exe");
            var p = new Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = args;
            p.Start();
        }

        /// <summary>
        /// Set the name to use in the title bar.
        /// This should be called every time we change the active window name.
        /// And then the main window title bar should be updated.
        /// </summary>
        public static void SetTitleName()
        {
            // Heuristic:
            // - if the window has a name, use it.
            // - if the window has no name but is the only instance in town, keep it empty.
            // - otherwise use the fake name based on the id.
            if (!string.IsNullOrEmpty(ActiveWindow.Name))
            {
                TitleName = ActiveWindow.Name;
            }
            else
            {
                // Debugging : always used the id name.
                TitleName = GetIdName(ActiveWindow);

                // Production:
                //Process[] instances = Process.GetProcessesByName("Kinovea");
                //TitleName = instances.Length == 1 ? "" : GetIdName(ActiveWindow);
            }
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
        private static void LoadSpecificWindow(WindowDescriptor descriptor)
        {
            // Only if it's not already active.
            // If it's already active switch to it and close ?
            // FIXME: How to we find if the window is active when all we have is an id?


            ActiveWindow = descriptor;
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

        /// <summary>
        /// Update the launch settings based on the active window.
        /// </summary>
        public static void UpdateLaunchSettings()
        {
            // Here we set the name to whatever is in the active window descriptor.
            // This is not what should be used for the main window title bar.
            // That one also depends on whether we are the only instance or not.
            // The LaunchSettings name shouldn't really be read by anything after this point, 
            // it is only used to request a particular instance on the command line.
            LaunchSettingsManager.RequestedWindowName = ActiveWindow.Name;

            // Set up the screen list. This will be used to restore the screens
            LaunchSettingsManager.ClearScreenDescriptions();
            foreach (var screen in ActiveWindow.ScreenList)
            {
                LaunchSettingsManager.AddScreenDescription(screen.Clone());
            }
        }

        private static string GetIdName(WindowDescriptor d)
        {
            return d.Id.ToString().Substring(0, 8);
        }
    }
}
