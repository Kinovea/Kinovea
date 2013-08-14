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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Bsc;

namespace Kinovea.Services
{
    /// <summary>
    /// Manages Command line arguments mechanics.
    /// </summary>
    /// <remarks>Design Pattern : Singleton</remarks>
    public class CommandLineArgumentManager
    {
        #region Properties
        public bool ParametersParsed
        {
            get { return parametersParsed; }
        }
        public string InputFile
        {
            get 
            { 
                if(inputFile == noInputFile)
                    return null;
                else
                    return inputFile;
            }
        }
        public int SpeedPercentage
        {
            get { return speedPercentage;}
        }
        public bool SpeedConsumed
        {
            // Indicates whether the SpeedPercentage argument has been used by a PlayerScreen.
            get { return speedConsumed;}
            set { speedConsumed = value;}
        }
        public bool StretchImage
        {
            get { return stretchImage;}
        }
        public bool HideExplorer
        {
            get { return hideExplorer;}
        }
        
        #endregion
        
        #region Members
        private static readonly string noInputFile = "none";
        private string inputFile = noInputFile;
        private int speedPercentage = 100;
        private bool stretchImage;
        private bool hideExplorer;
        private bool parametersParsed;
        private bool speedConsumed;
        private static CommandLineArgumentManager instance = null;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public static CommandLineArgumentManager Instance()
        {
            if (instance == null)
            {
                instance = new CommandLineArgumentManager();
                instance.InitializeCommandLineParser();
            }
            
            return instance;
        }

        private CommandLineArgumentManager()
        {
        }

        public void ParseArguments(string[] args)
        {
            if(args == null || args.Length < 2)
                return;
            
            // Remove first argument (name of the executable) before parsing.
            string[] arguments = new string[args.Length - 1];
            Array.Copy(args, 1, arguments, 0, args.Length - 1);
            
            log.Debug("Command line arguments:");
            foreach(string arg in arguments)
                log.Debug(arg);	
            
            try
            {
                // Check for the special case where the only argument is a filename.
                // this happens when you drag a video on kinovea.exe
                if(arguments.Length == 1 && File.Exists(arguments[0]))
                {
                    inputFile = arguments[0];
                }
                else if (arguments.Length == 1 && (arguments[0].Trim() == "-help" || arguments[0].Trim() == "-h"))
                {
                    PrintUsage();
                }
                else
                {
                    CommandLineArgumentParser.ParseArguments(arguments);
                    
                    // Reparse the types, (we do that in the try catch block in case it fails.)
                    inputFile = CommandLineArgumentParser.GetParamValue("-file");
                    
                    speedPercentage = int.Parse(CommandLineArgumentParser.GetParamValue("-speed"));
                    speedPercentage = Math.Min(Math.Max(speedPercentage, 1), 200);
                    
                    stretchImage = CommandLineArgumentParser.IsSwitchOn("-stretch");
                    hideExplorer = CommandLineArgumentParser.IsSwitchOn("-noexp");
                }
            }
            catch (CommandLineArgumentException e)
            {
                log.Error("Command line arguments couldn't be parsed.");
                log.Error(e.Message);
                PrintUsage();
            }
            
            // Validate parameters.
            // Here maybe we should check for the coherence of what the user entered.
            // for exemple if he entered a -speed but no -file...
            
            parametersParsed = true;
        }
        
        private void InitializeCommandLineParser()
        {
            // Define parameters and switches. (All optional).
            
            // Parameters and their default values.
            CommandLineArgumentParser.DefineOptionalParameter(
                new string[]{	"-file = " + inputFile,
                                "-speed = " + speedPercentage.ToString()
                             });

            // Switches. (default will be "false")
            CommandLineArgumentParser.DefineSwitches(
                new string[]{	"-stretch",
                                "-noexp"
                             });
        }
        
        private static void PrintUsage()
        {
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

