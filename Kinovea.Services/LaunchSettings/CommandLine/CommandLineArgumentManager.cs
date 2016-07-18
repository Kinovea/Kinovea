#region license
/*
Copyright © Joan Charmant 2009.
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Bsc;

namespace Kinovea.Services
{
    public class CommandLineArgumentManager
    {
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
            string[] optional = new string[]
            {
                "-file = none",
                "-speed = 100"
            };
            
            string[] switches = new string[]
            {
                "-stretch",
                "-noexp"
            };

            CommandLineArgumentParser.DefineOptionalParameter(optional);
            CommandLineArgumentParser.DefineSwitches(switches);
        }
        public void ParseArguments(string[] args)
        {
            string[] arguments = args.Skip(1).ToArray();
            
            if(arguments.Length < 1)
                return;
            
            string joined = string.Join(" ", arguments);
            log.DebugFormat("Command line arguments:{0}", string.Join(" ", arguments));
            
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
                        // Special case for dragging a file on kinovea.exe icon.
                        ScreenDescriptionPlayback sdp = new ScreenDescriptionPlayback();
                        sdp.FullPath = arguments[0];
                        LaunchSettingsManager.AddScreenDescription(sdp);
                    }
                }
                else
                {
                    CommandLineArgumentParser.ParseArguments(arguments);
                    
                    // TODO: check coherence.
                    ScreenDescriptionPlayback sdp = new ScreenDescriptionPlayback();
                    sdp.FullPath = CommandLineArgumentParser.GetParamValue("-file");
                    int speed = int.Parse(CommandLineArgumentParser.GetParamValue("-speed"));
                    speed = Math.Max(Math.Min(200, speed), 1);
                    sdp.SpeedPercentage = speed;
                    sdp.Stretch = CommandLineArgumentParser.IsSwitchOn("-stretch");
                    
                    LaunchSettingsManager.AddScreenDescription(sdp);
                    LaunchSettingsManager.ShowExplorer = !CommandLineArgumentParser.IsSwitchOn("-noexp");
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
            // Doesn't work ?
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("kinovea.exe");
            Console.WriteLine("    [-file <path>] [-speed <0-200>] [-noexp] [-stretch]");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  -file: complete path of a video to launch; default: 'unknown'.");
            Console.WriteLine("  -speed: percentage of original speed to play the video; default: 100.");
            Console.WriteLine("  -stretch: The video will be expanded to the screen size; default: false.");
            Console.WriteLine("  -noexp: The file explorer will not be visible; default: false.");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine("1. > kinovea.exe -file test.mkv -speed 50");
            Console.WriteLine();
            Console.WriteLine("2. > kinovea.exe -file test.mkv -stretch -noexp");
        }
    }
}

