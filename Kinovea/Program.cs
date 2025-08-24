/*
Copyright © Joan Charmant 2008.
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

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using Kinovea.Services;
using System.Diagnostics;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using log4net.Appender;

namespace Kinovea.Root
{
    internal static class Program
    {
        private static bool IsFirstInstance
        {
            get
            {
                bool gotMutex;
                mutex = new Mutex(false, "Local\\" + appGuid, out gotMutex);
                return gotMutex;
            }
        }
        private static Mutex mutex;
        private static string appGuid = "b049b83e-90f3-4e84-9289-52ee6ea2a9ea";
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));


        [STAThread]
        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;
            
            Thread.CurrentThread.Name = "Main";
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            Software.Initialize(assembly.GetName().Version);

            Software.LogInfo();
            Software.SanityCheckDirectories();
            PreferencesManager.Initialize();
            PreferencesManager.Refresh();

            // Check if this is the first instance of the application.
            // If we are not started on a specific name this will be used to
            // restore the last closed window if we are first or start a new one otherwise.
            bool isFirstInstance = IsFirstInstance;
            
            // Get launch settings from thecommand line, including instance name.
            // This will update the static LanchSettingsManager with the data.
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
                CommandLineArgumentManager.Instance.ParseArguments(args);

            // Read the list of saved instances and determine which one to load in this window.
            // This may exit the application if we figure out the user just wanted 
            // to open a named instance that is already active elsewhere.
            WindowManager.Startup(isFirstInstance);

            // Make sure each instance logs to its own log file.
            Software.ConfigureLogging();

            //----------------------------------------------------------------

            // General application startup workflow.
            log.Debug("Application level initialisations.");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            log.Debug("Showing SplashScreen.");
            FormSplashScreen splashForm = new FormSplashScreen();
            splashForm.Show();
            splashForm.Update();

            RootKernel kernel = new RootKernel();
            kernel.Prepare();
            
            log.Debug("Closing splash screen.");
            splashForm.Close();

            log.Debug("Launching.");
            kernel.Launch();
        }

        /// <summary>
        /// Top level catch-all for unhandled exceptions.
        /// Dump the data to a separate file if possible.
        /// </summary>
        private static void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = (Exception)args.ExceptionObject;
            
            string message = string.Format("Message: {0}", ex.Message);
            string source = string.Format("Source: {0}", ex.Source);
            string target = string.Format("Target site: {0}", ex.TargetSite);
            string inner = string.Format("InnerException: {0}", ex.InnerException);
            string trace = string.Format("Stack: {0}", ex.StackTrace);
            
            string dumpFile = string.Format("Unhandled Crash - {0}.txt", Guid.NewGuid());
            using (StreamWriter sw = File.AppendText(Path.Combine(Software.SettingsDirectory, dumpFile)))
            {
                sw.WriteLine(message);
                sw.WriteLine(source);
                sw.WriteLine(target);
                sw.WriteLine(inner);
                sw.WriteLine(trace);
                sw.Close();
            }
            
            // Dump again in the log.
            log.Error("----------------- Unhandled Crash -------------------------");
            log.Error(message);
            log.Error(source);
            log.Error(target);
            log.Error(inner);
            log.Error(trace);
        }
    }
}