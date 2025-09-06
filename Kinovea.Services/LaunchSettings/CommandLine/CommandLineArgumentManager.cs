#region license
/*
Copyright © Joan Charmant 2009.
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Bsc;

namespace Kinovea.Services
{
    public class CommandLineArgumentManager
    {
        #region Native methods
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        #endregion

        #region Members
        private static CommandLineArgumentManager instance = null;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public static CommandLineArgumentManager Instance
        {
            get 
            {
                if (instance == null)
                    instance = new CommandLineArgumentManager();
    
                return instance;
            }
        }
        private CommandLineArgumentManager()
        {
            InitializeCommandLineParser();
        }
        private void InitializeCommandLineParser()
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            options.Add("-name", "");
            options.Add("-id", "");
            options.Add("-workspace", "");
            options.Add("-video", "");
            options.Add("-speed", "");
            CommandLineArgumentParser.DefineOptionalParameter(options);

            // Note: it doesn't make sense to define flags that default to true.
            // The default is that the flag is not set, so false.
            List<string> switches = new List<string>();
            switches.Add("-stretch");
            switches.Add("-hideExplorer");
            CommandLineArgumentParser.DefineSwitches(switches);
        }

        public void ParseArguments(string[] args)
        {
            string[] arguments = args.Skip(1).ToArray();
            
            if(arguments.Length < 1)
                return;
            
            string joined = string.Join(" ", arguments);
            log.DebugFormat("Command line arguments: {0}", string.Join(" ", arguments));
            
            try
            {
                if(arguments.Length == 1)
                {
                    if(arguments[0].Trim().ToLower() == "-help" || arguments[0].Trim().ToLower() == "-h")
                    {
                        PrintUsage();
                    }
                    else if(File.Exists(arguments[0]))
                    {
                        // Special case for dragging a file on top of the program icon.
                        // Assume video and try to load it in a single screen.
                        ScreenDescriptorPlayback sdp = new ScreenDescriptorPlayback();
                        sdp.FullPath = arguments[0];
                        sdp.Autoplay = true;
                        sdp.Stretch = true;
                        LaunchSettingsManager.AddScreenDescriptor(sdp);
                    }
                    else
                    {
                        log.ErrorFormat("Single command line argument provided but not a file.");
                        PrintUsage();
                    }
                }
                else
                {
                    // There are two modes of operation.
                    // 1. we reload a specific window with all its settings.
                    // 2. we load a video in a new window.
                    //
                    // The goal is that by the time Program.cs > Main() starts initializing the 
                    // application, we have built a screen list. 
                    // In the case of loading a single video we construct a screen list
                    // manually here. Otherwise the screen list is coming from the saved window.

                    CommandLineArgumentParser.ParseArguments(arguments);

                    // One of the following 3 is required.
                    string name =   CommandLineArgumentParser.GetParamValue("-name");
                    string id =     CommandLineArgumentParser.GetParamValue("-id");
                    string video =  CommandLineArgumentParser.GetParamValue("-video");
                    
                    // These are only used with -video.
                    string speed =      CommandLineArgumentParser.GetParamValue("-speed");
                    bool stretch =      CommandLineArgumentParser.IsSwitchOn("-stretch");
                    bool hideExplorer = CommandLineArgumentParser.IsSwitchOn("-hideExplorer");

                    if (!string.IsNullOrEmpty(name))
                    {
                        LaunchSettingsManager.RequestedWindowName = name;
                        return;
                    }
                    else if (!string.IsNullOrEmpty(id))
                    {
                        LaunchSettingsManager.RequestedWindowId = id;
                        return;
                    }
                    else if (!string.IsNullOrEmpty(video))
                    {
                        // Build screen descriptor manually.
                        ScreenDescriptorPlayback sdp = new ScreenDescriptorPlayback();
                        sdp.FullPath = video;

                        double speedValue;
                        bool parsed = double.TryParse(speed, NumberStyles.Any, CultureInfo.InvariantCulture, out speedValue);
                        if (parsed)
                            speedValue = Math.Max(1, Math.Min(200, speedValue));
                        else
                            speedValue = 100;

                        sdp.SpeedPercentage = speedValue;
                        sdp.Stretch = stretch;

                        LaunchSettingsManager.AddScreenDescriptor(sdp);
                        LaunchSettingsManager.ExplorerVisible = !hideExplorer;

                        return;
                    }
                    else
                    {
                        // If we have neither a video nor a window, we shouldn't have any other arguments.
                        if (stretch)
                        {
                            string error = "Ignored command line parameter: -stretch. No video provided.";
                            Console.WriteLine(error);
                            log.ErrorFormat(error);
                        }

                        if (!string.IsNullOrEmpty(speed))
                        {
                            string error = "Ignored command line parameter: -speed. No video provided.";
                            Console.WriteLine(error);
                            log.ErrorFormat(error);
                        }
                    }
                }
            }
            catch (CommandLineArgumentException e)
            {
                log.Error("Command line arguments could not be parsed.");
                log.Error(e.Message);
                PrintUsage();
            }
        }
        private static void PrintUsage()
        {
            AttachConsole(-1);

            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("kinovea.exe");
            Console.WriteLine("    [-name <string>] [-id <string>] [-video <path>] [-speed <0-200>] [-stretch] [-hideExplorer]");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  -name: name of the Kinovea window to reload.");
            Console.WriteLine("  -id: id of the Kinovea window to reload.");
            Console.WriteLine("  -video: path to a video to play.");
            Console.WriteLine("  -speed: playback speed to play the video at, as a percentage of its original frame rate.");
            Console.WriteLine("  -stretch: the video will be expanded to fit the screen size. Default: false.");
            Console.WriteLine("  -hideExplorer: The explorer panel will not be visible. Default: false.");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine("1. > kinovea.exe -name \"Dual replay\"");
            Console.WriteLine("2. > kinovea.exe -video \"test.mkv\"");
            Console.WriteLine("3. > kinovea.exe -video \"test.mkv\" -speed 50");
        }
    }
}

