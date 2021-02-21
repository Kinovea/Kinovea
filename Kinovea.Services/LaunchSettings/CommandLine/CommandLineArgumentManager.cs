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
            options.Add("-workspace", "");
            options.Add("-video", "");
            options.Add("-speed", "100");
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
                    if(arguments[0].Trim() == "-help" || arguments[0].Trim() == "-h")
                    {
                        PrintUsage();
                    }
                    else if(File.Exists(arguments[0]))
                    {
                        // Special case for dragging a file on top of the program icon or starting with a workspace.
                        if (Path.GetExtension(arguments[0]).ToLower() == ".xml")
                        {
                            Workspace workspace = new Workspace();
                            bool loaded = workspace.Load(arguments[0]);
                            if (loaded)
                            {
                                foreach (IScreenDescription screen in workspace.Screens)
                                    LaunchSettingsManager.AddScreenDescription(screen);
                            }
                            else
                            {
                                log.ErrorFormat("Workspace from command line argument not loaded.");
                            }
                        }
                        else
                        {
                            // Assume video and try to load it in a single screen.
                            ScreenDescriptionPlayback sdp = new ScreenDescriptionPlayback();
                            sdp.FullPath = arguments[0];
                            sdp.Autoplay = true;
                            sdp.Stretch = true;
                            LaunchSettingsManager.AddScreenDescription(sdp);
                        }
                    }
                }
                else
                {
                    CommandLineArgumentParser.ParseArguments(arguments);

                    string name = CommandLineArgumentParser.GetParamValue("-name");
                    bool hideExplorer = CommandLineArgumentParser.IsSwitchOn("-hideExplorer");
                    string workspace = CommandLineArgumentParser.GetParamValue("-workspace");
                    string video = CommandLineArgumentParser.GetParamValue("-video");
                    string speed = CommandLineArgumentParser.GetParamValue("-speed");
                    bool stretch = CommandLineArgumentParser.IsSwitchOn("-stretch");

                    // General program state.
                    if (!string.IsNullOrEmpty(name))
                        LaunchSettingsManager.Name = name;

                    LaunchSettingsManager.ShowExplorer = !hideExplorer;

                    // Workspace.
                    if (!string.IsNullOrEmpty(workspace))
                    {
                        Workspace w = new Workspace();
                        bool loaded = w.Load(workspace);
                        if (loaded)
                        {
                            foreach (IScreenDescription screen in w.Screens)
                                LaunchSettingsManager.AddScreenDescription(screen);
                        }
                        else
                        {
                            log.ErrorFormat("Workspace from command line argument not loaded.");
                        }
                    }
                    else if (!string.IsNullOrEmpty(video))
                    {
                        // Manual description.
                        ScreenDescriptionPlayback sdp = new ScreenDescriptionPlayback();
                        sdp.FullPath = video;

                        double speedValue;
                        bool parsed = double.TryParse(speed, NumberStyles.Any, CultureInfo.InvariantCulture, out speedValue);
                        if (parsed)
                            speedValue = Math.Max(1, Math.Min(200, speedValue));
                        else
                            speedValue = 100;

                        sdp.SpeedPercentage = speedValue;
                        sdp.Stretch = stretch;

                        LaunchSettingsManager.AddScreenDescription(sdp);
                    }
                }
            }
            catch (CommandLineArgumentException e)
            {
                log.Error("Command line arguments couldn't be parsed.");
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
            Console.WriteLine("    [-name <string>] [-hideExplorer] [-workspace <path>] [-video <path>] [-speed <0-200>] [-stretch]");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  -name: name of this instance of Kinovea. Used in the window title and to select a preference file.");
            Console.WriteLine("  -hideExplorer: The explorer panel will not be visible. Default: false.");
            Console.WriteLine("  -workspace: path to a Kinovea workspace XML file. This overrides other video options.");
            Console.WriteLine("  -video: path to a video to load.");
            Console.WriteLine("  -speed: playback speed to play the video at, as a percentage of its original framerate. Default: 100.");
            Console.WriteLine("  -stretch: the video will be expanded to fit the screen size. Default: false.");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine("1. > kinovea.exe -name Replay -workspace myReplayWorkspace.xml");
            Console.WriteLine("2. > kinovea.exe -video test.mkv -stretch");
            Console.WriteLine("3. > kinovea.exe -video test.mkv -speed 50");
        }
    }
}

