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
			get { return m_bParametersParsed; }
		}
    	public string InputFile
    	{
    		get 
    		{ 
    			if(m_InputFile == NoInputFile)
    				return null;
    			else
	    			return m_InputFile;
    		}
    	}
    	public int SpeedPercentage
    	{
    		get { return m_iSpeedPercentage;}
    	}
    	public bool SpeedConsumed
    	{
    		// Indicates whether the SpeedPercentage argument has been used by a PlayerScreen.
    		get { return m_bSpeedConsumed;}
    		set { m_bSpeedConsumed = value;}
    	}
    	public bool StretchImage
    	{
    		get { return m_bStretchImage;}
    	}
    	public bool HideExplorer
    	{
    		get { return m_bHideExplorer;}
    	}
    	
    	#endregion
    	
        #region Members
        private static readonly string NoInputFile = "none";
        private string m_InputFile = NoInputFile;
        private int m_iSpeedPercentage = 100;
        private bool m_bStretchImage;
        private bool m_bHideExplorer;
		private bool m_bParametersParsed;
		private bool m_bSpeedConsumed;
        private static CommandLineArgumentManager _instance = null;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Instance and Ctor
        public static CommandLineArgumentManager Instance()
        {
        	// get singleton instance.
            if (_instance == null)
            {
                _instance = new CommandLineArgumentManager();
            }
            return _instance;
        }
        private CommandLineArgumentManager()
        {
        }
        #endregion

        #region Implementation
        public void InitializeCommandLineParser()
        {
        	// Define parameters and switches. (All optional).
        	
        	// Parameters and their default values.
        	CommandLineArgumentParser.DefineOptionalParameter(
        		new string[]{	"-file = " + m_InputFile,
        						"-speed = " + m_iSpeedPercentage.ToString()
        	                 });

        	// Switches. (default will be "false")
        	CommandLineArgumentParser.DefineSwitches(
        		new string[]{	"-stretch",
           	 					"-noexp"
        					 });
        }
        public void ParseArguments(string[] _args)
        {
        	// Log argumets.
        	if(_args.Length > 1)
        	{
        		log.Debug("Command line arguments:");
        		foreach(string arg in _args)
	        	{
	        		log.Debug(arg);	
	        	}	
        	}
        	
        	// Remove first argument (name of the executable) before parsing.
        	string[] args = new string[_args.Length-1];        	
        	for (int i = 1; i < _args.Length; i++)
            {
        		args[i-1] = _args[i];
            }
        	
        	try
        	{
        		// Check for the special case where the only argument is a filename.
        		// this happens when you drag a video on kinovea.exe
        		if(args.Length == 1)
        		{
        			if(File.Exists(args[0]))
        			{
        				m_InputFile = args[0];
        			}
        		}
        		else if (args.Length == 1 && (args[0].Trim() == "-help" || args[0].Trim() == "-h"))
		        {
        			// Check for the special parameter -help or -h, 
        			// and then output info on supported params.
		            PrintUsage();
		        }
		        else
		        {
		            CommandLineArgumentParser.ParseArguments(args);
		            
		            // Reparse the types, (we do that in the try catch block in case it fails.)
		            m_InputFile = CommandLineArgumentParser.GetParamValue("-file");
		            m_iSpeedPercentage = int.Parse(CommandLineArgumentParser.GetParamValue("-speed"));
		            if(m_iSpeedPercentage > 200) m_iSpeedPercentage = 200;
		            if(m_iSpeedPercentage < 1) m_iSpeedPercentage = 1;
		            m_bStretchImage = CommandLineArgumentParser.IsSwitchOn("-stretch");
		            m_bHideExplorer = CommandLineArgumentParser.IsSwitchOn("-noexp");
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
        	
        	m_bParametersParsed = true;
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
        #endregion

    }
}

