/*
Copyright © Joan Charmant 2008.
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

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

[assembly: CLSCompliant(false)]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "LogConf.xml", Watch = true)]
namespace Kinovea.Root
{
    static class Program
    {
        static Mutex mutex;
        static string AppGuid = "b049b83e-90f3-4e84-9289-52ee6ea2a9ea";
        static bool FirstInstance
        {
            get
            {
                bool bGotMutex;
                mutex = new Mutex(false, "Local\\" + AppGuid, out bGotMutex);
                return bGotMutex;
            }
        }
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        [STAThread]
        static void Main()
        {
            //--------------------------------------------------------
            // Each time the program runs, we try to register a mutex.
            // If it fails, we are already running. 
            //--------------------------------------------------------
            if (Program.FirstInstance)
            {
            	SanityCheckDirectories();
            	
                log.Debug("Kinovea starting.");
                log.Debug("Application level initialisations.");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                log.Debug("Show SplashScreen.");
                FormSplashScreen splashForm = new FormSplashScreen();
                splashForm.Show();
                splashForm.Update();

                RootKernel kernel = new RootKernel();
                kernel.Prepare();

                log.Debug("Close splash screen.");
                splashForm.Close();

                log.Debug("Launch.");
                kernel.Launch();
            }
        }
        private static void SanityCheckDirectories()
        {
        	// Create the Kinovea folder under App Data if it doesn't exist.
        	string prefDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Kinovea\\";
        	if(!Directory.Exists(prefDir))
        	{
        		Directory.CreateDirectory (prefDir);
        	}
        	
        	// Create the Kinovea\ColorProfiles if it doesn't exist.
        	string colDir = prefDir + "ColorProfiles\\";
        	if(!Directory.Exists(colDir))
        	{
        	   	Directory.CreateDirectory(colDir);
        	}
        }
    }
}